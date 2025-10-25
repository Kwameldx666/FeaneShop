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
        var form = document.querySelector('.checkout-form');
        if (!form || !window.feaneGateway) {
            return;
        }

        var feedback = document.getElementById('checkout-feedback');
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

            showFeedback(feedback, 'Processing payment via gateway…', 'info');

            window.feaneGateway.post(endpoint, payload.toString(), {
                headers: {
                    'Content-Type': 'application/x-www-form-urlencoded'
                }
            }).then(function (response) {
                var success = false;
                if (!response) {
                    showFeedback(feedback, 'No response received from the payment gateway.', 'error');
                    return;
                }

                if (typeof response === 'string') {
                    showFeedback(feedback, response, 'success');
                    success = true;
                } else {
                    success = Boolean(response.success || response.status === 'success');
                    var message = response.message || (success ? 'Payment processed successfully.' : 'Unable to process payment.');
                    showFeedback(feedback, message, success ? 'success' : 'error');

                    if (success) {
                        if (response.redirectUrl) {
                            window.location.href = response.redirectUrl;
                            return;
                        }

                        if (response.orderId || response.paymentId) {
                            var identifier = response.orderId || response.paymentId;
                            window.location.href = '/Pages/Payment/Confirmation.html?orderId=' + encodeURIComponent(identifier);
                            return;
                        }
                    }
                }
            }).catch(function (error) {
                showFeedback(feedback, 'Payment failed: ' + error.message, 'error');
            });
        });
    });
})();
