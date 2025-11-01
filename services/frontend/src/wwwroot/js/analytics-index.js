(function () {
    'use strict';

    function formatValue(value) {
        if (value == null) {
            return '—';
        }

        if (typeof value === 'number') {
            return value.toLocaleString();
        }

        return String(value);
    }

    function createCard(title, description) {
        return '<div class="col-md-4"><div class="stat-card"><h2>' + title + '</h2><p>' + description + '</p></div></div>';
    }

    document.addEventListener('DOMContentLoaded', function () {
        var main = document.querySelector('main[data-analytics-endpoint]');
        var statsContainer = document.getElementById('analytics-stats');
        var feedback = document.getElementById('analytics-feedback');
        var nextSteps = document.getElementById('analytics-next-steps');

        if (!main || !statsContainer || !window.feaneGateway) {
            return;
        }

        var endpoint = main.getAttribute('data-analytics-endpoint');
        if (!endpoint) {
            return;
        }

        function showFeedback(message, type) {
            if (!feedback) {
                console.log(message);
                return;
            }

            feedback.textContent = message;
            feedback.classList.remove('d-none', 'alert-success', 'alert-danger', 'alert-info');
            if (type === 'success') {
                feedback.classList.add('alert-success');
            } else if (type === 'info') {
                feedback.classList.add('alert-info');
            } else {
                feedback.classList.add('alert-danger');
            }
        }

        function renderMetrics(metrics) {
            statsContainer.innerHTML = '';

            if (!metrics || (Array.isArray(metrics) && metrics.length === 0)) {
                statsContainer.innerHTML = '<div class="col-12 text-muted">No analytics data available.</div>';
                return;
            }

            if (Array.isArray(metrics)) {
                metrics.forEach(function (metric) {
                    var title = formatValue(metric.value);
                    var description = metric.title || metric.name || 'Metric';
                    statsContainer.innerHTML += createCard(title, description);
                });
                return;
            }

            var mapping = [
                {key: 'totalRevenue', label: 'Total revenue'},
                {key: 'ordersThisMonth', label: 'Orders this month'},
                {key: 'averageRating', label: 'Average rating'}
            ];

            mapping.forEach(function (item) {
                if (metrics[item.key] != null) {
                    statsContainer.innerHTML += createCard(formatValue(metrics[item.key]), item.label);
                }
            });

            Object.keys(metrics).forEach(function (key) {
                if (mapping.some(function (item) {
                    return item.key === key;
                })) {
                    return;
                }
                statsContainer.innerHTML += createCard(formatValue(metrics[key]), key.replace(/([A-Z])/g, ' $1'));
            });
        }

        showFeedback('Loading analytics from gateway…', 'info');
        window.feaneGateway.get(endpoint).then(function (response) {
            if (!response) {
                showFeedback('Analytics gateway returned no data.', 'error');
                renderMetrics([]);
                return;
            }

            if (response.metrics) {
                renderMetrics(response.metrics);
            } else {
                renderMetrics(response);
            }

            showFeedback('Analytics data refreshed.', 'success');
            if (nextSteps) {
                nextSteps.classList.remove('d-none');
            }
        }).catch(function (error) {
            renderMetrics([]);
            showFeedback('Unable to load analytics: ' + error.message, 'error');
        });
    });
})();
