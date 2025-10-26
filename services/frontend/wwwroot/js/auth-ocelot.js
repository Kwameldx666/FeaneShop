(function () {
    'use strict';

    // === Ocelot base (Gateway) ===
    const GATEWAY_ORIGIN = (() => {
        const m = document.querySelector('meta[name="gateway-origin"]');
        if (m?.content) return m.content.replace(/\/$/, '');

        if (window.__GATEWAY_ORIGIN__) return String(window.__GATEWAY_ORIGIN__).replace(/\/$/, '');

        try {
            const u = new URL(location.href);
            if (u.port === '5003') { u.port = '5000'; return u.origin; }
        } catch { }

        return location.origin;
    })();

    const API_BASE = `${GATEWAY_ORIGIN}/api/auth`;
    const DEFAULT_REDIRECT = '/home/menu';

    // === Tiny HTTP client ===
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
                credentials: 'omit'
            });
            const data = await toJson(res);
            if (res.status === 405)
                console.warn('405 Method Not Allowed. Check UpstreamHttpMethod in ocelot.json.');
            return { ok: res.ok, status: res.status, ...data };
        } catch (e) {
            return { ok: false, status: 0, message: 'Network error', error: e };
        }
    }

    async function httpPost(url, body) {
        try {
            const res = await fetch(url, {
                method: 'POST',
                mode: 'cors',
                headers: { 'Content-Type': 'application/json', Accept: 'application/json', ...authHeader() },
                credentials: 'omit',
                body: JSON.stringify(body || {})
            });
            const data = await toJson(res);
            if (res.status === 405)
                console.warn('405 Method Not Allowed. Check UpstreamHttpMethod in ocelot.json.');
            return { ok: res.ok, status: res.status, ...data };
        } catch (e) {
            return { ok: false, status: 0, message: 'Network error', error: e };
        }
    }

    // === DOM helpers ===
    const $ = (s, r = document) => r.querySelector(s);
    const setHtml = (id, html) => { const el = document.getElementById(id); if (el) el.innerHTML = html; };
    const setVal = (id, v) => { const el = document.getElementById(id); if (el) el.value = v; };

    const coalesce = (...values) => values.find(v => v !== undefined && v !== null && v !== '') ?? null;

    const extractToken = (p) => coalesce(p?.token, p?.Token, p?.accessToken, p?.AccessToken, p?.data?.token);
    const extractRole = (p) => coalesce(p?.role, p?.Role, p?.user?.role, p?.User?.Role, p?.data?.role);

    const persistToken = (token) => {
        try {
            if (token) localStorage.setItem('jwt', token);
            else localStorage.removeItem('jwt');
        } catch (_) { }
    };

    const applyRole = (role) => {
        const normalized = role ? String(role).toLowerCase() : '';
        try { localStorage.setItem('userRole', normalized); } catch { }
        document.body?.setAttribute('data-user-role', normalized);
    };

    const show = (id) => { const el = document.getElementById(id); if (el) el.classList.remove('d-none'); };
    const hide = (id) => { const el = document.getElementById(id); if (el) el.classList.add('d-none'); };

    function safeRedirect(url) {
        try {
            if (!url) return false;
            const u = new URL(url, location.origin);
            if (u.origin === location.origin) { location.href = u.href; return true; }
            return false;
        } catch { return false; }
    }

    // === Main logic ===
    function initAuthFlows() {
        const params = new URLSearchParams(location.search);
        const returnUrl = params.get('returnUrl') || '';
        const authMode = (params.get('authMode') || '').toLowerCase();

        setVal('returnUrlRegister', returnUrl);
        setVal('returnUrlLogin', returnUrl);

        const container = $('#container');
        if (authMode === 'register') container?.classList.add('right-panel-active');
        else container?.classList.remove('right-panel-active');

        $('#signUp')?.addEventListener('click', () => container?.classList.add('right-panel-active'));
        $('#signIn')?.addEventListener('click', () => container?.classList.remove('right-panel-active'));

        // Registration
        $('#registerForm')?.addEventListener('submit', async (e) => {
            e.preventDefault();
            hide('registerError');
            setHtml('errorName', ''); setHtml('errorEmail', ''); setHtml('errorPassword', '');

            const username = $('#username')?.value.trim() || '';
            const email = $('#email')?.value.trim() || '';
            const password = $('#password')?.value.trim() || '';
            let err = false;

            if (username.length < 4) { setHtml('errorName', 'Минимум 4 символа.'); err = true; }
            if (!/^[^@\s]+@[^@\s]+\.[^@\s]+$/.test(email)) { setHtml('errorEmail', 'Некорректный email.'); err = true; }
            if (password.length < 8) { setHtml('errorPassword', 'Минимум 8 символов.'); err = true; }

            if (err) return;

            const res = await httpPost(`${API_BASE}/register`, { username, email, password });
            if (res.ok) {
                persistToken(extractToken(res));
                applyRole(extractRole(res));
                alert('Регистрация успешна! Войдите в систему.');
                $('#signIn')?.click();
            } else {
                show('registerError');
                setHtml('registerError', res.message || 'Ошибка регистрации.');
            }
        });

        // Login
        $('#loginForm')?.addEventListener('submit', async (e) => {
            e.preventDefault();
            hide('loginError');
            setHtml('errorCredential', '');

            const credential = $('#credential')?.value.trim() || '';
            const password = $('#login_password')?.value.trim() || '';
            if (!credential || !password) {
                setHtml('errorCredential', 'Введите логин и пароль.');
                return;
            }

            const res = await httpPost(`${API_BASE}/login`, { credential, password });
            if (res.ok) {
                persistToken(extractToken(res));
                applyRole(extractRole(res));
                alert('Вы вошли!');
                const redirect = $('#returnUrlLogin')?.value || '';
                if (!safeRedirect(redirect)) location.assign(DEFAULT_REDIRECT);
            } else {
                show('loginError');
                setHtml('loginError', res.message || 'Неверные учетные данные.');
            }
        });
    }

    ['DOMContentLoaded', 'feane:page-ready', 'partials:loaded'].forEach(evt =>
        document.addEventListener(evt, initAuthFlows)
    );
    if (document.readyState !== 'loading') initAuthFlows();
})();
