(() => {
    const lightbox = document.querySelector("[data-gallery-lightbox]");
    const triggers = Array.from(document.querySelectorAll("[data-gallery-trigger]"));

    if (!lightbox || triggers.length === 0) {
        return;
    }

    const image = lightbox.querySelector("[data-gallery-image]");
    const caption = lightbox.querySelector("[data-gallery-caption]");
    const counter = lightbox.querySelector("[data-gallery-counter]");
    const closeButton = lightbox.querySelector("[data-gallery-close]");
    const previousButton = lightbox.querySelector("[data-gallery-previous]");
    const nextButton = lightbox.querySelector("[data-gallery-next]");
    const advanceButton = lightbox.querySelector("[data-gallery-advance]");
    const swipeSurface = advanceButton || image;
    const reducedMotion = window.matchMedia("(prefers-reduced-motion: reduce)");
    const items = triggers.map(trigger => ({
        src: trigger.dataset.gallerySrc,
        caption: trigger.dataset.galleryCaption || ""
    }));

    let currentIndex = 0;
    let lastFocusedElement = null;
    let pointerStart = null;
    let suppressClick = false;
    let imageAnimation = null;

    image.addEventListener("load", () => {
        if (lightbox.hidden || !advanceButton || reducedMotion.matches) {
            return;
        }

        imageAnimation?.cancel();
        imageAnimation = image.animate([
            { opacity: .35, transform: "scale(.985)" },
            { opacity: 1, transform: "scale(1)" }
        ], { duration: 220, easing: "ease-out" });
    });
    image.draggable = false;

    const render = () => {
        const item = items[currentIndex];
        image.src = item.src;
        image.alt = item.caption;
        caption.textContent = item.caption;
        counter.textContent = currentIndex + 1;
    };

    const move = direction => {
        if (items.length < 2) {
            return;
        }
        currentIndex = (currentIndex + direction + items.length) % items.length;
        render();
    };

    const open = index => {
        currentIndex = index;
        lastFocusedElement = document.activeElement;
        render();
        lightbox.hidden = false;
        document.body.classList.add("gallery-open");
        closeButton.focus();
    };

    const close = () => {
        lightbox.hidden = true;
        image.removeAttribute("src");
        imageAnimation?.cancel();
        pointerStart = null;
        suppressClick = false;
        document.body.classList.remove("gallery-open");
        lastFocusedElement?.focus();
    };

    triggers.forEach((trigger, index) => {
        trigger.addEventListener("click", () => open(index));
    });

    closeButton.addEventListener("click", close);
    previousButton?.addEventListener("click", () => move(-1));
    nextButton?.addEventListener("click", () => move(1));
    advanceButton?.addEventListener("click", event => {
        // A swipe can also produce a click; consume it instead of skipping a photo.
        if (suppressClick && event.detail !== 0) {
            suppressClick = false;
            return;
        }
        move(1);
    });

    if (items.length === 1) {
        if (previousButton) previousButton.hidden = true;
        if (nextButton) nextButton.hidden = true;
        if (advanceButton) {
            advanceButton.disabled = true;
            advanceButton.removeAttribute("aria-label");
        }
    }

    lightbox.addEventListener("click", event => {
        if (event.target === lightbox) {
            close();
        }
    });

    swipeSurface.addEventListener("pointerdown", event => {
        if (!event.isPrimary || event.button !== 0 || items.length < 2) {
            return;
        }
        suppressClick = false;
        pointerStart = { x: event.clientX, y: event.clientY, id: event.pointerId };
        swipeSurface.setPointerCapture(event.pointerId);
    });

    swipeSurface.addEventListener("pointerup", event => {
        if (!pointerStart || pointerStart.id !== event.pointerId) {
            return;
        }

        const distance = event.clientX - pointerStart.x;
        const verticalDistance = event.clientY - pointerStart.y;
        pointerStart = null;
        suppressClick = Math.hypot(distance, verticalDistance) > 12;

        if (Math.abs(distance) > 50 && Math.abs(distance) > Math.abs(verticalDistance) * 1.25) {
            move(distance > 0 ? -1 : 1);
        }
    });

    swipeSurface.addEventListener("pointercancel", () => {
        pointerStart = null;
        suppressClick = true;
    });

    swipeSurface.addEventListener("lostpointercapture", () => {
        pointerStart = null;
    });

    document.addEventListener("keydown", event => {
        if (lightbox.hidden) {
            return;
        }

        if (event.key === "Escape") {
            event.preventDefault();
            close();
        } else if (event.key === "ArrowLeft" && items.length > 1) {
            event.preventDefault();
            move(-1);
        } else if (event.key === "ArrowRight" && items.length > 1) {
            event.preventDefault();
            move(1);
        } else if (event.key === "Tab") {
            const buttons = Array.from(lightbox.querySelectorAll("button:not([disabled])"))
                .filter(button => !button.hidden);
            const first = buttons[0];
            const last = buttons[buttons.length - 1];
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
