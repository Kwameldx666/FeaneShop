(function () {
  const routes = {
    '/': '/Pages/Home/Index.html',
    '/home': '/Pages/Home/Index.html',
    '/home/index': '/Pages/Home/Index.html',
    '/home/about': '/Pages/Home/About.html',
    '/home/menu': '/Pages/Home/Menu.html',
    '/home/privacy': '/Pages/Home/Privacy.html',

    '/account/addresses': '/Pages/Account/Addresses.html',
    '/account/authentication': '/Pages/Account/Authentication.html',
    '/account/contacts': '/Pages/Account/Contacts.html',
    '/account/profile': '/Pages/Account/Profile.html',
    '/account/resetpassword': '/Pages/Account/ResetPassword.html',

    '/analytics': '/Pages/Analytics/Index.html',
    '/analytics/index': '/Pages/Analytics/Index.html',

    '/cart': '/Pages/Cart/Cart.html',
    '/cart/cart': '/Pages/Cart/Cart.html',

    '/dish': '/Pages/Dish/Index.html',
    '/dish/index': '/Pages/Dish/Index.html',
    '/dish/adddish': '/Pages/Dish/AddDish.html',
    '/dish/editdish': '/Pages/Dish/EditDish.html',

    '/error/404': '/Pages/Error/Error404.html',

    '/notifications': '/Pages/Notifications/Index.html',
    '/notifications/index': '/Pages/Notifications/Index.html',
    '/notifications/filters': '/Pages/Notifications/Filters.html',

    '/payment/checkout': '/Pages/Payment/Checkout.html',
    '/payment/confirmation': '/Pages/Payment/Confirmation.html',

    '/reservation/book': '/Pages/Reservation/Book.html',

    '/user': '/Pages/User/Index.html',
    '/user/index': '/Pages/User/Index.html',

    '/weather': '/Pages/Weather/Index.html',
    '/weather/index': '/Pages/Weather/Index.html'
  };

  const defaultRoute = '/home/index';
  const notFoundRoute = '/error/404';

  const appRoot = document.getElementById('app');
  const loader = document.getElementById('feane-loader');

  const preservedBodyAttributes = new Set(['data-feane-router']);

  function normalizePath(path) {
    try {
      const url = new URL(path, window.location.origin);
      let pathname = url.pathname.toLowerCase();
      if (pathname !== '/' && pathname.endsWith('/')) {
        pathname = pathname.slice(0, -1);
      }
      return pathname || '/';
    } catch (error) {
      return '/';
    }
  }

  function resolveRoute(path) {
    const normalized = normalizePath(path);
    if (routes[normalized]) {
      return { key: normalized, file: routes[normalized] };
    }

    // Try /path/index variant
    const withIndex = normalized === '/' ? defaultRoute : `${normalized}/index`;
    if (routes[withIndex]) {
      return { key: withIndex, file: routes[withIndex] };
    }

    return { key: notFoundRoute, file: routes[notFoundRoute] };
  }

  function setLoading(isLoading) {
    if (!loader) {
      return;
    }

    if (isLoading) {
      loader.classList.remove('hidden');
      loader.setAttribute('aria-hidden', 'false');
    } else {
      loader.classList.add('hidden');
      loader.setAttribute('aria-hidden', 'true');
    }
  }

  function clearBodyAttributes() {
    const body = document.body;
    Array.from(body.attributes).forEach(attr => {
      if (attr.name === 'class') {
        return;
      }
      if (preservedBodyAttributes.has(attr.name)) {
        return;
      }
      body.removeAttribute(attr.name);
    });
  }

  function applyBodyAttributes(sourceBody) {
    const body = document.body;

    clearBodyAttributes();

    if (sourceBody) {
      Array.from(sourceBody.attributes).forEach(attr => {
        if (attr.name === 'class') {
          return;
        }
        body.setAttribute(attr.name, attr.value);
      });

      const pageClass = sourceBody.className ? sourceBody.className.trim() : '';
      body.className = ['feane-app', pageClass].filter(Boolean).join(' ');
    } else {
      body.className = 'feane-app';
    }
  }

  function executeScripts(container) {
    const scripts = Array.from(container.querySelectorAll('script'));
    const execution = scripts.reduce((promiseChain, script) => {
      return promiseChain.then(() => new Promise((resolve, reject) => {
        const replacement = document.createElement('script');
        Array.from(script.attributes).forEach(attr => {
          replacement.setAttribute(attr.name, attr.value);
        });

        if (script.textContent) {
          replacement.textContent = script.textContent;
        }

        replacement.onload = () => resolve();
        replacement.onerror = () => resolve(); // swallow individual script errors

        script.replaceWith(replacement);

        if (!replacement.src) {
          resolve();
        }
      }));
    }, Promise.resolve());

    return execution;
  }

  function updateHead(doc) {
    if (!doc) {
      return;
    }

    const title = doc.querySelector('title');
    if (title) {
      document.title = title.textContent;
    }
  }

  function render(htmlText, routeKey) {
    const parser = new DOMParser();
    const doc = parser.parseFromString(htmlText, 'text/html');
    const body = doc.body;

    updateHead(doc);
    applyBodyAttributes(body);

    if (appRoot) {
      appRoot.innerHTML = body ? body.innerHTML : htmlText;
    }

    return executeScripts(appRoot || document.body).then(() => {
      document.dispatchEvent(new CustomEvent('feane:page-ready', {
        detail: { route: routeKey }
      }));

      // Re-trigger DOMContentLoaded listeners for legacy scripts
      const domContentLoadedEvent = new Event('DOMContentLoaded', { bubbles: false, cancelable: false });
      document.dispatchEvent(domContentLoadedEvent);
    });
  }

  function navigate(path, { replaceState = false } = {}) {
    const { key, file } = resolveRoute(path);

    if (!file) {
      return Promise.resolve();
    }

    if (!replaceState) {
      history.pushState({ path: key }, '', key === defaultRoute ? '/' : key);
    } else {
      history.replaceState({ path: key }, '', key === defaultRoute ? '/' : key);
    }

    setLoading(true);

    return fetch(file, { cache: 'no-cache' })
      .then(response => {
        if (!response.ok) {
          throw new Error(`Failed to load ${file}`);
        }
        return response.text();
      })
      .then(html => render(html, key))
      .catch(() => {
        if (key !== notFoundRoute) {
          return navigate(notFoundRoute, { replaceState: true });
        }
        return Promise.resolve();
      })
      .finally(() => setLoading(false));
  }

  function handleLinkClick(event) {
    if (event.defaultPrevented) {
      return;
    }

    const anchor = event.target.closest('a[href]');
    if (!anchor) {
      return;
    }

    const href = anchor.getAttribute('href');
    if (!href || href.startsWith('#') || href.startsWith('mailto:') || href.startsWith('tel:')) {
      return;
    }

    const url = new URL(href, window.location.origin);
    if (url.origin !== window.location.origin) {
      return;
    }

    if (anchor.hasAttribute('download') || anchor.getAttribute('target') === '_blank') {
      return;
    }

    event.preventDefault();
    navigate(url.pathname + url.search + url.hash);
  }

  function handleFormSubmit(event) {
    const form = event.target;
    if (!(form instanceof HTMLFormElement)) {
      return;
    }

    const action = form.getAttribute('action');
    if (!action) {
      return;
    }

    if (action.startsWith('http') && !action.startsWith(window.location.origin)) {
      return;
    }

    if (action.startsWith('/')) {
      form.setAttribute('action', action.replace(/\.html$/i, ''));
    }
  }

  function init() {
    document.addEventListener('click', handleLinkClick);
    document.addEventListener('submit', handleFormSubmit, true);

    window.addEventListener('popstate', (event) => {
      const path = event.state?.path || normalizePath(window.location.pathname);
      navigate(path, { replaceState: true });
    });

    const initialPath = normalizePath(window.location.pathname);
    navigate(initialPath, { replaceState: true });
  }

  if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', init, { once: true });
  } else {
    init();
  }
})();
