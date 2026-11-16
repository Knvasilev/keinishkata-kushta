(() => {
    const toggle = document.querySelector('[data-site-menu-toggle]');
    const menu = document.getElementById('siteNavbar');
    if (!toggle || !menu || !window.bootstrap?.Collapse) return;

    const collapse = window.bootstrap.Collapse.getOrCreateInstance(menu, { toggle: false });
    const desktop = window.matchMedia('(min-width: 1200px)');
    const label = expanded => toggle.setAttribute('aria-label',
        expanded ? toggle.dataset.labelClose : toggle.dataset.labelOpen);

    // Bootstrap owns aria-expanded, transitions and native button activation.
    menu.addEventListener('show.bs.collapse', () => label(true));
    menu.addEventListener('hide.bs.collapse', () => label(false));
    menu.addEventListener('hidden.bs.collapse', () => {
        // Also recover focus if a close was requested while a link had focus.
        if (!desktop.matches && menu.contains(document.activeElement)) toggle.focus();
    });

    const header = toggle.closest('.site-header');
    header.addEventListener('keydown', event => {
        if (event.key !== 'Escape' || desktop.matches || !menu.classList.contains('show')) return;
        event.preventDefault();
        collapse.hide();
        toggle.focus();
    });

    // Collapse is CSS-visible on desktop. Keep focus on a visible control when
    // crossing back to mobile with a closed menu, or up with the toggle focused.
    desktop.addEventListener('change', () => {
        if (desktop.matches && document.activeElement === toggle) {
            menu.querySelector('a')?.focus();
        } else if (!desktop.matches && !menu.classList.contains('show') && menu.contains(document.activeElement)) {
            toggle.focus();
        }
    });
    label(toggle.getAttribute('aria-expanded') === 'true');
})();
