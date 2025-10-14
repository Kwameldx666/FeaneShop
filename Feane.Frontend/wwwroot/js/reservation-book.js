(function () {
    window.initMap = function () {
        const mapElement = document.getElementById('map');
        if (!mapElement) {
            return;
        }

        const options = {
            center: { lat: 47.0105, lng: 28.8638 },
            zoom: 12
        };

        new google.maps.Map(mapElement, options);
    };
})();
