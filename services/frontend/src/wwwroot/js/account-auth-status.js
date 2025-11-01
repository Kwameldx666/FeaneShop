(function () {
    'use strict';

    function setMessage(element, message) {
        if (!element) {
            console.log(message);
            return;
        }
        element.textContent = message;
    }

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

    function applyRole(role) {
        var normalized = role ? String(role).toLowerCase() : '';

        if (typeof window.feaneSetUserRole === 'function') {
            window.feaneSetUserRole(normalized);
            return;
        }

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

        try {
            var meta = document.querySelector('meta[name="feane-user-role"]');
            if (meta) {
                meta.setAttribute('content', normalized);
            }
        } catch (_) {
        }

        if (document && document.body) {
            if (normalized) {
                document.body.setAttribute('data-user-role', normalized);
            } else {
                document.body.removeAttribute('data-user-role');
            }
        }

        try {
            document.dispatchEvent(new CustomEvent('feane:user-role-changed', {detail: {role: normalized}}));
        } catch (_) {
        }
    }

    document.addEventListener('DOMContentLoaded', function () {
        var container = document.querySelector('main[data-auth-status-endpoint]');
        var message = document.getElementById('account-auth-status');
        if (!container || !window.feaneGateway) {
            return;
        }

        var endpoint = container.getAttribute('data-auth-status-endpoint');
        var loginUrl = container.getAttribute('data-auth-login-url') || '/account/authentication';
        var dashboardUrl = container.getAttribute('data-auth-dashboard-url') || '/account/reservationhistory';

        if (!endpoint) {
            return;
        }

        setMessage(message, 'Contacting the authentication gateway�');

        window.feaneGateway.get(endpoint).then(function (response) {
            if (response && (response.isAuthenticated || response.authenticated)) {
                var role = extractRole(response);
                if (role) {
                    applyRole(role);
                }

                setMessage(message, 'You are already signed in. Redirecting to your dashboard�');
                setTimeout(function () {
                    window.location.href = dashboardUrl;
                }, 1200);
            } else {
                setMessage(message, 'You are not signed in. Redirecting to the login page�');
                setTimeout(function () {
                    window.location.href = loginUrl;
                }, 1200);
            }
        }).catch(function (error) {
            setMessage(message, 'Unable to verify session via gateway: ' + (error && error.message ? error.message : error));
        });
    });
})();
