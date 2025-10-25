(function () {
    'use strict';

    var INITIALISED_FLAG = 'data-profile-ready';

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

    function setText(id, value, fallback) {
        var element = document.getElementById(id);
        if (element) {
            element.textContent = value != null && value !== '' ? value : fallback;
        }
    }

    function formatDate(isoString) {
        if (!isoString) {
            return '—';
        }
        var date = new Date(isoString);
        if (Number.isNaN(date.getTime())) {
            return isoString;
        }
        return date.toLocaleDateString(undefined, {
            year: 'numeric',
            month: 'long',
            day: 'numeric'
        });
    }

    function populateProfile(data) {
        var name = data.displayName || data.fullName || data.name || 'Guest User';
        var email = data.email || data.primaryEmail || 'guest@example.com';
        var phone = data.phone || data.phoneNumber || data.mobilePhone || 'Not provided';
        var altPhone = data.alternatePhone || data.secondaryPhone || phone || 'Not provided';
        var memberSince = data.createdAt || data.memberSince || '';

        setText('profile-name', name);
        setText('profile-email', email);
        setText('detail-name', name);
        setText('detail-email', email);
        setText('detail-phone', phone);
        setText('detail-phone-secondary', altPhone);
        setText('profile-member-since', formatDate(memberSince));
    }

    function initialiseProfile(main) {
        if (!main || main.hasAttribute(INITIALISED_FLAG)) {
            return;
        }
        main.setAttribute(INITIALISED_FLAG, 'true');

        var endpoint = main.getAttribute('data-profile-endpoint');
        if (!endpoint || !window.feaneGateway) {
            return;
        }

        var feedback = document.getElementById('profile-feedback');
        var refreshButton = document.getElementById('edit-profile');

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
            refreshButton.addEventListener('click', loadProfile);
        }

        loadProfile();
    }

    function init() {
        var main = document.querySelector('main[data-profile-endpoint]');
        initialiseProfile(main);
    }

    document.addEventListener('DOMContentLoaded', init);
    document.addEventListener('feane:page-ready', init);
})();
