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
                .val() || 1;

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

                    updateCartButton(
                        productId,
                        response.quantity
                    );

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
                updateCartButton(
                    productId,
                    res.quantity
                );
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

$(document).on("click", ".cart-plus", function () {

    let productId = $(this).data("product-id");

    $.post("/Cart/AddToCart",
        {
            productId: productId,
            quantity: 1
        },
        function (res) {

            if (!res.success) {
                showError(res.message);
                return;
            }

            updateCartButton(productId, res.quantity);

            $("#cart-count")
                .attr("data-notify", res.cartCount);

            loadMiniCart();

        });

});

$(document).on("click", ".cart-minus, .cart-remove", function () {

    let productId = $(this).data("product-id");

    $.post("/Cart/DecreaseQuantity",
        {
            productId: productId
        },
        function (res) {

            if (!res.success) {
                showError("Something went wrong.");
                return;
            }

            $("#cart-count")
                .attr("data-notify", res.cartCount);

            loadMiniCart();

            if (res.quantity == 0) {

                $(".cart-controls[data-product-id='" + productId + "']")
                    .html(`
                        <button class="btn-addcart-card js-addcart-detail"
                                data-product-id="${productId}">
                            Add to Cart
                        </button>
                    `);

            }
            else {

                updateCartButton(
                    productId,
                    res.quantity
                );

            }

        });

});
function updateCartButton(productId, quantity) {

    const container = $(".cart-controls[data-product-id='" + productId + "']");

    if (!container.length)
        return;

    const leftButton =
        quantity === 1
            ? `<button class="cart-remove"
                       data-product-id="${productId}">
                    <i class="zmdi zmdi-shopping-cart"></i>
               </button>`
            : `<button class="cart-minus"
                       data-product-id="${productId}">
                    -
               </button>`;


    container.html(`
        <div class="cart-counter">
            ${leftButton}
            <span>${quantity} in cart</span>
            <button class="cart-plus"
                    data-product-id="${productId}">
                +
            </button>
        </div>
    `);

}