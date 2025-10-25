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

    window.initMap = function () {
        var mapElement = document.getElementById('map');
        if (!mapElement) {
            return;
        }

        var options = {
            center: { lat: 47.0105, lng: 28.8638 },
            zoom: 12
        };

        new google.maps.Map(mapElement, options);
    };

    document.addEventListener('DOMContentLoaded', function () {
        var form = document.querySelector('.booking-form');
        var feedback = document.getElementById('reservation-feedback');
        if (!form || !window.feaneGateway) {
            return;
        }

        var endpoint = form.getAttribute('data-gateway-endpoint');
        if (!endpoint) {
            return;
        }

        form.addEventListener('submit', function (event) {
            event.preventDefault();

            var formData = new FormData(form);
            var payload = {};
            formData.forEach(function (value, key) {
                payload[key] = value;
            });

            showFeedback(feedback, 'Sending reservation request…', 'info');

            window.feaneGateway.post(endpoint, payload).then(function (data) {
                if (data && (data.success || data.status === 'success')) {
                    showFeedback(feedback, data.message || 'Reservation request sent successfully!', 'success');
                    form.reset();
                    return;
                }

                showFeedback((data && data.message) || 'Unable to submit reservation.', 'error');
            }).catch(function (error) {
                showFeedback(feedback, 'Failed to submit reservation: ' + error.message, 'error');
            });
        });
    });
})();
