(function () {
    'use strict';

    function showFeedback(container, message, type) {
        if (!container) {
            console.log(message);
            return;
        }

        container.textContent = message;
        container.classList.remove('d-none', 'alert-success', 'alert-danger', 'alert-info');
        if (type === 'success') {
            container.classList.add('alert-success');
        } else if (type === 'info') {
            container.classList.add('alert-info');
        } else {
            container.classList.add('alert-danger');
        }
    }

    document.addEventListener('DOMContentLoaded', function () {
        var form = document.getElementById('notification-filters-form');
        if (!form || !window.feaneGateway) {
            return;
        }

        var feedback = document.getElementById('notification-filters-feedback');
        var endpoint = form.getAttribute('data-gateway-endpoint');
        if (!endpoint) {
            return;
        }

        form.addEventListener('submit', function (event) {
            event.preventDefault();

            var payload = {};
            var formData = new FormData(form);
            formData.forEach(function (value, key) {
                if (Object.prototype.hasOwnProperty.call(payload, key)) {
                    if (!Array.isArray(payload[key])) {
                        payload[key] = [payload[key]];
                    }
                    payload[key].push(value);
                } else {
                    payload[key] = value;
                }
            });

            payload.Email = form.querySelector('#emailFilter').checked;
            payload.Sms = form.querySelector('#smsFilter').checked;

            showFeedback(feedback, 'Saving notification filters…', 'info');
            window.feaneGateway.post(endpoint, payload).then(function (response) {
                if (response && (response.success || response.status === 'success')) {
                    showFeedback(feedback, response.message || 'Filters updated.', 'success');
                    return;
                }

                var message = (response && response.message) || 'Failed to update filters.';
                showFeedback(feedback, message, 'error');
            }).catch(function (error) {
                showFeedback(feedback, 'Unable to update filters: ' + error.message, 'error');
            });
        });
    });
})();
