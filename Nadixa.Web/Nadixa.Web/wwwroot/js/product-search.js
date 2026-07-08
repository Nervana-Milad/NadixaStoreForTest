// product-search.js
let timeout = null;

function buildProductCard(p) {
    let cartHtml = "";

    if (p.cartQuantity > 0) {

        cartHtml = `
            <div class="cart-counter">
                <button class="cart-minus"
                        data-product-id="${p.id}">
                    ${p.cartQuantity == 1 ? "🗑" : "-"}
                </button>

                <span class="cart-qty">
                    ${p.cartQuantity} in cart
                </span>

                <button class="cart-plus"
                        data-product-id="${p.id}">
                    +
                </button>
            </div>
        `;

    } else {

        cartHtml = `
            <button class="btn-addcart-card js-addcart-detail hov-btn3"
                    data-product-id="${p.id}">
                Add to Cart
            </button>
        `;
    }

    return `
    <div class="col-sm-6 col-md-4 col-lg-3 isotope-item bag pb-3">
        <div class="block2 block2-shadow h-100">
            <div class="block2-pic hov-img0">
                <img src="${p.mainImageUrlPath || '/images/no-image.png'}"
                     alt="${p.name}"
                     class="img-fluid"
                     style="height:270px; object-fit:cover;">
                <a class="block2-btn flex-c-m stext-103 cl2 size-102 bg0 bor2 hov-btn1 p-lr-15 trans-04 js-show-modal1 pointer"
                   data-id="${p.id}">
                    Quick View
                </a>
            </div>
            <div class="block2-txt flex-w flex-t p-t-14">
                <div class="block2-txt-child1 flex-col-l px-3">
                    <a href="/Product/Detail/${p.id}"
                       class="stext-104 cl4 hov-cl1 trans-04 js-name-b2 p-b-6">
                        ${p.name}
                    </a>
                    <span class="stext-105 cl3">$${p.price}</span>
                </div>
                <div class="block2-txt-child2 flex-r p-3">
                    <button class="js-addwish-detail fs-20 cl3 hov-cl1 trans-04 lh-10 p-lr-5 p-tb-2 wishlist-btn"
                            data-product-id="${p.id}"
                            data-product-name="${p.name}">
                        <i class="zmdi zmdi-favorite-outline"></i>
                    </button>
                </div>
            </div>
            <div class="px-3 pt-2">
                <p class="card-text">${p.description}</p>
            </div>
            <div class="px-3 pt-2 d-flex justify-content-end">
                <span class="badge badge-secondary mb-4">${p.categoryName}</span>
            </div>
            <div class="px-3 pb-3 mt-auto">
                <div class="cart-controls" data-product-id="${p.id}">
                    ${cartHtml}
                </div>
            </div>
        </div>
    </div>`;
}

$("#searchInput").on("keyup", function () {
    let query = $(this).val();
    clearTimeout(timeout);

    timeout = setTimeout(function () {
        $.ajax({
            url: "/Product/Search",
            type: "GET",
            data: { term: query },
            success: function (data) {
                let html = data.length === 0
                    ? `<div class="col-12 text-center"><p>No products found</p></div>`
                    : data.map(p => buildProductCard(p)).join("");

                $("#productsContainer").html(html);
                $("#productsContainer").isotope("destroy");
                $("#productsContainer").isotope({
                    itemSelector: ".isotope-item",
                    layoutMode: "fitRows"
                });
            }
        });
    }, 300);
});