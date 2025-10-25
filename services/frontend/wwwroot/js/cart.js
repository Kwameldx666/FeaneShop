(function () {
    'use strict';

    var container = document.querySelector('.cart-container');
    var cartEndpoint = container ? container.getAttribute('data-cart-endpoint') : null; // например: /api/cart
    var tbody = document.getElementById('cart-table-body');
    var totalEl = document.getElementById('cart-total-amount');
    var tableWrap = document.getElementById('cart-table-wrap');
    var emptyBox = document.getElementById('cart-empty');
    var browseBox = document.getElementById('cart-browse');
    var errorBox = document.getElementById('cart-error');

    var items = [];          // [{ key, dishId, name, price, quantity }]
    var syncTimer = null;    // дебаунс

    if (!window.feaneGateway) {
        window.feaneGateway = {
            get: (u) => fetch(u, { credentials: 'include' }).then(r => r.json()),
            post: (u, b) => fetch(u, {
                method: 'POST', headers: { 'Content-Type': 'application/json' },
                credentials: 'include', body: JSON.stringify(b || {})
            }).then(r => r.json())
        };
    }

    function fmt(v) {
        v = Number(v || 0);
        try { return v.toLocaleString(undefined, { style: 'currency', currency: 'USD' }); }
        catch { return '$' + v.toFixed(2); }
    }

    function show(el) { el && el.classList.remove('d-none'); }
    function hide(el) { el && el.classList.add('d-none'); }
    function setText(el, t) { if (el) el.textContent = t; }
    function showError(msg) {
        if (!errorBox) return;
        errorBox.textContent = msg || 'Something went wrong.';
        errorBox.classList.remove('d-none', 'alert-info');
        errorBox.classList.add('alert-danger');
        setTimeout(() => hide(errorBox), 3500);
    }

    // --- Рендер таблицы целиком (первичная загрузка / удаления) ---
    function renderAll() {
        if (!tbody) return;
        tbody.innerHTML = '';

        if (!items.length) {
            hide(tableWrap); show(emptyBox); show(browseBox); setText(totalEl, fmt(0));
            return;
        }

        var total = 0;

        items.forEach(function (it) {
            var key = it.dishId || it.key; // уникальный ключ
            var p = Number(it.price || 0);
            var q = Math.max(1, Number(it.quantity || 1));
            var rowTotal = p * q;
            total += rowTotal;

            var tr = document.createElement('tr');
            tr.dataset.key = key;
            tr.innerHTML = `
        <td>${it.name || 'Dish'}</td>
        <td class="text-center">${fmt(p)}</td>
        <td class="text-center" data-role="row-price" data-price="${p}">
          <div class="input-group input-group-sm justify-content-center" style="max-width:180px;">
            <button class="btn btn-outline-secondary" data-action="decrease" data-key="${key}">-</button>
            <input type="number" class="form-control text-center"
                   data-role="quantity" data-key="${key}" value="${q}" min="1" />
            <button class="btn btn-outline-secondary" data-action="increase" data-key="${key}">+</button>
          </div>
        </td>
        <td class="text-center" data-role="row-total">${fmt(rowTotal)}</td>
        <td class="text-end">
          <button class="btn btn-outline-danger btn-sm" data-action="remove" data-key="${key}">Remove</button>
        </td>`;
            tbody.appendChild(tr);
        });

        setText(totalEl, fmt(total));
        show(tableWrap); hide(emptyBox); hide(browseBox);
    }

    // --- Локальный пересчёт одной строки + общего итога (без полного рендера) ---
    function recalcRowAndTotal(key, newQty) {
        var tr = tbody.querySelector('tr[data-key="' + key + '"]');
        if (!tr) return;

        // обновляем количество в массиве
        var i = items.findIndex(x => (x.dishId || x.key) == key);
        if (i === -1) return;
        items[i].quantity = newQty;

        // цена из дата-атрибута
        var priceCell = tr.querySelector('[data-role="row-price"]');
        var price = Number(priceCell?.getAttribute('data-price') || 0);
        var rowTotalCell = tr.querySelector('[data-role="row-total"]');
        if (rowTotalCell) rowTotalCell.textContent = fmt(price * newQty);

        // общий итог
        var sum = 0;
        items.forEach(function (it) {
            sum += Number(it.price || 0) * Math.max(1, Number(it.quantity || 1));
        });
        setText(totalEl, fmt(sum));
    }

    // --- Дебаунс-синхронизация одного товара ---
    function scheduleSyncOne(key, qty) {
        clearTimeout(syncTimer);
        syncTimer = setTimeout(function () {
            syncOne(key, qty);
        }, 400); // 400мс после последнего ввода
    }

    // --- Вызов API обновления количества ---
    function syncOne(key, qty) {
        var item = items.find(x => (x.dishId || x.key) == key);
        if (!item || !cartEndpoint || !window.feaneGateway) return;

        // Если у тебя эндпоинт другой — поменяй путь здесь
        var updateUrl = cartEndpoint.replace(/\/$/, '') + '/update';

        window.feaneGateway.post(updateUrl, { dishId: item.dishId || key, quantity: qty })
            .then(function (res) {
                // Если сервер вернул актуальную корзину — перерисуем целиком
                if (res && Array.isArray(res.items)) {
                    // нормализация на всякий случай
                    items = res.items.map(function (it, idx) {
                        return {
                            key: it.dishId || it.id || idx,
                            dishId: it.dishId || it.id,
                            name: it.name || it.dishName || 'Dish',
                            price: Number(it.price || it.unitPrice || 0),
                            quantity: Number(it.quantity || it.count || 1)
                        };
                    });
                    renderAll();
                }
                // иначе — ничего, мы уже оптимистично всё посчитали
            })
            .catch(function (e) {
                showError('Failed to update: ' + (e.message || e));
            });
    }

    // --- Снятие товара ---
    function syncRemove(key) {
        var item = items.find(x => (x.dishId || x.key) == key);
        if (!item || !cartEndpoint || !window.feaneGateway) return;
        var removeUrl = cartEndpoint.replace(/\/$/, '') + '/remove';

        window.feaneGateway.post(removeUrl, { dishId: item.dishId || key })
            .then(function (res) {
                if (res && Array.isArray(res.items)) {
                    items = res.items.map(function (it, idx) {
                        return {
                            key: it.dishId || it.id || idx,
                            dishId: it.dishId || it.id,
                            name: it.name || it.dishName || 'Dish',
                            price: Number(it.price || it.unitPrice || 0),
                            quantity: Number(it.quantity || it.count || 1)
                        };
                    });
                    renderAll();
                }
            })
            .catch(function (e) {
                showError('Failed to remove: ' + (e.message || e));
            });
    }

    // --- Загрузка корзины ---
    function loadCart() {
        if (!cartEndpoint || !window.feaneGateway) {
            // fallback на localStorage, если надо
            try {
                var raw = localStorage.getItem('cart') || '[]';
                var arr = JSON.parse(raw);
                items = (Array.isArray(arr) ? arr : []).map((it, idx) => ({
                    key: it.dishId || idx, dishId: it.dishId,
                    name: it.name, price: Number(it.price || 0), quantity: Number(it.quantity || 1)
                }));
            } catch { items = []; }
            renderAll();
            return;
        }

        window.feaneGateway.get(cartEndpoint).then(function (res) {
            var remote = Array.isArray(res && res.items) ? res.items : (Array.isArray(res) ? res : []);
            items = remote.map(function (it, idx) {
                return {
                    key: it.dishId || it.id || idx,
                    dishId: it.dishId || it.id,
                    name: it.name || it.dishName || 'Dish',
                    price: Number(it.price || it.unitPrice || 0),
                    quantity: Number(it.quantity || it.count || 1)
                };
            });
            renderAll();
        }).catch(function (e) {
            showError('Unable to load cart: ' + (e.message || e));
            items = [];
            renderAll();
        });
    }

    // --- Слушатели: +/- кнопки и ввод в инпуте ---
    document.addEventListener('DOMContentLoaded', function () {
        loadCart();

        if (!tbody) return;

        // Клики по +/- и Remove
        tbody.addEventListener('click', function (e) {
            var el = e.target.closest('[data-action]');
            if (!el) return;

            var action = el.getAttribute('data-action');
            var key = el.getAttribute('data-key');
            if (!key) return;

            if (action === 'remove') {
                // локально
                items = items.filter(x => (x.dishId || x.key) != key);
                renderAll();
                // сервер
                syncRemove(key);
                return;
            }

            if (action === 'increase' || action === 'decrease') {
                var input = tbody.querySelector('input[data-role="quantity"][data-key="' + key + '"]');
                if (!input) return;
                var n = Math.max(1, (parseInt(input.value, 10) || 1) + (action === 'increase' ? 1 : -1));
                input.value = String(n);

                // моментальный пересчёт
                recalcRowAndTotal(key, n);
                // дебаунс-синхрон
                scheduleSyncOne(key, n);
            }
        });

        // Мгновенный пересчёт на вводе (без блюра)
        tbody.addEventListener('input', function (e) {
            var input = e.target;
            if (!(input instanceof HTMLInputElement)) return;
            if (input.getAttribute('data-role') !== 'quantity') return;

            var key = input.getAttribute('data-key');
            var n = Math.max(1, parseInt(input.value, 10) || 1);
            // локальный пересчёт
            recalcRowAndTotal(key, n);
            // дебаунс-синхрон
            scheduleSyncOne(key, n);
        });
    });
})();
