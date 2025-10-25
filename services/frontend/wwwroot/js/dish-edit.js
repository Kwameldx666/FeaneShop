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

    function resolveEndpoint(template, id) {
        if (!template) {
            return null;
        }

        if (template.indexOf('{id}') >= 0) {
            return template.replace('{id}', encodeURIComponent(id));
        }

        if (template.charAt(template.length - 1) === '/') {
            return template + encodeURIComponent(id);
        }

        return template + '/' + encodeURIComponent(id);
    }

    document.addEventListener('DOMContentLoaded', function () {
        var form = document.getElementById('edit-dish-form');
        var main = document.querySelector('main[data-dish-endpoint]');
        if (!form || !main || !window.feaneGateway) {
            return;
        }

        var feedback = document.getElementById('edit-dish-feedback');
        var hiddenId = document.getElementById('dishId');
        var params = new URLSearchParams(window.location.search);
        var dishId = params.get('id') || hiddenId.value;

        if (!dishId) {
            showFeedback(feedback, 'Dish identifier is missing in the request.', 'error');
            return;
        }

        hiddenId.value = dishId;

        var loadEndpointTemplate = main.getAttribute('data-dish-endpoint');
        var updateEndpointTemplate = form.getAttribute('data-gateway-endpoint');
        var loadEndpoint = resolveEndpoint(loadEndpointTemplate, dishId);
        var updateEndpoint = resolveEndpoint(updateEndpointTemplate, dishId);

        if (!loadEndpoint || !updateEndpoint) {
            showFeedback(feedback, 'Gateway endpoints are not configured.', 'error');
            return;
        }

        showFeedback(feedback, 'Loading dish data…', 'info');
        window.feaneGateway.get(loadEndpoint).then(function (dish) {
            if (!dish) {
                showFeedback(feedback, 'Dish data was not returned by the gateway.', 'error');
                return;
            }

            var nameField = document.getElementById('editDishName');
            var descriptionField = document.getElementById('editDishDescription');
            var priceField = document.getElementById('editDishPrice');
            var categoryField = document.getElementById('editDishCategory');

            nameField.value = dish.name || dish.Name || '';
            descriptionField.value = dish.description || dish.Description || '';
            priceField.value = dish.price || dish.Price || '';
            if (categoryField && (dish.category || dish.Category)) {
                categoryField.value = (dish.category || dish.Category).toString().toLowerCase();
            }

            showFeedback(feedback, 'Dish loaded. You can now update the details.', 'success');
        }).catch(function (error) {
            showFeedback(feedback, 'Failed to load dish: ' + error.message, 'error');
        });

        form.addEventListener('submit', function (event) {
            event.preventDefault();

            var formData = new FormData(form);
            showFeedback(feedback, 'Saving changes…', 'info');

            window.feaneGateway.post(updateEndpoint, formData).then(function (response) {
                if (response && (response.success || response.status === 'success')) {
                    showFeedback(feedback, response.message || 'Dish updated successfully.', 'success');
                    return;
                }

                var message = (response && response.message) || 'Failed to update the dish.';
                showFeedback(feedback, message, 'error');
            }).catch(function (error) {
                showFeedback(feedback, 'Unable to save dish: ' + error.message, 'error');
            });
        });
    });
})();
