(function () {
  function loadPartial(element) {
    const url = element.getAttribute('data-include');
    if (!url) {
      return Promise.resolve();
    }

    return fetch(url)
      .then(function (response) {
        if (!response.ok) {
          throw new Error('Failed to load partial: ' + url);
        }
        return response.text();
      })
      .then(function (html) {
        element.innerHTML = html;
        if (url.includes('navbar')) {
          initializeNavbar(element);
        }
      })
      .catch(function (error) {
        console.error(error);
      });
  }

  function initializeNavbar(root) {
    const userRole = (localStorage.getItem('userRole') || '').toLowerCase();
    const adminButtons = root.querySelectorAll('[data-role]');
    adminButtons.forEach(function (button) {
      const roles = button.getAttribute('data-role');
      if (!roles) {
        return;
      }

      const allowedRoles = roles.split(',').map(function (role) {
        return role.trim().toLowerCase();
      }).filter(Boolean);

      if (!userRole || allowedRoles.indexOf(userRole) === -1) {
        button.classList.add('d-none');
      } else {
        button.classList.remove('d-none');
      }
    });
  }

  document.addEventListener('DOMContentLoaded', function () {
    const elements = Array.prototype.slice.call(document.querySelectorAll('[data-include]'));
    Promise.all(elements.map(loadPartial)).then(function () {
      document.dispatchEvent(new CustomEvent('partials:loaded'));
    });
  });
})();
