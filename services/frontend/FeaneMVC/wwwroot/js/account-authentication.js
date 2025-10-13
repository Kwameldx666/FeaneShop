(function () {
    document.addEventListener('DOMContentLoaded', function () {
        const registerForm = document.getElementById('registerForm');
        if (registerForm) {
            registerForm.addEventListener('submit', function (event) {
                var username = document.getElementById('username').value;
                var email = document.getElementById('email').value;
                var password = document.getElementById('password').value;

                var errorName = '';
                var errorEmail = '';
                var errorPassword = '';

                if (username.length === 0) {
                    errorName += 'Пожалуйста, заполните поле имени.<br>';
                } else if (username.length < 4) {
                    errorName += 'Некорректная длина имени (минимум 4 символа).<br>';
                } else if (username.length > 20) {
                    errorName += 'Некорректная длина имени (максимум 20 символов).<br>';
                }

                if (email.length === 0) {
                    errorEmail += 'Пожалуйста, заполните поле email.<br>';
                }

                if (password.length === 0) {
                    errorPassword += 'Пожалуйста, введите пароль.<br>';
                } else if (password.length < 8) {
                    errorPassword += 'Слишком короткий пароль (минимум 8 символов).<br>';
                } else {
                    if (!/[A-Z]/.test(password)) {
                        errorPassword += 'Пароль должен содержать хотя бы одну заглавную букву.<br>';
                    }
                    if (!/[0-9]/.test(password)) {
                        errorPassword += 'Пароль должен содержать хотя бы одну цифру.<br>';
                    }
                }

                if (errorName !== '') {
                    document.getElementById('errorName').innerHTML = errorName;
                    event.preventDefault();
                }

                if (errorEmail !== '') {
                    document.getElementById('errorEmail').innerHTML = errorEmail;
                    event.preventDefault();
                }

                if (errorPassword !== '') {
                    document.getElementById('errorPassword').innerHTML = errorPassword;
                    event.preventDefault();
                }
            });
        }

        const loginForm = document.getElementById('loginForm');
        if (loginForm) {
            loginForm.addEventListener('submit', function (event) {
                var credential = document.getElementById('credential').value;
                var password = document.getElementById('login_password').value;

                var errorCredential = '';
                var errorPassword = '';

                if (credential.length === 0) {
                    errorCredential += 'Пожалуйста, заполните поле логина.<br>';
                }

                if (password.length === 0) {
                    errorPassword += 'Пожалуйста, введите пароль.<br>';
                }

                if (errorCredential !== '') {
                    document.getElementById('errorCredential').innerHTML = errorCredential;
                    event.preventDefault();
                }

                if (errorPassword !== '') {
                    document.getElementById('errorPassword').innerHTML = errorPassword;
                    event.preventDefault();
                }
            });
        }
    });
})();
