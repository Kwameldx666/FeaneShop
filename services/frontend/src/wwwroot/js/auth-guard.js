(function () {
    'use strict';

    function coalesce() {
        for (var i = 0; i < arguments.length; i += 1) {
            var value = arguments[i];
            if (value !== undefined && value !== null && value !== '') {
                return value;
            }
        }
        return null;
    }

    function extractRole(payload) {
        if (!payload) {
            return null;
        }

        return coalesce(
            payload.role,
            payload.Role,
            payload.user && (payload.user.role || payload.user.Role),
            payload.User && (payload.User.role || payload.User.Role),
            payload.data && (payload.data.role || payload.data.Role)
        );
    }

    function redirectTo(url) {
        if (!url) {
            return;
        }
        console.log('[AuthGuard] Redirecting to:', url);
        window.location.replace(url);
    }

    function checkAuth() {
        console.log('[AuthGuard] checkAuth() called at', new Date().toISOString());

        // Увеличенная задержка чтобы точно дать время localStorage загрузиться
        setTimeout(function () {
            console.log('[AuthGuard] Starting auth check after 300ms delay...');

            var body = document.body;

            if (!body) {
                console.log('[AuthGuard] document.body is null, skipping');
                return;
            }

            var requireAuth = body.getAttribute('data-require-auth');
            console.log('[AuthGuard] data-require-auth =', requireAuth);

            if (requireAuth !== 'true') {
                console.log('[AuthGuard] Page does not require auth, skipping');
                return;
            }

            console.log('[AuthGuard] Page requires auth, checking token...');

            // Проверяем localStorage доступен ли
            try {
                var storageAvailable = typeof localStorage !== 'undefined' && localStorage !== null;
                console.log('[AuthGuard] localStorage available:', storageAvailable);
            } catch (e) {
                console.error('[AuthGuard] localStorage check failed:', e);
            }

            // Сначала проверяем наличие JWT токена в localStorage
            var token = null;
            var retryCount = 0;
            var maxRetries = 3;

            function tryGetToken() {
                try {
                    token = localStorage.getItem('jwt');
                    console.log('[AuthGuard] Attempt', retryCount + 1, '- Token from localStorage:', token ? 'EXISTS (' + token.length + ' chars)' : 'NULL');

                    // Если токена нет и есть попытки - пробуем еще раз через 100ms
                    if (!token && retryCount < maxRetries) {
                        retryCount++;
                        console.log('[AuthGuard] Token not found, retrying in 100ms...');
                        setTimeout(tryGetToken, 100);
                        return;
                    }

                    // Показываем первые и последние символы токена для отладки
                    if (token) {
                        console.log('[AuthGuard] Token preview:', token.substring(0, 20) + '...' + token.substring(token.length - 20));
                    }

                    continueAuthCheck();
                } catch (e) {
                    console.error('[AuthGuard] Cannot access localStorage:', e);
                    continueAuthCheck();
                }
            }

            function continueAuthCheck() {

                var loginUrl = body.getAttribute('data-login-url');
                if (!loginUrl) {
                    var redir = encodeURIComponent(window.location.pathname + window.location.search + window.location.hash);
                    loginUrl = '/account/authentication?redir=' + redir;
                }
                console.log('[AuthGuard] Login URL:', loginUrl);

                // Если токена нет - сразу редиректим
                if (!token) {
                    console.log('[AuthGuard] No JWT token found after', maxRetries, 'attempts, redirecting to login in 2 seconds...');
                    console.log('[AuthGuard] You can cancel redirect with: window.stop()');
                    setTimeout(function () {
                        redirectTo(loginUrl);
                    }, 2000);
                    return;
                }

                // Проверяем срок действия токена и извлекаем роль
                var payload = null;
                try {
                    payload = JSON.parse(atob(token.split('.')[1]));
                    var exp = payload.exp;
                    var now = Math.floor(Date.now() / 1000);

                    console.log('[AuthGuard] Token payload parsed successfully');
                    console.log('[AuthGuard] Token expires at:', exp ? new Date(exp * 1000).toLocaleString() : 'N/A');
                    console.log('[AuthGuard] Current time:', new Date(now * 1000).toLocaleString());
                    console.log('[AuthGuard] Is expired:', exp && exp < now);

                    if (exp && exp < now) {
                        console.log('[AuthGuard] JWT token expired, redirecting to login in 1 second...');
                        localStorage.removeItem('jwt');
                        setTimeout(function () {
                            redirectTo(loginUrl);
                        }, 1000);
                        return;
                    }
                } catch (e) {
                    console.error('[AuthGuard] Invalid JWT token format:', e);
                    console.log('[AuthGuard] Redirecting to login in 1 second...');
                    localStorage.removeItem('jwt');
                    setTimeout(function () {
                        redirectTo(loginUrl);
                    }, 1000);
                    return;
                }

                // Токен есть и валиден - пользователь авторизован
                console.log('[AuthGuard] ✅ JWT token is VALID, user authenticated!');
                console.log('[AuthGuard] User will NOT be redirected');

                // Извлекаем роль из токена
                if (payload) {
                    var role = extractRole(payload);
                    console.log('[AuthGuard] User role:', role || 'authenticated');
                    if (typeof window.feaneSetUserRole === 'function') {
                        window.feaneSetUserRole(role || 'authenticated');
                    }
                }

                // Опционально: проверка на сервере (если указан endpoint)
                var endpoint = body.getAttribute('data-auth-check-endpoint');
                if (endpoint && window.feaneGateway) {
                    console.log('[AuthGuard] Performing server-side auth check:', endpoint);
                    window.feaneGateway.get(endpoint).then(function (data) {
                        console.log('[AuthGuard] Server-side check successful');
                        var serverRole = extractRole(data);
                        if (serverRole && typeof window.feaneSetUserRole === 'function') {
                            window.feaneSetUserRole(serverRole);
                        }
                    }).catch(function (error) {
                        console.warn('[AuthGuard] Server-side check failed (but JWT is valid, continuing):', error.message);
                    });
                }
            } // конец continueAuthCheck

            // Начинаем попытки получения токена
            tryGetToken();
        }, 300); // увеличенная задержка до 300ms
    }

    document.addEventListener('DOMContentLoaded', checkAuth);
    document.addEventListener('feane:page-ready', checkAuth);
})();
