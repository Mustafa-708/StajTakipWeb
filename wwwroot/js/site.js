// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

(() => {
    const panel = document.querySelector('.sidebar-disclosure');
    if (!panel) return;
    const desktop = window.matchMedia('(min-width: 1200px)');
    const adjust = () => { panel.open = desktop.matches; };
    desktop.addEventListener('change', adjust);
    adjust();
})();
