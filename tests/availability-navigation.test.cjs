const { test } = require('node:test');
const assert = require('node:assert/strict');
const { readFileSync } = require('node:fs');
const { join } = require('node:path');
const { runInNewContext } = require('node:vm');
const source = readFileSync(join(__dirname, '../KeinishkataKushta/wwwroot/js/availability.js'), 'utf8');

function setup({ fetchAvailable = true } = {}) {
    const requests = [];
    const timers = new Map();
    const document = { activeElement: null };
    const status = { textContent: '' };
    let handler;
    let current;
    let nextFragment;
    let fetchImpl = async () => ({ ok: true, text: async () => 'partial' });
    const attributes = {};
    const widget = {
        dataset: { loading: 'Loading...', error: 'Try again.' },
        setAttribute: (name, value) => { attributes[name] = value; },
        contains: element => Object.values(current.controls).includes(element),
        querySelectorAll: () => Object.values(current.controls).filter(control => !control.disabled),
        querySelector: selector => selector === '[data-calendar-fragment]' ? current
            : current.controls[selector.match(/="(.+)"/)[1]],
        addEventListener: (_, fn) => { handler = fn; }
    };
    function fragment(month, disabled = []) {
        const result = { month, controls: {} };
        for (const name of ['today', 'previous', 'next']) {
            const control = {
                dataset: { calendarNav: name }, disabled: disabled.includes(name), attributes: {},
                href: `https://localhost/Availability?lang=bg&month=${month}-${name}`,
                closest() { return this.disabled ? null : this; },
                setAttribute(key, value) { this.attributes[key] = value; },
                removeAttribute(key) { delete this.attributes[key]; },
                focus(options) { document.activeElement = this; this.focusOptions = options; }
            };
            result.controls[name] = control;
        }
        result.contains = element => Object.values(result.controls).includes(element);
        result.querySelector = () => ({ textContent: month });
        result.replaceWith = replacement => { current = replacement; };
        return result;
    }
    current = fragment('September');
    nextFragment = fragment('October');
    const window = { location: { href: 'https://localhost/Availability' }, AbortController };
    const fetch = (...args) => { requests.push(args); return fetchImpl(...args); };
    if (fetchAvailable) window.fetch = fetch;
    document.querySelector = selector => selector === '[data-availability]' ? widget : status;
    runInNewContext(source, {
        window, document, fetch, AbortController,
        DOMParser: class { parseFromString() { return { querySelector: () => nextFragment }; } },
        setTimeout(fn) { const id = timers.size + 1; timers.set(id, fn); return id; },
        clearTimeout(id) { timers.delete(id); }
    });
    return {
        requests, timers, document, status, attributes, window, fragment,
        get current() { return current; },
        get handler() { return handler; },
        set response(fn) { fetchImpl = fn; },
        set next(value) { nextFragment = value; },
        click(name = 'next', overrides = {}) {
            const event = {
                button: 0, target: current.controls[name], prevented: false,
                preventDefault() { this.prevented = true; }, ...overrides
            };
            return { event, done: handler(event) };
        }
    };
}

test('loads only the partial and leaves the page URL unchanged', async () => {
    const v = setup();
    const click = v.click();
    assert.equal(v.attributes['aria-busy'], 'true');
    assert.equal(v.status.textContent, 'Loading...');
    await click.done;
    assert.equal(click.event.prevented, true);
    assert.equal(v.current.month, 'October');
    assert.match(v.requests[0][0], /lang=bg/);
    assert.equal(v.requests[0][1].headers['X-Requested-With'], 'XMLHttpRequest');
    assert.equal(v.requests[0][1].cache, 'no-store');
    assert.equal(v.window.location.href, 'https://localhost/Availability');
    assert.equal(v.attributes['aria-busy'], 'false');
    assert.equal(v.timers.size, 0);
});

test('delegated previous and Today links work after replacing the month', async () => {
    const v = setup();
    await v.click().done;
    v.next = v.fragment('September');
    await v.click('previous').done;
    assert.equal(v.current.month, 'September');
    v.next = v.fragment('Current month');
    await v.click('today').done;
    assert.equal(v.current.month, 'Current month');
    assert.equal(v.requests.length, 3);
});

test('rapid clicks cannot start competing requests', async () => {
    const v = setup();
    let finish;
    v.response = () => new Promise(resolve => { finish = resolve; });
    const first = v.click();
    const second = v.click();
    await second.done;
    assert.equal(v.requests.length, 1);
    assert.equal(second.event.prevented, true);
    finish({ ok: true, text: async () => 'partial' });
    await first.done;
    assert.equal(v.current.month, 'October');
});

test('HTTP, network, redirect, and malformed responses retain the old month', async () => {
    for (const failure of ['http', 'network', 'redirect', 'fragment']) {
        const v = setup();
        const original = v.current;
        v.response = async () => {
            if (failure === 'network') throw Error('offline');
            return { ok: failure !== 'http', redirected: failure === 'redirect', text: async () => 'html' };
        };
        if (failure === 'fragment') v.next = null;
        await v.click().done;
        assert.equal(v.current, original);
        assert.equal(v.status.textContent, 'Try again.');
        assert.equal(v.attributes['aria-busy'], 'false');
        assert.equal(v.current.controls.next.attributes['aria-disabled'], undefined);
        v.response = async () => ({ ok: true, text: async () => 'partial' });
        v.next = v.fragment('October');
        await v.click().done;
        assert.equal(v.current.month, 'October');
    }
});

test('timed-out requests abort and allow another attempt', async () => {
    const v = setup();
    v.response = (_, options) => new Promise((_, reject) => {
        options.signal.addEventListener('abort', () => reject(Error('aborted')));
    });
    const click = v.click();
    [...v.timers.values()][0]();
    await click.done;
    assert.equal(v.requests[0][1].signal.aborted, true);
    assert.equal(v.current.month, 'September');
    assert.equal(v.attributes['aria-busy'], 'false');
});

test('keyboard focus is restored without scrolling, including month boundaries', async () => {
    const v = setup();
    v.document.activeElement = v.current.controls.next;
    await v.click().done;
    assert.equal(v.document.activeElement, v.current.controls.next);
    assert.equal(v.document.activeElement.focusOptions.preventScroll, true);
    v.next = v.fragment('Last month', ['next']);
    await v.click().done;
    assert.equal(v.document.activeElement, v.current.controls.today);
});

test('does not steal focus moved outside the calendar', async () => {
    const v = setup();
    const outside = {};
    v.document.activeElement = outside;
    await v.click().done;
    assert.equal(v.document.activeElement, outside);
});

test('modified clicks and unsupported browsers keep native navigation', async () => {
    for (const modifier of [{ ctrlKey: true }, { metaKey: true }, { shiftKey: true }, { altKey: true }, { button: 1 }]) {
        const v = setup();
        const click = v.click('next', modifier);
        await click.done;
        assert.equal(click.event.prevented, false);
        assert.equal(v.requests.length, 0);
    }
    assert.equal(setup({ fetchAvailable: false }).handler, undefined);
});
