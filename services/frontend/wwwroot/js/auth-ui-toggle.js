(function () {
    document.addEventListener('DOMContentLoaded', function () {
        const container = document.getElementById('container');
        const signUpButton = document.getElementById('signUp');
        const signInButton = document.getElementById('signIn');

        // Кнопки переключения
        if (signUpButton && container) {
            signUpButton.addEventListener('click', function () {
                container.classList.add('right-panel-active');
            });
        }

        if (signInButton && container) {
            signInButton.addEventListener('click', function () {
                container.classList.remove('right-panel-active');
            });
        }

        // Автооткрытие регистрации по URL
        const params = new URLSearchParams(window.location.search);
        const authMode = params.get('authMode');
        if (authMode && authMode.toLowerCase() === 'register') {
            container.classList.add('right-panel-active');
        }
    });
})();
