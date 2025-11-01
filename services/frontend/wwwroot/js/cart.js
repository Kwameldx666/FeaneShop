(function () {
    'use strict';

    console.log('[Cart] Initializing cart.js');

    // DOM Elements
    var container = document.querySelector('.cart-container');
    var cartEndpoint = container ? container.getAttribute('data-cart-endpoint') : '/api/cart';
    var checkoutUrl = container ? container.getAttribute('data-checkout-url') : '/order/checkout';
    

    var errorBox = document.getElementById('cart-error');
    var skeletonsBox = document.getElementById('cart-skeletons');
    var emptyBox = document.getElementById('cart-empty');
    var contentBox = document.getElementById('cart-content');
    var itemsContainer = document.getElementById('cart-items');
    var totalEl = document.getElementById('cart-total-amount');
    var subtotalEl = document.getElementById('subtotal-amount');
    var shippingEl = document.getElementById('shipping-amount');
    var itemsCountEl = document.getElementById('items-count');
    var totalItemsEl = document.getElementById('total-items');
    var checkoutBtn = document.getElementById('checkout-btn');
    var clearCartBtn = document.getElementById('clear-cart-btn');
    var applyPromoBtn = document.getElementById('apply-promo');

    var items = [];
    var syncTimer = null;

    // Helper: Get Gateway Client
    function getGateway() {
        if (window.feaneGateway && typeof window.feaneGateway.get === 'function') {
            console.log('[Cart] Using global feaneGateway');
            return window.feaneGateway;
        }

        console.warn('[Cart] feaneGateway not available, cannot proceed');
        throw new Error('Gateway client not initialized. Please refresh the page.');
    }

    // Helper: Format currency
    function fmt(v) {
        v = Number(v || 0);
        try {
            return v.toLocaleString('ro-RO', {style: 'currency', currency: 'MDL'}).replace('MDL', 'LEI');
        } catch {
            return v.toFixed(2) + ' LEI';
        }
    }

    // Helper: Show/Hide elements
    function show(el) {
        el && el.classList.remove('d-none');
    }

    function hide(el) {
        el && el.classList.add('d-none');
    }

    function setText(el, t) {
        if (el) el.textContent = t;
    }

    // Helper: Show error/success messages
    function showMessage(msg, type) {
        if (!errorBox) return;
        errorBox.textContent = msg || 'Something went wrong.';
        errorBox.className = 'alert';
        if (type === 'success') {
            errorBox.classList.add('alert-success');
        } else if (type === 'info') {
            errorBox.classList.add('alert-info');
        } else if (type === 'danger') {
            errorBox.classList.add('alert-danger');
        } else {
            errorBox.classList.add('alert-warning');
        }
        show(errorBox);
        setTimeout(function () {
            hide(errorBox);
        }, 4000);
    }

    // Render cart items
    function renderCart() {
        console.log('[Cart] Rendering', items.length, 'items');

        if (!items.length) {
            hide(skeletonsBox);
            hide(contentBox);
            show(emptyBox);
            return;
        }

        hide(skeletonsBox);
        hide(emptyBox);
        show(contentBox);

        if (!itemsContainer) {
            console.error('[Cart] Items container not found');
            return;
        }

        itemsContainer.innerHTML = '';

        var subtotal = 0;

        items.forEach(function (item) {
            var itemId = item.id;
            var name = item.productName || item.name || 'Блюдо';
            var imageUrl = item.productImageUrl || item.imageUrl || '/images/Default.png';
            var unitPrice = Number(item.unitPrice || item.price || 0);
            var qty = Math.max(1, Number(item.quantity || 1));
            var itemTotal = unitPrice * qty;
            subtotal += itemTotal;

            var itemDiv = document.createElement('div');
            itemDiv.className = 'cart-item';
            itemDiv.dataset.itemId = itemId;
            itemDiv.dataset.unitPrice = unitPrice;

            // Escape HTML in name
            var safeName = name.replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;').replace(/"/g, '&quot;');

            itemDiv.innerHTML = `
                <img src="${imageUrl}" alt="${safeName}" class="item-image" onerror="this.src='/images/Default.png'"/>
                <div class="item-details">
                    <div class="item-name" title="${safeName}">${safeName}</div>
                    <div class="item-price">${fmt(unitPrice)} за шт.</div>
                </div>
                <div class="item-actions">
                    <div class="qty">
                        <button type="button" class="qty-btn" data-action="decrease" data-item-id="${itemId}" aria-label="Уменьшить количество">−</button>
                        <input type="number" class="qty-input" data-role="quantity" data-item-id="${itemId}" 
                               value="${qty}" min="1" readonly aria-label="Количество"/>
                        <button type="button" class="qty-btn" data-action="increase" data-item-id="${itemId}" aria-label="Увеличить количество">+</button>
                    </div>
                    <div class="item-total" data-role="item-total">${fmt(itemTotal)}</div>
                    <button type="button" class="btn btn-danger" data-action="remove" data-item-id="${itemId}" aria-label="Удалить товар">
                        <span>🗑️</span>
                    </button>
                </div>
            `;

            itemsContainer.appendChild(itemDiv);
        });

        console.log('[Cart] Rendered', items.length, 'items, subtotal:', subtotal);

        // Update summary
        var shipping = 0; // Free shipping for now
        var total = subtotal + shipping;

        var itemsText = items.length === 1 ? ' товар' : (items.length < 5 ? ' товара' : ' товаров');
        setText(itemsCountEl, '(' + items.length + itemsText + ')');
        setText(totalItemsEl, items.length);
        setText(subtotalEl, fmt(subtotal));
        setText(shippingEl, shipping === 0 ? 'Бесплатно' : fmt(shipping));
        setText(totalEl, fmt(total));
    }

    // Update single item total locally
    function updateItemTotal(itemId, newQty) {
        var itemDiv = itemsContainer.querySelector('.cart-item[data-item-id="' + itemId + '"]');
        if (!itemDiv) {
            console.warn('[Cart] Item div not found for id:', itemId);
            return;
        }

        var unitPrice = Number(itemDiv.dataset.unitPrice || 0);
        var itemTotal = unitPrice * newQty;

        var totalEl = itemDiv.querySelector('[data-role="item-total"]');
        if (totalEl) {
            totalEl.textContent = fmt(itemTotal);
        }

        // Update in items array
        var item = items.find(x => x.id == itemId);
        if (item) {
            item.quantity = newQty;
            console.log('[Cart] Updated item quantity:', itemId, 'new qty:', newQty);
        }

        // Recalculate totals
        var subtotal = 0;
        items.forEach(function (it) {
            subtotal += Number(it.unitPrice || 0) * Number(it.quantity || 1);
        });

        var shipping = 0;
        var total = subtotal + shipping;

        setText(subtotalEl, fmt(subtotal));
        setText(totalEl, fmt(total));

        console.log('[Cart] Updated totals - Subtotal:', subtotal, 'Total:', total);
    }

    // Sync quantity change to server (debounced)
    function scheduleSyncQuantity(itemId, qty) {
        clearTimeout(syncTimer);
        syncTimer = setTimeout(function () {
            syncQuantity(itemId, qty);
        }, 500);
    }

    function syncQuantity(itemId, qty) {
        var gateway = getGateway();
        var updateUrl = cartEndpoint.replace(/\/$/, '') + '/items/' + itemId;

        console.log('[Cart] Syncing quantity for item', itemId, ':', qty);

        gateway.put(updateUrl, {Quantity: qty})
            .then(function (res) {
                console.log('[Cart] Quantity updated:', res);
                if (res && (res.success || res.Success) && (res.item || res.Item)) {
                    var updatedItem = res.item || res.Item;
                    var idx = items.findIndex(x => x.id == itemId);
                    if (idx !== -1) {
                        items[idx] = {
                            id: updatedItem.id || updatedItem.Id,
                            productId: updatedItem.productId || updatedItem.ProductId,
                            productName: updatedItem.productName || updatedItem.ProductName,
                            productImageUrl: updatedItem.productImageUrl || updatedItem.ProductImageUrl,
                            unitPrice: Number(updatedItem.unitPrice || updatedItem.UnitPrice || 0),
                            quantity: Number(updatedItem.quantity || updatedItem.Quantity || 1),
                            totalPrice: Number(updatedItem.totalPrice || updatedItem.TotalPrice || 0)
                        };
                    }
                }
            })
            .catch(function (e) {
                console.error('[Cart] Failed to update quantity:', e);
                showMessage('Failed to update: ' + (e.message || e), 'danger');
                loadCart(); // Reload to sync
            });
    }

    // Remove item from cart
    function removeItem(itemId) {
        var gateway = getGateway();
        var removeUrl = cartEndpoint.replace(/\/$/, '') + '/items/' + itemId;

        console.log('[Cart] Removing item:', itemId);

        // Find the item before removing for better UX
        var removedItem = items.find(x => x.id == itemId);
        var removedItemName = removedItem ? (removedItem.productName || 'Товар') : 'Товар';

        // Optimistically remove from UI with animation
        var itemDiv = itemsContainer.querySelector('.cart-item[data-item-id="' + itemId + '"]');
        if (itemDiv) {
            itemDiv.style.opacity = '0.5';
            itemDiv.style.transform = 'scale(0.95)';
            itemDiv.style.transition = 'all 0.2s ease';
        }

        items = items.filter(x => x.id != itemId);

        setTimeout(function () {
            renderCart();
        }, 200);

        gateway.delete(removeUrl)
            .then(function (res) {
                console.log('[Cart] Item removed successfully:', res);
                showMessage(removedItemName + ' удален из корзины', 'success');
            })
            .catch(function (e) {
                console.error('[Cart] Failed to remove item:', e);
                showMessage('Ошибка удаления: ' + (e.message || e), 'danger');
                loadCart(); // Reload to sync state
            });
    }

    // Clear entire cart
    function clearCart() {
        if (!items || items.length === 0) {
            showMessage('Корзина уже пуста', 'info');
            return;
        }

        var itemCount = items.length;
        var confirmMsg = 'Вы уверены, что хотите удалить все товары (' + itemCount + ') из корзины?';

        if (!confirm(confirmMsg)) return;

        var gateway = getGateway();

        console.log('[Cart] Clearing cart with', itemCount, 'items');

        // Show loading state
        show(skeletonsBox);
        hide(contentBox);

        gateway.delete(cartEndpoint)
            .then(function (res) {
                console.log('[Cart] Cart cleared successfully:', res);
                items = [];
                renderCart();
                showMessage('Корзина очищена. Удалено товаров: ' + itemCount, 'success');
            })
            .catch(function (e) {
                console.error('[Cart] Failed to clear cart:', e);
                showMessage('Ошибка очистки корзины: ' + (e.message || e), 'danger');
                hide(skeletonsBox);
                show(contentBox);
            });
    }

    // Checkout
    function checkout() {
        if (!items || items.length === 0) {
            showMessage('Корзина пуста. Добавьте товары для оформления заказа.', 'warning');
            return;
        }

        // Calculate total
        var subtotal = 0;
        items.forEach(function (it) {
            subtotal += Number(it.unitPrice || 0) * Number(it.quantity || 1);
        });

        if (subtotal <= 0) {
            showMessage('Сумма заказа должна быть больше нуля', 'warning');
            return;
        }

        console.log('[Cart] Proceeding to checkout with', items.length, 'items, total:', subtotal);

        // Disable checkout button to prevent double-click
        if (checkoutBtn) {
            checkoutBtn.disabled = true;
            checkoutBtn.innerHTML = '<span>⏳</span><span>Загрузка...</span>';
        }

        showMessage('Переход к оформлению заказа...', 'info');

        // Redirect to checkout page
        setTimeout(function () {
            window.location.href = checkoutUrl;
        }, 800);
    }

    // Load cart from server
    function loadCart() {
        console.log('[Cart] Loading cart from:', cartEndpoint);

        show(skeletonsBox);
        hide(emptyBox);
        hide(contentBox);
        hide(errorBox);

        var gateway = getGateway();

        gateway.get(cartEndpoint)
            .then(function (res) {
                console.log('[Cart] Cart loaded successfully - RAW RESPONSE:', res);
                console.log('[Cart] Response type:', typeof res);
                console.log('[Cart] Response is array:', Array.isArray(res));
                console.log('[Cart] Response keys:', res ? Object.keys(res) : 'null');

                var remote = [];

                // Handle different response formats
                if (res && res.success) {
                    console.log('[Cart] Response has success=true');
                    if (Array.isArray(res.items)) {
                        console.log('[Cart] Using res.items, length:', res.items.length);
                        remote = res.items;
                    } else if (Array.isArray(res.data)) {
                        console.log('[Cart] Using res.data, length:', res.data.length);
                        remote = res.data;
                    } else if (res.cart && Array.isArray(res.cart.items)) {
                        console.log('[Cart] Using res.cart.items, length:', res.cart.items.length);
                        remote = res.cart.items;
                    } else {
                        console.warn('[Cart] res.success=true but no items array found!');
                    }
                } else if (Array.isArray(res)) {
                    console.log('[Cart] Response is array directly, length:', res.length);
                    remote = res;
                } else if (res && res.data && Array.isArray(res.data)) {
                    console.log('[Cart] Using res.data, length:', res.data.length);
                    remote = res.data;
                } else if (res && res.cart && Array.isArray(res.cart.items)) {
                    console.log('[Cart] Using res.cart.items, length:', res.cart.items.length);
                    remote = res.cart.items;
                } else if (res && Array.isArray(res.items)) {
                    console.log('[Cart] Using res.items, length:', res.items.length);
                    remote = res.items;
                } else {
                    console.warn('[Cart] Could not find items array in response!');
                }

                console.log('[Cart] Remote items array:', remote);
                console.log('[Cart] Remote items length:', remote.length);

                items = remote.map(function (it) {
                    console.log('[Cart] Processing item:', it);
                    return {
                        id: it.id || it.Id || it.cartItemId || it.CartItemId,
                        productId: it.productId || it.ProductId || it.dishId || it.DishId,
                        productName: it.productName || it.ProductName || it.dishName || it.DishName || it.name || it.Name || 'Блюдо',
                        productImageUrl: it.productImageUrl || it.ProductImageUrl || it.dishImageUrl || it.DishImageUrl || it.imageUrl || it.ImageUrl || it.image || it.Image || '/images/Default.png',
                        unitPrice: Number(it.unitPrice || it.UnitPrice || it.price || it.Price || 0),
                        quantity: Number(it.quantity || it.Quantity || 1),
                        totalPrice: Number(it.totalPrice || it.TotalPrice || it.total || it.Total || 0)
                    };
                });

                console.log('[Cart] Processed items:', items);
                console.log('[Cart] Processed items length:', items.length);
                renderCart();
            })
            .catch(function (e) {
                console.error('[Cart] Failed to load cart:', e);
                hide(skeletonsBox);

                if (e && e.status === 401) {
                    showMessage('Пожалуйста, войдите в систему чтобы просмотреть корзину', 'info');
                    show(emptyBox);
                } else if (e && e.status === 404) {
                    console.log('[Cart] Cart is empty (404)');
                    items = [];
                    renderCart();
                } else {
                    showMessage('Не удалось загрузить корзину: ' + (e.message || e), 'danger');
                    show(emptyBox);
                }
            });
    }

    // Event Listeners
    document.addEventListener('DOMContentLoaded', function () {
        console.log('[Cart] DOM loaded');

        // Check if user is logged in
        var token = localStorage.getItem('jwtToken') || localStorage.getItem('jwt');
        if (!token) {
            console.warn('[Cart] No JWT token found');
            hide(skeletonsBox);
            show(emptyBox);
            showMessage('Пожалуйста, войдите в систему', 'info');
            return;
        }

        loadCart();

        // Item actions (increase, decrease, remove)
        if (itemsContainer) {
            itemsContainer.addEventListener('click', function (e) {
                var btn = e.target.closest('[data-action]');
                if (!btn) return;

                var action = btn.getAttribute('data-action');
                var itemId = btn.getAttribute('data-item-id');

                if (!itemId) {
                    console.warn('[Cart] No item ID found on button');
                    return;
                }

                e.preventDefault();
                e.stopPropagation();

                console.log('[Cart] Button clicked - Action:', action, 'ItemID:', itemId);

                if (action === 'remove') {
                    if (confirm('Удалить этот товар из корзины?')) {
                        removeItem(itemId);
                    }
                    return;
                }

                if (action === 'increase' || action === 'decrease') {
                    var input = itemsContainer.querySelector('input[data-role="quantity"][data-item-id="' + itemId + '"]');
                    if (!input) {
                        console.error('[Cart] Quantity input not found for item:', itemId);
                        return;
                    }

                    var currentQty = parseInt(input.value, 10) || 1;
                    var newQty = action === 'increase' ? currentQty + 1 : Math.max(1, currentQty - 1);

                    if (newQty < 1) newQty = 1;
                    if (newQty > 99) newQty = 99; // Max quantity limit

                    console.log('[Cart] Quantity change:', currentQty, '->', newQty);

                    input.value = newQty;
                    updateItemTotal(itemId, newQty);
                    scheduleSyncQuantity(itemId, newQty);
                }
            });
        }

        // Checkout button
        if (checkoutBtn) {
            checkoutBtn.addEventListener('click', function (e) {
                e.preventDefault();
                checkout();
            });
        }

        // Clear cart button
        if (clearCartBtn) {
            clearCartBtn.addEventListener('click', function (e) {
                e.preventDefault();
                clearCart();
            });
        }

        // Clear cart button (bottom)
        var clearCartBtnBottom = document.getElementById('clear-cart-btn-bottom');
        if (clearCartBtnBottom) {
            clearCartBtnBottom.addEventListener('click', function (e) {
                e.preventDefault();
                clearCart();
            });
        }

        // Apply promo code
        if (applyPromoBtn) {
            applyPromoBtn.addEventListener('click', function (e) {
                e.preventDefault();
                var promoInput = document.getElementById('promo-code');
                var code = promoInput ? promoInput.value.trim() : '';

                if (!code) {
                    showMessage('Введите промокод', 'warning');
                    return;
                }

                showMessage('Функция промокодов пока не реализована', 'info');
                // TODO: Implement promo code logic
            });
        }
    });
})();
