(() => {
    const all = document.getElementById('TumOgrenciler');
    const list = document.getElementById('ogrenci-secimi');
    const summary = document.getElementById('secim-ozeti');
    if (!all || !list || !summary) return;
    const students = [...list.querySelectorAll('input[type="checkbox"]')];
    const update = () => {
        students.forEach(input => { input.disabled = all.checked; });
        list.classList.toggle('assignment-all-active', all.checked);
        const count = all.checked ? students.length : students.filter(input => input.checked).length;
        summary.textContent = all.checked ? `Tüm öğrenciler seçildi (${count} öğrenci).` : `${count} öğrenci seçildi.`;
    };
    all.addEventListener('change', update);
    students.forEach(input => input.addEventListener('change', update));
    update();
})();
