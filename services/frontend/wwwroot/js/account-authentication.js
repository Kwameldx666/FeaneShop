        (function () {
    document.addEventListener('DOMContentLoaded', function () {

        const apiBase = "http://localhost:5000/api/auth"; // Gateway → AuthService

        // ================== РЕГИСТРАЦИЯ ==================
        const registerForm = document.getElementById('registerForm');
        if (registerForm) {
            registerForm.addEventListener('submit', async function (event) {
                event.preventDefault();

                const username = document.getElementById('username').value.trim();
                const email = document.getElementById('email').value.trim();
                const password = document.getElementById('password').value.trim();

                let errorName = '';
                let errorEmail = '';
                let errorPassword = '';

                // ---- Валидация ----
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

                // ---- Отображение ошибок ----
                document.getElementById('errorName').innerHTML = errorName;
                document.getElementById('errorEmail').innerHTML = errorEmail;
                document.getElementById('errorPassword').innerHTML = errorPassword;

                if (errorName || errorEmail || errorPassword) {
                    return;
                }

                // ---- Отправка на микросервис ----
                try {
                    const response = await fetch(`${apiBase}/register`, {
                        method: "POST",
                        headers: { "Content-Type": "application/json" },
                        body: JSON.stringify({ username, email, password })
                    });

                    if (!response.ok) {
                        const err = await response.text();
                        alert("Ошибка регистрации: " + err);
                        return;
                    }

                    alert("Регистрация прошла успешно!");
                    document.getElementById('signIn')?.click(); // переключаем на экран авторизации
                } catch (error) {
                    alert("Ошибка подключения к серверу: " + error.message);
                }
            });
        }

        // ================== АВТОРИЗАЦИЯ ==================
        const loginForm = document.getElementById('loginForm');
        if (loginForm) {
            loginForm.addEventListener('submit', async function (event) {
                event.preventDefault();

                const credential = document.getElementById('credential').value.trim();
                const password = document.getElementById('login_password').value.trim();
                const rememberMe = document.querySelector('input[name="RememberMe"]')?.checked ?? false;

                let errorCredential = '';
                let errorPassword = '';

                // ---- Валидация ----
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

                // ---- Отправка на микросервис ----
                try {
                    const response = await fetch(`${apiBase}/login`, {
                        method: "POST",
                        headers: { "Content-Type": "application/json" },
                        body: JSON.stringify({ credential, password, rememberMe })
                    });

                    if (!response.ok) {
                        alert("Неверные учетные данные.");
                        return;
                    }

                    const data = await response.json();
                    if (!data.token) {
                        alert("Ответ сервера не содержит токена.");
                        return;
                    }

                    // ---- Сохраняем токен ----
                    localStorage.setItem("jwt", data.token);
                    alert("Вы успешно вошли!");
                    window.location.href = "/"; // редирект на главную
                } catch (error) {
                    alert("Ошибка подключения к серверу: " + error.message);
                }
            });
        }

        // ================== ПЕРЕКЛЮЧЕНИЕ ПАНЕЛЕЙ ==================
        const container = document.getElementById('container');
        const signUpButton = document.getElementById('signUp');
        const signInButton = document.getElementById('signIn');

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
