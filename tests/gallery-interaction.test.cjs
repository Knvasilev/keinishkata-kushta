const { test } = require('node:test');
const assert = require('node:assert/strict');
const { readFileSync } = require('node:fs');
const { join } = require('node:path');
const { runInNewContext } = require('node:vm');

const script = readFileSync(join(__dirname, '../KeinishkataKushta/wwwroot/js/site.js'), 'utf8');

function viewer(count = 3, interactive = true, reducedMotion = false) {
    const document = { body: { classList: { add() {}, remove() {} } } };
    function element() {
        const listeners = {};
        return {
            hidden: false, dataset: {}, animations: 0,
            addEventListener(name, fn) { listeners[name] = fn; },
            fire(name, data = {}) { listeners[name]?.({ preventDefault() {}, ...data }); },
            focus() { document.activeElement = this; },
            removeAttribute(name) { delete this[name]; },
            setPointerCapture() {},
            animate() { this.animations++; return { cancel() {} }; }
        };
    }
    const lightbox = element();
    lightbox.hidden = true;
    const nodes = Object.fromEntries(['image', 'caption', 'counter', 'close',
        ...(interactive ? ['advance'] : ['previous', 'next'])].map(key => [key, element()]));
    const buttons = [nodes.close, ...(interactive ? [nodes.advance] : [nodes.previous, nodes.next])];
    lightbox.querySelector = selector => nodes[selector.slice(14, -1)];
    lightbox.querySelectorAll = () => buttons.filter(button => !button.disabled);
    const triggers = Array.from({ length: count }, (_, index) => {
        const trigger = element();
        trigger.dataset = { gallerySrc: `photo-${index}.jpg`, galleryCaption: `Photo ${index}` };
        return trigger;
    });
    const keyboard = element();
    document.addEventListener = keyboard.addEventListener;
    document.querySelector = () => lightbox;
    document.querySelectorAll = () => triggers;
    runInNewContext(script, { document, window: { matchMedia: () => ({ matches: reducedMotion }) } });
    const swipe = (x, y = 0, cancel = false) => {
        const surface = nodes.advance || nodes.image;
        surface.fire('pointerdown', { isPrimary: true, button: 0, pointerId: 1, clientX: 200, clientY: 200 });
        surface.fire(cancel ? 'pointercancel' : 'pointerup', { pointerId: 1, clientX: 200 + x, clientY: 200 + y });
    };
    return { ...nodes, lightbox, triggers, keyboard, document, swipe };
}

test('photo click advances and wraps; arrows still work from the keyboard', () => {
    const v = viewer();
    v.triggers[0].fire('click');
    for (const expected of [2, 3, 1]) {
        v.advance.fire('click', { detail: 1 });
        assert.equal(v.counter.textContent, expected);
    }
    v.keyboard.fire('keydown', { key: 'ArrowLeft' });
    assert.equal(v.counter.textContent, 3);
});

test('swipe changes exactly one photo, including the following synthesized click', () => {
    const v = viewer();
    v.triggers[0].fire('click');
    v.swipe(-100);
    v.advance.fire('click', { detail: 1 });
    assert.equal(v.counter.textContent, 2);
    v.swipe(100);
    assert.equal(v.counter.textContent, 1);
});

test('vertical and cancelled gestures do not advance', () => {
    const v = viewer();
    v.triggers[0].fire('click');
    v.swipe(70, 130);
    v.advance.fire('click', { detail: 1 });
    assert.equal(v.counter.textContent, 1);
    v.swipe(-100, 0, true);
    v.advance.fire('click', { detail: 1 });
    assert.equal(v.counter.textContent, 1);
});

test('single image does not advance and legacy gallery arrows remain supported', () => {
    const single = viewer(1);
    single.triggers[0].fire('click');
    assert.equal(single.advance.disabled, true);
    single.keyboard.fire('keydown', { key: 'ArrowRight' });
    assert.equal(single.counter.textContent, 1);
    const legacy = viewer(3, false);
    legacy.triggers[0].fire('click');
    legacy.next.fire('click');
    assert.equal(legacy.counter.textContent, 2);
    legacy.previous.fire('click');
    assert.equal(legacy.counter.textContent, 1);
});

test('focus stays inside the dialog and returns to the trigger on Escape', () => {
    const v = viewer();
    v.triggers[0].focus();
    v.triggers[0].fire('click');
    v.keyboard.fire('keydown', { key: 'Tab', shiftKey: true });
    assert.equal(v.document.activeElement, v.advance);
    v.keyboard.fire('keydown', { key: 'Tab' });
    assert.equal(v.document.activeElement, v.close);
    v.keyboard.fire('keydown', { key: 'Escape' });
    assert.equal(v.lightbox.hidden, true);
    assert.equal(v.document.activeElement, v.triggers[0]);
    assert.equal(v.image.src, undefined);
});

test('photo effect respects reduced motion and does not run after closing', () => {
    for (const reduced of [false, true]) {
        const v = viewer(3, true, reduced);
        v.triggers[0].fire('click');
        v.image.fire('load');
        assert.equal(v.image.animations, reduced ? 0 : 1);
        v.close.fire('click');
        v.image.fire('load');
        assert.equal(v.image.animations, reduced ? 0 : 1);
    }
});
