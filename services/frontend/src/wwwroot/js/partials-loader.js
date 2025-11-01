(function () {
    'use strict';

    const loadedScripts = new Set();
    const loadedStyles = new Set();

    function normUrl(u) {
        try {
            return new URL(u, location.origin).href;
        } catch {
            return u;
        }
    }

    function copyAttrs(from, to, skip = []) {
        for (const a of from.attributes) if (!skip.includes(a.name)) to.setAttribute(a.name, a.value);
    }

    function ensureStyleLink(linkEl) {
        return new Promise((resolve, reject) => {
            const href = normUrl(linkEl.getAttribute('href') || '');
            if (!href) return resolve();                // пусто — пропускаем
            if (loadedStyles.has(href)) return resolve(); // уже подключали

            // если в <head> уже есть такой href — считаем загруженным
            const exists = Array.from(document.head.querySelectorAll('link[rel="stylesheet"]'))
                .some(l => normUrl(l.getAttribute('href') || '') === href);
            if (exists) {
                loadedStyles.add(href);
                return resolve();
            }

            const l = document.createElement('link');
            copyAttrs(linkEl, l, []);                  // переносим атрибуты
            l.rel = 'stylesheet';
            l.href = href;
            l.onload = () => {
                loadedStyles.add(href);
                resolve();
            };
            l.onerror = () => reject(new Error('CSS load failed: ' + href));
            document.head.appendChild(l);
        });
    }

    function ensureInlineStyle(styleEl) {
        const s = document.createElement('style');
        copyAttrs(styleEl, s, []);
        s.textContent = styleEl.textContent || '';
        document.head.appendChild(s);
        return Promise.resolve();
    }

    function runScriptTag(node) {
        return new Promise((resolve, reject) => {
            const src = node.getAttribute('src');
            if (src) {
                const href = normUrl(src);
                if (loadedScripts.has(href)) return resolve();
                const s = document.createElement('script');
                copyAttrs(node, s, ['src']);
                s.src = href;
                s.onload = () => {
                    loadedScripts.add(href);
                    resolve();
                };
                s.onerror = () => reject(new Error('Script load failed: ' + href));
                document.body.appendChild(s);
                return;
            }
            const s = document.createElement('script');
            copyAttrs(node, s, ['src']);
            s.textContent = node.textContent || '';
            document.body.appendChild(s);
            resolve();
        });
    }

    function chain(promises) {
        return promises.reduce((p, fn) => p.then(fn), Promise.resolve());
    }

    async function loadPartial(holder) {
        const url = holder.getAttribute('data-include');
        if (!url) return;

        const res = await fetch(url, {cache: 'no-cache', credentials: 'same-origin'});
        if (!res.ok) throw new Error('Include failed ' + res.status + ' ' + url);

        const html = await res.text();
        const tmp = document.createElement('div');
        tmp.innerHTML = html;

        // вытащим CSS и JS
        const links = Array.from(tmp.querySelectorAll('link[rel="stylesheet"]'));
        const styles = Array.from(tmp.querySelectorAll('style'));
        const scripts = Array.from(tmp.querySelectorAll('script'));

        links.forEach(n => n.remove());
        styles.forEach(n => n.remove());
        scripts.forEach(n => n.remove());

        // сначала гарантируем CSS (сохранённый порядок)
        await chain(links.map(linkEl => () => ensureStyleLink(linkEl)));
        await chain(styles.map(styleEl => () => ensureInlineStyle(styleEl)));

        // заменяем плейсхолдер на узлы партиала (без лишнего div)
        const nodes = Array.from(tmp.childNodes);
        holder.replaceWith(...nodes);

        // затем выполняем скрипты по порядку
        await chain(scripts.map(scriptEl => () => runScriptTag(scriptEl)));
    }

    async function init() {
        const holders = Array.from(document.querySelectorAll('[data-include]'));
        await Promise.all(holders.map(loadPartial));
        document.dispatchEvent(new CustomEvent('partials:loaded'));
    }

    document.addEventListener('DOMContentLoaded', init);
})();
