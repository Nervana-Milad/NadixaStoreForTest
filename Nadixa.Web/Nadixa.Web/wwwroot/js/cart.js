$(document).on(
    'click',
    '.js-addcart-detail',
    function (e) {

        e.preventDefault();
        e.stopPropagation();

        var button = $(this);

        var productId =
            button.data("product-id");

        var nameProduct =
            $(this)
                .closest('.js-product')
                .find('.js-name-detail')
                .text();

        var quantity =
            button
                .closest('.flex-w')
                .find('.num-product')
                .val();

        $.ajax({

            url: "/Cart/AddToCart",

            type: "POST",

            data: {
                productId: productId,
                quantity: quantity
            },

            success: function (response) {

                if (
                    response.requiresLogin
                ) {

                    showLoginRequired(
                        response.message,
                        window.location.pathname
                    );

                    return;
                }

                if (response.success) {

                    showSuccess(
                        response.message,
                        nameProduct
                    );

                    $("#cart-count")
                        .attr(
                            "data-notify",
                            response.cartCount
                        );

                    loadMiniCart();

                } else {

                    showError(
                        response.message
                    );
                }
            },

            error: function () {

                showError(
                    "Something went wrong."
                );
            }
        });
    }
);

$(document).on('click', '.move-to-cart', function () {

    var button = $(this);

    var productId =
        button.data("product-id");

    $.post(
        "/Cart/AddToCart",
        { productId: productId },
        function (res) {

            if (res.requiresLogin) {

                showLoginRequired(
                    res.message,
                    window.location.pathname
                );

                return;
            }

            if (res.success) {

                showSuccess(res.message);

                $("#cart-count")
                    .attr(
                        "data-notify",
                        res.cartCount
                    );

                loadMiniCart();

            } else {

                showError(res.message);
            }
        }
    )
        .fail(function () {

            showError(
                "Something went wrong."
            );
        });

});