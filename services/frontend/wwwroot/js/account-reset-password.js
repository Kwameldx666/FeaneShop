(function () {
    'use strict';

    function showFeedback(container, message, type) {
        if (!container) {
            alert(message);
            return;
        }

        container.textContent = message;
        container.classList.remove('d-none', 'alert-success', 'alert-danger', 'alert-info');
        if (type === 'success') {
            container.classList.add('alert-success');
        } else if (type === 'info') {
            container.classList.add('alert-info');
        } else {
            container.classList.add('alert-danger');
        }
    }

    document.addEventListener('DOMContentLoaded', function () {
        var toggleButton = document.getElementById('forgot-password-btn');
        var form = document.getElementById('forgot-password-form');
        var feedback = document.getElementById('reset-feedback');

        if (!toggleButton || !form || !window.feaneGateway) {
            return;
        }

        toggleButton.addEventListener('click', function () {
            form.classList.toggle('hidden');
        });

        var endpoint = form.getAttribute('data-gateway-endpoint');
        if (!endpoint) {
            return;
        }

        form.addEventListener('submit', function (event) {
            event.preventDefault();

            var email = document.getElementById('resetEmail').value.trim();
            if (!email) {
                showFeedback(feedback, 'Введите корректный email.', 'error');
                return;
            }

            showFeedback(feedback, 'Отправляем ссылку для восстановления…', 'info');

            window.feaneGateway.post(endpoint, { email: email }).then(function (response) {
                if (response && (response.success || response.status === 'success')) {
                    showFeedback(feedback, response.message || 'Ссылка отправлена на указанную почту.', 'success');
                    form.reset();
                    form.classList.add('hidden');
                    return;
                }

                var message = (response && response.message) || 'Не удалось отправить ссылку для восстановления.';
                showFeedback(feedback, message, 'error');
            }).catch(function (error) {
                showFeedback(feedback, 'Ошибка при обращении к шлюзу: ' + error.message, 'error');
            });
        });
    });
})();
