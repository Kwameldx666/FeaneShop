(function () {
    'use strict';

    function createStatistic(label, value) {
        return '<div class="d-flex justify-content-between"><span class="text-muted">' + label + '</span><strong>' + value + '</strong></div>';
    }

    document.addEventListener('DOMContentLoaded', function () {
        var main = document.querySelector('main[data-account-summary-endpoint]');
        var summary = document.getElementById('account-summary');
        if (!main || !summary || !window.feaneGateway) {
            return;
        }

        var endpoint = main.getAttribute('data-account-summary-endpoint');
        if (!endpoint) {
            return;
        }

        summary.innerHTML = '<h2 class="h5">Account snapshot</h2><p class="text-muted mb-0">Loading account details…</p>';

        window.feaneGateway.get(endpoint).then(function (response) {
            if (!response) {
                summary.innerHTML = '<h2 class="h5">Account snapshot</h2><p class="text-danger mb-0">No data returned from the gateway.</p>';
                return;
            }

            var name = response.displayName || response.name || 'Guest';
            var email = response.email || 'unknown';
            var orders = response.orderCount != null ? response.orderCount : '—';
            var reservations = response.reservationCount != null ? response.reservationCount : '—';

            var content = '<h2 class="h5">Welcome, ' + name + '</h2>' +
                '<p class="mb-3">We pulled these details from the gateway. Connect your account service to customise this information.</p>' +
                createStatistic('Email', email) +
                createStatistic('Orders placed', orders) +
                createStatistic('Reservations made', reservations);

            summary.innerHTML = content;
        }).catch(function (error) {
            summary.innerHTML = '<h2 class="h5">Account snapshot</h2><p class="text-danger mb-0">Unable to load account summary: ' + error.message + '</p>';
        });
    });
})();
