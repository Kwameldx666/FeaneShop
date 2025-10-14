(function () {
    'use strict';

    function setMessage(element, message) {
        if (!element) {
            console.log(message);
            return;
        }
        element.textContent = message;
    }

    document.addEventListener('DOMContentLoaded', function () {
        var container = document.querySelector('main[data-auth-status-endpoint]');
        var message = document.getElementById('account-auth-status');
        if (!container || !window.feaneGateway) {
            return;
        }

        var endpoint = container.getAttribute('data-auth-status-endpoint');
        var loginUrl = container.getAttribute('data-auth-login-url') || '/Pages/Home/Authentication.html';
        var dashboardUrl = container.getAttribute('data-auth-dashboard-url') || '/Pages/Account/Index.html';

        if (!endpoint) {
            return;
        }

        setMessage(message, 'Contacting the authentication gateway…');

        window.feaneGateway.get(endpoint).then(function (response) {
            if (response && (response.isAuthenticated || response.authenticated)) {
                setMessage(message, 'You are already signed in. Redirecting to your dashboard…');
                setTimeout(function () {
                    window.location.href = dashboardUrl;
                }, 1500);
            } else {
                setMessage(message, 'You are not signed in. Redirecting to the login page…');
                setTimeout(function () {
                    window.location.href = loginUrl;
                }, 1500);
            }
        }).catch(function (error) {
            setMessage(message, 'Unable to verify session via gateway: ' + error.message);
        });
    });
})();
