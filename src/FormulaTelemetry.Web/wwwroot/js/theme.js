window.ftTheme = (function () {
    const key = "ft-theme";

    function apply(theme) {
        const t = theme === "light" ? "light" : "dark";
        document.documentElement.setAttribute("data-theme", t);
        try { localStorage.setItem(key, t); } catch { /* ignore */ }
        return t;
    }

    function applySaved() {
        let t = "dark";
        try {
            const stored = localStorage.getItem(key);
            if (stored === "light" || stored === "dark") t = stored;
        } catch { /* ignore */ }
        return apply(t);
    }

    function toggle() {
        const cur = document.documentElement.getAttribute("data-theme") === "light" ? "light" : "dark";
        return apply(cur === "dark" ? "light" : "dark");
    }

    applySaved();
    return { apply, applySaved, toggle };
})();
