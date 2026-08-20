function setupPasswordToggle(inputId, toggleBtnId, toggleIconId) {
    const input = document.getElementById(inputId);
    const toggleBtn = document.getElementById(toggleBtnId);
    const toggleIcon = document.getElementById(toggleIconId);

    if (!input || !toggleBtn || !toggleIcon) return;

    input.addEventListener('input', function () {
        toggleBtn.style.display = this.value.length > 0 ? 'block' : 'none';
    });

    toggleBtn.addEventListener('click', function () {
        if (input.type === 'password') {
            input.type = 'text';
            toggleIcon.classList.remove('bi-eye');
            toggleIcon.classList.add('bi-eye-slash');
        } else {
            input.type = 'password';
            toggleIcon.classList.remove('bi-eye-slash');
            toggleIcon.classList.add('bi-eye');
        }
    });
}