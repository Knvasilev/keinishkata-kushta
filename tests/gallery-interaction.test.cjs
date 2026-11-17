const { test } = require('node:test');
const assert = require('node:assert/strict');
const { readFileSync } = require('node:fs');
const { join } = require('node:path');
const { runInNewContext } = require('node:vm');

const script = readFileSync(join(__dirname, '../KeinishkataKushta/wwwroot/js/site.js'), 'utf8');

function viewer(count = 3, withOpener = true, descriptions = []) {
    const document = { body: { classList: { add() {}, remove() {} } } };
    function element() {
        const listeners = {};
        return {
            hidden: false, isConnected: true, dataset: {}, animations: 0,
            addEventListener(name, fn) { listeners[name] = fn; },
            fire(name, data = {}) { listeners[name]?.({ preventDefault() {}, ...data }); },
            focus() { document.activeElement = this; },
            removeAttribute(name) { delete this[name]; },
            setPointerCapture() {},
            animate() { this.animations++; }
        };
    }
    const lightbox = element();
    lightbox.open = false;
    lightbox.showModal = () => { lightbox.open = true; };
    lightbox.close = () => { lightbox.open = false; lightbox.fire('close'); };
    const nodes = Object.fromEntries(['image', 'caption', 'counter', 'close', 'previous', 'next', 'swipe'].map(key => [key, element()]));
    lightbox.querySelector = selector => nodes[selector.slice(14, -1)];
    lightbox.querySelectorAll = () => [nodes.close, nodes.previous, nodes.caption, nodes.next];
    const triggers = Array.from({ length: count }, (_, index) => {
        const trigger = element();
        trigger.dataset = { gallerySrc: `room/photo-${index}.jpg`, galleryCaption: descriptions[index] ?? `Photo ${index}` };
        return trigger;
    });
    const opener = element();
    const unrelated = element();
    unrelated.dataset = { gallerySrc: 'property/unrelated.jpg', galleryCaption: 'Other collection' };
    const collection = { querySelectorAll: selector => selector === '[data-gallery-open]' ? (withOpener ? [opener] : []) : triggers };
    document.querySelector = selector => selector === '[data-gallery-lightbox]' ? lightbox : collection;
    // A document-wide query would accidentally pull another collection into the dialog.
    document.querySelectorAll = () => [...triggers, unrelated];
    runInNewContext(script, { document });
    const swipe = (x, y = 0, end = 'pointerup') => {
        nodes.swipe.fire('pointerdown', { isPrimary: true, button: 0, pointerId: 1, clientX: 200, clientY: 200 });
        nodes.swipe.fire(end, { pointerId: 1, clientX: 200 + x, clientY: 200 + y });
    };
    return { ...nodes, surface: nodes.swipe, lightbox, triggers, opener, unrelated, document, swipe };
}

for (const withOpener of [true, false]) {
    test(`${withOpener ? 'room' : 'property'} navigation wraps first/last and keeps collection isolated`, () => {
        const v = viewer(9, withOpener);
        v.triggers.slice(5).forEach(trigger => { trigger.hidden = true; });
        if (withOpener) v.opener.fire('click');
        else v.triggers[0].fire('click');
        v.previous.fire('click');
        assert.equal(v.image.src, 'room/photo-8.jpg');
        assert.equal(v.counter.textContent, 9);
        v.next.fire('click');
        assert.equal(v.counter.textContent, 1);
        v.triggers[3].fire('click');
        assert.equal(v.image.src, 'room/photo-3.jpg');
        v.unrelated.fire('click');
        assert.equal(v.image.src, 'room/photo-3.jpg');
    });
}

test('image taps, clicks and clicks following a swipe never advance', () => {
    const v = viewer();
    v.opener.fire('click');
    v.image.fire('click', { detail: 1 });
    v.swipe(0);
    v.surface.fire('click');
    assert.equal(v.counter.textContent, 1);
    v.swipe(-100);
    v.image.fire('click', { detail: 1 });
    assert.equal(v.counter.textContent, 2);
    v.swipe(100);
    assert.equal(v.counter.textContent, 1);
});

test('vertical, short, cancelled and lost-capture gestures do not advance', () => {
    const v = viewer();
    v.opener.fire('click');
    v.swipe(70, 130);
    v.swipe(49);
    v.swipe(-100, 0, 'pointercancel');
    v.surface.fire('pointerup', { pointerId: 1, clientX: 0, clientY: 200 });
    v.swipe(-100, 0, 'lostpointercapture');
    v.surface.fire('pointerup', { pointerId: 1, clientX: 0, clientY: 200 });
    assert.equal(v.counter.textContent, 1);
});

