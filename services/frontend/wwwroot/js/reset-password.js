(function () {
    'use strict';

    var $ = function (id) { return document.getElementById(id); };
    var toggleBtn = $('forgot-password-btn');
    var panel = $('forgot-password-panel');
    var emailEl = $('reset-email');
    var sendBtn = $('reset-send-btn');
    var okBox = $('reset-ok');
    var errBox = $('reset-err');

    function show(el, on) { if (!el) return; el.classList[on ? 'remove' : 'add']('hidden'); }
    function setMsg(el, txt) { if (!el) return; el.textContent = txt; show(el, !!txt); }
    function clearMsgs() { setMsg(okBox, ''); setMsg(errBox, ''); }

    // Показ/скрытие панели
    toggleBtn && toggleBtn.addEventListener('click', function () {
        var willShow = panel.classList.contains('hidden');
        show(panel, willShow);
        panel.setAttribute('aria-hidden', willShow ? 'false' : 'true');
        if (willShow) emailEl && emailEl.focus();
    });

    // Отправка без формы/сервера
    function handleSend() {
        clearMsgs();
        var email = (emailEl && emailEl.value || '').trim();
        var valid = /^[^@\s]+@[^@\s]+\.[^@\s]+$/.test(email);

        if (!valid) {
            setMsg(errBox, 'Укажите корректный E-mail.');
            emailEl && emailEl.focus();
            return;
        }

        // Имитируем процесс
        sendBtn && (sendBtn.disabled = true);
        setMsg(okBox, 'Отправляем инструкции…');

        setTimeout(function () {
            setMsg(okBox, 'Если такой адрес существует, мы отправили письмо со ссылкой для сброса.');
            sendBtn && (sendBtn.disabled = false);
            if (emailEl) emailEl.value = '';
        }, 700);
    }

    sendBtn && sendBtn.addEventListener('click', handleSend);

    // Enter в поле — как клик по кнопке
    emailEl && emailEl.addEventListener('keydown', function (e) {
        if (e.key === 'Enter') { e.preventDefault(); handleSend(); }
    });
})();
