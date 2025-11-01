(function () {
    'use strict';

    function showFeedback(container, message, type) {
        if (!container) {
            alert(message);
            return;
        }

        container.textContent = message;
        container.classList.remove('d-none', 'show', 'alert-success', 'alert-danger', 'alert-info');
        if (type === 'success') {
            container.classList.add('alert-success');
        } else if (type === 'info') {
            container.classList.add('alert-info');
        } else {
            container.classList.add('alert-danger');
        }
        container.classList.add('show');
    }

    window.initMap = function () {
        var mapElement = document.getElementById('map');
        if (!mapElement) return;

        var options = {
            center: {lat: 47.0105, lng: 28.8638},
            zoom: 12
        };

        new google.maps.Map(mapElement, options);
    };

    document.addEventListener('DOMContentLoaded', function () {
        var form = document.querySelector('.booking-form');
        var feedback = document.getElementById('reservation-feedback');
        var dateInput = document.getElementById('reservationDateTime');
        var summary = {
            guests: document.getElementById('sum-guests'),
            date: document.getElementById('sum-date'),
            budget: document.getElementById('sum-budget'),
            total: document.getElementById('sum-total')
        };

        if (!form) return;

        var endpoint = form.getAttribute('data-gateway-endpoint');
        if (!endpoint) return;

        if (dateInput && !dateInput.value) {
            var defaultDate = new Date(Date.now() + 2 * 60 * 60 * 1000);
            dateInput.value = toLocalInputValue(defaultDate);
        }

        updateSummary(form, summary);
        form.addEventListener('input', function () {
            updateSummary(form, summary);
        });

        // 🔐 Извлечение userId из JWT
        function getUserIdFromToken(token) {
            if (!token) {
                console.warn('⚠️ JWT не найден в localStorage');
                return null;
            }
            try {
                const payload = JSON.parse(atob(token.split('.')[1]));
                return (
                    payload["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"] ||
                    payload.sub ||
                    payload.userId ||
                    null
                );
            } catch (e) {
                console.error('Ошибка при разборе токена:', e);
                return null;
            }
        }

        // Создаём/переопределяем feaneGateway.post
        if (!window.feaneGateway || typeof window.feaneGateway.post !== 'function') {
            window.feaneGateway = window.feaneGateway || {};
            window.feaneGateway.post = async function (url, body) {
                var token = localStorage.getItem('jwt');
                var response = await fetch(url, {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                        'Accept': 'application/json',
                        'Authorization': token ? `Bearer ${token}` : ''
                    },
                    credentials: 'include',
                    body: JSON.stringify(body || {})
                });
                var payload = await response.json().catch(() => ({}));
                if (!response.ok) {
                    var error = new Error(payload && payload.message ? payload.message : 'Request failed');
                    error.data = payload;
                    error.status = response.status;
                    throw error;
                }
                return payload;
            };
        }

        form.addEventListener('submit', function (event) {
            event.preventDefault();

            var formData = new FormData(form);
            var requiredFields = ['customerName', 'phoneNumber', 'userEmail', 'numberOfPeople', 'reservationDateTime'];
            var missing = requiredFields.filter(function (field) {
                var value = formData.get(field);
                return !value || String(value).trim() === '';
            });
            if (missing.length) {
                showFeedback(feedback, 'Проверьте обязательные поля и попробуйте ещё раз.', 'error');
                return;
            }

            var payload = createPayload(formData);

            // ✅ Добавляем userId из JWT
            var token = localStorage.getItem('jwt');
            var userId = getUserIdFromToken(token) || localStorage.getItem('userId');
            if (userId) {
                payload.userId = userId;
            }

            showFeedback(feedback, 'Отправляем запрос на бронирование…', 'info');

            window.feaneGateway.post(endpoint, payload)
                .then(function (data) {
                    if (data && (data.success || data.status === 'success')) {
                        showFeedback(feedback, data.message || 'Бронь создана успешно!', 'success');
                        form.reset();
                        form.dispatchEvent(new CustomEvent('reservation:created', {detail: data}));
                    } else {
                        var errorMessage = (data && data.message) || 'Не удалось отправить заявку на бронь.';
                        showFeedback(feedback, errorMessage, 'error');
                    }
                })
                .catch(function (error) {
                    var message = error && error.message ? error.message : 'Неизвестная ошибка отправки.';
                    if (error && error.data && error.data.errors) {
                        var errors = Object.values(error.data.errors).flat().join(' ');
                        if (errors) message = errors;
                    }
                    showFeedback(feedback, 'Ошибка: ' + message, 'error');
                });
        });
    });

    function createPayload(formData) {
        var payload = {};
        formData.forEach(function (value, key) {
            if (value === null || typeof value === 'undefined') {
                payload[key] = null;
                return;
            }

            var trimmed = typeof value === 'string' ? value.trim() : value;
            switch (key) {
                case 'numberOfPeople':
                    payload[key] = parseInt(trimmed, 10) || 0;
                    break;
                case 'budgetPerGuest':
                    payload[key] = trimmed === '' ? null : parseFloat(trimmed);
                    break;
                case 'reservationDateTime':
                    var date = new Date(trimmed);
                    payload[key] = isNaN(date) ? trimmed : date.toISOString();
                    break;
                default:
                    payload[key] = trimmed;
            }
        });
        return payload;
    }

    function updateSummary(form, summary) {
        if (!summary) return;

        var guests = parseInt(form.numberOfPeople?.value, 10) || 0;
        var budgetValue = form.budgetPerGuest?.value;
        var budget = budgetValue === '' ? null : parseFloat(budgetValue);
        var when = form.reservationDateTime?.value;

        if (summary.guests) summary.guests.textContent = guests > 0 ? guests : '—';
        if (summary.date) summary.date.textContent = when ? formatHumanDate(when) : '—';
        if (summary.budget)
            summary.budget.textContent =
                typeof budget === 'number' && !isNaN(budget) ? budget.toFixed(2) + ' LEI' : '—';
        if (summary.total)
            summary.total.textContent =
                guests > 0 && typeof budget === 'number' && !isNaN(budget)
                    ? (guests * budget).toFixed(2) + ' LEI'
                    : '—';
    }

    function toLocalInputValue(date) {
        if (!(date instanceof Date) || isNaN(date)) return '';
        var tzOffset = date.getTimezoneOffset();
        var local = new Date(date.getTime() - tzOffset * 60 * 1000);
        return local.toISOString().slice(0, 16);
    }

    function formatHumanDate(value) {
        var date = new Date(value);
        if (isNaN(date)) return '—';
        return date.toLocaleString('ru-RU', {
            day: '2-digit',
            month: 'long',
            year: 'numeric',
            hour: '2-digit',
            minute: '2-digit'
        });
    }
})();