test('multi-touch and unmatched pointers cannot complete a swipe', () => {
    const v = viewer();
    v.opener.fire('click');
    const start = { isPrimary: true, button: 0, pointerId: 1, clientX: 200, clientY: 200 };
    v.surface.fire('pointerdown', start);
    v.surface.fire('pointerup', { pointerId: 2, clientX: 0, clientY: 200 });
    assert.equal(v.counter.textContent, 1);
    v.surface.fire('pointerdown', { ...start, isPrimary: false, pointerId: 2 });
    v.surface.fire('pointerup', { pointerId: 1, clientX: 0, clientY: 200 });
    assert.equal(v.counter.textContent, 1);
});

test('single photo opens, hides arrows and ignores swipe/arrow keys', () => {
    const v = viewer(1);
    v.opener.fire('click');
    assert.equal(v.lightbox.open, true);
    assert.equal(v.previous.hidden, true);
    assert.equal(v.next.hidden, true);
    v.lightbox.fire('keydown', { key: 'ArrowRight' });
    v.swipe(-100);
    assert.equal(v.counter.textContent, 1);
});

test('clicks in the image do not close; clicking the surrounding dialog closes', () => {
    const v = viewer();
    v.opener.fire('click');
    v.lightbox.fire('click', { target: v.image });
    assert.equal(v.lightbox.open, true);
    v.lightbox.fire('click', { target: v.lightbox });
    assert.equal(v.lightbox.open, false);
});

test('zero photos and pages without a gallery are safe no-ops', () => {
    const v = viewer(0);
    v.opener.fire('click');
    assert.equal(v.lightbox.open, false);
    runInNewContext(script, { document: { querySelector: () => null } });
});

test('arrow keys navigate, Escape closes and restores opener focus', () => {
    const v = viewer();
    v.opener.focus();
    v.opener.fire('click');
    assert.equal(v.document.activeElement, v.close);
    v.lightbox.fire('keydown', { key: 'ArrowLeft' });
    assert.equal(v.counter.textContent, 3);
    v.lightbox.fire('keydown', { key: 'ArrowRight' });
    assert.equal(v.counter.textContent, 1);
    v.lightbox.fire('keydown', { key: 'Escape' });
    assert.equal(v.lightbox.open, false);
    assert.equal(v.document.activeElement, v.opener);
    assert.equal(v.image.src, undefined);
    v.lightbox.fire('keydown', { key: 'ArrowRight' });
    assert.equal(v.counter.textContent, 1);
});

test('Tab traps focus with caption, without caption and with one photo', () => {
    for (const count of [1, 3]) {
        for (const description of ['', 'A description']) {
            const v = viewer(count, true, [description]);
            v.opener.fire('click');
            v.lightbox.fire('keydown', { key: 'Tab', shiftKey: true });
            const last = count > 1 ? v.next : description ? v.caption : v.close;
            assert.equal(v.document.activeElement, last);
            v.lightbox.fire('keydown', { key: 'Tab' });
            assert.equal(v.document.activeElement, v.close);
        }
    }
});

test('native cancellation and close button clean up and restore thumbnail focus', () => {
    for (const nativeCancel of [true, false]) {
        const v = viewer();
        v.triggers[2].focus();
        v.triggers[2].fire('click');
        if (nativeCancel) v.lightbox.fire('cancel');
        else v.close.fire('click');
        assert.equal(v.document.activeElement, v.triggers[2]);
        assert.equal(v.lightbox.open, false);
        assert.equal(v.image.src, undefined);
    }
});

test('BG/EN captions remain literal alt text; optional captions hide without losing count', () => {
    const v = viewer(3, true, ['Стая с гледка', 'Room <with> a view', '']);
    v.opener.fire('click');
    assert.equal(v.image.alt, 'Стая с гледка');
    v.next.fire('click');
    assert.equal(v.image.alt, 'Room <with> a view');
    assert.equal(v.caption.textContent, 'Room <with> a view');
    v.next.fire('click');
    assert.equal(v.caption.hidden, true);
    assert.equal(v.counter.textContent, 3);
});

test('no image animation or stale swipe state survives closing and reopening', () => {
    const v = viewer();
    v.opener.fire('click');
    v.image.fire('load');
    assert.equal(v.image.animations, 0);
    v.surface.fire('pointerdown', { isPrimary: true, button: 0, pointerId: 1, clientX: 200, clientY: 200 });
    v.close.fire('click');
    v.opener.fire('click');
    v.surface.fire('pointerup', { pointerId: 1, clientX: 0, clientY: 200 });
    assert.equal(v.counter.textContent, 1);
});
