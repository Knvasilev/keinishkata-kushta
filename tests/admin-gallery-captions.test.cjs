const { test } = require('node:test');
const assert = require('node:assert/strict');
const { readFileSync } = require('node:fs');
const { join } = require('node:path');
const { runInNewContext } = require('node:vm');
const source = readFileSync(join(__dirname, '../KeinishkataKushta/wwwroot/js/admin-gallery-captions.js'), 'utf8');

function setup() {
    function element(value = '') {
        return { value, checked: false, hidden: true, textContent: '', handlers: {},
            addEventListener(name, handler) { this.handlers[name] = handler; },
            fire(name = 'click', event = {}) { this.handlers[name]?.(event); } };
    }
    const rows = [['Двор', ''], ['Механа', 'Tavern'], ['', '']].map(values => {
        const select = element();
        const fields = { Caption: element(values[0]), CaptionEn: element(values[1]) };
        return { select, fields,
            querySelector(selector) { return selector === '[data-caption-select]' ? select : fields[selector.match(/="(.+)"/)[1]]; },
            querySelectorAll() { return Object.values(fields); } };
    });
    const selectors = ['status', 'replace', 'tools', 'language', 'value', 'select-all', 'select-none', 'apply'];
    const controls = Object.fromEntries(selectors.map(key => [`[data-caption-${key}]`, element()]));
    controls['#caption-batch'] = element();
    const window = element();
    const editor = {
        querySelector(selector) { return controls[selector]; },
        querySelectorAll(selector) {
            return selector === '[data-caption-row]' ? rows : selector === '[data-caption-field]'
                ? rows.flatMap(row => Object.values(row.fields)) : rows.map(() => element());
        }
    };
    runInNewContext(source, { document: { querySelector: () => editor }, window });
    return { rows, window, form: controls['#caption-batch'], control: name => controls[`[data-caption-${name}]`] };
}

test('Applies English to selected empty fields and preserves existing translations and Bulgarian', () => {
    const ui = setup();
    ui.control('language').value = 'CaptionEn'; ui.control('value').value = 'Courtyard';
    ui.rows[0].select.checked = true; ui.rows[1].select.checked = true;
    ui.control('apply').fire();
    assert.equal(ui.rows[0].fields.CaptionEn.value, 'Courtyard');
    assert.equal(ui.rows[1].fields.CaptionEn.value, 'Tavern');
    assert.equal(ui.rows[2].fields.CaptionEn.value, '');
    assert.equal(ui.rows[0].fields.Caption.value, 'Двор');
});
test('Explicit replacement affects one language and resets after each application and language switch', () => {
    const ui = setup();
    ui.control('select-all').fire();
    ui.control('language').value = 'Caption'; ui.control('value').value = 'Къща';
    ui.control('replace').checked = true; ui.control('apply').fire();
    assert.ok(ui.rows.every(row => row.fields.Caption.value === 'Къща'));
    assert.equal(ui.rows[1].fields.CaptionEn.value, 'Tavern');
    assert.equal(ui.control('replace').checked, false);
    ui.control('replace').checked = true; ui.control('language').fire('change');
    assert.equal(ui.control('replace').checked, false);
    ui.control('select-none').fire();
    assert.ok(ui.rows.every(row => !row.select.checked));
});
test('Blank, oversized and unselected applications leave values intact', () => {
    const ui = setup();
    ui.control('language').value = 'CaptionEn';
    ui.control('value').value = 'No selection'; ui.control('apply').fire();
    ui.control('select-all').fire();
    for (const value of ['   ', 'x'.repeat(501)]) {
        ui.control('value').value = value; ui.control('apply').fire();
        assert.equal(ui.rows[0].fields.CaptionEn.value, '');
        assert.equal(ui.rows[1].fields.CaptionEn.value, 'Tavern');
    }
});
test('Applied fields remain independently editable and unsaved navigation warns until saving', () => {
    const ui = setup();
    const event = { prevented: false, preventDefault() { this.prevented = true; } };
    ui.window.fire('beforeunload', event); assert.equal(event.prevented, false);
    ui.control('select-all').fire(); ui.control('language').value = 'CaptionEn';
    ui.control('value').value = 'Shared'; ui.control('apply').fire();
    ui.rows[0].fields.CaptionEn.value = 'Individual';
    ui.rows[0].fields.CaptionEn.fire('input');
    assert.equal(ui.rows[2].fields.CaptionEn.value, 'Shared');
    assert.match(ui.control('status').textContent, /Unsaved/);
    ui.window.fire('beforeunload', event); assert.equal(event.prevented, true);
    event.prevented = false; ui.form.fire('submit');
    ui.window.fire('beforeunload', event); assert.equal(event.prevented, false);
});
