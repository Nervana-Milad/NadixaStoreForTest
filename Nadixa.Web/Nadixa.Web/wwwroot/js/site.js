//$(document).ready(function () {
//    $(".js-show-modal1").click(function (e) {
//        e.preventDefault();
//        var productId = $(this).data("id");

//        $.get("/Product/QuickView/" + productId, function (html) {
//            // نحط الـ html في body أو container مخصص
//            $("body").append(html);
//            $(".js-modal1").fadeIn();

//            // غلق المودال
//            $(".js-hide-modal1").click(function () {
//                $(".js-modal1").fadeOut(function () { $(this).remove(); });
//            });
//        });
//    });
//});


//$(document).on("click", ".js-show-modal1", function (e) {

//    e.preventDefault();

//    var productId = $(this).data("id");

//    $.get("/Product/QuickView/" + productId, function (data) {

//        $("#quickViewContainer").html(data);

//        $(".js-modal1").addClass("show-modal1");

//        // تشغيل السلايدر بعد تحميل المودال
//        $('.slick3').slick({
//            slidesToShow: 1,
//            slidesToScroll: 1,
//            fade: true
//        });

//    });

//});