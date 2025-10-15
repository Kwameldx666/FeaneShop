(function () {
    'use strict';

    // Универсальный API-клиент
    if (!window.feaneGateway) {
        window.feaneGateway = {
            post: function (url, body) {
                console.log('POST', url, body);
                // Пример имитации ответа сервера
                return new Promise(resolve => {
                    setTimeout(() => resolve({ ok: true, token: 'demo-jwt-token' }), 700);
                });
            }
        };
    }

    document.addEventListener('DOMContentLoaded', function () {
        const apiBase = '/api/auth';

        // ===== РЕГИСТРАЦИЯ =====
        const registerForm = document.getElementById('registerForm');
        if (registerForm) {
            registerForm.addEventListener('submit', async function (e) {
                e.preventDefault(); // 🚫 предотвращает перезагрузку страницы

                const username = document.getElementById('username').value.trim();
                const email = document.getElementById('email').value.trim();
                const password = document.getElementById('password').value.trim();

                let errName = '', errEmail = '', errPass = '';

                if (username.length < 4) errName = 'Минимум 4 символа.<br>';
                if (!/^[^@\s]+@[^@\s]+\.[^@\s]+$/.test(email)) errEmail = 'Некорректный email.<br>';
                if (password.length < 8) errPass = 'Минимум 8 символов.<br>';

                document.getElementById('errorName').innerHTML = errName;
                document.getElementById('errorEmail').innerHTML = errEmail;
                document.getElementById('errorPassword').innerHTML = errPass;

                if (errName || errEmail || errPass) return;

                const res = await window.feaneGateway.post(apiBase + '/register', { username, email, password });
                if (res.ok) {
                    alert('✅ Регистрация успешна! Теперь войдите.');
                    document.getElementById('signIn').click();
                } else {
                    alert(res.message || 'Ошибка регистрации.');
                }
            });
        }

        // ===== АВТОРИЗАЦИЯ =====
        const loginForm = document.getElementById('loginForm');
        if (loginForm) {
            loginForm.addEventListener('submit', async function (e) {
                e.preventDefault(); // 🚫 предотвращает перезагрузку страницы

                const credential = document.getElementById('credential').value.trim();
                const password = document.getElementById('login_password').value.trim();

                let errCred = '', errPass = '';
                if (credential.length === 0) errCred = 'Введите логин.<br>';
                if (password.length === 0) errPass = 'Введите пароль.<br>';

                document.getElementById('errorCredential').innerHTML = errCred;
                document.getElementById('errorPasswordLogin').innerHTML = errPass;

                if (errCred || errPass) return;

                const res = await window.feaneGateway.post(apiBase + '/login', { credential, password });
                if (res.ok && res.token) {
                    localStorage.setItem('jwt', res.token);
                    alert('🎉 Вы успешно вошли!');
                    // Можно сделать redirect:
                    // window.location.href = '/';
                } else {
                    alert(res.message || 'Неверные данные.');
                }
            });
        }
    });
})();
