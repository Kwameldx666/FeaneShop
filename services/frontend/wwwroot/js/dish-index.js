(function () {
    'use strict';

    function escapeHtml(value) {
        if (value == null) {
            return '';
        }

        return String(value).replace(/[&<>"']/g, function (character) {
            return ({
                '&': '&amp;',
                '<': '&lt;',
                '>': '&gt;',
                '"': '&quot;',
                "'": '&#39;'
            })[character];
        });
    }

    function formatCurrency(value) {
        var number = Number(value);
        if (!Number.isFinite(number)) {
            number = 0;
        }

        return '$' + number.toFixed(2);
    }

    function showFeedback(message, type, feedback) {
        if (!feedback) {
            console.log(message);
            return;
        }

        feedback.textContent = message;
        feedback.classList.remove('d-none', 'alert-success', 'alert-danger', 'alert-info');
        if (type === 'success') {
            feedback.classList.add('alert-success');
        } else if (type === 'info') {
            feedback.classList.add('alert-info');
        } else {
            feedback.classList.add('alert-danger');
        }
    }

    document.addEventListener('DOMContentLoaded', function () {
        var container = document.querySelector('.admin-container');
        if (!container || !window.feaneGateway) {
            return;
        }

        var listEndpoint = container.getAttribute('data-dishes-endpoint');
        var deleteTemplate = container.getAttribute('data-delete-template');
        var feedback = document.getElementById('dish-feedback');
        var tableBody = document.getElementById('dish-table-body');

        if (!tableBody || !listEndpoint) {
            return;
        }

        function renderRows(dishes) {
            tableBody.innerHTML = '';

            if (!Array.isArray(dishes) || dishes.length === 0) {
                var emptyRow = document.createElement('tr');
                emptyRow.innerHTML = '<td colspan="5" class="text-center text-muted">No dishes found.</td>';
                tableBody.appendChild(emptyRow);
                return;
            }

            dishes.forEach(function (dish) {
                var id = dish.id || dish.Id || dish.dishId || '';
                var name = dish.name || dish.Name || 'Dish';
                var description = dish.description || dish.Description || '';
                var rawPrice = dish.price;
                if (rawPrice == null) {
                    rawPrice = dish.Price;
                }
                var price = Number(rawPrice);
                if (!Number.isFinite(price)) {
                    price = 0;
                }
                var category = dish.category || dish.Category || '-';
                var imageMime = dish.imageMimeType || dish.ImageMimeType || 'image/jpeg';
                var imageBase64 = dish.imageBase64 || dish.ImageBase64 || '';
                var imageUrl = dish.imageUrl || dish.ImageUrl || '';

                var imageSource = '';
                if (imageUrl) {
                    imageSource = imageUrl;
                } else if (imageBase64) {
                    imageSource = 'data:' + imageMime + ';base64,' + imageBase64;
                }

                var imageCell = imageSource
                    ? '<img class="thumb" src="' + escapeHtml(imageSource) + '" alt="' + escapeHtml(name) + '" />'
                    : '<span class="text-muted">No image</span>';

                var editHref = '/dish/editdish';
                if (id) {
                    editHref += '?id=' + encodeURIComponent(id);
                }

                var row = document.createElement('tr');
                row.setAttribute('data-id', id);
                row.innerHTML = '' +
                    '<td>' + escapeHtml(name) + '</td>' +
                    '<td>' + escapeHtml(description) + '</td>' +
                    '<td>' + formatCurrency(price) + '</td>' +
                    '<td>' + escapeHtml(category) + '</td>' +
                    '<td>' + imageCell + '</td>' +
                    '<td class="text-end">' +
                    '  <a class="btn btn-outline-secondary btn-sm me-2" href="' + editHref + '" data-role="edit-dish" data-id="' + escapeHtml(id) + '">Edit</a>' +
                    '  <button type="button" class="btn btn-outline-danger btn-sm" data-role="delete-dish" data-id="' + escapeHtml(id) + '">Delete</button>' +
                    '</td>';
                tableBody.appendChild(row);

                if (id && window.sessionStorage) {
                    try {
                        sessionStorage.setItem('feane.editDish.' + id, JSON.stringify(dish));
                    } catch (error) {
                        console.warn('Unable to cache dish for editing', error);
                    }
                }
            });
        }

        function loadDishes() {
            showFeedback('Loading dishes from gateway...', 'info', feedback);

            window.feaneGateway.get(listEndpoint)
                .then(function (response) {
                    var items = [];

                    if (response && typeof response === 'object' && Array.isArray(response.items)) {
                        items = response.items;
                    } else if (Array.isArray(response)) {
                        items = response;
                    }

                    renderRows(items);

                    var isSuccess = !response || response.success !== false;
                    var message = (response && response.message) || (isSuccess ? 'Dishes are up to date.' : 'Unable to load dishes.');
                    showFeedback(message, isSuccess ? 'success' : 'error', feedback);
                })
                .catch(function (error) {
                    renderRows([]);
                    showFeedback('Failed to load dishes: ' + error.message, 'error', feedback);
                });
        }

        container.addEventListener('click', function (event) {
            var editLink = event.target instanceof Element ? event.target.closest('[data-role="edit-dish"]') : null;
            if (editLink) {
                var editId = editLink.getAttribute('data-id');
                if (editId && window.sessionStorage) {
                    try {
                        sessionStorage.setItem('feane.editDishId', editId);
                    } catch (storageError) {
                        console.warn('Unable to persist edit dish id', storageError);
                    }
                }
                return;
            }

            var button = event.target instanceof Element ? event.target.closest('[data-role="delete-dish"]') : null;
            if (!button) {
                return;
            }

            var id = button.getAttribute('data-id');
            if (!id) {
                showFeedback('Dish identifier is missing.', 'error', feedback);
                return;
            }

            if (!confirm('Are you sure you want to delete this dish?')) {
                return;
            }

            var deleteEndpoint = deleteTemplate ? deleteTemplate.replace('{id}', encodeURIComponent(id)) : (listEndpoint + '/' + encodeURIComponent(id));

            showFeedback('Deleting dish...', 'info', feedback);
            window.feaneGateway.delete(deleteEndpoint).then(function (response) {
                if (response && (response.success || response.status === 'success')) {
                    showFeedback(response.message || 'Dish deleted successfully.', 'success', feedback);
                    loadDishes();
                    return;
                }

                var message = (response && response.message) || 'Unable to delete the dish.';
                showFeedback(message, 'error', feedback);
            }).catch(function (error) {
                showFeedback('Failed to delete the dish: ' + error.message, 'error', feedback);
            });
        });

        loadDishes();
    });
})();
