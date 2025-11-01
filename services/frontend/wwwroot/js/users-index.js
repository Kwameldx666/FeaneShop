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

    function toArray(payload) {
        if (Array.isArray(payload)) {
            return payload;
        }
        if (payload && Array.isArray(payload.items)) {
            return payload.items;
        }
        return [];
    }

    function firstDefined(source, keys) {
        if (!source) {
            return undefined;
        }
        for (var i = 0; i < keys.length; i += 1) {
            var key = keys[i];
            if (source[key] !== undefined && source[key] !== null) {
                return source[key];
            }
            var alt = key.charAt(0).toLowerCase() + key.slice(1);
            if (source[alt] !== undefined && source[alt] !== null) {
                return source[alt];
            }
        }
        return undefined;
    }

    function canonicalRole(role) {
        if (role === null || role === undefined) {
            return 'User';
        }
        if (typeof role === 'number') {
            switch (role) {
                case 1:
                    return 'User';
                case 2:
                    return 'Moderator';
                case 3:
                    return 'Admin';
                case 4:
                    return 'VIP';
                default:
                    return 'User';
            }
        }

        var normalized = String(role).toLowerCase();
        switch (normalized) {
            case '0':
            case 'none':
                return 'User';
            case '1':
            case 'user':
            case 'member':
                return 'User';
            case '2':
            case 'moderator':
            case 'manager':
                return 'Moderator';
            case '3':
            case 'admin':
            case 'administrator':
                return 'Admin';
            case '4':
            case 'vip':
                return 'VIP';
            default:
                return normalized.charAt(0).toUpperCase() + normalized.slice(1);
        }
    }

    function pickId(user) {
        return user == null ? '' : (firstDefined(user, ['Id', 'ID', 'UserId']) || '');
    }

    function pickRole(user) {
        if (!user) {
            return 'user';
        }
        var role = firstDefined(user, ['Role', 'Roles', 'PrimaryRole']);
        var roles = user && (user.roles || user.Roles || []);
        if (Array.isArray(role)) {
            roles = role;
            role = null;
        }
        if (!role && Array.isArray(roles) && roles.length) {
            role = roles[0];
        }
        if (!role && typeof roles === 'string') {
            role = roles.split(/[;,\s]+/).filter(Boolean)[0];
        }
        if (!role) {
            role = 'user';
        }
        return canonicalRole(role);
    }

    function pickStatus(user) {
        if (!user) {
            return 'Unknown';
        }
        var active = firstDefined(user, ['IsActive', 'Active']);
        if (typeof active !== 'boolean') {
            active = active != null ? Boolean(active) : null;
        }
        if (active === false) {
            return 'Inactive';
        }
        var status = firstDefined(user, ['Status', 'State']);
        if (typeof status === 'number') {
            if (status === 0) {
                status = 'Inactive';
            } else if (status === 1) {
                status = 'Active';
            } else {
                status = String(status);
            }
        }
        if (status) {
            return String(status);
        }
        return active === true ? 'Active' : 'Unknown';
    }

    function formatStateChip(status) {
        var normalized = String(status || '').toLowerCase();
        var classes = 'state-chip';
        if (normalized === 'inactive' || normalized === 'disabled' || normalized === 'blocked') {
            classes += ' inactive';
        }
        return '<span class="' + classes + '">' + escapeHtml(status || 'Unknown') + '</span>';
    }

    function formatRoleBadge(role) {
        var canonical = canonicalRole(role);
        return '<span class="role-badge" data-role="' + escapeHtml(canonical.toLowerCase()) + '">' + escapeHtml(canonical) + '</span>';
    }

    function buildRow(user) {
        var id = pickId(user);
        var username = user.username || user.userName || user.name || user.fullName || 'User';
        var email = user.email || user.Email || 'N/A';
        var role = pickRole(user);
        var status = pickStatus(user);

        var row = document.createElement('tr');
        row.dataset.id = id;
        row.innerHTML = '' +
            '<td>' + escapeHtml(username) + ' ' + formatStateChip(status) + '</td>' +
            '<td>' + escapeHtml(email) + '</td>' +
            '<td>' + formatRoleBadge(role) + '</td>' +
            '<td>' +
            '  <div class="actions">' +
            '    <button type="button" class="btn btn-edit" data-action="edit" data-id="' + escapeHtml(id) + '">Edit</button>' +
            '    <button type="button" class="btn btn-delete" data-action="delete" data-id="' + escapeHtml(id) + '">Delete</button>' +
            '  </div>' +
            '</td>';
        return row;
    }

    function normalizeRoleOptions(payload) {
        var roles = toArray(payload);
        if (!roles.length && payload && Array.isArray(payload.roles)) {
            roles = payload.roles;
        }
        if (!roles.length && payload && typeof payload === 'object') {
            roles = Object.keys(payload);
        }
        var result = roles.map(function (entry) {
            if (typeof entry === 'string') {
                return entry;
            }
            if (entry && typeof entry.name === 'string') {
                return entry.name;
            }
            if (entry && typeof entry.Role === 'string') {
                return entry.Role;
            }
            return null;
        }).filter(Boolean);
        var seen = Object.create(null);
        return result.map(canonicalRole).filter(function (role) {
            var key = role.toLowerCase();
            if (seen[key]) {
                return false;
            }
            seen[key] = true;
            return true;
        });
    }

    document.addEventListener('DOMContentLoaded', function () {
        if (!window.feaneGateway) {
            return;
        }

        var root = document.querySelector('main[data-users-endpoint]');
        if (!root) {
            return;
        }

        if (!document.body.classList.contains('users-page')) {
            document.body.classList.add('users-page');
        }

        var usersEndpoint = root.getAttribute('data-users-endpoint');
        var rolesEndpoint = root.getAttribute('data-roles-endpoint') || (usersEndpoint ? usersEndpoint.replace(/\/+$/, '') + '/roles' : '');

        var feedback = document.getElementById('users-feedback');
        var tableBody = document.getElementById('users-tbody');
        var editor = document.getElementById('user-editor');
        var form = document.getElementById('user-form');
        var inputId = document.getElementById('user-id');
        var inputUsername = document.getElementById('user-username');
        var inputEmail = document.getElementById('user-email');
        var selectRole = document.getElementById('user-role');
        var checkboxActive = document.getElementById('user-active');
        var cancelButton = document.getElementById('user-cancel');

        if (!usersEndpoint || !tableBody || !form) {
            return;
        }

        var currentUsers = [];
        var availableRoles = [];

        function showFeedback(message, type) {
            if (!feedback) {
                if (message) {
                    console.log(message);
                }
                return;
            }
            feedback.textContent = message || '';
            feedback.classList.remove('d-none', 'alert-success', 'alert-danger', 'alert-info');
            if (type === 'success') {
                feedback.classList.add('alert-success');
            } else if (type === 'info') {
                feedback.classList.add('alert-info');
            } else {
                feedback.classList.add('alert-danger');
            }
        }

        function hideFeedbackIfOk() {
            if (feedback) {
                feedback.classList.add('d-none');
            }
        }

        function renderUsers(users) {
            tableBody.innerHTML = '';
            if (!users.length) {
                var empty = document.createElement('tr');
                empty.innerHTML = '<td colspan="4" class="text-center text-muted">No users found.</td>';
                tableBody.appendChild(empty);
                return;
            }
            users.forEach(function (user) {
                tableBody.appendChild(buildRow(user));
            });
        }

        function fillRolesSelect(selectedRole) {
            if (!selectRole) {
                return;
            }
            selectRole.innerHTML = '';
            if (!availableRoles.length) {
                availableRoles = ['Admin', 'Moderator', 'VIP', 'User'];
            }
            var selectedCanonical = selectedRole ? canonicalRole(selectedRole) : '';
            availableRoles.forEach(function (role) {
                var canonical = canonicalRole(role);
                var option = document.createElement('option');
                option.value = canonical;
                option.textContent = canonical;
                if (selectedCanonical && canonical === selectedCanonical) {
                    option.selected = true;
                }
                selectRole.appendChild(option);
            });
        }

        function openEditor(user) {
            if (!editor) {
                return;
            }
            editor.classList.remove('d-none');
            inputId.value = pickId(user);
            inputUsername.value = user.username || user.userName || user.name || user.fullName || '';
            inputEmail.value = user.email || user.Email || '';
            var role = pickRole(user);
            fillRolesSelect(role);
            checkboxActive.checked = pickStatus(user).toLowerCase() !== 'inactive';
            window.scrollTo({top: editor.offsetTop - 20, behavior: 'smooth'});
        }

        function closeEditor() {
            if (!editor) {
                return;
            }
            editor.classList.add('d-none');
            form.reset();
            inputId.value = '';
        }

        function findUserById(id) {
            return currentUsers.find(function (user) {
                return String(pickId(user)) === String(id);
            });
        }

        function loadUsers() {
            showFeedback('Loading users from gateway…', 'info');
            return window.feaneGateway.get(usersEndpoint).then(function (response) {
                currentUsers = toArray(response);
                renderUsers(currentUsers);
                showFeedback('Users synchronised successfully.', 'success');
            }).catch(function (error) {
                currentUsers = [];
                renderUsers(currentUsers);
                showFeedback('Unable to load users: ' + (error && error.message || 'Unknown error'), 'error');
            });
        }

        function loadRoles() {
            if (!rolesEndpoint) {
                fillRolesSelect();
                return Promise.resolve();
            }
            return window.feaneGateway.get(rolesEndpoint).then(function (response) {
                availableRoles = normalizeRoleOptions(response);
                fillRolesSelect();
            }).catch(function () {
                availableRoles = [];
                fillRolesSelect();
            });
        }

        tableBody.addEventListener('click', function (event) {
            var button = event.target instanceof Element ? event.target.closest('button[data-action]') : null;
            if (!button) {
                return;
            }
            var id = button.getAttribute('data-id');
            if (!id) {
                showFeedback('User identifier is missing.', 'error');
                return;
            }
            var user = findUserById(id);
            if (!user) {
                showFeedback('User record not found in current list.', 'error');
                return;
            }
            var action = button.getAttribute('data-action');
            if (action === 'edit') {
                openEditor(user);
                hideFeedbackIfOk();
            } else if (action === 'delete') {
                if (!confirm('Are you sure you want to delete this user?')) {
                    return;
                }
                showFeedback('Deleting user…', 'info');
                window.feaneGateway.delete(usersEndpoint + '/' + encodeURIComponent(id)).then(function (resp) {
                    showFeedback((resp && resp.message) || 'User deleted.', 'success');
                    closeEditor();
                    loadUsers();
                }).catch(function (error) {
                    showFeedback('Failed to delete user: ' + (error && error.message || 'Unknown error'), 'error');
                });
            }
        });

        form.addEventListener('submit', function (event) {
            event.preventDefault();
            var id = inputId.value;
            if (!id) {
                showFeedback('User identifier is required.', 'error');
                return;
            }
            var existing = findUserById(id) || {};
            var payload = {
                Username: inputUsername.value.trim(),
                Email: inputEmail.value.trim(),
                Role: canonicalRole(selectRole.value),
                IsActive: checkboxActive.checked,
                Address: firstDefined(existing, ['Address']),
                PhoneNumber: firstDefined(existing, ['PhoneNumber'])
            };
            if (!payload.Username || !payload.Email) {
                showFeedback('Username and email are required.', 'error');
                return;
            }
            Object.keys(payload).forEach(function (key) {
                if (payload[key] === undefined || payload[key] === null) {
                    delete payload[key];
                }
            });
            showFeedback('Saving user…', 'info');
            window.feaneGateway.put(usersEndpoint + '/' + encodeURIComponent(id), payload).then(function (resp) {
                showFeedback((resp && resp.message) || 'User updated successfully.', 'success');
                closeEditor();
                loadUsers();
            }).catch(function (error) {
                showFeedback('Failed to update user: ' + (error && error.message || 'Unknown error'), 'error');
            });
        });

        if (cancelButton) {
            cancelButton.addEventListener('click', function () {
                closeEditor();
            });
        }

        loadRoles().then(loadUsers);
    });
})();
