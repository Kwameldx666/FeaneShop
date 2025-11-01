(function () {
    'use strict';

    // === Ocelot base (Gateway) ===
    // Reads from <meta name="gateway-origin"> or window.__GATEWAY_ORIGIN__.
    // If the page is on port 5003, fallback to 5000 (gateway). Otherwise keep current origin.
    const GATEWAY_ORIGIN = (() => {
        const m = document.querySelector('meta[name="gateway-origin"]');
        if (m?.content) return m.content.replace(/\/$/, '');

        if (window.__GATEWAY_ORIGIN__) return String(window.__GATEWAY_ORIGIN__).replace(/\/$/, '');

        try {
            const u = new URL(location.href);
            if (u.port === '5003') {
                u.port = '5000';
                return u.origin;
            }
        } catch {
        }

        return location.origin;
    })();

    const API_BASE = `${GATEWAY_ORIGIN}/api/auth`;
    const DEFAULT_REDIRECT = '/home/menu';

    // === Tiny HTTP client ===
    function authHeader() {
        const t = localStorage.getItem('jwt');
        return t ? {Authorization: 'Bearer ' + t} : {};
    }

    async function toJson(res) {
        try {
            return await res.json();
        } catch {
            return {};
        }
    }

    async function httpGet(url) {
        try {
            const res = await fetch(url, {
                method: 'GET',
                mode: 'cors',
                headers: {Accept: 'application/json', ...authHeader()},
                credentials: 'omit'
            });
            const data = await toJson(res);
            if (res.status === 405) console.warn('405 Method Not Allowed. Check UpstreamHttpMethod (add "Options", "Post") in ocelot.json.');
            return {ok: res.ok, status: res.status, ...data};
        } catch (e) {
            return {ok: false, status: 0, message: 'Network error', error: e};
        }
    }

    async function httpPost(url, body) {
        try {
            const res = await fetch(url, {
                method: 'POST',
                mode: 'cors',
                headers: {'Content-Type': 'application/json', Accept: 'application/json', ...authHeader()},
                credentials: 'omit', // using JWT in header; no cookies
                body: JSON.stringify(body || {})
            });
            const data = await toJson(res);
            if (res.status === 405) console.warn('405 Method Not Allowed. Check UpstreamHttpMethod (add "Options", "Post") in ocelot.json.');
            return {ok: res.ok, status: res.status, ...data};
        } catch (e) {
            return {ok: false, status: 0, message: 'Network error', error: e};
        }
    }

    // === DOM helpers ===
    const $ = (s, r = document) => r.querySelector(s);
    const setHtml = (id, html) => {
        const el = document.getElementById(id);
        if (el) el.innerHTML = html;
    };
    const setVal = (id, v) => {
        const el = document.getElementById(id);
        if (el) el.value = v;
    };

    const coalesce = (...values) => {
        for (let i = 0; i < values.length; i += 1) {
            const value = values[i];
            if (value !== undefined && value !== null && value !== '') return value;
        }
        return null;
    };

    const extractToken = (payload) => coalesce(
        payload?.token, payload?.Token,
        payload?.accessToken, payload?.AccessToken,
        payload?.data?.token, payload?.data?.Token
    );

    const extractRole = (payload) => coalesce(
        payload?.role, payload?.Role,
        payload?.user?.role, payload?.user?.Role,
        payload?.User?.role, payload?.User?.Role,
        payload?.data?.role, payload?.data?.Role
    );

    const persistToken = (token, refreshToken) => {
        try {
            if (token) {
                localStorage.setItem('jwt', token);
                localStorage.setItem('jwtToken', token);
            } else {
                localStorage.removeItem('jwt');
                localStorage.removeItem('jwtToken');
            }

            if (refreshToken) {
                localStorage.setItem('refreshToken', refreshToken);
            } else {
                localStorage.removeItem('refreshToken');
            }
        } catch (_) {
        }
    };

    const syncRoleArtifacts = (role) => {
        try {
            const meta = document.querySelector('meta[name="feane-user-role"]');
            if (meta) meta.setAttribute('content', role || '');
        } catch (_) {
        }

        if (document?.body) {
            if (role) document.body.setAttribute('data-user-role', role);
            else document.body.removeAttribute('data-user-role');
        }
    };

    const applyRole = (role) => {
        const normalized = role ? String(role).toLowerCase() : '';

        let handled = false;
        if (typeof window.feaneSetUserRole === 'function') {
            try {
                window.feaneSetUserRole(normalized);
                handled = true;
            } catch (_) {
                handled = false;
            }
        }

        if (!handled) {
            try {
                if (normalized) {
                    localStorage.setItem('userRole', normalized);
                    sessionStorage.setItem('userRole', normalized);
                } else {
                    localStorage.removeItem('userRole');
                    sessionStorage.removeItem('userRole');
                }
            } catch (_) {
            }
            window.__FEANE_USER_ROLE__ = normalized || null;
            syncRoleArtifacts(normalized);
        }

        try {
            document.dispatchEvent(new CustomEvent('feane:user-role-changed', {detail: {role: normalized}}));
        } catch (_) {
        }
    };

    function show(id) {
        const el = document.getElementById(id);
        if (!el) return;
        el.classList.remove('d-none');
        el.hidden = false;
        el.style.removeProperty('display'); // in case style="display:none"
    }

    function hide(id) {
        const el = document.getElementById(id);
        if (!el) return;
        el.classList.add('d-none');
        el.hidden = true;
        el.style.display = 'none';
    }

    function ensureLoginPassError() {
        const id = 'errorPasswordLogin';
        if (!document.getElementById(id)) {
            const span = document.createElement('span');
            span.id = id;
            span.className = 'text-danger';
            const wrap = document.createElement('div');
            wrap.className = 'col-md-12 form-group text-center';
            wrap.appendChild(span);
            document.getElementById('login_password')?.insertAdjacentElement('afterend', wrap);
        }
        return id;
    }

    // Safe same-origin redirect
    function safeRedirect(url) {
        try {
            if (!url) return false;
            const u = new URL(url, location.origin);
            if (u.origin === location.origin) {
                location.href = u.href;
                return true;
            }
            return false;
        } catch {
            return false;
        }
    }

    function initAuthFlows() {
        const params = new URLSearchParams(location.search);
        const returnUrl = params.get('returnUrl') || '';
        const authMode = (params.get('authMode') || '').toLowerCase();

        // Hidden ReturnUrl fields stay in sync with current query
        setVal('returnUrlRegister', returnUrl);
        setVal('returnUrlLogin', returnUrl);

        // Panel toggle (SPA-friendly: bind once per element)
        const container = document.getElementById('container');
        if (container) {
            if (authMode === 'register') container.classList.add('right-panel-active');
            else container.classList.remove('right-panel-active');
        }

        const signUpBtn = document.getElementById('signUp');
        if (signUpBtn && !signUpBtn.hasAttribute('data-auth-bound')) {
            signUpBtn.addEventListener('click', function () {
                const host = document.getElementById('container');
                host?.classList.add('right-panel-active');
            });
            signUpBtn.setAttribute('data-auth-bound', 'true');
        }

        const signInBtn = document.getElementById('signIn');
        if (signInBtn && !signInBtn.hasAttribute('data-auth-bound')) {
            signInBtn.addEventListener('click', function () {
                const host = document.getElementById('container');
                host?.classList.remove('right-panel-active');
            });
            signInBtn.setAttribute('data-auth-bound', 'true');
        }

        // --- Registration ---
        const registerForm = document.getElementById('registerForm');
        if (registerForm && !registerForm.hasAttribute('data-auth-bound')) {
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
                if (username.length < 4) errName = 'Minimum 4 characters.<br>';
                if (!/^[^@\s]+@[^@\s]+\.[^@\s]+$/.test(email)) errEmail = 'Invalid email.<br>';
                if (password.length < 8) errPass = 'Minimum 8 characters.<br>';

                setHtml('errorName', errName);
                setHtml('errorEmail', errEmail);
                setHtml('errorPassword', errPass);

                if (errName || errEmail || errPass) {
                    btn?.removeAttribute('disabled');
                    return;
                }

                try {
                    const res = await httpPost(`${API_BASE}/register`, {username, email, password});
                    if (res.ok) {
                        const token = extractToken(res);
                        const refreshToken = res.refreshToken || res.RefreshToken;
                        const role = extractRole(res);
                        if (token) {
                            persistToken(token, refreshToken);
                        }
                        if (role) applyRole(role);

                        alert('Registration successful! Please sign in.');
                        document.getElementById('signIn')?.click();
                    } else {
                        show('registerError');
                        setHtml('registerError', res.message || 'Registration failed.');
                        alert(res.message || 'Registration failed.');
                    }
                } finally {
                    btn?.removeAttribute('disabled');
                }
            });
            registerForm.setAttribute('data-auth-bound', 'true');
        }

        // --- Login ---
        const loginForm = document.getElementById('loginForm');
        if (loginForm && !loginForm.hasAttribute('data-auth-bound')) {
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
                if (!credential) errCred = 'Enter username or email.<br>';
                if (!password) errPass = 'Enter password.<br>';

                setHtml('errorCredential', errCred);
                setHtml(passErrId, errPass);

                if (errCred || errPass) {
                    btn?.removeAttribute('disabled');
                    return;
                }

                try {
                    const res = await httpPost(`${API_BASE}/login`, {credential, password});
                    if (res && res.ok) {
                        const token = extractToken(res);
                        const refreshToken = res.refreshToken || res.RefreshToken;
                        const role = extractRole(res);

                        if (token) {
                            persistToken(token, refreshToken);
                        }
                        if (role) {
                            applyRole(role);
                        }

                        alert('You are signed in!');
                        const currentReturnUrl = (document.getElementById('returnUrlLogin')?.value || '').trim();
                        // Prefer returnUrl; otherwise go to /home/menu
                        if (!safeRedirect(currentReturnUrl)) {
                            location.assign(DEFAULT_REDIRECT);
                        }
                    } else {
                        const errorMessage = res?.message || 'Invalid credentials.';
                        show('loginError');
                        setHtml('loginError', errorMessage);
                        alert(errorMessage);
                    }
                } finally {
                    btn?.removeAttribute('disabled');
                }
            });
            loginForm.setAttribute('data-auth-bound', 'true');
        }
    }

    ['DOMContentLoaded', 'feane:page-ready', 'partials:loaded'].forEach(function (evt) {
        document.addEventListener(evt, initAuthFlows);
    });

    if (document.readyState !== 'loading') {
        initAuthFlows();
    }
})();
