function updateWishlistUI() {

    $(".js-addwish-detail").each(function () {

        var btn = $(this);
        var productId = btn.data("product-id");

        if (window.wishlistIds.includes(productId)) {

            btn.addClass("js-addedwish-detail");

            btn.find("i")
                .removeClass("zmdi-favorite-outline")
                .addClass("zmdi-favorite");

        } else {

            btn.removeClass("js-addedwish-detail");

            btn.find("i")
                .removeClass("zmdi-favorite")
                .addClass("zmdi-favorite-outline");
        }
    });
}

$(document).on('click', '.js-addwish-detail', function (e) {

    e.preventDefault();
    e.stopPropagation();

    var button = $(this);

    if (button.prop("disabled"))
        return;

    button.prop("disabled", true);

    var productId = button.data("product-id");
    var nameProduct = button.data("product-name");

    $.ajax({
        url: "/Wishlist/Toggle",
        type: "POST",
        data: { productId: productId },

        success: function (res) {

            if (res.requiresLogin) {

                showLoginRequired(
                    res.message,
                    window.location.pathname
                );

                return;
            }

            if (res.success) {

                if (res.isAdded) {

                    if (!window.wishlistIds.includes(productId)) {

                        window.wishlistIds.push(productId);
                    }

                } else {

                    window.wishlistIds =
                        window.wishlistIds.filter(
                            id => id !== productId
                        );
                }

                updateWishlistUI();

                showSuccess(
                    res.message,
                    nameProduct
                );

                $("#wishlist-count")
                    .attr(
                        "data-notify",
                        res.count ?? 0
                    );
            }
        },

        error: function () {

            showError(
                "Something went wrong."
            );
        },

        complete: function () {

            button.prop("disabled", false);
        }
    });
});

$(document).ready(function () {

    updateWishlistUI();

});