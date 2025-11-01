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
        var form = document.getElementById('add-dish-form');
        if (!form || !window.feaneGateway) {
            return;
        }

        var feedback = document.getElementById('add-dish-feedback');
        var endpoint = form.getAttribute('data-gateway-endpoint');
        if (!endpoint) {
            return;
        }

        form.addEventListener('submit', function (event) {
            event.preventDefault();

            var formData = new FormData(form);
            showFeedback(feedback, 'Submitting new dish to gateway…', 'info');

            window.feaneGateway.post(endpoint, formData).then(function (response) {
                if (response && (response.success || response.status === 'success')) {
                    showFeedback(feedback, response.message || 'Dish added successfully.', 'success');
                    setTimeout(function () {
                        window.location.href = '/dish/index';
                    }, 600);
                    return;
                }

                var message = (response && response.message) || 'Failed to add the dish.';
                showFeedback(feedback, message, 'error');
            }).catch(function (error) {
                showFeedback(feedback, 'Unable to add dish: ' + error.message, 'error');
            });
        });
    });
})();
