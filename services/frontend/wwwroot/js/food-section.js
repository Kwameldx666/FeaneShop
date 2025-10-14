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

    function createMenuCard(item) {
        var category = (item.category || 'all').toLowerCase();
        var price = Number(item.price || 0).toFixed(2);
        var id = item.id || item.dishId || '';
        var quantity = item.quantity || 1;

        return '<div class="col-sm-6 col-lg-4 all ' + category + '" data-name="' + item.name + '" data-price="' + price + '">' +
            '<div class="box"><div><div class="img-box">' +
            '<img src="' + (item.imageUrl || '/images/Default.png') + '" alt="' + item.name + '">' +
            '</div><div class="detail-box">' +
            '<h5 class="name">' + item.name + '</h5>' +
            '<p>' + (item.description || '') + '</p>' +
            '<div class="options">' +
            '<h6 class="price">$' + price + '</h6>' +
            '<a href="#" class="cart-icon add-to-cart" data-id="' + id + '" data-name="' + item.name + '" data-price="' + price + '" data-quantity="' + quantity + '">' +
            '<svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="feather feather-shopping-cart">' +
            '<circle cx="9" cy="21" r="1"></circle><circle cx="20" cy="21" r="1"></circle>' +
            '<path d="M1 1h4l2.68 13.39a2 2 0 0 0 2 1.61h9.72a2 2 0 0 0 2-1.61L23 6H6"></path>' +
            '</svg>' +
            '</a>' +
            '</div></div></div></div></div>';
    }

    document.addEventListener('DOMContentLoaded', function () {
        var section = document.querySelector('.food_section');
        if (!section) {
            return;
        }

        var filtersMenu = section.querySelector('.filters_menu');
        var grid = section.querySelector('.grid');
        var addToCartUrl = section.dataset.addToCartUrl;
        var menuEndpoint = section.dataset.menuEndpoint;
        var feedback = document.getElementById('food-feedback');

        if (filtersMenu && grid) {
            filtersMenu.addEventListener('click', function (event) {
                var target = event.target instanceof HTMLElement ? event.target : null;
                if (!target) {
                    return;
                }

                var filterValue = target.getAttribute('data-filter');
                if (!filterValue) {
                    return;
                }

                Array.prototype.forEach.call(filtersMenu.querySelectorAll('li'), function (item) {
                    item.classList.remove('active');
                });
                target.classList.add('active');

                Array.prototype.forEach.call(grid.querySelectorAll('.all'), function (item) {
                    if (filterValue === '*' || item.matches(filterValue)) {
                        item.classList.remove('hidden');
                    } else {
                        item.classList.add('hidden');
                    }
                });
            });
        }

        if (grid && menuEndpoint && window.feaneGateway) {
            showFeedback(feedback, 'Loading menu from gateway…', 'info');
            window.feaneGateway.get(menuEndpoint).then(function (response) {
                var items = Array.isArray(response && response.items) ? response.items : response;
                if (!Array.isArray(items) || items.length === 0) {
                    showFeedback(feedback, 'No dishes returned from gateway.', 'error');
                    return;
                }

                grid.innerHTML = items.map(createMenuCard).join('');
                showFeedback(feedback, 'Menu synchronised with gateway.', 'success');
            }).catch(function (error) {
                showFeedback(feedback, 'Unable to load menu: ' + error.message, 'error');
            });
        }

        if (grid && addToCartUrl && window.feaneGateway) {
            grid.addEventListener('click', function (event) {
                var button = event.target instanceof Element ? event.target.closest('.add-to-cart') : null;
                if (!button) {
                    return;
                }

                event.preventDefault();

                var payload = {
                    DishId: button.getAttribute('data-id'),
                    DishName: button.getAttribute('data-name'),
                    DishPrice: button.getAttribute('data-price'),
                    Quantity: button.getAttribute('data-quantity') || '1'
                };

                showFeedback(feedback, 'Adding dish to cart…', 'info');
                window.feaneGateway.post(addToCartUrl, payload).then(function (data) {
                    if (data && (data.success || data.status === 'success')) {
                        showFeedback(feedback, data.message || 'Dish added to cart successfully!', 'success');
                        if (data.redirect) {
                            setTimeout(function () {
                                window.location.href = data.redirect;
                            }, 1500);
                        }
                        return;
                    }

                    showFeedback(feedback, (data && data.message) || 'Unable to add dish to cart.', 'error');
                }).catch(function (error) {
                    showFeedback(feedback, 'Failed to add dish: ' + error.message, 'error');
                });
            });
        }
    });
})();
