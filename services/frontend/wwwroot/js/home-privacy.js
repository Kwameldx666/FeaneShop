(function () {
    'use strict';

    var INIT_FLAG = 'data-privacy-ready';

    function $(sel, root) {
        return (root || document).querySelector(sel);
    }
    function $all(sel, root) {
        return Array.prototype.slice.call((root || document).querySelectorAll(sel));
    }

    function setStatus(container, type, message) {
        if (!container) return;
        container.className = 'alert alert-' + type;
        container.textContent = message;
    }

    function asHtml(content) {
        // сервер может вернуть уже HTML (string) или JSON со свойствами
        if (typeof content === 'string') {
            return content;
        }
        if (content && typeof content === 'object') {
            // ожидаемый JSON: { title?, updatedAt?, sections?: [{heading, html|text}] }
            var parts = [];
            if (Array.isArray(content.sections)) {
                content.sections.forEach(function (s, i) {
                    var id = (s.id || (s.heading || 'section-' + (i + 1))).toString()
                        .toLowerCase().replace(/[^\w\-]+/g, '-');
                    if (s.heading) {
                        parts.push('<h2 id="' + id + '">' + escapeHtml(s.heading) + '</h2>');
                    }
                    if (s.html) {
                        parts.push(s.html);
                    } else if (s.text) {
                        parts.push('<p>' + escapeHtml(s.text) + '</p>');
                    }
                });
            } else if (content.html) {
                parts.push(content.html);
            } else if (content.text) {
                parts.push('<p>' + escapeHtml(content.text) + '</p>');
            }
            return parts.join('\n');
        }
        return '<p>No privacy content available.</p>';
    }

    function escapeHtml(s) {
        return String(s)
            .replace(/&/g, '&amp;').replace(/</g, '&lt;')
            .replace(/>/g, '&gt;').replace(/"/g, '&quot;')
            .replace(/'/g, '&#039;');
    }

    function buildToc(container, toc) {
        if (!container || !toc) return;

        var headings = $all('h2, h3', container);
        if (!headings.length) {
            toc.classList.add('d-none');
            return;
        }

        var list = document.createElement('ol');
        list.className = 'mb-0';

        headings.forEach(function (h) {
            if (!h.id) {
                h.id = h.textContent.trim().toLowerCase().replace(/[^\w\-]+/g, '-');
            }
            var li = document.createElement('li');
            var a = document.createElement('a');
            a.href = '#' + h.id;
            a.textContent = h.textContent;
            li.appendChild(a);

            // вложенность: h2 — верхний уровень, h3 — вложенный
            if (h.tagName === 'H3') {
                // поместим в последний пункт h2
                var lastLi = list.lastElementChild;
                if (lastLi) {
                    var sub = lastLi.querySelector('ol');
                    if (!sub) {
                        sub = document.createElement('ol');
                        sub.className = 'mb-0';
                        lastLi.appendChild(sub);
                    }
                    sub.appendChild(li);
                } else {
                    list.appendChild(li);
                }
            } else {
                list.appendChild(li);
            }
        });

        toc.innerHTML = '';
        var title = document.createElement('div');
        title.className = 'toc-title';
        title.textContent = 'Contents';
        toc.appendChild(title);
        toc.appendChild(list);
        toc.classList.remove('d-none');
    }

    function renderMeta(metaEl, title, updatedAt) {
        if (!metaEl) return;
        var parts = [];
        if (title) parts.push(title);
        if (updatedAt) {
            var d = new Date(updatedAt);
            if (!isNaN(d.getTime())) {
                parts.push('Last updated: ' + d.toLocaleDateString());
            }
        }
        if (parts.length) {
            metaEl.textContent = parts.join(' • ');
            metaEl.classList.remove('d-none');
        }
    }

    function loadPrivacy(main) {
        var endpoint = main.getAttribute('data-privacy-endpoint');
        var content = $('#privacy-content');
        var meta = $('#privacy-meta');
        var toc = $('#privacy-toc');

        if (!endpoint) {
            content.innerHTML = '<div class="alert alert-warning mb-0">No endpoint provided.</div>';
            return;
        }

        // Пытаемся через feaneGateway, иначе fetch:
        var reader = (window.feaneGateway && typeof window.feaneGateway.get === 'function')
            ? window.feaneGateway.get(endpoint)
            : fetch(endpoint, { credentials: 'include' })
                .then(function (r) {
                    if (!r.ok) throw new Error(r.statusText);
                    // пробуем как JSON, затем как текст
                    return r.clone().json().catch(function () {
                        return r.text();
                    });
                });

        Promise.resolve(reader).then(function (data) {
            // data может быть строкой-HTML, либо объектом
            var html = asHtml(data);
            content.innerHTML = html;

            // мета
            var title = (data && data.title) ? String(data.title) : '';
            var updatedAt = (data && data.updatedAt) ? String(data.updatedAt) : '';
            renderMeta(meta, title, updatedAt);

            // построить оглавление
            buildToc(content, toc);
        }).catch(function (err) {
            content.innerHTML = '';
            var box = document.createElement('div');
            setStatus(box, 'danger', 'Failed to load privacy policy: ' + err.message);
            content.appendChild(box);
        });
    }

    function init() {
        var main = document.querySelector('main[data-privacy-endpoint]');
        if (!main || main.hasAttribute(INIT_FLAG)) return;
        main.setAttribute(INIT_FLAG, 'true');
        loadPrivacy(main);
    }

    document.addEventListener('DOMContentLoaded', init);
    document.addEventListener('partials:loaded', init);
})();
