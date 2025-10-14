(function () {
  var container = document.querySelector('.cart-container');
  var cartEndpoint = container ? container.getAttribute('data-cart-endpoint') : null;

  function formatCurrency(value) {
    return '$' + Number(value || 0).toFixed(2);
  }

  function renderCart(items) {
    var tableBody = document.querySelector('#cart-table tbody');
    var totalElement = document.getElementById('cart-total');
    var summary = document.getElementById('cart-summary');
    var alertBox = document.getElementById('cart-alert');

    if (!tableBody || !totalElement || !summary) {
      return;
    }

    tableBody.innerHTML = '';

    if (!items.length) {
      summary.classList.add('d-none');
      if (alertBox) {
        alertBox.textContent = 'Your cart is empty.';
        alertBox.classList.remove('d-none', 'alert-danger');
        alertBox.classList.add('alert-info');
      }
      return;
    }

    var total = 0;

    items.forEach(function (item, index) {
      var quantity = Number(item.quantity || 1);
      var price = Number(item.price || 0);
      var rowTotal = price * quantity;
      total += rowTotal;

      var row = document.createElement('tr');
      row.innerHTML = '
        <td>' + (item.name || 'Dish') + '</td>
        <td class="text-center">' + formatCurrency(price) + '</td>
        <td class="text-center">
          <div class="input-group input-group-sm justify-content-center">
            <button class="btn btn-outline-secondary" data-action="decrease" data-index="' + index + '">-</button>
            <input type="number" class="form-control text-center" data-role="quantity" data-index="' + index + '" value="' + quantity + '" min="1" />
            <button class="btn btn-outline-secondary" data-action="increase" data-index="' + index + '">+</button>
          </div>
        </td>
        <td class="text-center">' + formatCurrency(rowTotal) + '</td>
        <td class="text-end">
          <button class="btn btn-outline-danger btn-sm" data-action="remove" data-index="' + index + '">Remove</button>
        </td>';
      tableBody.appendChild(row);
    });

    totalElement.textContent = formatCurrency(total);
    summary.classList.remove('d-none');
  }

  function loadCart() {
    try {
      var raw = localStorage.getItem('cart');
      if (!raw) {
        return [];
      }
      var parsed = JSON.parse(raw);
      if (Array.isArray(parsed)) {
        return parsed;
      }
      return [];
    } catch (error) {
      console.error('Failed to parse cart', error);
      return [];
    }
  }

  function saveCart(items) {
    localStorage.setItem('cart', JSON.stringify(items));
  }

  function updateQuantity(items, index, newQuantity) {
    if (index < 0 || index >= items.length) {
      return items;
    }

    var quantity = Math.max(1, newQuantity);
    items[index].quantity = quantity;
    return items;
  }

  function normalizeGatewayItems(remoteItems) {
    if (!Array.isArray(remoteItems)) {
      return [];
    }

    return remoteItems.map(function (item) {
      return {
        name: item.name || item.dishName || 'Dish',
        price: item.price || item.unitPrice || 0,
        quantity: item.quantity || item.count || 1
      };
    });
  }

  function syncGateway(items) {
    if (!cartEndpoint || !window.feaneGateway) {
      return Promise.resolve();
    }

    return window.feaneGateway.post(cartEndpoint, { items: items }).catch(function (error) {
      console.warn('Failed to sync cart with gateway:', error);
    });
  }

  document.addEventListener('DOMContentLoaded', function () {
    var items = loadCart();

    if (cartEndpoint && window.feaneGateway) {
      window.feaneGateway.get(cartEndpoint).then(function (response) {
        var remoteItems = Array.isArray(response && response.items) ? response.items : response;
        var normalised = normalizeGatewayItems(remoteItems);
        if (normalised.length) {
          items = normalised;
          saveCart(items);
        }
        renderCart(items);
      }).catch(function (error) {
        console.warn('Failed to load cart from gateway:', error);
        renderCart(items);
      });
    } else {
      renderCart(items);
    }

    var table = document.getElementById('cart-table');
    if (table) {
      table.addEventListener('click', function (event) {
        var target = event.target;
        if (!(target instanceof HTMLElement)) {
          return;
        }

        var action = target.getAttribute('data-action');
        var index = Number(target.getAttribute('data-index'));
        if (Number.isNaN(index)) {
          return;
        }

        if (action === 'remove') {
          items.splice(index, 1);
          saveCart(items);
          renderCart(items);
          syncGateway(items);
          return;
        }

        if (action === 'increase' || action === 'decrease') {
          var current = Number(items[index].quantity || 1);
          if (action === 'increase') {
            current += 1;
          } else {
            current = Math.max(1, current - 1);
          }
          items = updateQuantity(items, index, current);
          saveCart(items);
          renderCart(items);
          syncGateway(items);
        }
      });

      table.addEventListener('change', function (event) {
        var target = event.target;
        if (!(target instanceof HTMLInputElement) || target.getAttribute('data-role') !== 'quantity') {
          return;
        }

        var index = Number(target.getAttribute('data-index'));
        var value = Number(target.value);
        if (!Number.isNaN(index) && value > 0) {
          items = updateQuantity(items, index, value);
          saveCart(items);
          renderCart(items);
          syncGateway(items);
        }
      });
    }
  });
})();
