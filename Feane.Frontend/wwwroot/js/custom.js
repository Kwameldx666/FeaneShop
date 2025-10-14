// to get current year
function getYear() {
    var currentDate = new Date();
    var currentYear = currentDate.getFullYear();
    var displayYearElement = document.querySelector("#displayYear");
    if (displayYearElement) {
        displayYearElement.innerHTML = currentYear;
    } else {
        console.error('Element with id "displayYear" not found');
    }
}






// isotope js


    // Фильтрация по категории
    $(window).on('load', function () {
        var $grid = $('.grid').isotope({
            itemSelector: '.all',
            percentPosition: false,
            masonry: {
                columnWidth: '.all'
            },
            getSortData: {
                name: '.name',
                price: function ($elem) {
                    if ($elem instanceof jQuery) {
                        return parseFloat($elem.find('.price').text().replace('$', ''));
                    } else {
                        console.error('$elem is not a jQuery object');
                        return 0;
                    }
                }
            }
        });

        $('.filters_menu li').click(function () {
            $('.filters_menu li').removeClass('active');
            $(this).addClass('active');

            var filterValue = $(this).attr('data-filter');
            $grid.isotope({
                filter: filterValue
            });
        });

        $('#sort').on('change', function () {
            var sortValue = $(this).val();
            $grid.isotope({
                sortBy: sortValue
            });
        });

        $(window).resize(function () {
            $grid.isotope('layout');
        });
    });


    // Обновление сетки при изменении размера окна
    $(window).resize(function () {
        $grid.isotope('layout');
    });


// nice select
$(document).ready(function () {
    $('select').niceSelect();
});

/** google_map js **/
/** google_map js **/
/*function myMap() {
    var mapProp = {
        center: new google.maps.LatLng(40.712775, -74.005973),
        zoom: 18,
    };
    var map = new google.maps.Map(document.getElementById("googleMap"), mapProp);
}*/




// client section owl carousel
$(".client_owl-carousel").owlCarousel({
    loop: true,
    margin: 0,
    dots: false,
    nav: true,
    navText: [],
    autoplay: true,
    autoplayHoverPause: true,
    navText: [
        '<i class="fa fa-angle-left" aria-hidden="true"></i>',
        '<i class="fa fa-angle-right" aria-hidden="true"></i>'
    ],
    responsive: {
        0: {
            items: 1
        },
        768: {
            items: 2
        },
        1000: {
            items: 2
        }
    }
});
