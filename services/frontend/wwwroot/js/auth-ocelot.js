(function () {
    'use strict';

    // === База для прокси (Ocelot) ===
    // Берём из <meta name="gateway-origin"> или пытаемся подставить порт 5000 по умолчанию
    const GATEWAY_ORIGIN = (() => {
        const m = document.querySelector('meta[name="gateway-origin"]');
        if (m?.content) return m.content.replace(/\/$/, '');

        // Доп. фолбэк через глобальную переменную, если вдруг задашь window.__GATEWAY_ORIGIN__
        if (window.__GATEWAY_ORIGIN__) return String(window.__GATEWAY_ORIGIN__).replace(/\/$/, '');

        // Последний фолбэк: если страница на 5003 — меняем на 5000
        try {
            const u = new URL(location.href);
            if (u.port === '5003') {
                u.port = '5000';
                return u.origin;
            }
        } catch { }
        return location.origin; // на случай если фронт реально проксирует /api/auth сам
    })();

    const API_BASE = `${GATEWAY_ORIGIN}/api/auth`;

    // === Мини-клиент ===
    function authHeader() {
        const t = localStorage.getItem('jwt');
        return t ? { Authorization: 'Bearer ' + t } : {};
    }
    async function toJson(res) { try { return await res.json(); } catch { return {}; } }

    async function httpGet(url) {
        try {
            const res = await fetch(url, {
                method: 'GET',
                mode: 'cors',
                headers: { Accept: 'application/json', ...authHeader() },
                // Если используешь JWT в заголовке — cookies не нужны. Если нужны куки — оставь include и настрой CORS.
                credentials: 'omit'
            });
            const data = await toJson(res);
            if (res.status === 405) console.warn('405 Method Not Allowed. Проверь UpstreamHttpMethod (добавь "Options", "Post") в ocelot.json.');
            return { ok: res.ok, status: res.status, ...data };
        } catch (e) {
            return { ok: false, status: 0, message: 'Сетевая ошибка', error: e };
        }
    }

    async function httpPost(url, body) {
        try {
            const res = await fetch(url, {
                method: 'POST',
                mode: 'cors',
                headers: { 'Content-Type': 'application/json', Accept: 'application/json', ...authHeader() },
                // см. комментарий выше про cookies
                credentials: 'omit',
                body: JSON.stringify(body || {})
            });
            const data = await toJson(res);
            if (res.status === 405) console.warn('405 Method Not Allowed. Проверь UpstreamHttpMethod (добавь "Options", "Post") в ocelot.json.');
            return { ok: res.ok, status: res.status, ...data };
        } catch (e) {
            return { ok: false, status: 0, message: 'Сетевая ошибка', error: e };
        }
    }

    // === Утилиты DOM/валидации ===
    const $ = (s, r = document) => r.querySelector(s);
    const setHtml = (id, html) => { const el = document.getElementById(id); if (el) el.innerHTML = html; };
    const setVal = (id, v) => { const el = document.getElementById(id); if (el) el.value = v; };
    const show = (id) => { const el = document.getElementById(id); if (el) el.classList.remove('d-none'); };
    const hide = (id) => { const el = document.getElementById(id); if (el) el.classList.add('d-none'); };

    function ensureLoginPassError() {
        const id = 'errorPasswordLogin';
        if (!document.getElementById(id)) {
            const span = document.createElement('span');
            span.id = id; span.className = 'text-danger';
            const wrap = document.createElement('div');
            wrap.className = 'col-md-12 form-group text-center';
            wrap.appendChild(span);
            document.getElementById('login_password')?.insertAdjacentElement('afterend', wrap);
        }
        return id;
    }

    function safeRedirect(url) {
        try {
            if (!url) return false;
            const u = new URL(url, location.origin);
            if (u.origin === location.origin) {
                location.href = u.href;
                return true;
            }
            return false;
        } catch { return false; }
    }

    // === Инициализация форм ===
    document.addEventListener('DOMContentLoaded', function () {
        const params = new URLSearchParams(location.search);
        const returnUrl = params.get('returnUrl') || '';

        // Заполняем hidden ReturnUrl (для SSR-fallback)
        setVal('returnUrlRegister', returnUrl);
        setVal('returnUrlLogin', returnUrl);

        // --- Регистрация ---
        const registerForm = document.getElementById('registerForm');
        if (registerForm) {
            registerForm.addEventListener('submit', async function (e) {
                e.preventDefault();

                hide('registerError');
                setHtml('errorName', '');
                setHtml('errorEmail', '');
                setHtml('errorPassword', '');

                const btn = registerForm.querySelector('button[type="submit"]');
                btn?.setAttribute('disabled', 'disabled');

                const username = $('#username')?.value.trim() || '';
                const email = $('#email')?.value.trim() || '';
                const password = $('#password')?.value.trim() || '';

                let errName = '', errEmail = '', errPass = '';
                if (username.length < 4) errName = 'Минимум 4 символа.<br>';
                if (!/^[^@\s]+@[^@\s]+\.[^@\s]+$/.test(email)) errEmail = 'Некорректный email.<br>';
                if (password.length < 8) errPass = 'Минимум 8 символов.<br>';

                setHtml('errorName', errName);
                setHtml('errorEmail', errEmail);
                setHtml('errorPassword', errPass);

                if (errName || errEmail || errPass) { btn?.removeAttribute('disabled'); return; }

                try {
                    const res = await httpPost(`${API_BASE}/register`, { username, email, password });
                    if (res.ok) {
                        if (res.token) localStorage.setItem('jwt', res.token);
                        alert('✅ Регистрация успешна! Теперь войдите.');
                        document.getElementById('signIn')?.click();
                    } else {
                        show('registerError');
                        setHtml('registerError', res.message || 'Ошибка регистрации.');
                        alert(res.message || 'Ошибка регистрации.');
                    }
                } finally {
                    btn?.removeAttribute('disabled');
                }
            });
        }

        // --- Логин ---
        const loginForm = document.getElementById('loginForm');
        if (loginForm) {
            loginForm.addEventListener('submit', async function (e) {
                e.preventDefault();

                hide('loginError');
                setHtml('errorCredential', '');
                const passErrId = ensureLoginPassError();
                setHtml(passErrId, '');

                const btn = loginForm.querySelector('button[type="submit"]');
                btn?.setAttribute('disabled', 'disabled');

                const credential = $('#credential')?.value.trim() || '';
                const password = $('#login_password')?.value.trim() || '';

                let errCred = '', errPass = '';
                if (!credential) errCred = 'Введите логин.<br>';
                if (!password) errPass = 'Введите пароль.<br>';

                setHtml('errorCredential', errCred);
                setHtml(passErrId, errPass);

                if (errCred || errPass) { btn?.removeAttribute('disabled'); return; }

                try {
                    const res = await httpPost(`${API_BASE}/login`, { credential, password });
                    if (res.ok && res.token) {
                        localStorage.setItem('jwt', res.token);
                        if (res.role) localStorage.setItem('userRole', String(res.role));
                        alert('🎉 Вы успешно вошли!');
                        if (!safeRedirect(returnUrl)) location.href = '/';
                    } else {
                        show('loginError');
                        setHtml('loginError', res.message || 'Неверные данные.');
                        alert(res.message || 'Неверные данные.');
                    }
                } finally {
                    btn?.removeAttribute('disabled');
                }
            });
        }
    });
})();
