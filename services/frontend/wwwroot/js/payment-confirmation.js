(function () {
    'use strict';

    function renderDetails(container, payload) {
        if (!container) {
            return;
        }

        var content = '';
        if (!payload) {
            content = '<p class="mb-0">No additional payment information is available.</p>';
        } else if (typeof payload === 'string') {
            content = '<p class="mb-0">' + payload + '</p>';
        } else {
            content = '<dl class="row mb-0">';
            Object.keys(payload).forEach(function (key) {
                var value = payload[key];
                if (value && typeof value === 'object') {
                    value = JSON.stringify(value);
                }
                content += '<dt class="col-sm-4 text-capitalize">' + key + '</dt>';
                content += '<dd class="col-sm-8">' + (value || '—') + '</dd>';
            });
            content += '</dl>';
        }

        container.innerHTML = content;
        container.classList.remove('d-none');
    }

    function renderError(container, message) {
        if (!container) {
            return;
        }

        container.innerHTML = '<p class="text-danger mb-0">' + message + '</p>';
        container.classList.remove('d-none');
    }

    document.addEventListener('DOMContentLoaded', function () {
        var main = document.querySelector('main[data-confirmation-endpoint]');
        var container = document.getElementById('payment-details');
        if (!main || !container || !window.feaneGateway) {
            return;
        }

        var params = new URLSearchParams(window.location.search);
        var orderId = params.get('orderId') || params.get('paymentId');
        if (!orderId) {
            renderError(container, 'Payment identifier is missing.');
            return;
        }

        var endpoint = main.getAttribute('data-confirmation-endpoint');
        var url = endpoint + (endpoint.indexOf('?') >= 0 ? '&' : '?') + 'id=' + encodeURIComponent(orderId);

        window.feaneGateway.get(url).then(function (response) {
            if (!response) {
                renderError(container, 'The gateway returned an empty response.');
                return;
            }

            if (typeof response === 'string') {
                renderDetails(container, response);
                return;
            }

            if (response.error || response.success === false) {
                renderError(container, response.message || 'Unable to load payment details.');
                return;
            }

            renderDetails(container, response);
        }).catch(function (error) {
            renderError(container, 'Failed to load payment information: ' + error.message);
        });
    });
})();
