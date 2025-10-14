(function () {
    'use strict';

    document.addEventListener('DOMContentLoaded', function () {
        var main = document.querySelector('main[data-privacy-endpoint]');
        var content = document.getElementById('privacy-content');
        if (!main || !content || !window.feaneGateway) {
            return;
        }

        var endpoint = main.getAttribute('data-privacy-endpoint');
        if (!endpoint) {
            return;
        }

        window.feaneGateway.get(endpoint).then(function (response) {
            if (!response) {
                content.innerHTML = '<p class="text-danger">No privacy policy data returned.</p>';
                return;
            }

            if (Array.isArray(response.sections)) {
                content.innerHTML = response.sections.map(function (section) {
                    var title = section.title ? '<h2 class="h5 mt-4">' + section.title + '</h2>' : '';
                    var body = Array.isArray(section.paragraphs)
                        ? section.paragraphs.map(function (paragraph) { return '<p>' + paragraph + '</p>'; }).join('')
                        : '<p>' + (section.content || '') + '</p>';
                    return title + body;
                }).join('');
                return;
            }

            if (typeof response === 'string') {
                content.innerHTML = '<p>' + response + '</p>';
                return;
            }

            var fallback = response.content || 'Replace this text with your privacy policy fetched from the content gateway.';
            content.innerHTML = '<p>' + fallback + '</p>';
        }).catch(function (error) {
            content.innerHTML = '<p class="text-danger">Unable to load privacy policy: ' + error.message + '</p>';
        });
    });
})();
