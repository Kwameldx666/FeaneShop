(function () {
    'use strict';

    function normalizeRole(value) {
        return (value || '')
            .toString()
            .toLowerCase()
            .split(/[;,\\s]+/)
            .map(function (part) {
                return part.trim();
            })
            .filter(Boolean);
    }

    function syncRoleArtifacts(role) {
        try {
            var meta = document.querySelector('meta[name="feane-user-role"]');
            if (!meta) {
                meta = document.createElement('meta');
                meta.setAttribute('name', 'feane-user-role');
                document.head.appendChild(meta);
            }
            meta.setAttribute('content', role || '');
        } catch (_) {
        }

        if (document && document.body && document.body.setAttribute) {
            if (role) {
                document.body.setAttribute('data-user-role', role);
            } else {
                document.body.removeAttribute('data-user-role');
            }
        }
    }

    function decodeRolesFromJwt(token) {
        if (!token) {
            return [];
        }

        var parts = token.split('.');
        if (parts.length < 2) {
            return [];
        }

        var payload = parts[1].replace(/-/g, '+').replace(/_/g, '/');
        while (payload.length % 4 !== 0) {
            payload += '=';
        }

        try {
            var json = atob(payload);
            var data = JSON.parse(json);
            var claim = data['http://schemas.microsoft.com/ws/2008/06/identity/claims/role']
                || data.role
                || data.roles;

            if (!claim) {
                return [];
            }

            if (Array.isArray(claim)) {
                return claim.map(function (value) {
                    return String(value);
                }).filter(Boolean);
            }

            if (typeof claim === 'string') {
                return claim.split(/[;,\\s]+/)
                    .map(function (segment) {
                        return segment.trim();
                    })
                    .filter(Boolean);
            }

            return [String(claim)];
        } catch (error) {
            console.warn('Failed to decode JWT role', error);
            return [];
        }
    }

    function expandRoles(baseRoles) {
        var set = new Set();

        for (var i = 0; i < baseRoles.length; i += 1) {
            var role = baseRoles[i];
            if (!role) {
                continue;
            }

            switch (role) {
                case 'administrator':
                case 'admin':
                    set.add('admin');
                    set.add('administrator');
                    set.add('moderator');
                    set.add('user');
                    break;
                case 'moderator':
                    set.add('moderator');
                    set.add('user');
                    break;
                case 'vip':
                    set.add('vip');
                    set.add('user');
                    break;
                case 'auth':
                case 'authenticated':
                    set.add('authenticated');
                    set.add('auth');
                    break;
                default:
                    set.add(role);
                    break;
            }
        }

        if (set.size > 1 && set.has('guest')) {
            set.delete('guest');
        }

        if (!set.size) {
            set.add('guest');
        }

        set.add('authenticated');
        set.add('auth');

        return Array.from(set);
    }

    function collectRoles() {
        var tokens = [];

        try {
            var meta = document.querySelector('meta[name="feane-user-role"]');
            if (meta && meta.content) {
                tokens.push(meta.content);
            }
        } catch (_) {
        }

        if (document && document.body && document.body.getAttribute) {
            var bodyRole = document.body.getAttribute('data-user-role');
            if (bodyRole) {
                tokens.push(bodyRole);
            }
        }

        try {
            var localRole = localStorage.getItem('userRole');
            if (localRole) {
                tokens.push(localRole);
            }
            var localJwt = localStorage.getItem('jwt');
            tokens = tokens.concat(decodeRolesFromJwt(localJwt));
        } catch (_) {
        }

        try {
            var sessionRole = sessionStorage.getItem('userRole');
            if (sessionRole) {
                tokens.push(sessionRole);
            }
            var sessionJwt = sessionStorage.getItem('jwt');
            tokens = tokens.concat(decodeRolesFromJwt(sessionJwt));
        } catch (_) {
        }

        if (window && window.__FEANE_USER_ROLE__) {
            tokens.push(window.__FEANE_USER_ROLE__);
        }

        var normalized = normalizeRole(tokens.join(' '));

        if (!normalized.length) {
            return ['guest', 'authenticated', 'auth'];
        }

        return expandRoles(normalized);
    }

    function shouldShow(element, currentRoles) {
        var raw = element.getAttribute('data-role');
        if (!raw) {
            return true;
        }

        var allowed = normalizeRole(raw);
        if (!allowed.length) {
            return true;
        }

        if (allowed.indexOf('*') !== -1) {
            return true;
        }

        for (var i = 0; i < allowed.length; i += 1) {
            var role = allowed[i];
            if (currentRoles.indexOf(role) !== -1) {
                return true;
            }
        }

        return false;
    }

    function toggleForRole(element, isAllowed) {
        if (!element) {
            return;
        }

        if (isAllowed) {
            element.classList.remove('d-none');
            element.removeAttribute('aria-hidden');
            if (element.style) {
                element.style.removeProperty('display');
                element.style.removeProperty('visibility');
                element.style.removeProperty('opacity');
            }
        } else {
            element.classList.add('d-none');
            element.setAttribute('aria-hidden', 'true');
            if (element.style && !element.classList.contains('d-none')) {
                element.style.display = 'none';
            }
        }
    }

    function handleRoleAwareElements(root) {
        if (!root) {
            return;
        }

        primeRoleFromJwt();

        var currentRoles = collectRoles();
        var nodes = root.querySelectorAll('[data-role]');

        try {
            console.debug('[navbar-role] roles:', currentRoles, 'nodes:', nodes.length);
        } catch (_) {
        }

        try {
            document.body.setAttribute('data-role-debug', currentRoles.join(','));
        } catch (_) {
        }

        Array.prototype.forEach.call(nodes, function (node) {
            toggleForRole(node, shouldShow(node, currentRoles));
        });

        if (currentRoles.indexOf('admin') !== -1) {
            var adminNodes = document.querySelectorAll('.feane-admin');
            Array.prototype.forEach.call(adminNodes, function (node) {
                node.classList.remove('d-none');
                node.removeAttribute('aria-hidden');
            });
        }
    }

    function setUserRole(role) {
        var normalized = role ? String(role).toLowerCase() : '';

        var storeValue = normalized || 'guest';

        try {
            localStorage.setItem('userRole', storeValue);
            sessionStorage.setItem('userRole', storeValue);
        } catch (_) {
        }

        window.__FEANE_USER_ROLE__ = storeValue;
        syncRoleArtifacts(storeValue === 'guest' ? '' : storeValue);
        try {
            console.debug('[navbar-role] setUserRole ->', storeValue);
        } catch (_) {
        }
        handleRoleAwareElements(document);
    }

    window.feaneSetUserRole = setUserRole;

    function selectPrimaryRole(roles) {
        if (!roles || !roles.length) {
            return null;
        }

        var lowered = roles.map(function (r) {
            return String(r || '').toLowerCase();
        });

        if (lowered.indexOf('admin') !== -1 || lowered.indexOf('administrator') !== -1) {
            return 'admin';
        }

        if (lowered.indexOf('moderator') !== -1) {
            return 'moderator';
        }

        if (lowered.indexOf('vip') !== -1) {
            return 'vip';
        }

        if (lowered.indexOf('user') !== -1) {
            return 'user';
        }

        if (lowered.indexOf('authenticated') !== -1) {
            return 'authenticated';
        }

        return lowered[0];
    }

    function primeRoleFromJwt() {
        var token = null;

        try {
            token = localStorage.getItem('jwt') || sessionStorage.getItem('jwt');
        } catch (_) {
        }

        if (!token) {
            return;
        }

        var decodedRoles = decodeRolesFromJwt(token);
        if (!decodedRoles.length) {
            return;
        }

        var primary = selectPrimaryRole(decodedRoles);
        if (!primary) {
            return;
        }

        try {
            var existing = localStorage.getItem('userRole');
            if (existing && existing.toLowerCase() === primary) {
                return;
            }
        } catch (_) {
        }

        setUserRole(primary);
    }

    document.addEventListener('DOMContentLoaded', function () {
        primeRoleFromJwt();
        handleRoleAwareElements(document);
    });

    document.addEventListener('partials:loaded', function () {
        handleRoleAwareElements(document);
    });

    document.addEventListener('feane:page-ready', function () {
        handleRoleAwareElements(document);
    });

    document.addEventListener('feane:user-role-changed', function (event) {
        if (event && event.detail && event.detail.role) {
            setUserRole(event.detail.role);
        } else {
            handleRoleAwareElements(document);
        }
    });

    window.addEventListener('storage', function (event) {
        if (event.key === 'userRole') {
            handleRoleAwareElements(document);
        }
    });

    window.feaneRefreshRoleViews = function () {
        handleRoleAwareElements(document);
    };

    handleRoleAwareElements(document);
})();
