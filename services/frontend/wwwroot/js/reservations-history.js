(function () {
    'use strict';

    // Подробное логирование - если токена нет, редиректим
    console.log('[ReservationsHistory] ===== SCRIPT LOADED =====');
    console.log('[ReservationsHistory] Current URL:', window.location.href);
    console.log('[ReservationsHistory] Checking token...');

    var token = null;
    try {
        token = localStorage.getItem('jwt');
        console.log('[ReservationsHistory] JWT token:', token ? 'EXISTS (' + token.length + ' chars)' : 'NULL');
        console.log('[ReservationsHistory] localStorage available:', typeof localStorage !== 'undefined');

        if (token) {
            // Показываем превью токена
            console.log('[ReservationsHistory] Token preview:', token.substring(0, 30) + '...');
        }
    } catch (e) {
        console.error('[ReservationsHistory] Error accessing localStorage:', e);
    }

    if (!token) {
        console.error('[ReservationsHistory] ❌ NO JWT TOKEN FOUND!');
        console.log('[ReservationsHistory] Redirecting to authentication in 3 seconds...');
        console.log('[ReservationsHistory] To cancel redirect, execute in console: window.cancelRedirect = true');

        var redirectUrl = '/account/authentication?redir=' + encodeURIComponent(window.location.pathname);
        console.log('[ReservationsHistory] Redirect URL:', redirectUrl);

        window.cancelRedirect = false;
        setTimeout(function () {
            if (!window.cancelRedirect) {
                console.log('[ReservationsHistory] Redirecting now...');
                window.location.replace(redirectUrl);
            } else {
                console.log('[ReservationsHistory] Redirect cancelled by user');
            }
        }, 3000);
        return;
    }

    console.log('[ReservationsHistory] ✅ TOKEN FOUND - continuing with page initialization');

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
        return d.toLocaleString('ru-RU', {
            year: 'numeric',
            month: 'long',
            day: 'numeric',
            hour: '2-digit',
            minute: '2-digit'
        });
    }

    function row(r) {
        var tr = document.createElement('tr');

        // Дата
        var tdDate = document.createElement('td');
        tdDate.innerHTML = '<div style="font-weight:600;">' + fmtDateTime(r.date || r.reservationDate) + '</div>' +
            (r.specialRequests ? '<div style="font-size:12px;color:var(--muted);margin-top:4px;">' + r.specialRequests + '</div>' : '');
        tr.appendChild(tdDate);

        // Повод
        var tdOcc = document.createElement('td');
        tdOcc.textContent = r.occasion || '—';
        tr.appendChild(tdOcc);

        // Гостей
        var tdGuests = document.createElement('td');
        tdGuests.textContent = r.guests || r.numberOfPeople || '—';
        tr.appendChild(tdGuests);

        // Статус
        var tdStatus = document.createElement('td');
        var status = (r.status || r.reservationStatus || 'Unknown').toLowerCase();
        var statusClass = 'status';
        if (status.includes('pending') || status.includes('upcoming') || status.includes('confirmed')) {
            statusClass += ' upcoming';
        } else if (status.includes('cancel') || status.includes('declined')) {
            statusClass += ' cancelled';
        } else if (status.includes('completed') || status.includes('done')) {
            statusClass += ' done';
        }
        tdStatus.innerHTML = '<span class="' + statusClass + '">' + (r.status || r.reservationStatus || 'Unknown') + '</span>';
        tr.appendChild(tdStatus);

        // Сумма
        var tdAmount = document.createElement('td');
        tdAmount.className = 'money';
        var amount = Number(r.amount ?? r.total ?? r.estimatedTotal ?? 0);
        tdAmount.textContent = amount > 0 ? amount.toFixed(2) + ' LEI' : '—';
        tr.appendChild(tdAmount);

        // Действия
        var tdAct = document.createElement('td');
        tdAct.style.textAlign = 'center';

        // Проверяем можно ли отменить (статус pending или confirmed)
        var canCancel = r.canCancel || (status.includes('pending') || status.includes('confirmed') || status.includes('upcoming'));

        if (canCancel) {
            var btn = document.createElement('button');
            btn.type = 'button';
            btn.className = 'btn btn-danger';
            btn.textContent = 'Отменить';
            btn.dataset.reservationId = r.id || r.reservationId || '';
            tdAct.appendChild(btn);
        } else {
            tdAct.innerHTML = '<span style="color:var(--muted);">—</span>';
        }
        tr.appendChild(tdAct);

        return tr;
    }

    function init() {
        var main = document.querySelector('main[data-reservations-endpoint]');
        if (!main || main.hasAttribute(FLAG)) return;
        main.setAttribute(FLAG, 'true');

        var endpoint = main.getAttribute('data-reservations-endpoint');
        if (!endpoint) {
            console.error('Reservations: data-reservations-endpoint not found');
            return;
        }

        var fb = document.getElementById('reservations-feedback');
        var tbody = document.getElementById('reservation-table-body');

        // Проверяем наличие gateway-client
        if (!window.feaneGateway || typeof window.feaneGateway.get !== 'function') {
            console.error('Reservations: feaneGateway not available. Make sure gateway-client.js is loaded.');
            showFeedback(fb, 'Ошибка: Gateway client не загружен. Проверьте подключение gateway-client.js', 'error');
            return;
        }
        if (!tbody) return;

        function load() {
            console.log('Loading reservations from:', endpoint);
            showFeedback(fb, 'Загрузка резерваций…', 'info');

            // userId будет автоматически извлечен сервером из JWT токена
            // Не передаем его в query параметрах для безопасности

            window.feaneGateway.get(endpoint).then(function (resp) {
                console.log('Reservations response:', resp);

                // Проверяем, есть ли ошибка в ответе
                if (resp && resp.success === false) {
                    var errorMsg = resp.message || 'Неизвестная ошибка';
                    if (resp.error) {
                        errorMsg += ' (Детали: ' + resp.error + ')';
                    }
                    tbody.innerHTML = '<tr><td colspan="6" class="empty" style="color:#ff6b6b;">' + errorMsg + '</td></tr>';
                    showFeedback(fb, errorMsg, 'error');
                    return;
                }

                var list = Array.isArray(resp?.items) ? resp.items : (Array.isArray(resp) ? resp : []);

                if (!list.length) {
                    tbody.innerHTML = '<tr><td colspan="6" class="empty">Пока нет резерваций.</td></tr>';
                    showFeedback(fb, 'Нет данных.', 'info');
                    return;
                }

                tbody.innerHTML = '';
                list.forEach(function (r) {
                    tbody.appendChild(row(r));
                });
                showFeedback(fb, 'Загружено резерваций: ' + list.length, 'success');
            }).catch(function (e) {
                console.error('Reservations load error:', e);
                var errorDetails = e?.message || String(e);
                if (e?.data) {
                    console.error('Error data:', e.data);
                    if (e.data.message) {
                        errorDetails = e.data.message;
                    }
                    if (e.data.error) {
                        errorDetails += ' (' + e.data.error + ')';
                    }
                }
                tbody.innerHTML = '<tr><td colspan="6" class="empty" style="color:#ff6b6b;">Ошибка: ' + errorDetails + '</td></tr>';
                showFeedback(fb, 'Ошибка загрузки: ' + errorDetails, 'error');
            });
        }

        tbody.addEventListener('click', function (ev) {
            var btn = ev.target;
            // Проверяем что это кнопка отмены
            if (!btn || btn.tagName !== 'BUTTON' || !btn.classList.contains('btn-danger')) return;

            var id = btn.dataset.reservationId;
            if (!id) return;
            if (!confirm('Отменить резервацию?')) return;

            showFeedback(fb, 'Отмена…', 'info');
            btn.disabled = true;
            btn.textContent = 'Отмена...';

            window.feaneGateway.post(endpoint + '/' + encodeURIComponent(id) + '/cancel', {})
                .then(function (r) {
                    if (r && (r.success || r.status === 'success' || r.ok)) {
                        showFeedback(fb, r.message || 'Резервация отменена.', 'success');
                        load();
                    } else {
                        showFeedback(fb, (r && r.message) || 'Не удалось отменить.', 'error');
                        btn.disabled = false;
                        btn.textContent = 'Отменить';
                    }
                })
                .catch(function (e) {
                    showFeedback(fb, 'Ошибка отмены: ' + (e?.message || e), 'error');
                    btn.disabled = false;
                    btn.textContent = 'Отменить';
                });
        });

        load();
    }

    document.addEventListener('DOMContentLoaded', init);
    document.addEventListener('feane:page-ready', init);
})();
