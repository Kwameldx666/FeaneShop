(function () {
    'use strict';

    document.addEventListener('DOMContentLoaded', function () {
        var main = document.querySelector('main[data-error-endpoint]');
        if (!main || !window.feaneGateway) {
            return;
        }

        var endpoint = main.getAttribute('data-error-endpoint');
        if (!endpoint) {
            return;
        }

        var payload = {
            path: window.location.pathname,
            referrer: document.referrer || null,
            occurredAt: new Date().toISOString()
        };

        window.feaneGateway.post(endpoint, payload).catch(function (error) {
            console.warn('Failed to report 404 event to gateway:', error);
        });
    });
})();
