(function () {
  'use strict';

  function normalizeRole(value) {
    return (value || '')
      .toString()
      .toLowerCase()
      .split(/[;,\s]+/)
      .filter(Boolean);
  }

  function syncRoleArtifacts(role) {
    try {
      var meta = document.querySelector('meta[name="feane-user-role"]');
      if (meta) {
        meta.setAttribute('content', role || '');
      }
    } catch (_) { }

    if (document && document.body && document.body.setAttribute) {
      if (role) {
        document.body.setAttribute('data-user-role', role);
      } else {
        document.body.removeAttribute('data-user-role');
      }
    }
  }

  function decodeRoleFromJwt(token) {
    if (!token) {
      return null;
    }

    var parts = token.split('.');
    if (parts.length < 2) {
      return null;
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
        return null;
      }

      if (Array.isArray(claim)) {
        return claim.join(' ');
      }

      return String(claim);
    } catch (error) {
      console.warn('Failed to decode JWT role', error);
      return null;
    }
  }

  function collectRoles() {
    var tokens = [];

    try {
      var meta = document.querySelector('meta[name="feane-user-role"]');
      if (meta && meta.content) {
        tokens.push(meta.content);
      }
    } catch (_) { }

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
      var decoded = decodeRoleFromJwt(localJwt);
      if (decoded) {
        tokens.push(decoded);
      }
    } catch (_) { }

    try {
      var sessionRole = sessionStorage.getItem('userRole');
      if (sessionRole) {
        tokens.push(sessionRole);
      }
      var sessionJwt = sessionStorage.getItem('jwt');
      var decodedSession = decodeRoleFromJwt(sessionJwt);
      if (decodedSession) {
        tokens.push(decodedSession);
      }
    } catch (_) { }

    if (window && window.__FEANE_USER_ROLE__) {
      tokens.push(window.__FEANE_USER_ROLE__);
    }

    return normalizeRole(tokens.join(' '));
  }

  function updateRoleButtons(root) {
    if (!root) {
      return;
    }

    var currentRoles = collectRoles();
    var buttons = root.querySelectorAll('.admin_button[data-role], .feane-admin[data-role]');

    if (!buttons.length) {
      return;
    }

    Array.prototype.forEach.call(buttons, function (button) {
      var allowedRoles = normalizeRole(button.getAttribute('data-role'));
      var isAllowed = currentRoles.length > 0 && allowedRoles.some(function (role) {
        return currentRoles.indexOf(role) !== -1;
      });

      if (isAllowed) {
        button.classList.remove('d-none');
        button.removeAttribute('aria-hidden');
      } else {
        button.classList.add('d-none');
        button.setAttribute('aria-hidden', 'true');
      }
    });
  }

  function handleScope(scope) {
    var containers = [];
    var selectors = ['.user_option', '.feane-actions'];

    if (scope) {
      if (scope.matches) {
        selectors.forEach(function (selector) {
          if (scope.matches(selector)) {
            containers.push(scope);
          }
        });
      }

      if (scope.querySelectorAll) {
        selectors.forEach(function (selector) {
          Array.prototype.forEach.call(scope.querySelectorAll(selector), function (element) {
            containers.push(element);
          });
        });
      }
    }

    if (!containers.length) {
      containers.push(document);
    }

    containers.forEach(function (container) {
      updateRoleButtons(container);
    });
  }

  function setUserRole(role) {
    var normalized = role ? String(role).toLowerCase() : '';

    try {
      if (normalized) {
        localStorage.setItem('userRole', normalized);
        sessionStorage.setItem('userRole', normalized);
      } else {
        localStorage.removeItem('userRole');
        sessionStorage.removeItem('userRole');
      }
    } catch (_) { }

    window.__FEANE_USER_ROLE__ = normalized || null;
    syncRoleArtifacts(normalized);
    handleScope(document);
  }

  window.feaneSetUserRole = setUserRole;

  document.addEventListener('DOMContentLoaded', function () {
    handleScope(document);
  });

  document.addEventListener('partials:loaded', function () {
    handleScope(document);
  });

  document.addEventListener('feane:page-ready', function () {
    handleScope(document);
  });

  document.addEventListener('feane:user-role-changed', function (event) {
    if (event && event.detail && event.detail.role) {
      setUserRole(event.detail.role);
    } else {
      handleScope(document);
    }
  });

  window.addEventListener('storage', function (event) {
    if (event.key === 'userRole') {
      handleScope(document);
    }
  });
})();
