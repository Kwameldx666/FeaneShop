(function () {
  const routes = {
    '/': '/Pages/Home/Menu.html',
    '/home': '/Pages/Home/Menu.html',
    '/home/index': '/Pages/Home/Menu.html',
    '/home/about': '/Pages/Home/About.html',
    '/home/menu': '/Pages/Home/Menu.html',
    '/home/privacy': '/Pages/Home/Privacy.html',
    '/menu': '/Pages/Home/Menu.html',
    '/about': '/Pages/Home/About.html',
    '/privacy': '/Pages/Home/Privacy.html',

    '/account': '/Pages/Account/ReservationHistory.html',
    '/account/index': '/Pages/Account/ReservationHistory.html',
    '/account/addresses': '/Pages/Account/Addresses.html',
    '/account/authentication': '/Pages/Account/Authentication.html',
    '/account/contacts': '/Pages/Account/Contacts.html',
    '/account/reservationhistory': '/Pages/Account/ReservationHistory.html',
    '/account/reservation-history': '/Pages/Account/ReservationHistory.html',
    '/account/reservations': '/Pages/Account/ReservationHistory.html',
    '/account/history': '/Pages/Account/ReservationHistory.html',
    '/account/login': '/Pages/Account/Authentication.html',
    '/account/signin': '/Pages/Account/Authentication.html',
    '/account/register': '/Pages/Account/Authentication.html',
    '/account/profile': '/Pages/Account/ReservationHistory.html',
    '/account/resetpassword': '/Pages/Account/ResetPassword.html',
    '/account/reset-password': '/Pages/Account/ResetPassword.html',
    '/account/logout': '/Pages/Account/Authentication.html',

    '/addresses': '/Pages/Account/Addresses.html',
    '/contacts': '/Pages/Account/Contacts.html',
    '/profile': '/Pages/Account/ReservationHistory.html',

    '/analytics': '/Pages/Analytics/Index.html',
    '/analytics/index': '/Pages/Analytics/Index.html',

    '/cart': '/Pages/Cart/Cart.html',
    '/cart/cart': '/Pages/Cart/Cart.html',

    '/dish': '/Pages/Dish/Index.html',
    '/dish/index': '/Pages/Dish/Index.html',
    '/dish/adddish': '/Pages/Dish/AddDish.html',
    '/dish/editdish': '/Pages/Dish/EditDish.html',

    '/error/404': '/Pages/Error/Error404.html',
    '/404': '/Pages/Error/Error404.html',

    '/notifications': '/Pages/Notifications/Index.html',
    '/notifications/index': '/Pages/Notifications/Index.html',
    '/notifications/filters': '/Pages/Notifications/Filters.html',

    '/payment/checkout': '/Pages/Payment/Checkout.html',
    '/payment/confirmation': '/Pages/Payment/Confirmation.html',

    '/reservation': '/Pages/Reservation/Book.html',
    '/reservation/book': '/Pages/Reservation/Book.html',

    '/user': '/Pages/User/Index.html',
    '/user/index': '/Pages/User/Index.html',

    '/weather': '/Pages/Weather/Index.html',
    '/weather/index': '/Pages/Weather/Index.html'
  };

  const defaultRoute = '/';
  const notFoundRoute = '/error/404';

  const appRoot = document.getElementById('app');
  const loader = document.getElementById('feane-loader');

  const preservedBodyAttributes = new Set(['data-feane-router']);

  function normalizePath(path) {
    try {
      const url = new URL(path, window.location.origin);
      let pathname = url.pathname.toLowerCase();

      pathname = pathname
        .replace(/\.html?$/i, '')         // /home/menu.html -> /home/menu
        .replace(/^\/pages(?=\/|$)/i, ''); // /pages/home/menu -> /home/menu

      if (pathname !== '/' && pathname.endsWith('/')) {
        pathname = pathname.slice(0, -1);
      }
      return pathname || '/';
    } catch {
      return '/';
    }
  }

  function resolveRoute(path) {
    const normalized = normalizePath(path);
    if (routes[normalized]) {
      return { path: normalized, file: routes[normalized], match: normalized };
    }

    // Try /path/index variant (keep the originally requested path for history)
    const indexKey = normalized === '/' ? '/' : `${normalized}/index`;
    if (routes[indexKey]) {
      return { path: normalized, file: routes[indexKey], match: indexKey };
    }

    return { path: notFoundRoute, file: routes[notFoundRoute], match: notFoundRoute };
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

  const managedHeadAttr = 'data-feane-router-head';

  function shouldAdoptHeadNode(node) {
    if (!(node instanceof Element)) {
      return false;
    }

    const tag = node.tagName.toLowerCase();
    if (tag === 'title' || tag === 'script') {
      return false;
    }
    if (tag === 'link') {
      const rel = (node.getAttribute('rel') || '').toLowerCase();
      return ['stylesheet', 'icon', 'shortcut icon', 'apple-touch-icon', 'preload', 'prefetch'].includes(rel);
    }
    if (tag === 'meta' || tag === 'style') {
      return true;
    }
    return false;
  }

  function adoptHeadNode(node, head) {
    const clone = node.cloneNode(true);
    clone.setAttribute(managedHeadAttr, 'true');
    head.appendChild(clone);
  }

  function updateHead(doc) {
    if (!doc) {
      return;
    }

    const title = doc.querySelector('title');
    if (title) {
      document.title = title.textContent;
    }

    const sourceHead = doc.head;
    if (!sourceHead) {
      return;
    }

    const head = document.head;
    head.querySelectorAll(`[${managedHeadAttr}]`).forEach(node => node.remove());

    Array.from(sourceHead.children)
      .filter(shouldAdoptHeadNode)
      .forEach(node => {
        const tag = node.tagName.toLowerCase();

        if (tag === 'link') {
          const rel = (node.getAttribute('rel') || '').toLowerCase();
          const href = node.getAttribute('href');
          if (href) {
            const absoluteHref = new URL(href, window.location.origin).href;
            const alreadyPresent = Array.from(head.querySelectorAll(`link[rel="${rel}"]`))
              .some(existing => existing.getAttribute('href') && new URL(existing.getAttribute('href'), window.location.origin).href === absoluteHref);
            if (alreadyPresent) {
              return;
            }
          }
        }

        if (tag === 'meta') {
          const name = node.getAttribute('name');
          const property = node.getAttribute('property');
          const httpEquiv = node.getAttribute('http-equiv');
          const selectorParts = [];
          if (name) {
            selectorParts.push(`meta[name="${name}"]`);
          }
          if (property) {
            selectorParts.push(`meta[property="${property}"]`);
          }
          if (httpEquiv) {
            selectorParts.push(`meta[http-equiv="${httpEquiv}"]`);
          }
          if (selectorParts.length) {
            head.querySelectorAll(selectorParts.join(',')).forEach(existing => {
              if (existing.hasAttribute(managedHeadAttr)) {
                existing.remove();
              }
            });
          }
        }

        adoptHeadNode(node, head);
      });
  }

  function render(htmlText, routeKey, matchedRoute) {
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
        detail: {
          route: routeKey,
          resolvedRoute: matchedRoute,
          file: matchedRoute ? routes[matchedRoute] : null
        }
      }));

      // Re-trigger DOMContentLoaded listeners for legacy scripts
      const domContentLoadedEvent = new Event('DOMContentLoaded', { bubbles: false, cancelable: false });
      document.dispatchEvent(domContentLoadedEvent);
    });
  }

  function navigate(path, { replaceState = false } = {}) {
    const { path: resolvedPath, file, match } = resolveRoute(path);

    if (!file) {
      return Promise.resolve();
    }

    const urlPath = resolvedPath === defaultRoute ? '/' : resolvedPath;

    if (!replaceState) {
      history.pushState({ path: resolvedPath }, '', urlPath);
    } else {
      history.replaceState({ path: resolvedPath }, '', urlPath);
    }

    setLoading(true);

    return fetch(file, { cache: 'no-cache' })
      .then(response => {
        if (!response.ok) {
          throw new Error(`Failed to load ${file}`);
        }
        return response.text();
      })
      .then(html => render(html, resolvedPath, match))
      .catch(() => {
        if (resolvedPath !== notFoundRoute) {
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
