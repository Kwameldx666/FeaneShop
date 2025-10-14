(function () {
    document.addEventListener('DOMContentLoaded', function () {
        const section = document.querySelector('.food_section');
        if (!section) {
            return;
        }

        const filtersMenu = section.querySelector('.filters_menu');
        const grid = section.querySelector('.grid');
        const addToCartUrl = section.dataset.addToCartUrl;
        const feedback = document.getElementById('food-feedback');

        function showFeedback(message, type) {
            if (!feedback) {
                alert(message);
                return;
            }

            feedback.textContent = message;
            feedback.classList.remove('d-none', 'alert-success', 'alert-danger');
            feedback.classList.add(type === 'success' ? 'alert-success' : 'alert-danger');
        }

        function handleResponse(response) {
            return response.text().then(function (text) {
                var data = null;
                if (text) {
                    try {
                        data = JSON.parse(text);
                    } catch (error) {
                        data = null;
                    }
                }

                if (!response.ok) {
                    var message = (data && data.message) || text || response.statusText;
                    throw new Error(message);
                }

                if (data) {
                    return data;
                }

                throw new Error('Unexpected server response.');
            });
        }

        if (filtersMenu && grid) {
            filtersMenu.addEventListener('click', function (event) {
                const target = event.target;
                if (!(target instanceof HTMLElement)) {
                    return;
                }

                const filterValue = target.getAttribute('data-filter');
                if (!filterValue) {
                    return;
                }

                Array.from(filtersMenu.querySelectorAll('li')).forEach(function (item) {
                    item.classList.remove('active');
                });
                target.classList.add('active');

                const items = Array.from(grid.querySelectorAll('.all'));
                if (filterValue === '*') {
                    items.forEach(function (item) {
                        item.classList.remove('hidden');
                    });
                    return;
                }

                items.forEach(function (item) {
                    if (item.matches(filterValue)) {
                        item.classList.remove('hidden');
                    } else {
                        item.classList.add('hidden');
                    }
                });
            });
        }

        if (grid && addToCartUrl) {
            grid.addEventListener('click', function (event) {
                const button = (event.target instanceof Element) ? event.target.closest('.add-to-cart') : null;
                if (!button) {
                    return;
                }

                event.preventDefault();

                const formData = new URLSearchParams();
                formData.append('DishId', String(button.getAttribute('data-id') || ''));
                formData.append('DishName', String(button.getAttribute('data-name') || ''));
                formData.append('DishPrice', String(button.getAttribute('data-price') || ''));
                formData.append('Quantity', String(button.getAttribute('data-quantity') || '1'));

                fetch(addToCartUrl, {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/x-www-form-urlencoded'
                    },
                    body: formData.toString()
                })
                    .then(handleResponse)
                    .then(function (data) {
                        if (data && data.success) {
                            showFeedback(data.message || 'Dish added to cart successfully!', 'success');
                            return;
                        }

                        showFeedback((data && data.message) || 'Unable to add dish to cart.', 'error');

                        if (data && data.redirect) {
                            setTimeout(function () {
                                window.location.href = data.redirect;
                            }, 1500);
                        }
                    })
                    .catch(function (error) {
                        showFeedback('An error occurred: ' + error.message, 'error');
                    });
            });
        }
    });
})();
