(() => {
    const password = document.getElementById('Password');
    const warning = document.getElementById('caps-lock-warning');
    if (!password || !warning) return;
    const update = event => { warning.hidden = !event.getModifierState?.('CapsLock'); };
    password.addEventListener('keydown', update);
    password.addEventListener('keyup', update);
    password.addEventListener('blur', () => { warning.hidden = true; });
})();
