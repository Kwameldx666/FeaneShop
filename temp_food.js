(function () {
    'use strict';

    /** ********************************************************************
     *  Food Section (Our Menu)
     *  - Поддержка data-атрибутов: data-menu-endpoint | data-dishes-endpoint,
     *    data-add-to-cart-url | data-add-to-cart-endpoint, data-currency
     *  - Рендер карточек из <template id="food-card-template">
     *  - Фильтрация (Isotope, если подключен; иначе fallback через .hidden)
     *  - Добавление в корзину (gateway + оптимистично в localStorage)
     *  - Идемпотентная инициализация (через атрибут data-food-section-ready)
     ********************************************************************* */

    var INIT_FLAG_ATTR = 'data-food-section-ready';
    var CARD_TEMPLATE_ID = 'food-card-template';
    var FEEDBACK_ID = 'food-feedback';
    var GRID_SELECTOR = '.grid';                // контейнер для карточек (внутри .food_section)
    var FILTERS_SELECTOR = '.filters_menu';     // <ul> с <li data-filter="*|.pizza|.burger">
    var BASE_CARD_CLASS = 'all';                // базовый класс карточки для фильтра по категориям

    // ---- Helpers --------------------------------------------------------

    function $(sel, root) { return (root || document).querySelector(sel); }
    function $all(sel, root) { return Array.prototype.slice.call((root || document).querySelectorAll(sel)); }

    function toNumber(v, fallback) {
        var n = Number(v);
        return Number.isFinite(n) ? n : (fallback != null ? Number(fallback) : 0);
    }

    function slugifyCategory(v) {
        return String(v || 'all')
            .toLowerCase()
            .replace(/[^a-z0-9_-]+/g, '-');
    }

    function fmtMoney(value, currency) {
        var sign = currency || '$';
        return sign + toNumber(value, 0).toFixed(2);
    }

    function ensureGateway() {
        // Используем window.feaneGateway, если есть, иначе простой fetch-бэкап
        if (window.feaneGateway && typeof window.feaneGateway.get === 'function' && typeof window.feaneGateway.post === 'function') {
            return window.feaneGateway;
        }
        return {
            get: function (url) {
                return fetch(url, { credentials: 'include' }).then(function (r) { return r.json(); });
            },
            post: function (url, body) {
                return fetch(url, {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    credentials: 'include',
                    body: JSON.stringify(body || {})
                }).then(function (r) { return r.json(); });
            }
        };
    }

    function showFeedback(message, type, root) {
        var box = $('#' + FEEDBACK_ID, root) || document.getElementById(FEEDBACK_ID);
        if (!box) {
            console.log(message);
            return;
        }
        box.textContent = message || '';
        box.classList.remove('d-none', 'alert-success', 'alert-danger', 'alert-info');
        if (type === 'success') box.classList.add('alert-success');
        else if (type === 'info') box.classList.add('alert-info');
        else box.classList.add('alert-danger');

        // Авто-скрытие информ-сообщений
        if (type === 'success' || type === 'info') {
            window.clearTimeout(box._hideTimer);
            box._hideTimer = window.setTimeout(function () {
                box.classList.add('d-none');
            }, 2500);
        }
    }

    // ---- Card rendering -------------------------------------------------

    function createCardFromTemplate(template, item, addToCartUrl, currency) {
        if (!(template instanceof HTMLTemplateElement)) return null;

        var root = template.content.firstElementChild.cloneNode(true);
        var cat = slugifyCategory(item.category);
        root.classList.add(BASE_CARD_CLASS, cat);
        root.dataset.name = item.name || '';
        root.dataset.price = String(item.price != null ? item.price : 0);

        var img = $('.img-box img', root);
        if (img) {
            img.src = item.imageUrl || '/images/default.png';
            img.alt = item.name || 'Menu item';
        }

        var nameEl = $('.name', root);
        if (nameEl) nameEl.textContent = item.name || 'Menu item';

        var descEl = $('.description', root);
        if (descEl) descEl.textContent = item.description || 'No description provided.';

        var priceEl = $('.price', root);
        if (priceEl) {
            var price = toNumber(item.price, 0);
            priceEl.textContent = price > 0 ? fmtMoney(price, currency) : 'Price on request';
        }

        var btn = $('.add-to-cart', root);
        if (btn) {
            if (!addToCartUrl) {
                btn.classList.add('d-none');
            } else {
                btn.dataset.id = item.id || item.dishId || '';
                btn.dataset.name = item.name || '';
                btn.dataset.price = String(item.price != null ? item.price : 0);
                btn.dataset.quantity = String(item.quantity != null ? item.quantity : 1);
            }
        }

        return root;
    }

    function renderItems(grid, template, items, addToCartUrl, currency) {
        grid.innerHTML = '';
        items.forEach(function (item) {
            var card = createCardFromTemplate(template, item, addToCartUrl, currency);
            if (card) grid.appendChild(card);
        });
    }

    // ---- Filtering ------------------------------------------------------

    function initFiltering(section, filtersMenu, grid) {
        if (!filtersMenu || !grid) return;

        // Если Isotope подключён — используем его
        var useIsotope = !!(window.jQuery && jQuery.fn && jQuery.fn.isotope);
        var $iso = null;
        if (useIsotope) {
            $iso = jQuery(grid).isotope({
                itemSelector: '.' + BASE_CARD_CLASS,
                layoutMode: 'fitRows'
            });
        }

        filtersMenu.addEventListener('click', function (ev) {
            var target = ev.target instanceof Element ? ev.target.closest('li[data-filter]') : null;
            if (!target) return;

            var filter = target.getAttribute('data-filter') || '*';
            $all('li', filtersMenu).forEach(function (li) { li.classList.remove('active'); });
            target.classList.add('active');

            if (useIsotope && $iso) {
                $iso.isotope({ filter: filter });
            } else {
                // Fallback: просто скрываем/показываем по классу
                $all('.' + BASE_CARD_CLASS, grid).forEach(function (el) {
                    if (filter === '*' || el.matches(filter)) el.classList.remove('hidden');
                    else el.classList.add('hidden');
                });
            }
        });
    }

    // ---- Add to cart ----------------------------------------------------

    function optimisticAddToLocalStorage(payload) {
        try {
            var raw = localStorage.getItem('cart');
            var arr = raw ? JSON.parse(raw) : [];
            if (!Array.isArray(arr)) arr = [];

            var idx = arr.findIndex(function (x) {
                return String(x.id || x.DishId) === String(payload.DishId);
            });

            if (idx === -1) {
                arr.push({
                    id: payload.DishId,
                    name: payload.DishName,
                    price: toNumber(payload.DishPrice, 0),
                    quantity: toNumber(payload.Quantity, 1)
                });
            } else {
                arr[idx].quantity = toNumber(arr[idx].quantity, 1) + toNumber(payload.Quantity, 1);
            }
            localStorage.setItem('cart', JSON.stringify(arr));
            document.dispatchEvent(new CustomEvent('cart:updated', { detail: { items: arr } }));
        } catch (_) { /* silent */ }
    }

    function attachAddToCart(grid, addToCartUrl, section) {
        if (!grid || !addToCartUrl) return;

        var gateway = ensureGateway();

        grid.addEventListener('click', function (ev) {
            var btn = ev.target instanceof Element ? ev.target.closest('.add-to-cart') : null;
            if (!btn) return;
            ev.preventDefault();

            var payload = {
                DishId: btn.getAttribute('data-id'),
                DishName: btn.getAttribute('data-name'),
                DishPrice: btn.getAttribute('data-price'),
                Quantity: btn.getAttribute('data-quantity') || '1'
            };

            // Оптимистично — сразу обновим localStorage (и уведомим слушателей)
            optimisticAddToLocalStorage(payload);

            showFeedback('Adding dish to cart…', 'info', section);

            gateway.post(addToCartUrl, payload).then(function (res) {
                if (res && (res.success || res.status === 'success')) {
                    showFeedback(res.message || 'Dish added to cart successfully!', 'success', section);
                    if (res.redirect) {
                        setTimeout(function () { window.location.href = res.redirect; }, 1200);
                    }
                } else {
                    showFeedback((res && res.message) || 'Unable to add dish to cart.', 'error', section);
                }
            }).catch(function (err) {
                showFeedback('Failed to add dish: ' + (err && err.message || 'Network error'), 'error', section);
            });
        });
    }

    // ---- Fetch menu -----------------------------------------------------

    function normaliseMenuResponse(data) {
        // Принимаем либо массив, либо { items: [...] }
        var items = Array.isArray(data && data.items) ? data.items : data;
        return Array.isArray(items) ? items : [];
    }

    function fetchAndRenderMenu(section, grid, template, menuEndpoint, addToCartUrl, currency) {
        var gateway = ensureGateway();
        if (!menuEndpoint || !gateway || !grid || !template) return;

        grid.setAttribute('aria-busy', 'true');
        showFeedback('Loading menu from gateway…', 'info', section);

        return gateway.get(menuEndpoint).then(function (resp) {
            var items = normaliseMenuResponse(resp);
            if (!items.length) {
                grid.innerHTML = '';
                showFeedback('No dishes returned from gateway.', 'error', section);
                return;
            }
            renderItems(grid, template, items, addToCartUrl, currency);
            showFeedback('Menu synchronised with gateway.', 'success', section);
        }).catch(function (err) {
            grid.innerHTML = '';
            showFeedback('Unable to load menu: ' + (err && err.message || 'Network error'), 'error', section);
        }).finally(function () {
            grid.setAttribute('aria-busy', 'false');
        });
    }

    // ---- Init -----------------------------------------------------------

    function initOne(section) {
        if (!section || section.hasAttribute(INIT_FLAG_ATTR)) return;
        section.setAttribute(INIT_FLAG_ATTR, 'true');

        var grid = $(GRID_SELECTOR, section);
        var filtersMenu = $(FILTERS_SELECTOR, section);
        var template = document.getElementById(CARD_TEMPLATE_ID);

        // data-* (поддерживаем пару названий)
        var addToCartUrl = section.dataset.addToCartUrl || section.dataset.addToCartEndpoint || '';
        var menuEndpoint = section.dataset.menuEndpoint || section.dataset.dishesEndpoint || '';
        var currency = section.dataset.currency || '$';

        // Фильтрация
        initFiltering(section, filtersMenu, grid);

        // Загрузка меню из API (если указан endpoint)
        if (menuEndpoint && grid && template) {
            fetchAndRenderMenu(section, grid, template, menuEndpoint, addToCartUrl, currency);
        }

        // Add to cart
        attachAddToCart(grid, addToCartUrl, section);
    }

    function init() {
        $all('.food_section').forEach(initOne);
    }

    // Первый запуск
    document.addEventListener('DOMContentLoaded', init);
    // Для твоих внутренних эвентов (если перерисовываешь DOM)
    document.addEventListener('feane:page-ready', init);
    // Если navbar/footer или куски страницы подгружаются динамически
    document.addEventListener('partials:loaded', init);
})();

