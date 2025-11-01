(function () {
    'use strict';

    function showFeedback(container, message, type) {
        if (!container) {
            alert(message);
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
        var form = document.querySelector('.address-form');
        var feedback = document.getElementById('addresses-feedback');
        if (!form || !window.feaneGateway) {
            return;
        }

        var endpoint = form.getAttribute('data-gateway-endpoint');
        if (!endpoint) {
            return;
        }

        form.addEventListener('submit', function (event) {
            event.preventDefault();

            var formData = new FormData(form);
            var payload = new URLSearchParams();
            formData.forEach(function (value, key) {
                payload.append(key, value == null ? '' : value.toString());
            });

            showFeedback(feedback, 'Saving address via gateway…', 'info');

            window.feaneGateway.post(endpoint, payload.toString(), {
                headers: {
                    'Content-Type': 'application/x-www-form-urlencoded'
                }
            }).then(function (data) {
                if (data && (data.success || data.status === 'success')) {
                    showFeedback(feedback, data.message || 'Address updated successfully.', 'success');
                    return;
                }

                showFeedback((data && data.message) || 'Failed to update the address.', 'error');
            }).catch(function (error) {
                showFeedback(feedback, 'An error occurred: ' + error.message, 'error');
            });
        });
    });
})();
