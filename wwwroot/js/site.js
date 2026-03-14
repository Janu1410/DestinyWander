document.addEventListener("DOMContentLoaded", () => {
    const isAuthenticated = window.dwIsAuthenticated === true || window.dwIsAuthenticated === "true";

    const buildLoginUrl = () => {
        const returnUrl = encodeURIComponent(`${window.location.pathname}${window.location.search}${window.location.hash}`);
        return `/Account/Login?returnUrl=${returnUrl}`;
    };

    const requireAuth = (event, message) => {
        if (isAuthenticated) {
            return true;
        }

        if (event) {
            event.preventDefault();
            event.stopPropagation();
        }

        const promptMessage = message || "Please sign up or log in to continue with booking and payment.";
        alert(promptMessage);
        window.location.href = buildLoginUrl();
        return false;
    };

    window.dwGetLoginUrl = buildLoginUrl;
    window.dwRequireAuth = requireAuth;
    window.dwAuthPrompt = (message) => requireAuth(null, message);

    document.addEventListener("click", (event) => {
        const target = event.target.closest("[data-requires-auth='true']");
        if (!target) {
            return;
        }

        const customMessage = target.getAttribute("data-auth-message");
        requireAuth(event, customMessage);
    }, true);

    document.addEventListener("submit", (event) => {
        const form = event.target.closest("form[data-requires-auth='true']");
        if (!form) {
            return;
        }

        const customMessage = form.getAttribute("data-auth-message");
        requireAuth(event, customMessage);
    }, true);

    const menuButton = document.querySelector(".dw-menu-btn");
    const navLinks = document.getElementById("dw-main-nav");

    if (menuButton && navLinks) {
        menuButton.addEventListener("click", () => {
            const isOpen = navLinks.classList.toggle("open");
            menuButton.setAttribute("aria-expanded", isOpen ? "true" : "false");
        });

        document.addEventListener("click", (event) => {
            const clickedInsideNav = navLinks.contains(event.target) || menuButton.contains(event.target);
            if (!clickedInsideNav && navLinks.classList.contains("open")) {
                navLinks.classList.remove("open");
                menuButton.setAttribute("aria-expanded", "false");
            }
        });

        window.addEventListener("resize", () => {
            if (window.innerWidth > 992 && navLinks.classList.contains("open")) {
                navLinks.classList.remove("open");
                menuButton.setAttribute("aria-expanded", "false");
            }
        });
    }

    const profileMenu = document.querySelector(".profile-menu");
    if (profileMenu) {
        const profileToggle = profileMenu.querySelector(".avatar-circle");

        if (profileToggle) {
            profileToggle.addEventListener("click", (event) => {
                event.preventDefault();
                event.stopPropagation();
                profileMenu.classList.toggle("active");
            });

            document.addEventListener("click", (event) => {
                if (!profileMenu.contains(event.target)) {
                    profileMenu.classList.remove("active");
                }
            });
        }
    }

    const logoutForm = document.querySelector(".dw-logout-form");
    if (logoutForm) {
        logoutForm.addEventListener("submit", async (event) => {
            event.preventDefault();

            try {
                const formData = new FormData(logoutForm);
                await fetch(logoutForm.action, {
                    method: "POST",
                    body: formData,
                    credentials: "same-origin"
                });
            } catch (error) {
                console.error("Logout failed:", error);
            } finally {
                const redirectUrl = logoutForm.getAttribute("data-logout-redirect") || "/";
                window.location.href = redirectUrl;
            }
        });
    }

    const yearNode = document.getElementById("dw-year");
    if (yearNode) {
        yearNode.textContent = String(new Date().getFullYear());
    }
});
