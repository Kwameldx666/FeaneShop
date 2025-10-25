// Год в футере
function getYear() {
    var el = document.getElementById('displayYear');
    if (el) el.textContent = new Date().getFullYear();
}

(function () {
    'use strict';

    // ---- Isotope (с защитой) ----
    var $grid = null;

    function initIsotope() {
        if (!window.jQuery || !jQuery.fn || !jQuery.fn.isotope) {
            console.warn('Isotope не загружен');
            return;
        }
        var $g = jQuery('.grid');
        if (!$g.length) return;

        $grid = $g.isotope({
            itemSelector: '.all',
            percentPosition: false,
            masonry: { columnWidth: '.all' },
            getSortData: {
                name: '.name',
                price: function ($elem) {
                    var $p = $elem.find('.price');
                    var n = parseFloat(String($p.text() || '').replace('$', ''));
                    return isNaN(n) ? 0 : n;
                }
            }
        });

        // фильтр по категории
        jQuery('.filters_menu li').off('click.feane').on('click.feane', function () {
            jQuery('.filters_menu li').removeClass('active');
            jQuery(this).addClass('active');
            var val = jQuery(this).attr('data-filter') || '*';
            $grid.isotope({ filter: val });
        });

        // сортировка
        jQuery('#sort').off('change.feane').on('change.feane', function () {
            $grid.isotope({ sortBy: jQuery(this).val() || 'original-order' });
        });
    }

    // дебаунс для resize
    var resizeTimer = null;
    function onResize() {
        if (!$grid) return;
        clearTimeout(resizeTimer);
        resizeTimer = setTimeout(function () {
            try { $grid.isotope('layout'); } catch (e) { }
        }, 150);
    }

    // ---- Nice Select (с защитой и реинитом) ----
    function initNiceSelect(scope) {
        if (!window.jQuery || !jQuery.fn || !jQuery.fn.niceSelect) {
            console.warn('niceSelect не загружен');
            return;
        }
        var $root = scope ? jQuery(scope) : jQuery(document);
        $root.find('select').each(function () {
            var $sel = jQuery(this);
            if (!$sel.next('.nice-select').length) {
                $sel.niceSelect();
            }
        });
    }

    // ---- Owl Carousel (с защитой) ----
    function initOwl() {
        if (!window.jQuery || !jQuery.fn || !jQuery.fn.owlCarousel) {
            console.warn('OwlCarousel не загружен');
            return;
        }
        var $owl = jQuery('.client_owl-carousel');
        if (!$owl.length) return;
        $owl.owlCarousel({
            loop: true,
            margin: 0,
            dots: false,
            nav: true,
            autoplay: true,
            autoplayHoverPause: true,
            navText: [
                '<i class="fa fa-angle-left" aria-hidden="true"></i>',
                '<i class="fa fa-angle-right" aria-hidden="true"></i>'
            ],
            responsive: { 0: { items: 1 }, 768: { items: 2 }, 1000: { items: 2 } }
        });
    }

    // ---- Инициализация ----
    jQuery(function () {
        getYear();
        initIsotope();
        initNiceSelect();
        initOwl();
        jQuery(window).on('resize.feane', onResize);
    });

    // после подгрузки navbar/footer через partials-loader
    document.addEventListener('partials:loaded', function () {
        initNiceSelect(document);
        // при необходимости можно повторно инициализировать Owl внутри партиалов
        initOwl();
    });

    (function () {
        // чтобы не инициализировать карту дважды
        var mapInited = false;
        var map, marker;

        // Глобальная функция для callback из Google SDK
        window.initMap = function () {
            if (mapInited) return;
            var container = document.getElementById('googleMap');
            if (!container || !window.google || !google.maps) {
                console.warn('Google Maps: контейнер не найден или SDK не загружен');
                return;
            }

            mapInited = true;

            // стартовый центр (замени на свой)
            var center = { lat: 40.712775, lng: -74.005973 };

            map = new google.maps.Map(container, {
                center: center,
                zoom: 14,
                mapTypeControl: false,
                streetViewControl: false,
                fullscreenControl: false
            });

            marker = new google.maps.Marker({
                position: center,
                map: map
            });

            // Autocomplete по полю #map-search (если есть)
            var input = document.getElementById('map-search');
            if (input && google.maps.places) {
                var ac = new google.maps.places.Autocomplete(input, { fields: ['geometry', 'name'] });
                ac.addListener('place_changed', function () {
                    var place = ac.getPlace();
                    if (!place || !place.geometry || !place.geometry.location) return;
                    map.panTo(place.geometry.location);
                    map.setZoom(15);
                    if (marker) marker.setPosition(place.geometry.location);
                });
            }

            // Перестроение при ресайзе
            window.addEventListener('resize', function () {
                if (!map) return;
                var c = map.getCenter();
                google.maps.event.trigger(map, 'resize');
                map.setCenter(c);
            }, { passive: true });
        };

        // Если partial’ы подгружаются и контейнер появится позже,
        // можно инициировать повторно (initMap сам себя защитит).
        document.addEventListener('partials:loaded', function () {
            if (window.google && google.maps && typeof window.initMap === 'function') {
                window.initMap();
            }
        });
    })();
})();
