(function () {
    'use strict';

    document.addEventListener('DOMContentLoaded', function () {
        var section = document.querySelector('.about_section[data-about-endpoint]');
        var content = document.getElementById('about-content');
        if (!section || !content || !window.feaneGateway) {
            return;
        }

        var endpoint = section.getAttribute('data-about-endpoint');
        if (!endpoint) {
            return;
        }

        content.innerHTML = '<p>Loading our story…</p>';

        window.feaneGateway.get(endpoint).then(function (response) {
            if (!response) {
                content.innerHTML = '<p class="text-danger">No story data returned from the gateway.</p>';
                return;
            }

            if (Array.isArray(response.paragraphs)) {
                content.innerHTML = response.paragraphs.map(function (paragraph) {
                    return '<p>' + paragraph + '</p>';
                }).join('');
                return;
            }

            if (typeof response === 'string') {
                content.innerHTML = '<p>' + response + '</p>';
                return;
            }

            var description = response.description || response.story || 'Feane is ready to be described via your content service.';
            content.innerHTML = '<p>' + description + '</p>';
        }).catch(function (error) {
            content.innerHTML = '<p class="text-danger">Unable to load story: ' + error.message + '</p>';
        });
    });
})();
