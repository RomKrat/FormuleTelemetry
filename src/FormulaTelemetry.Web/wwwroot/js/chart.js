window.ftChart = (function () {
    function svgBounds(el) {
        if (!el) return null;
        const r = el.getBoundingClientRect();
        if (r.width < 1 || r.height < 1) return null;
        return { left: r.left, top: r.top, width: r.width, height: r.height };
    }

    return { svgBounds };
})();
