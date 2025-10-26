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

            if (!form.checkValidity()) {
                form.reportValidity();
                return;
            }

            var nameInput = form.querySelector('#dish-name');
            var descriptionInput = form.querySelector('#dish-description');
            var priceInput = form.querySelector('#dish-price');
            var categoryInput = form.querySelector('#dish-category');
            var availableInput = form.querySelector('#dish-is-available');
            var featuredInput = form.querySelector('#dish-is-featured');
            var popularityInput = form.querySelector('#dish-popularity');

            var name = (nameInput && nameInput.value || '').trim();
            var description = (descriptionInput && descriptionInput.value || '').trim();
            var category = (categoryInput && categoryInput.value || '').trim();
            var price = priceInput ? parseFloat(priceInput.value) : NaN;
            var popularity = popularityInput ? parseInt(popularityInput.value, 10) : 0;

            if (!name || !description || !category) {
                showFeedback(feedback, 'Fill in all required fields before submitting.', 'error');
                return;
            }

            if (!Number.isFinite(price) || price <= 0) {
                showFeedback(feedback, 'Price must be a number greater than zero.', 'error');
                return;
            }

            if (!Number.isFinite(popularity) || popularity < 0) {
                popularity = 0;
            }

            var isAvailable = !!(availableInput && availableInput.checked);
            var isFeatured = !!(featuredInput && featuredInput.checked);

            var formData = new FormData(form);
            formData.set('name', name);
            formData.set('description', description);
            formData.set('category', category);
            formData.set('price', price.toFixed(2));
            formData.set('popularityScore', String(popularity));
            formData.set('isAvailable', isAvailable ? 'true' : 'false');
            formData.set('isFeatured', isFeatured ? 'true' : 'false');

            showFeedback(feedback, 'Submitting new dish to gateway...', 'info');

            var submitButton = form.querySelector('button[type="submit"]');
            if (submitButton) {
                submitButton.setAttribute('disabled', 'disabled');
            }

            window.feaneGateway.post(endpoint, formData)
                .then(function (response) {
                    if (response && (response.success || response.status === 'success')) {
                        var message = response.message || 'Dish added successfully.';
                        showFeedback(feedback, message, 'success');

                        var redirectTarget = null;
                        if (typeof response === 'object' && response !== null) {
                            redirectTarget = response.redirectUrl || response.redirect || response.location;
                        }

                        if (redirectTarget) {
                            try {
                                var url = new URL(redirectTarget, window.location.origin);
                                window.location.assign(url.pathname + url.search + url.hash);
                            } catch (error) {
                                console.warn('Failed to parse redirect URL.', error);
                                window.location.assign(redirectTarget);
                            }
                            return;
                        }

                        form.reset();
                        return;
                    }

                    var message = (response && response.message) || 'Failed to add the dish.';
                    showFeedback(feedback, message, 'error');
                })
                .catch(function (error) {
                    showFeedback(feedback, 'Unable to add dish: ' + error.message, 'error');
                })
                .finally(function () {
                    if (submitButton) {
                        submitButton.removeAttribute('disabled');
                    }
                });
        });
    });
})();
