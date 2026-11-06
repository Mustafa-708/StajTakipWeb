(() => {
    document.querySelectorAll('.password-toggle').forEach(toggle => {
        const password = document.getElementById(toggle.getAttribute('aria-controls'));
        if (!password) return;
        const name = password.id === 'ConfirmPassword' ? 'Şifre tekrarını' : 'Şifreyi';
        toggle.hidden = false;
        toggle.addEventListener('click', () => {
            const visible = password.type === 'password';
            password.type = visible ? 'text' : 'password';
            toggle.setAttribute('aria-pressed', String(visible));
            toggle.setAttribute('aria-label', `${name} ${visible ? 'gizle' : 'göster'}`);
        });
    });
})();
