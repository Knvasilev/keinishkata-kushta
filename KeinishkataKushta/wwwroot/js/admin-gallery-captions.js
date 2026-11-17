(() => {
    'use strict';
    const editor = document.querySelector('[data-caption-editor]');
    if (!editor) return;
    const rows = Array.from(editor.querySelectorAll('[data-caption-row]'));
    const status = editor.querySelector('[data-caption-status]');
    const replace = editor.querySelector('[data-caption-replace]');
    const fields = Array.from(editor.querySelectorAll('[data-caption-field]'));
    const initialValues = fields.map(field => field.value);
    let saving = false;
    editor.querySelector('#caption-batch').addEventListener('submit', () => { saving = true; });
    window.addEventListener('beforeunload', event => {
        if (!saving && fields.some((field, index) => field.value !== initialValues[index])) {
            event.preventDefault();
            event.returnValue = '';
        }
    });
    editor.querySelector('[data-caption-tools]').hidden = false;
    editor.querySelectorAll('[data-caption-selection]').forEach(item => { item.hidden = false; });
    const announceSelection = () => {
        const count = rows.filter(row => row.querySelector('[data-caption-select]').checked).length;
        status.textContent = `${count} photo(s) selected. Caption changes still need Save all changes.`;
    };
    editor.querySelector('[data-caption-select-all]').addEventListener('click', () => {
        rows.forEach(row => { row.querySelector('[data-caption-select]').checked = true; });
        announceSelection();
    });
    editor.querySelector('[data-caption-select-none]').addEventListener('click', () => {
        rows.forEach(row => { row.querySelector('[data-caption-select]').checked = false; });
        announceSelection();
    });
    rows.forEach(row => {
        row.querySelector('[data-caption-select]').addEventListener('change', announceSelection);
        row.querySelectorAll('[data-caption-field]').forEach(field => field.addEventListener('input', () => {
            status.textContent = 'Unsaved caption changes. Choose Save all changes when ready.';
        }));
    });
    editor.querySelector('[data-caption-language]').addEventListener('change', () => { replace.checked = false; });
    editor.querySelector('[data-caption-apply]').addEventListener('click', () => {
        const selected = rows.filter(row => row.querySelector('[data-caption-select]').checked);
        const value = editor.querySelector('[data-caption-value]').value.trim();
        const language = editor.querySelector('[data-caption-language]').value;
        if (!selected.length || !value || value.length > 500) {
            status.textContent = 'Select at least one photo and enter a caption of 1–500 characters.';
            return;
        }
        let applied = 0;
        selected.forEach(row => {
            const field = row.querySelector(`[data-caption-field="${language}"]`);
            if (field.value.trim() && !replace.checked) return;
            if (field.value !== value) { field.value = value; applied++; }
        });
        replace.checked = false;
        status.textContent = `Applied to ${applied} photo(s). Other values kept. Review and choose Save all changes.`;
    });
})();
