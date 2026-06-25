import {
    animate,
    stagger,
    inView,
    hover,
    press
} from "https://cdn.jsdelivr.net/npm/motion@latest/+esm";

const reduceMotion = window.matchMedia("(prefers-reduced-motion: reduce)").matches;
const softEase = [0.22, 1, 0.36, 1];
const quickEase = [0.16, 1, 0.3, 1];
const enterFromTop = { opacity: [0, 1], y: [-28, 0], filter: ["blur(6px)", "blur(0px)"] };
const enterFromTopCompact = { opacity: [0, 1], y: [-18, 0], filter: ["blur(4px)", "blur(0px)"] };

if (!reduceMotion) {
    document.documentElement.classList.add("motion-ready");
    document.body.classList.add("motion-ready");

    animatePage();
    animateSidebar();
    animateOnView();
    wireInteractions();
    wireRouteTransitions();
    wireBootstrapModals();
    wireNotificationMotion();
}

function animatePage() {
    const pageChildren = Array.from(document.querySelectorAll(".page-content > *"))
        .filter(el => !el.classList.contains("modal"));

    animate(
        ".topbar",
        { opacity: [0, 1], y: [-24, 0], filter: ["blur(5px)", "blur(0px)"] },
        { duration: .62, ease: softEase }
    );

    if (pageChildren.length) {
        animate(
            pageChildren,
            enterFromTop,
            { duration: .72, delay: stagger(.075, { startDelay: .08 }), ease: softEase }
        );
    }
}

function animateSidebar() {
    const sidebar = document.querySelector("#sidebar.app-sidebar");
    if (!sidebar) return;

    animate(
        sidebar,
        { opacity: [0, 1], y: [-18, 0], filter: ["blur(5px)", "blur(0px)"] },
        { duration: .58, ease: softEase }
    );

    animate(
        "#sidebar .sidebar-brand, #sidebar .sidebar-section, #sidebar .sidebar-user",
        enterFromTopCompact,
        { duration: .58, delay: stagger(.07, { startDelay: .12 }), ease: softEase }
    );
}

function animateOnView() {
    const selectors = [
        ".audit-metric",
        ".audit-panel",
        ".dashboard-card",
        ".stat-card",
        ".chart-card",
        ".content-card",
        ".employee-card",
        ".card",
        ".table-dark-theme",
        ".audit-table-panel"
    ].join(",");

    document.querySelectorAll(selectors).forEach((element, index) => {
        element.classList.add("ui-lift");

        inView(
            element,
            () => {
                animate(
                    element,
                    { opacity: [0, 1], y: [-22, 0], scale: [.99, 1], filter: ["blur(5px)", "blur(0px)"] },
                    { duration: .62, delay: Math.min(index % 4, 3) * .035, ease: softEase }
                );
            },
            { margin: "0px 0px -10% 0px", amount: .18 }
        );
    });

    document.querySelectorAll("table tbody tr").forEach((row, index) => {
        inView(
            row,
            () => {
                animate(
                    row,
                    { opacity: [0, 1], y: [-10, 0] },
                    { duration: .42, delay: Math.min(index, 8) * .025, ease: quickEase }
                );
            },
            { margin: "0px 0px -6% 0px", amount: .1 }
        );
    });
}

function wireInteractions() {
    document.querySelectorAll(".btn, button:not(.btn-close), .nav-link").forEach((element) => {
        if (element.closest("#sidebar")) return;

        hover(element, (target) => {
            animate(target, { y: -1 }, { duration: .18, ease: "easeOut" });
            return () => animate(target, { y: 0 }, { duration: .2, ease: "easeOut" });
        });

        press(element, (target) => {
            animate(target, { y: 0, scale: .985 }, { duration: .08, ease: "easeOut" });
            return () => animate(target, { scale: 1 }, { duration: .16, ease: quickEase });
        });
    });
}

function wireRouteTransitions() {
    const mask = document.createElement("div");
    mask.className = "motion-route-mask";
    document.body.appendChild(mask);

    document.addEventListener("click", (event) => {
        const link = event.target.closest("a[href]");
        if (!link) return;
        if (link.closest("#sidebar")) return;

        const href = link.getAttribute("href");
        const target = link.getAttribute("target");

        if (!href ||
            href.startsWith("#") ||
            href.startsWith("javascript:") ||
            target === "_blank" ||
            link.hasAttribute("download")) {
            return;
        }

        let url;
        try {
            url = new URL(href, window.location.href);
        } catch {
            return;
        }

        if (url.origin !== window.location.origin)
            return;

        event.preventDefault();

        animate(mask, { opacity: [0, 1] }, { duration: .18, ease: "easeOut" });
        const exitAnimation = animate(".page-content", { opacity: [1, 0], y: [0, 4], filter: ["blur(0px)", "blur(2px)"] }, {
            duration: .16,
            ease: "easeOut"
        });

        exitAnimation.then(() => {
            window.location.href = url.href;
        });
    });
}

function wireBootstrapModals() {
    document.addEventListener("shown.bs.modal", (event) => {
        const modal = event.target;
        const dialog = modal.querySelector(".modal-dialog");
        if (!dialog) return;

        animate(
            dialog,
            { opacity: [0, 1], y: [-20, 0], scale: [.975, 1], filter: ["blur(5px)", "blur(0px)"] },
            { duration: .34, ease: softEase }
        );
    });
}

function wireNotificationMotion() {
    const bell = document.getElementById("notifBell");
    const dropdown = document.getElementById("notifDropdown");
    if (!bell || !dropdown) return;

    bell.addEventListener("click", () => {
        requestAnimationFrame(() => {
            if (!dropdown.classList.contains("show")) return;

            animate(
                dropdown,
                { opacity: [0, 1], y: [-14, 0], scale: [.985, 1], filter: ["blur(4px)", "blur(0px)"] },
                { duration: .28, ease: softEase }
            );
        });
    });

    const list = document.getElementById("notifList");
    if (!list) return;

    const observer = new MutationObserver((mutations) => {
        mutations.forEach((mutation) => {
            mutation.addedNodes.forEach((node) => {
                if (!(node instanceof HTMLElement)) return;

                animate(
                    node,
                    { opacity: [0, 1], y: [-10, 0], scale: [.99, 1] },
                    { duration: .3, ease: softEase }
                );
            });
        });
    });

    observer.observe(list, { childList: true });
}