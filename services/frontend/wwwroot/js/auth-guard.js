(function () {
  'use strict';

  function coalesce() {
    for (var i = 0; i < arguments.length; i += 1) {
      var value = arguments[i];
      if (value !== undefined && value !== null && value !== '') {
        return value;
      }
    }
    return null;
  }

  function extractRole(payload) {
    if (!payload) {
      return null;
    }

    return coalesce(
      payload.role,
      payload.Role,
      payload.user && (payload.user.role || payload.user.Role),
      payload.User && (payload.User.role || payload.User.Role),
      payload.data && (payload.data.role || payload.data.Role)
    );
  }

  function redirectTo(url) {
    if (!url) {
      return;
    }

    window.location.replace(url);
  }

  document.addEventListener('DOMContentLoaded', function () {
    var body = document.body;
    if (!body || body.getAttribute('data-require-auth') !== 'true') {
      return;
    }

    if (!window.feaneGateway) {
      redirectTo(body.getAttribute('data-login-url') || '/account/authentication');
      return;
    }

    var endpoint = body.getAttribute('data-auth-check-endpoint') || '/api/auth/profile';
    var loginUrl = body.getAttribute('data-login-url');
    if (!loginUrl) {
      var redir = encodeURIComponent(window.location.pathname + window.location.search + window.location.hash);
      loginUrl = '/account/authentication?redir=' + redir;
    }

    window.feaneGateway.get(endpoint).then(function (data) {
      if (!data || data.status === 401 || data.status === 403) {
        throw new Error('Unauthorized');
      }

      var role = extractRole(data);
      if (typeof window.feaneSetUserRole === 'function') {
        window.feaneSetUserRole(role || 'authenticated');
      }
    }).catch(function () {
      redirectTo(loginUrl);
    });
  });
})();
