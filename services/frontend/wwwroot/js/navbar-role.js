(function () {
  'use strict';

  function normalizeRole(value) {
    return (value || '')
      .toString()
      .toLowerCase()
      .split(/[;,\\s]+/)
      .map(function (part) { return part.trim(); })
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

    var normalized = normalizeRole(tokens.join(' '));

    if (!normalized.length) {
      return ['guest'];
    }

    var set = new Set(normalized);
    set.add('authenticated');
    set.add('auth');

    return Array.from(set);
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
    } else {
      element.classList.add('d-none');
      element.setAttribute('aria-hidden', 'true');
    }
  }

  function handleRoleAwareElements(root) {
    if (!root) {
      return;
    }

    var currentRoles = collectRoles();
    var nodes = root.querySelectorAll('[data-role]');

    Array.prototype.forEach.call(nodes, function (node) {
      toggleForRole(node, shouldShow(node, currentRoles));
    });
  }

  function setUserRole(role) {
    var normalized = role ? String(role).toLowerCase() : '';

    var storeValue = normalized || 'guest';

    try {
      localStorage.setItem('userRole', storeValue);
      sessionStorage.setItem('userRole', storeValue);
    } catch (_) { }

    window.__FEANE_USER_ROLE__ = storeValue;
    syncRoleArtifacts(storeValue === 'guest' ? '' : storeValue);
    handleRoleAwareElements(document);
  }

  window.feaneSetUserRole = setUserRole;

  document.addEventListener('DOMContentLoaded', function () {
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
})();

