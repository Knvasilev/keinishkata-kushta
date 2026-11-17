(() => {
    const lightbox = document.querySelector("[data-gallery-lightbox]");
    const collection = document.querySelector("[data-gallery-collection]");
    const triggers = Array.from(collection?.querySelectorAll("[data-gallery-trigger]") || []);
    if (!lightbox || triggers.length === 0) return;

    const image = lightbox.querySelector("[data-gallery-image]");
    const caption = lightbox.querySelector("[data-gallery-caption]");
    const counter = lightbox.querySelector("[data-gallery-counter]");
    const closeButton = lightbox.querySelector("[data-gallery-close]");
    const previousButton = lightbox.querySelector("[data-gallery-previous]");
    const nextButton = lightbox.querySelector("[data-gallery-next]");
    const swipeSurface = lightbox.querySelector("[data-gallery-swipe]");
    const items = triggers.map(trigger => ({
        src: trigger.dataset.gallerySrc,
        caption: trigger.dataset.galleryCaption || ""
    }));
    let currentIndex = 0;
    let lastFocusedElement = null;
    let pointerStart = null;

    const render = () => {
        const item = items[currentIndex];
        image.src = item.src;
        image.alt = item.caption;
        caption.textContent = item.caption;
        caption.hidden = !item.caption;
        caption.scrollTop = 0;
        counter.textContent = currentIndex + 1;
    };
    const move = direction => {
        if (!lightbox.open || items.length < 2) return;
        currentIndex = (currentIndex + direction + items.length) % items.length;
        render();
    };
    const open = index => {
        currentIndex = index;
        pointerStart = null;
        lastFocusedElement = document.activeElement;
        render();
        lightbox.showModal();
        document.body.classList.add("gallery-open");
        closeButton.focus();
    };
    const close = () => lightbox.close();

    // Native modality makes the rest of the document inert while open.
    lightbox.addEventListener("close", () => {
        image.removeAttribute("src");
        pointerStart = null;
        document.body.classList.remove("gallery-open");
        if (lastFocusedElement?.isConnected) lastFocusedElement.focus();
    });
    lightbox.addEventListener("cancel", event => {
        event.preventDefault();
        close();
    });
    triggers.forEach((trigger, index) => trigger.addEventListener("click", () => open(index)));
    // Collection openers never become duplicate photo items.
    collection.querySelectorAll("[data-gallery-open]").forEach(button => {
        button.addEventListener("click", () => open(0));
    });
    closeButton.addEventListener("click", close);
    lightbox.addEventListener("click", event => {
        if (event.target === lightbox) close();
    });
    previousButton.addEventListener("click", () => move(-1));
    nextButton.addEventListener("click", () => move(1));
    previousButton.hidden = nextButton.hidden = items.length === 1;

    // The image is deliberately not a button and has no click-to-advance handler.
    swipeSurface.addEventListener("pointerdown", event => {
        if (!event.isPrimary) {
            pointerStart = null;
            return;
        }
        if (!lightbox.open || event.button !== 0 || items.length < 2) return;
        pointerStart = { x: event.clientX, y: event.clientY, id: event.pointerId };
        swipeSurface.setPointerCapture(event.pointerId);
    });
    swipeSurface.addEventListener("pointerup", event => {
        if (!pointerStart || pointerStart.id !== event.pointerId) return;
        const distance = event.clientX - pointerStart.x;
        const verticalDistance = event.clientY - pointerStart.y;
        pointerStart = null;
        if (Math.abs(distance) > 50 && Math.abs(distance) > Math.abs(verticalDistance) * 1.25) {
            move(distance > 0 ? -1 : 1);
        }
    });
    ["pointercancel", "lostpointercapture"].forEach(name => {
        swipeSurface.addEventListener(name, () => { pointerStart = null; });
    });
    lightbox.addEventListener("keydown", event => {
        if (!lightbox.open) return;
        if (event.key === "Escape") {
            event.preventDefault();
            close();
        } else if (event.key === "ArrowLeft" || event.key === "ArrowRight") {
            event.preventDefault();
            move(event.key === "ArrowLeft" ? -1 : 1);
        } else if (event.key === "Tab") {
            const controls = Array.from(lightbox.querySelectorAll("button, [tabindex='0']"))
                .filter(control => !control.hidden && !control.disabled);
            const first = controls[0];
            const last = controls[controls.length - 1];
            if (event.shiftKey && document.activeElement === first) {
                event.preventDefault();
                last.focus();
            } else if (!event.shiftKey && document.activeElement === last) {
                event.preventDefault();
                first.focus();
            }
        }
    });
})();
