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

    function createStatusBadge(status) {
        var normalized = String(status || 'unknown').toLowerCase();
        var className = 'bg-secondary';
        if (normalized === 'active') {
            className = 'bg-success';
        } else if (normalized === 'disabled' || normalized === 'blocked') {
            className = 'bg-danger';
        } else if (normalized === 'pending') {
            className = 'bg-warning text-dark';
        }
        return '<span class="badge ' + className + '">' + escapeHtml(status || 'Unknown') + '</span>';
    }

    document.addEventListener('DOMContentLoaded', function () {
        var container = document.querySelector('main[data-users-endpoint]');
        var tableBody = document.getElementById('users-table-body');
        if (!container || !tableBody || !window.feaneGateway) {
            return;
        }

        var feedback = document.getElementById('users-feedback');
        var endpoint = container.getAttribute('data-users-endpoint');
        if (!endpoint) {
            return;
        }

        function showFeedback(message, type) {
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

        function render(users) {
            tableBody.innerHTML = '';

            if (!Array.isArray(users) || users.length === 0) {
                var empty = document.createElement('tr');
                empty.innerHTML = '<td colspan="4" class="text-center text-muted">No users found.</td>';
                tableBody.appendChild(empty);
                return;
            }

            users.forEach(function (user) {
                var row = document.createElement('tr');
                var name = user.name || user.fullName || user.username || 'User';
                var email = user.email || 'N/A';
                var role = user.role || user.roles || 'Member';
                var status = user.status || user.state || 'Active';

                row.innerHTML = '<td>' + escapeHtml(name) + '</td>' +
                    '<td>' + escapeHtml(email) + '</td>' +
                    '<td>' + escapeHtml(Array.isArray(role) ? role.join(', ') : role) + '</td>' +
                    '<td>' + createStatusBadge(status) + '</td>';
                tableBody.appendChild(row);
            });
        }

        showFeedback('Loading users from gateway…', 'info');
        window.feaneGateway.get(endpoint).then(function (response) {
            if (response && Array.isArray(response.items)) {
                render(response.items);
            } else {
                render(response);
            }
            showFeedback('Users synchronised successfully.', 'success');
        }).catch(function (error) {
            render([]);
            showFeedback('Unable to load users: ' + error.message, 'error');
        });
    });
})();
