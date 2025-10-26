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

    function tryGetCachedDish(id) {
        if (!id || !window.sessionStorage) {
            return null;
        }

        try {
            var raw = sessionStorage.getItem('feane.editDish.' + id);
            if (!raw) {
                return null;
            }
            return JSON.parse(raw);
        } catch (error) {
            console.warn('Unable to read cached dish', error);
            return null;
        }
    }

    function cacheDish(id, dish) {
        if (!id || !dish || !window.sessionStorage) {
            return;
        }

        try {
            sessionStorage.setItem('feane.editDish.' + id, JSON.stringify(dish));
        } catch (error) {
            console.warn('Unable to cache dish after fetch', error);
        }
    }

    function clearCachedDish(id) {
        if (!id || !window.sessionStorage) {
            return;
        }

        try {
            sessionStorage.removeItem('feane.editDish.' + id);
        } catch (error) {
            console.warn('Unable to remove cached dish', error);
        }
    }

    function populateForm(dish) {
        if (!dish) {
            return;
        }

        var nameField = document.getElementById('editDishName');
        var descriptionField = document.getElementById('editDishDescription');
        var priceField = document.getElementById('editDishPrice');
        var categoryField = document.getElementById('editDishCategory');
        var popularityField = document.getElementById('editDishPopularity');
        var availableField = document.getElementById('editDishIsAvailable');
        var featuredField = document.getElementById('editDishIsFeatured');
        var imagePreview = document.getElementById('editDishImagePreview');

        if (nameField) {
            nameField.value = dish.name || dish.Name || '';
        }

        if (descriptionField) {
            descriptionField.value = dish.description || dish.Description || '';
        }

        if (priceField) {
            var rawPrice = dish.price != null ? dish.price : dish.Price;
            var numericPrice = Number(rawPrice);
            priceField.value = Number.isFinite(numericPrice) ? numericPrice.toFixed(2) : '';
        }

        if (categoryField && (dish.category || dish.Category)) {
            categoryField.value = (dish.category || dish.Category).toString().toLowerCase();
        }

        if (popularityField) {
            var popularity = dish.popularityScore != null ? dish.popularityScore : dish.PopularityScore;
            var numericPopularity = Number(popularity);
            popularityField.value = Number.isFinite(numericPopularity) && numericPopularity >= 0
                ? numericPopularity
                : 0;
        }

        if (availableField) {
            var isAvailable = dish.isAvailable != null ? dish.isAvailable : dish.IsAvailable;
            availableField.checked = Boolean(isAvailable);
        }

        if (featuredField) {
            var isFeatured = dish.isFeatured != null ? dish.isFeatured : dish.IsFeatured;
            featuredField.checked = Boolean(isFeatured);
        }

        if (imagePreview) {
            var imageUrl = dish.imageUrl || dish.ImageUrl || '';
            var imageBase64 = dish.imageBase64 || dish.ImageBase64 || '';
            var mimeType = dish.imageMimeType || dish.ImageMimeType || 'image/jpeg';
            if (imageUrl) {
                imagePreview.innerHTML = '<img src="' + imageUrl + '" alt="Dish image" />';
            } else if (imageBase64) {
                imagePreview.innerHTML = '<img src="data:' + mimeType + ';base64,' + imageBase64 + '" alt="Dish image" />';
            } else {
                imagePreview.textContent = 'No image uploaded.';
            }
        }
    }

    function init() {
        var form = document.getElementById('edit-dish-form');
        var main = document.querySelector('main[data-dish-endpoint-template]');
        if (!form || !main || !window.feaneGateway) {
            return;
        }

        var feedback = document.getElementById('edit-dish-feedback');
        var hiddenId = document.getElementById('dishId');
        var params = new URLSearchParams(window.location.search);
        var storedId = null;
        try {
            storedId = sessionStorage.getItem('feane.editDishId');
        } catch (error) {
            storedId = null;
        }
        var dishId = params.get('id') || (hiddenId ? hiddenId.value : '') || storedId || '';

        if (!dishId) {
            showFeedback(feedback, 'Dish identifier is missing in the request.', 'error');
            return;
        }

        if (hiddenId) {
            hiddenId.value = dishId;
        }
        if (storedId) {
            try {
                sessionStorage.removeItem('feane.editDishId');
            } catch (error) {
                // ignore
            }
        }

        var loadEndpointTemplate = main.getAttribute('data-dish-endpoint-template');
        var updateEndpointTemplate = form.getAttribute('data-update-endpoint-template');
        var loadEndpoint = resolveEndpoint(loadEndpointTemplate, dishId);
        var updateEndpoint = resolveEndpoint(updateEndpointTemplate, dishId);

        if (form.dataset.initialized === 'true') {
            return;
        }
        form.dataset.initialized = 'true';

        if (!loadEndpoint || !updateEndpoint) {
            showFeedback(feedback, 'Gateway endpoints are not configured.', 'error');
            return;
        }

        var cachedDish = tryGetCachedDish(dishId);
        if (cachedDish) {
            populateForm(cachedDish);
        }

        showFeedback(feedback, 'Loading dish data from gateway...', 'info');
        window.feaneGateway.get(loadEndpoint)
            .then(function (payload) {
                var dish = null;
                if (payload && typeof payload === 'object') {
                    if (payload.item) {
                        dish = payload.item;
                    } else if (payload.data && payload.data.item) {
                        dish = payload.data.item;
                    } else {
                        dish = payload;
                    }
                }

        if (!dish) {
            console.warn('[dish-edit] Gateway response did not include a dish', payload);
            showFeedback(feedback, 'Dish data was not returned by the gateway.', 'error');
            return;
        }

        console.log('[dish-edit] Loaded dish from gateway', dish);

                populateForm(dish);
                cacheDish(dishId, dish);

                showFeedback(feedback, 'Dish loaded. You can now update the details.', 'success');
            })
            .catch(function (error) {
                showFeedback(feedback, 'Failed to load dish: ' + error.message, 'error');
            });

        form.addEventListener('submit', function (event) {
            event.preventDefault();

            var formData = new FormData(form);
            var priceField = document.getElementById('editDishPrice');
            var popularityField = document.getElementById('editDishPopularity');
            var availableField = document.getElementById('editDishIsAvailable');
            var featuredField = document.getElementById('editDishIsFeatured');

            if (priceField) {
                var parsedPrice = Number(priceField.value);
                if (!Number.isFinite(parsedPrice)) {
                    parsedPrice = 0;
                }
                formData.set('price', parsedPrice.toFixed(2));
            }

            if (popularityField) {
                var popularityValue = Number(popularityField.value);
                if (!Number.isFinite(popularityValue) || popularityValue < 0) {
                    popularityValue = 0;
                }
                formData.set('popularityScore', String(popularityValue));
            }

            if (availableField) {
                var availableValue = availableField.checked ? 'true' : 'false';
                formData.set('isAvailable', availableValue);
                console.debug('[dish-edit] isAvailable ->', availableValue);
            }

            if (featuredField) {
                var featuredValue = featuredField.checked ? 'true' : 'false';
                formData.set('isFeatured', featuredValue);
                console.debug('[dish-edit] isFeatured ->', featuredValue);
            }

            showFeedback(feedback, 'Saving changes via gateway...', 'info');
            console.log('[dish-edit] Submitting to', updateEndpoint, 'payload entries:', Array.from(formData.entries()));

            window.feaneGateway.post(updateEndpoint, formData)
                .then(function (response) {
                    if (response && (response.success || response.status === 'success')) {
                        var successMessage = response.message || 'Dish updated successfully.';
                        showFeedback(feedback, successMessage, 'success');
                        clearCachedDish(dishId);

                        var redirectTarget = null;
                        if (typeof response === 'object' && response !== null) {
                            redirectTarget = response.redirectUrl || response.redirect || response.location;
                        }

                        setTimeout(function () {
                            if (redirectTarget && redirectTarget !== '#') {
                                window.location.assign(redirectTarget);
                            } else {
                                window.location.assign('/dish/index');
                            }
                        }, 800);
                        return;
                    }

                    var message = (response && response.message) || 'Failed to update the dish.';
                    showFeedback(feedback, message, 'error');
                })
                .catch(function (error) {
                    showFeedback(feedback, 'Unable to save dish: ' + error.message, 'error');
                });
        });
    }

    ['DOMContentLoaded', 'feane:page-ready', 'partials:loaded'].forEach(function (eventName) {
        document.addEventListener(eventName, init);
    });

    if (document.readyState !== 'loading') {
        init();
    }
})();
