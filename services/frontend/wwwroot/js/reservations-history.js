(function () {
    'use strict';

    // Шим API-клиента (если не задан где-то ещё)
    window.feaneGateway = window.feaneGateway || {};
    window.feaneGateway.get = window.feaneGateway.get || (async function (url) {
        try {
            const res = await fetch(url, { credentials: 'same-origin' });
            const data = await res.json().catch(() => ({}));
            return { ok: res.ok, status: res.status, ...data };
        } catch (e) { return { ok: false, message: 'Network error', error: e }; }
    });
    window.feaneGateway.post = window.feaneGateway.post || (async function (url, body) {
        try {
            const res = await fetch(url, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                credentials: 'same-origin',
                body: JSON.stringify(body || {})
            });
            const data = await res.json().catch(() => ({}));
            return { ok: res.ok, status: res.status, ...data };
        } catch (e) { return { ok: false, message: 'Network error', error: e }; }
    });

    var FLAG = 'data-reservations-ready';

    function showFeedback(el, msg, type) {
        if (!el) return;
        el.textContent = msg || '';
        el.classList.remove('d-none', 'alert-success', 'alert-danger', 'alert-info');
        el.classList.add(type === 'success' ? 'alert-success' : type === 'info' ? 'alert-info' : 'alert-danger');
    }

    function fmtDateTime(v) {
        var d = new Date(v);
        if (isNaN(d)) return '—';
        return d.toLocaleString('ru-RU', { year: 'numeric', month: 'long', day: 'numeric', hour: '2-digit', minute: '2-digit' });
    }

    function row(r) {
        var tr = document.createElement('tr');

        var tdDate = document.createElement('td');
        tdDate.innerHTML = '<div class="fw-semibold">' + fmtDateTime(r.date || r.reservationDate) + '</div>' +
            (r.specialRequests ? '<div class="text-muted small">' + r.specialRequests + '</div>' : '');
        tr.appendChild(tdDate);

        var tdOcc = document.createElement('td'); tdOcc.textContent = r.occasion || '—'; tr.appendChild(tdOcc);
        var tdGuests = document.createElement('td'); tdGuests.textContent = r.guests || r.numberOfPeople || '—'; tr.appendChild(tdGuests);

        var tdStatus = document.createElement('td');
        var status = r.status || r.reservationStatus || 'Unknown';
        tdStatus.innerHTML = '<span class="badge bg-light text-dark">' + status + '</span>';
        tr.appendChild(tdStatus);

        var tdAmount = document.createElement('td');
        var amount = Number(r.amount || r.total || 0);
        tdAmount.textContent = amount > 0 ? amount.toFixed(2) + ' BYN' : '—';
        tr.appendChild(tdAmount);

        var tdAct = document.createElement('td'); tdAct.className = 'text-center';
        if (r.canCancel) {
            var btn = document.createElement('button');
            btn.type = 'button';
            btn.className = 'btn btn-outline-danger btn-sm cancel-reservation';
            btn.textContent = 'Отменить';
            btn.dataset.reservationId = r.id || r.reservationId || '';
            tdAct.appendChild(btn);
        } else {
            tdAct.innerHTML = '<span class="text-muted">—</span>';
        }
        tr.appendChild(tdAct);

        return tr;
    }

    function init() {
        var main = document.querySelector('main[data-reservations-endpoint]');
        if (!main || main.hasAttribute(FLAG)) return;
        main.setAttribute(FLAG, 'true');

        var endpoint = main.getAttribute('data-reservations-endpoint');
        if (!endpoint) return;

        var fb = document.getElementById('reservations-feedback');
        var tbody = document.getElementById('reservation-table-body');
        if (!tbody) return;

        function load() {
            showFeedback(fb, 'Загрузка резерваций…', 'info');
            window.feaneGateway.get(endpoint).then(function (resp) {
                var list = Array.isArray(resp?.items) ? resp.items : (Array.isArray(resp) ? resp : []);
                if (!list.length) {
                    tbody.innerHTML = '<tr><td colspan="6" class="text-muted text-center py-4">Пока нет резерваций.</td></tr>';
                    showFeedback(fb, 'Нет данных.', 'info');
                    return;
                }
                tbody.innerHTML = '';
                list.forEach(function (r) { tbody.appendChild(row(r)); });
                showFeedback(fb, 'Готово.', 'success');
            }).catch(function (e) {
                tbody.innerHTML = '<tr><td colspan="6" class="text-danger text-center py-4">Не удалось загрузить.</td></tr>';
                showFeedback(fb, 'Ошибка загрузки: ' + (e?.message || e), 'error');
            });
        }

        tbody.addEventListener('click', function (ev) {
            var btn = ev.target.closest('.cancel-reservation');
            if (!btn) return;
            var id = btn.dataset.reservationId;
            if (!id) return;
            if (!confirm('Отменить резервацию?')) return;

            showFeedback(fb, 'Отмена…', 'info');
            window.feaneGateway.post(endpoint + '/' + encodeURIComponent(id) + '/cancel', {})
                .then(function (r) {
                    if (r && (r.success || r.status === 'success' || r.ok)) {
                        showFeedback(fb, r.message || 'Резервация отменена.', 'success');
                        load();
                    } else {
                        showFeedback(fb, (r && r.message) || 'Не удалось отменить.', 'error');
                    }
                })
                .catch(function (e) {
                    showFeedback(fb, 'Ошибка отмены: ' + (e?.message || e), 'error');
                });
        });

        load();
    }

    document.addEventListener('DOMContentLoaded', init);
    document.addEventListener('feane:page-ready', init);
})();
