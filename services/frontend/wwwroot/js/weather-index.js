(function () {
    'use strict';

    document.addEventListener('DOMContentLoaded', function () {
        var main = document.querySelector('main[data-weather-endpoint]');
        if (!main || !window.feaneGateway) {
            return;
        }

        var endpoint = main.getAttribute('data-weather-endpoint');
        var cityEl = document.getElementById('weather-city');
        var descriptionEl = document.getElementById('weather-description');
        var tempEl = document.getElementById('weather-temp');
        var humidityEl = document.getElementById('weather-humidity');
        var windEl = document.getElementById('weather-wind');
        var feelsEl = document.getElementById('weather-feels');

        window.feaneGateway.get(endpoint).then(function (data) {
            if (!data) {
                descriptionEl.textContent = 'Weather data unavailable.';
                return;
            }

            cityEl.textContent = data.city || data.location || cityEl.textContent;
            descriptionEl.textContent = data.description || data.summary || descriptionEl.textContent;
            tempEl.textContent = data.temperature != null ? data.temperature + '°C' : tempEl.textContent;
            humidityEl.textContent = data.humidity != null ? data.humidity + '%' : humidityEl.textContent;
            windEl.textContent = data.windSpeed != null ? data.windSpeed + ' km/h' : windEl.textContent;
            feelsEl.textContent = data.feelsLike != null ? data.feelsLike + '°C' : feelsEl.textContent;
        }).catch(function (error) {
            descriptionEl.textContent = 'Failed to load weather: ' + error.message;
        });
    });
})();
