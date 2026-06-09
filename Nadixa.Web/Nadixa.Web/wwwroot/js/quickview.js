$(document).ready(function () {
    $(".js-show-modal1").click(function (e) {
        e.preventDefault();
        var productId = $(this).data("id");

        $.get("/Product/QuickView/" + productId, function (html) {
            // نحط الـ html في body أو container مخصص
            $("body").append(html);
            $(".js-modal1").fadeIn();

            // غلق المودال
            $(".js-hide-modal1").click(function () {
                $(".js-modal1").fadeOut(function () { $(this).remove(); });
            });
        });
    });
});

$(document).on('click', '.js-show-modal1', function (e) {
	e.preventDefault();
	var productId = $(this).data('id');

	$.get("/Product/QuickView/" + productId, function (data) {
		$("#quickViewContainer").html(data);
		if (typeof updateWishlistUI === "function") {
			updateWishlistUI();
		}
		$('.js-modal1').addClass('show-modal1');
		$('.wrap-slick3').each(function () {
			var slick = $(this).find('.slick3');
			if (slick.hasClass('slick-initialized')) {
				slick.slick('unslick');
			}
			slick.slick({
				slidesToShow: 1,
				slidesToScroll: 1,
				fade: true,
				infinite: true,
				arrows: true,
				appendArrows: $(this).find('.wrap-slick3-arrows'),
				prevArrow: '<button class="arrow-slick3 prev-slick3"><i class="fa fa-angle-left"></i></button>',
				nextArrow: '<button class="arrow-slick3 next-slick3"><i class="fa fa-angle-right"></i></button>',
				dots: true,
				appendDots: $(this).find('.wrap-slick3-dots'),
				dotsClass: 'slick3-dots',
				customPaging: function (slick, index) {
					var portrait = $(slick.$slides[index]).data('thumb');
					return '<img src="' + portrait + '"/><div class="slick3-dot-overlay"></div>';
				}
			});
		});
	});
});
$(document).on('click', '.js-hide-modal1', function () {
	$('.js-modal1').removeClass('show-modal1');
});