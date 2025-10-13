(function () {
    document.addEventListener('DOMContentLoaded', function () {
        const toggleButton = document.getElementById('forgot-password-btn');
        const form = document.getElementById('forgot-password-form');

        if (!toggleButton || !form) {
            return;
        }

        toggleButton.addEventListener('click', function () {
            form.classList.toggle('hidden');
        });
    });
})();
