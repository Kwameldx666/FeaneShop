(function () {
    'use strict';

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
    }

    function populateProfile(data) {
        document.getElementById('profile-name').textContent = data.displayName || data.name || 'Guest User';
        document.getElementById('profile-email').textContent = data.email || 'guest@example.com';
        document.getElementById('detail-name').textContent = data.fullName || data.name || 'Guest User';
        document.getElementById('detail-email').textContent = data.email || 'guest@example.com';
        document.getElementById('detail-phone').textContent = data.phone || data.phoneNumber || 'Not provided';
    }

    document.addEventListener('DOMContentLoaded', function () {
        var main = document.querySelector('main[data-profile-endpoint]');
        if (!main || !window.feaneGateway) {
            return;
        }

        var endpoint = main.getAttribute('data-profile-endpoint');
        var feedback = document.getElementById('profile-feedback');
        var refreshButton = document.getElementById('edit-profile');

        if (!endpoint) {
            return;
        }

        function loadProfile() {
            showFeedback(feedback, 'Loading profile from gateway…', 'info');
            window.feaneGateway.get(endpoint).then(function (profile) {
                if (!profile) {
                    showFeedback(feedback, 'Profile data is empty.', 'error');
                    return;
                }

                populateProfile(profile);
                showFeedback(feedback, 'Profile synced with gateway data.', 'success');
            }).catch(function (error) {
                showFeedback(feedback, 'Unable to load profile: ' + error.message, 'error');
            });
        }

        if (refreshButton) {
            refreshButton.addEventListener('click', function () {
                loadProfile();
            });
        }

        loadProfile();
    });
})();
