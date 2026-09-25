(() => {
    const widget = document.querySelector("[data-availability]");
    const status = document.querySelector("[data-availability-status]");
    if (!widget || !status || !window.fetch || !window.AbortController) return;

    let loading = false;
    const setBusy = busy => {
        loading = busy;
        widget.setAttribute("aria-busy", String(busy));
        widget.querySelectorAll("a[data-calendar-nav]").forEach(link => {
            if (busy) link.setAttribute("aria-disabled", "true");
            else link.removeAttribute("aria-disabled");
        });
    };

    widget.addEventListener("click", async event => {
        const link = event.target.closest("a[data-calendar-nav]");
        if (!link || !widget.contains(link) || event.defaultPrevented || event.button !== 0
            || event.ctrlKey || event.metaKey || event.shiftKey || event.altKey) return;

        event.preventDefault();
        if (loading) return;
        setBusy(true);
        status.textContent = widget.dataset.loading;
        const request = new AbortController();
        const timeout = setTimeout(() => request.abort(), 15000);

        try {
            const response = await fetch(link.href, {
                headers: { "X-Requested-With": "XMLHttpRequest" },
                credentials: "same-origin",
                cache: "no-store",
                signal: request.signal
            });
            if (!response.ok || response.redirected) throw new Error("Calendar request failed.");
            const html = new DOMParser().parseFromString(await response.text(), "text/html");
            const replacement = html.querySelector("[data-calendar-fragment]");
            const monthCaption = replacement?.querySelector("caption");
            if (!monthCaption) throw new Error("Calendar fragment missing.");

            const previous = widget.querySelector("[data-calendar-fragment]");
            const restoreFocus = previous.contains(document.activeElement);
            previous.replaceWith(replacement);
            if (restoreFocus) {
                const matchingControl = widget.querySelector(`[data-calendar-nav="${link.dataset.calendarNav}"]`);
                const focusTarget = matchingControl && !matchingControl.disabled
                    ? matchingControl : widget.querySelector('[data-calendar-nav="today"]');
                focusTarget.focus({ preventScroll: true });
            }
            // Announce only the first month, which is also the visible month on phones.
            status.textContent = monthCaption.textContent;
        } catch {
            // Keep the existing month and controls intact so the visitor can retry.
            status.textContent = widget.dataset.error;
        } finally {
            clearTimeout(timeout);
            setBusy(false);
        }
    });
})();
