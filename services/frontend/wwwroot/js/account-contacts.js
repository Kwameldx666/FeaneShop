(function () {
    'use strict';

    // Подробное логирование - если токена нет, редиректим
    console.log('[AccountContacts] Script loaded, checking token...');
    var token = localStorage.getItem('jwt');
    console.log('[AccountContacts] JWT token:', token ? 'EXISTS (' + token.length + ' chars)' : 'NULL');
    console.log('[AccountContacts] localStorage available:', typeof localStorage !== 'undefined');

    if (!token) {
        console.log('[AccountContacts] No JWT token found, redirecting to authentication');
        var redirectUrl = '/account/authentication?redir=' + encodeURIComponent(window.location.pathname);
        console.log('[AccountContacts] Redirect URL:', redirectUrl);
        window.location.replace(redirectUrl);
        return;
    }

    console.log('[AccountContacts] Token found, continuing with page initialization');

    var FLAG = 'data-contacts-ready';

    function showFeedback(container, message, type) {
        if (!container) {
            console.log(message);
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
        container.classList.remove('d-none');
    }

    function init() {
        console.log('[init] Starting initialization...');

        var main = document.querySelector('main[data-contacts-endpoint]');
        console.log('[init] Main element found:', !!main);

        if (!main) {
            console.error('[init] Main element with data-contacts-endpoint not found!');
            return;
        }

        if (main.hasAttribute(FLAG)) {
            console.log('[init] Already initialized, skipping');
            return;
        }

        main.setAttribute(FLAG, 'true');
        console.log('[init] Flag set, continuing initialization');

        var endpoint = main.getAttribute('data-contacts-endpoint');
        console.log('[init] Endpoint from data attribute:', endpoint);

        if (!endpoint) {
            console.error('[init] data-contacts-endpoint attribute is empty or missing');
            return;
        }

        // Проверяем наличие gateway-client
        console.log('[init] Checking for feaneGateway:', typeof window.feaneGateway);
        console.log('[init] feaneGateway.get exists:', typeof window.feaneGateway?.get);

        if (!window.feaneGateway || typeof window.feaneGateway.get !== 'function') {
            console.error('[init] feaneGateway not available. Make sure gateway-client.js is loaded before account-contacts.js');
            return;
        }

        var feedback = document.getElementById('contacts-feedback');
        var nameInput = document.getElementById('contact-name');
        var emailInput = document.getElementById('contact-email');
        var phoneInput = document.getElementById('contact-phone');
        var addressInput = document.getElementById('contact-address');
        var saveBtn = document.getElementById('save-contacts-btn');
        var resetBtn = document.getElementById('reset-contacts-btn');

        console.log('[init] Elements found:', {
            feedback: !!feedback,
            nameInput: !!nameInput,
            emailInput: !!emailInput,
            phoneInput: !!phoneInput,
            addressInput: !!addressInput,
            saveBtn: !!saveBtn,
            resetBtn: !!resetBtn
        });

        if (!nameInput || !emailInput || !phoneInput || !addressInput) {
            console.error('[init] Required input fields not found');
            return;
        }

        var originalData = {};
        var userId = null;

        console.log('[init] All checks passed, setting up event handlers...');

        // Извлекаем userId из JWT токена
        function getUserIdFromToken() {
            try {
                var token = localStorage.getItem('jwt');
                console.log('[getUserIdFromToken] Token exists:', !!token);

                if (!token) {
                    console.error('[getUserIdFromToken] No token found in localStorage');
                    return null;
                }

                var parts = token.split('.');
                console.log('[getUserIdFromToken] Token parts:', parts.length);

                if (parts.length !== 3) {
                    console.error('[getUserIdFromToken] Invalid token format, expected 3 parts, got:', parts.length);
                    return null;
                }

                var payload = JSON.parse(atob(parts[1]));
                console.log('[getUserIdFromToken] Decoded payload:', payload);
                console.log('[getUserIdFromToken] Payload keys:', Object.keys(payload));

                var userId =
                    payload["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"] ||
                    payload.sub ||
                    payload.userId ||
                    payload.id ||
                    null;

                console.log('[getUserIdFromToken] Extracted userId:', userId);
                return userId;
            } catch (e) {
                console.error('[getUserIdFromToken] Error parsing JWT token:', e);
                console.error('[getUserIdFromToken] Error stack:', e.stack);
                return null;
            }
        }

        // Загружаем данные профиля
        function loadContactData() {
            userId = getUserIdFromToken();
            console.log('[loadContactData] Extracted userId:', userId);

            if (!userId) {
                console.error('[loadContactData] No userId found!');
                showFeedback(feedback, 'Пожалуйста, войдите в систему', 'error');
                return;
            }

            // Используем /api/users/{id} для получения полных данных пользователя из user-service
            var url = '/api/users/' + userId;
            console.log('[loadContactData] Loading contact data from:', url);
            showFeedback(feedback, 'Загрузка данных...', 'info');

            window.feaneGateway.get(url)
                .then(function (resp) {
                    console.log('[loadContactData] Raw response:', resp);
                    console.log('[loadContactData] Response type:', typeof resp);
                    console.log('[loadContactData] Response keys:', resp ? Object.keys(resp) : 'null');

                    // /api/users/{id} возвращает OperationResult<UserProfile>: { status, message, data: { user: UserData } }
                    if (!resp || !resp.status || !resp.data) {
                        console.error('[loadContactData] Invalid response format!');
                        showFeedback(feedback, resp?.message || 'Ошибка: не удалось получить данные пользователя', 'error');
                        return;
                    }

                    // UserProfile содержит свойство User с данными пользователя
                    var userProfile = resp.data;
                    var userData = userProfile.user || userProfile.User;

                    console.log('[loadContactData] User profile:', userProfile);
                    console.log('[loadContactData] User data:', userData);
                    console.log('[loadContactData] userData keys:', userData ? Object.keys(userData) : 'null');

                    if (!userData || !userData.id) {
                        console.error('[loadContactData] Could not extract user data from response!');
                        showFeedback(feedback, 'Ошибка: не удалось получить данные пользователя', 'error');
                        return;
                    }

                    // Сохраняем userId из ответа
                    userId = userData.id || userData.Id;

                    // Сохраняем оригинальные данные
                    // Roles может быть числом (enum) или строкой
                    var roleValue = userData.roles || userData.Roles;
                    console.log('[loadContactData] Role value:', roleValue, 'type:', typeof roleValue);

                    if (typeof roleValue === 'number') {
                        var roleNames = ['None', 'User', 'Moderator', 'Admin', 'VIP'];
                        roleValue = roleNames[roleValue] || 'User';
                        console.log('[loadContactData] Converted numeric role to:', roleValue);
                    } else if (!roleValue) {
                        roleValue = 'User';
                        console.log('[loadContactData] Using default role: User');
                    }

                    originalData = {
                        name: userData.username || userData.Username || '',
                        email: userData.email || userData.Email || '',
                        phone: userData.phoneNumber || userData.PhoneNumber || '',
                        address: userData.address || userData.Address || '',
                        role: roleValue,
                        isActive: userData.isActive !== undefined ? userData.isActive : (userData.IsActive !== undefined ? userData.IsActive : true)
                    };

                    console.log('[loadContactData] Original data populated:', originalData);

                    // Заполняем поля
                    nameInput.value = originalData.name;
                    emailInput.value = originalData.email;
                    phoneInput.value = originalData.phone;
                    addressInput.value = originalData.address;

                    console.log('[loadContactData] Form fields populated successfully');
                    showFeedback(feedback, 'Данные загружены', 'success');
                    setTimeout(function () {
                        feedback.classList.add('d-none');
                    }, 2000);
                })
                .catch(function (e) {
                    console.error('[loadContactData] Contact data load error:', e);
                    console.error('[loadContactData] Error type:', typeof e);
                    console.error('[loadContactData] Error keys:', e ? Object.keys(e) : 'null');

                    var errorDetails = e?.message || String(e);
                    if (e?.data) {
                        console.error('[loadContactData] Error data:', e.data);
                        if (e.data.message) {
                            errorDetails = e.data.message;
                        }
                    }
                    if (e?.response) {
                        console.error('[loadContactData] Error response:', e.response);
                    }
                    showFeedback(feedback, 'Ошибка загрузки: ' + errorDetails, 'error');
                });
        }

        // Сохраняем данные
        function saveContactData() {
            if (!userId) {
                showFeedback(feedback, 'Ошибка: userId не определен', 'error');
                return;
            }

            var url = '/api/users/' + userId;
            console.log('Saving contact data to:', url);
            showFeedback(feedback, 'Сохранение...', 'info');

            if (saveBtn) saveBtn.disabled = true;

            // UserUpdateRequest требует все обязательные поля
            // Role должен быть строкой из enum: None, User, Moderator, Admin, VIP
            var roleValue = originalData.role;
            if (typeof roleValue === 'number') {
                // Конвертируем число в строку enum
                var roleNames = ['None', 'User', 'Moderator', 'Admin', 'VIP'];
                roleValue = roleNames[roleValue] || 'User';
            } else if (!roleValue || typeof roleValue !== 'string') {
                roleValue = 'User';
            }

            var phoneValue = phoneInput.value.trim();
            var addressValue = addressInput.value.trim();

            var data = {
                Username: nameInput.value.trim() || originalData.name,
                Email: emailInput.value.trim() || originalData.email,
                PhoneNumber: phoneValue || '',
                Address: addressValue || '',
                Role: roleValue,
                IsActive: originalData.isActive !== false
            };

            console.log('Sending update data:', data);

            window.feaneGateway.put(url, data)
                .then(function (resp) {
                    console.log('Save response:', resp);

                    if (resp && (resp.status || resp.success)) {
                        showFeedback(feedback, resp.message || 'Данные успешно сохранены', 'success');

                        // Обновляем оригинальные данные из ответа или из отправленных данных
                        var savedUserData = resp.data && resp.data.user ? resp.data.user : null;

                        originalData = {
                            name: savedUserData ? savedUserData.username : data.username,
                            email: savedUserData ? savedUserData.email : data.email,
                            phone: savedUserData ? savedUserData.phoneNumber : data.phoneNumber,
                            address: savedUserData ? savedUserData.address : data.address,
                            role: savedUserData ? (savedUserData.roles || savedUserData.role) : data.role,
                            isActive: savedUserData ? savedUserData.isActive : data.isActive
                        };
                    } else {
                        showFeedback(feedback, resp.message || 'Не удалось сохранить данные', 'error');
                    }
                })
                .catch(function (e) {
                    console.error('Save error:', e);
                    var errorDetails = e?.message || String(e);
                    if (e?.data?.message) {
                        errorDetails = e.data.message;
                    }
                    if (e?.data?.errors) {
                        var errors = Object.values(e.data.errors).flat().join(', ');
                        if (errors) errorDetails += ': ' + errors;
                    }
                    showFeedback(feedback, 'Ошибка сохранения: ' + errorDetails, 'error');
                })
                .finally(function () {
                    if (saveBtn) saveBtn.disabled = false;
                });
        }

        // Сброс к оригинальным данным
        function resetContactData() {
            nameInput.value = originalData.name || '';
            emailInput.value = originalData.email || '';
            phoneInput.value = originalData.phone || '';
            addressInput.value = originalData.address || '';
            showFeedback(feedback, 'Изменения отменены', 'info');
            setTimeout(function () {
                feedback.classList.add('d-none');
            }, 2000);
        }

        // Обработчики событий
        if (saveBtn) {
            saveBtn.addEventListener('click', saveContactData);
            console.log('[init] Save button event listener attached');
        } else {
            console.warn('[init] Save button not found, cannot attach event listener');
        }

        if (resetBtn) {
            resetBtn.addEventListener('click', resetContactData);
            console.log('[init] Reset button event listener attached');
        } else {
            console.warn('[init] Reset button not found, cannot attach event listener');
        }

        // Загружаем данные при инициализации
        console.log('[init] Calling loadContactData()...');
        loadContactData();
        console.log('[init] Initialization complete');
    }

    console.log('[AccountContacts] Setting up DOMContentLoaded listener');
    document.addEventListener('DOMContentLoaded', function () {
        console.log('[AccountContacts] DOMContentLoaded fired');
        init();
    });

    console.log('[AccountContacts] Setting up feane:page-ready listener');
    document.addEventListener('feane:page-ready', function () {
        console.log('[AccountContacts] feane:page-ready fired');
        init();
    });

    console.log('[AccountContacts] Script fully loaded');
})();

