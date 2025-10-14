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

    function renderNotifications(list, notifications) {
        list.innerHTML = '';

        if (!Array.isArray(notifications) || notifications.length === 0) {
            var empty = document.createElement('li');
            empty.className = 'list-group-item text-muted';
            empty.textContent = 'No notifications available.';
            list.appendChild(empty);
            return;
        }

        notifications.forEach(function (notification) {
            var item = document.createElement('li');
            item.className = 'list-group-item d-flex justify-content-between align-items-center';

            var message = notification.message || notification.title || 'Notification';
            var timeAgo = notification.timeAgo || notification.sentAt || notification.createdAt || '';

            item.innerHTML = '<span>' + message + '</span>' +
                '<span class="badge bg-secondary">' + (timeAgo || 'Just now') + '</span>';
            list.appendChild(item);
        });
    }

    document.addEventListener('DOMContentLoaded', function () {
        var main = document.querySelector('main[data-notifications-endpoint]');
        var list = document.getElementById('notifications-list');
        if (!main || !list || !window.feaneGateway) {
            return;
        }

        var feedback = document.getElementById('notifications-feedback');
        var endpoint = main.getAttribute('data-notifications-endpoint');
        if (!endpoint) {
            return;
        }

        showFeedback(feedback, 'Loading notifications…', 'info');
        window.feaneGateway.get(endpoint).then(function (response) {
            if (response && Array.isArray(response.items)) {
                renderNotifications(list, response.items);
            } else {
                renderNotifications(list, response);
            }
            showFeedback(feedback, 'Notifications fetched from gateway.', 'success');
        }).catch(function (error) {
            renderNotifications(list, []);
            showFeedback(feedback, 'Unable to load notifications: ' + error.message, 'error');
        });
    });
})();
