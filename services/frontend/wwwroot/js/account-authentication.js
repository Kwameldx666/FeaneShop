(function () {
    'use strict';

    document.addEventListener('DOMContentLoaded', function () {
        if (!window.feaneGateway) {
            return;
        }

        var apiBase = '/api/auth';

        // ================== РЕГИСТРАЦИЯ ==================
        var registerForm = document.getElementById('registerForm');
        if (registerForm) {
            registerForm.addEventListener('submit', function (event) {
                event.preventDefault();

                var username = document.getElementById('username').value.trim();
                var email = document.getElementById('email').value.trim();
                var password = document.getElementById('password').value.trim();

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

                document.getElementById('errorName').innerHTML = errorName;
                document.getElementById('errorEmail').innerHTML = errorEmail;
                document.getElementById('errorPassword').innerHTML = errorPassword;

                if (errorName || errorEmail || errorPassword) {
                    return;
                }

                window.feaneGateway.post(apiBase + '/register', {
                    username: username,
                    email: email,
                    password: password
                }).then(function (response) {
                    if (!response || response.error) {
                        alert('Ошибка регистрации: ' + (response && response.message ? response.message : 'Неизвестная ошибка.'));
                        return;
                    }

                    alert('Регистрация прошла успешно!');
                    var signInButton = document.getElementById('signIn');
                    if (signInButton) {
                        signInButton.click();
                    }
                }).catch(function (error) {
                    alert('Ошибка подключения к серверу: ' + error.message);
                });
            });
        }

        // ================== АВТОРИЗАЦИЯ ==================
        var loginForm = document.getElementById('loginForm');
        if (loginForm) {
            loginForm.addEventListener('submit', function (event) {
                event.preventDefault();

                var credential = document.getElementById('credential').value.trim();
                var password = document.getElementById('login_password').value.trim();
                var rememberMe = document.querySelector('input[name="RememberMe"]');
                rememberMe = rememberMe ? rememberMe.checked : false;

                var errorCredential = '';
                var errorPassword = '';

                if (credential.length === 0) {
                    errorCredential += 'Пожалуйста, заполните поле логина.<br>';
                }

                if (password.length === 0) {
                    errorPassword += 'Пожалуйста, введите пароль.<br>';
                }

                document.getElementById('errorCredential').innerHTML = errorCredential;
                document.getElementById('errorPassword').innerHTML = errorPassword;

                if (errorCredential || errorPassword) {
                    return;
                }

                window.feaneGateway.post(apiBase + '/login', {
                    credential: credential,
                    password: password,
                    rememberMe: rememberMe
                }).then(function (data) {
                    if (!data || data.error) {
                        alert('Неверные учетные данные.');
                        return;
                    }

                    if (!data.token) {
                        alert('Ответ сервера не содержит токена.');
                        return;
                    }

                    localStorage.setItem('jwt', data.token);
                    alert('Вы успешно вошли!');
                    window.location.href = '/';
                }).catch(function (error) {
                    alert('Ошибка подключения к серверу: ' + error.message);
                });
            });
        }

        // ================== ПЕРЕКЛЮЧЕНИЕ ПАНЕЛЕЙ ==================
        var container = document.getElementById('container');
        var signUpButton = document.getElementById('signUp');
        var signInButton = document.getElementById('signIn');

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
    });
})();
