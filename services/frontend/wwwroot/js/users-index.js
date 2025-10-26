(function () {
    'use strict';

    var ROLE_OPTIONS = ['User', 'Moderator', 'Admin', 'VIP'];

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
        if (normalized === 'active' || normalized === 'true') {
            className = 'bg-success';
        } else if (normalized === 'inactive' || normalized === 'disabled' || normalized === 'false') {
            className = 'bg-danger';
        } else if (normalized === 'pending') {
            className = 'bg-warning text-dark';
        }
        var label = status || (normalized === 'bg-success' ? 'Active' : 'Unknown');
        return '<span class="badge ' + className + '">' + escapeHtml(label) + '</span>';
    }

    function normaliseUser(source) {
        if (!source || typeof source !== 'object') {
            return null;
        }
        var user = source.user || source;
        return {
            id: user.id || user.Id || user.authUserId || user.AuthUserId || '',
            authUserId: user.authUserId || user.AuthUserId || user.id || user.Id || '',
            username: user.username || user.Username || '',
            email: user.email || user.Email || '',
            role: user.role || user.Role || user.roles || user.Roles || 'User',
            isActive: user.isActive !== undefined ? Boolean(user.isActive) : Boolean(user.IsActive),
            phoneNumber: user.phoneNumber || user.PhoneNumber || '',
            address: user.address || user.Address || ''
        };
    }

    document.addEventListener('DOMContentLoaded', function () {
        var container = document.querySelector('main[data-users-endpoint]');
        var tableBody = document.getElementById('users-tbody');
        var feedback = document.getElementById('users-feedback');
        var editor = document.getElementById('user-editor');
        var form = document.getElementById('user-form');
        var cancelBtn = document.getElementById('user-cancel');
        var roleSelect = document.getElementById('user-role');
        var endpoint = container ? container.getAttribute('data-users-endpoint') : null;

        if (!container || !tableBody || !form || !endpoint || !window.feaneGateway) {
            return;
        }

        var usersCache = [];

        function fillRoles() {
            if (!roleSelect) {
                return;
            }
            roleSelect.innerHTML = '';
            ROLE_OPTIONS.forEach(function (role) {
                var option = document.createElement('option');
                option.value = role;
                option.textContent = role;
                roleSelect.appendChild(option);
            });
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

        function renderTable(users) {
            tableBody.innerHTML = '';

            if (!Array.isArray(users) || users.length === 0) {
                var empty = document.createElement('tr');
                empty.innerHTML = '<td colspan="4" class="text-center text-muted">No users found.</td>';
                tableBody.appendChild(empty);
                return;
            }

            users.forEach(function (raw) {
                var user = normaliseUser(raw);
                if (!user) {
                    return;
                }

                var row = document.createElement('tr');
                row.dataset.id = user.id;
                row.innerHTML =
                    '<td>' + escapeHtml(user.username) + '</td>' +
                    '<td>' + escapeHtml(user.email) + '</td>' +
                    '<td>' + escapeHtml(user.role) + '</td>' +
                    '<td class="text-end">' +
                        '<div class="d-inline-flex gap-2">' +
                            '<button type="button" class="btn btn-outline-secondary btn-sm" data-action="edit">Edit</button>' +
                            '<button type="button" class="btn btn-outline-danger btn-sm" data-action="delete">Delete</button>' +
                        '</div>' +
                    '</td>';
                tableBody.appendChild(row);
            });
        }

        function fetchUsers() {
            showFeedback('Loading users...', 'info');
            return window.feaneGateway.get(endpoint).then(function (response) {
                var data = Array.isArray(response?.items) ? response.items : response;
                usersCache = Array.isArray(data) ? data : [];
                renderTable(usersCache);
                showFeedback('Users loaded.', 'success');
            }).catch(function (error) {
                usersCache = [];
                renderTable(usersCache);
                showFeedback('Unable to load users: ' + error.message, 'error');
            });
        }

        function hideEditor() {
            if (editor) {
                editor.classList.add('d-none');
            }
            form.reset();
        }

        function populateEditor(user) {
            fillRoles();
            document.getElementById('user-id').value = user.id;
            document.getElementById('user-username').value = user.username;
            document.getElementById('user-email').value = user.email;
            document.getElementById('user-active').checked = !!user.isActive;
            roleSelect.value = ROLE_OPTIONS.indexOf(user.role) >= 0 ? user.role : 'User';
            editor.classList.remove('d-none');
        }

        tableBody.addEventListener('click', function (event) {
            var button = event.target.closest('button[data-action]');
            if (!button) {
                return;
            }

            var row = button.closest('tr[data-id]');
            var userId = row ? row.dataset.id : '';
            if (!userId) {
                showFeedback('User identifier is missing.', 'error');
                return;
            }

            if (button.dataset.action === 'edit') {
                window.feaneGateway.get(endpoint + '/' + encodeURIComponent(userId))
                    .then(function (response) {
                        if (!response || response.status === false) {
                            showFeedback((response && response.message) || 'Failed to load user.', 'error');
                            return;
                        }
                        var payload = normaliseUser(response.data || response);
                        if (!payload) {
                            showFeedback('User payload was not recognised.', 'error');
                            return;
                        }
                        populateEditor(payload);
                    })
                    .catch(function (error) {
                        showFeedback('Unable to load user: ' + error.message, 'error');
                    });
                return;
            }

            if (button.dataset.action === 'delete') {
                if (!confirm('Are you sure you want to delete this user?')) {
                    return;
                }
                window.feaneGateway.delete(endpoint + '/' + encodeURIComponent(userId))
                    .then(function (response) {
                        if (response && response.status === false) {
                            showFeedback(response.message || 'Failed to delete user.', 'error');
                            return;
                        }
                        showFeedback((response && response.message) || 'User deleted.', 'success');
                        hideEditor();
                        fetchUsers();
                    })
                    .catch(function (error) {
                        showFeedback('Unable to delete user: ' + error.message, 'error');
                    });
            }
        });

        form.addEventListener('submit', function (event) {
            event.preventDefault();
            var id = document.getElementById('user-id').value;
            if (!id) {
                showFeedback('User identifier is missing.', 'error');
                return;
            }

            var payload = {
                authUserId: id,
                username: document.getElementById('user-username').value.trim(),
                email: document.getElementById('user-email').value.trim(),
                role: roleSelect.value,
                isActive: document.getElementById('user-active').checked
            };

            showFeedback('Saving user...', 'info');
            window.feaneGateway.put(endpoint + '/' + encodeURIComponent(id), payload)
                .then(function (response) {
                    if (response && response.status === false) {
                        showFeedback(response.message || 'Failed to update user.', 'error');
                        return;
                    }
                    showFeedback((response && response.message) || 'User updated.', 'success');
                    hideEditor();
                    fetchUsers();
                })
                .catch(function (error) {
                    showFeedback('Unable to update user: ' + error.message, 'error');
                });
        });

        if (cancelBtn) {
            cancelBtn.addEventListener('click', function () {
                hideEditor();
            });
        }

        fetchUsers();
    });
})();
