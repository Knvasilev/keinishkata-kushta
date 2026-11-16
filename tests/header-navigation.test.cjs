const { test } = require('node:test');
const assert = require('node:assert/strict');
const { readFileSync } = require('node:fs');
const { join } = require('node:path');
const { runInNewContext } = require('node:vm');

const script = readFileSync(join(__dirname, '../KeinishkataKushta/wwwroot/js/navigation.js'), 'utf8');

function navigation() {
    const document = {};
    function element() {
        const listeners = {};
        return {
            addEventListener(name, fn) { listeners[name] = fn; },
            fire(name, event = {}) { listeners[name]?.(event); },
            focus() { document.activeElement = this; }
        };
    }
    const header = element();
    const menu = element();
    const toggle = element();
    const link = element();
    const desktop = element();
    desktop.matches = false;
    let open = false;
    let hideCalls = 0;
    const attributes = { 'aria-expanded': 'false' };
    toggle.dataset = { labelOpen: 'Отвори менюто', labelClose: 'Затвори менюто' };
    toggle.getAttribute = name => attributes[name];
    toggle.setAttribute = (name, value) => { attributes[name] = value; };
    toggle.closest = () => header;
    menu.classList = { contains: () => open };
    menu.contains = node => node === link;
    menu.querySelector = () => link;
    document.querySelector = () => toggle;
    document.getElementById = () => menu;
    const collapse = {
        hide() {
            hideCalls++;
            menu.fire('hide.bs.collapse');
            open = false;
            attributes['aria-expanded'] = 'false';
        }
    };
    runInNewContext(script, {
        document,
        window: { matchMedia: () => desktop, bootstrap: { Collapse: { getOrCreateInstance: () => collapse } } }
    });
    return { document, header, menu, toggle, link, desktop, attributes,
        get hideCalls() { return hideCalls; },
        open() { open = true; attributes['aria-expanded'] = 'true'; menu.fire('show.bs.collapse'); }
    };
}

test('localized action label follows Bootstrap opening and closing', () => {
    const n = navigation();
    assert.equal(n.attributes['aria-label'], 'Отвори менюто');
    n.open();
    assert.equal(n.attributes['aria-label'], 'Затвори менюто');
    n.menu.fire('hide.bs.collapse');
    assert.equal(n.attributes['aria-label'], 'Отвори менюто');
});

test('Escape closes an open mobile menu and restores focus to the toggle', () => {
    const n = navigation();
    n.open();
    n.link.focus();
    let prevented = false;
    n.header.fire('keydown', { key: 'Escape', preventDefault() { prevented = true; } });
    assert.equal(n.hideCalls, 1);
    assert.equal(n.document.activeElement, n.toggle);
    assert.equal(n.attributes['aria-expanded'], 'false');
    assert.equal(prevented, true);
});

test('Escape ignores closed menus and desktop; ordinary Tab is unchanged', () => {
    const n = navigation();
    const event = { key: 'Escape', preventDefault() { assert.fail('should not intercept'); } };
    n.header.fire('keydown', event);
    n.open();
    n.desktop.matches = true;
    n.header.fire('keydown', event);
    n.desktop.matches = false;
    n.header.fire('keydown', { ...event, key: 'Tab' });
    assert.equal(n.hideCalls, 0);
});

test('crossing the breakpoint keeps focus on visible navigation controls', () => {
    const n = navigation();
    n.toggle.focus();
    n.desktop.matches = true;
    n.desktop.fire('change');
    assert.equal(n.document.activeElement, n.link);
    n.desktop.matches = false;
    n.desktop.fire('change');
    assert.equal(n.document.activeElement, n.toggle);
    n.open();
    n.link.focus();
    n.desktop.fire('change');
    assert.equal(n.document.activeElement, n.link);
});

test('finishing a close recovers hidden link focus without stealing outside focus', () => {
    const n = navigation();
    n.link.focus();
    n.menu.fire('hidden.bs.collapse');
    assert.equal(n.document.activeElement, n.toggle);
    const outside = {};
    n.document.activeElement = outside;
    n.menu.fire('hidden.bs.collapse');
    assert.equal(n.document.activeElement, outside);
});

test('pages without a header or Bootstrap need no navigation enhancement', () => {
    for (const window of [{}, { bootstrap: {} }]) {
        assert.doesNotThrow(() => runInNewContext(script, {
            document: { querySelector: () => null, getElementById: () => null }, window
        }));
    }
});
