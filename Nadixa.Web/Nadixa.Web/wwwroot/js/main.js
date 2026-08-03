
(function ($) {
    "use strict";

    /*[ Load page ]
    ===========================================================*/
    $(".animsition").animsition({
        inClass: 'fade-in',
        outClass: 'fade-out',
        inDuration: 1500,
        outDuration: 800,
        linkElement: '.animsition-link',
        loading: true,
        loadingParentElement: 'html',
        loadingClass: 'animsition-loading-1',
        loadingInner: '<div class="loader05"></div>',
        timeout: false,
        timeoutCountdown: 5000,
        onLoadEvent: true,
        browser: [ 'animation-duration', '-webkit-animation-duration'],
        overlay : false,
        overlayClass : 'animsition-overlay-slide',
        overlayParentElement : 'html',
        transition: function(url){ window.location.href = url; }
    });
    
    /*[ Back to top ]
    ===========================================================*/
    var windowH = $(window).height()/2;

    $(window).on('scroll',function(){
        if ($(this).scrollTop() > windowH) {
            $("#myBtn").css('display','flex');
        } else {
            $("#myBtn").css('display','none');
        }
    });

    $('#myBtn').on("click", function(){
        $('html, body').animate({scrollTop: 0}, 300);
    });


    /*==================================================================
    [ Fixed Header ]*/
    var headerDesktop = $('.container-menu-desktop');
    var wrapMenu = $('.wrap-menu-desktop');

    if($('.top-bar').length > 0) {
        var posWrapHeader = $('.top-bar').height();
    }
    else {
        var posWrapHeader = 0;
    }
    

    if($(window).scrollTop() > posWrapHeader) {
        $(headerDesktop).addClass('fix-menu-desktop');
        $(wrapMenu).css('top',0); 
    }  
    else {
        $(headerDesktop).removeClass('fix-menu-desktop');
        $(wrapMenu).css('top',posWrapHeader - $(this).scrollTop()); 
    }

    $(window).on('scroll',function(){
        if($(this).scrollTop() > posWrapHeader) {
            $(headerDesktop).addClass('fix-menu-desktop');
            $(wrapMenu).css('top',0); 
        }  
        else {
            $(headerDesktop).removeClass('fix-menu-desktop');
            $(wrapMenu).css('top',posWrapHeader - $(this).scrollTop()); 
        } 
    });


    /*==================================================================
    [ Menu mobile ]*/
    $('.btn-show-menu-mobile').on('click', function(){
        $(this).toggleClass('is-active');
        $('.menu-mobile').slideToggle();
    });

    var arrowMainMenu = $('.arrow-main-menu-m');

    for(var i=0; i<arrowMainMenu.length; i++){
        $(arrowMainMenu[i]).on('click', function(){
            $(this).parent().find('.sub-menu-m').slideToggle();
            $(this).toggleClass('turn-arrow-main-menu-m');
        })
    }

    $(window).resize(function(){
        if($(window).width() >= 992){
            if($('.menu-mobile').css('display') == 'block') {
                $('.menu-mobile').css('display','none');
                $('.btn-show-menu-mobile').toggleClass('is-active');
            }

            $('.sub-menu-m').each(function(){
                if($(this).css('display') == 'block') { console.log('hello');
                    $(this).css('display','none');
                    $(arrowMainMenu).removeClass('turn-arrow-main-menu-m');
                }
            });
                
        }
    });


    /*==================================================================
    [ Show / hide modal search ]*/
    $('.js-show-modal-search').on('click', function(){
        $('.modal-search-header').addClass('show-modal-search');
        $(this).css('opacity','0');
    });

    $('.js-hide-modal-search').on('click', function(){
        $('.modal-search-header').removeClass('show-modal-search');
        $('.js-show-modal-search').css('opacity','1');
    });

    $('.container-search-header').on('click', function(e){
        e.stopPropagation();
    });


    /*==================================================================
    [ Isotope ]*/
    var $topeContainer = $('.isotope-grid');
    var $filter = $('.filter-tope-group');

    // filter items on button click
    $filter.each(function () {
        $filter.on('click', 'button', function () {
            var filterValue = $(this).attr('data-filter');
            $topeContainer.isotope({filter: filterValue});
        });
        
    });

    // init Isotope
    $(window).on('load', function () {
        var $grid = $topeContainer.each(function () {
            $(this).isotope({
                itemSelector: '.isotope-item',
                layoutMode: 'fitRows',
                percentPosition: true,
                animationEngine : 'best-available',
                masonry: {
                    columnWidth: '.isotope-item'
                }
            });
        });
    });

    var isotopeButton = $('.filter-tope-group button');

    $(isotopeButton).each(function(){
        $(this).on('click', function(){
            for(var i=0; i<isotopeButton.length; i++) {
                $(isotopeButton[i]).removeClass('how-active1');
            }

            $(this).addClass('how-active1');
        });
    });

    /*==================================================================
    [ Filter / Search product ]*/
    $('.js-show-filter').on('click',function(){
        $(this).toggleClass('show-filter');
        $('.panel-filter').slideToggle(400);

        if($('.js-show-search').hasClass('show-search')) {
            $('.js-show-search').removeClass('show-search');
            $('.panel-search').slideUp(400);
        }    
    });

    $('.js-show-search').on('click',function(){
        $(this).toggleClass('show-search');
        $('.panel-search').slideToggle(400);

        if($('.js-show-filter').hasClass('show-filter')) {
            $('.js-show-filter').removeClass('show-filter');
            $('.panel-filter').slideUp(400);
        }    
    });

    /*==================================================================
[ Cart ]*/
    $(document).on('click', '.js-show-cart', function () {
        $('.js-panel-cart').addClass('show-header-cart');
    });

    $(document).on('click', '.js-hide-cart', function () {
        $('.js-panel-cart').removeClass('show-header-cart');
    });

    /*==================================================================
    [ Cart ]*/
    $('.js-show-sidebar').on('click',function(){
        $('.js-sidebar').addClass('show-sidebar');
    });

    $('.js-hide-sidebar').on('click',function(){
        $('.js-sidebar').removeClass('show-sidebar');
    });

    /*==================================================================
    [ +/- num product ]*/


    $(document).on('click', '.btn-num-product-up', function () {

        var input = $(this).prev();

        var numProduct = Number(input.val());

        var max = Number(input.attr('max'));

        if (!max || numProduct < max) {

            input.val(numProduct + 1);

        } else {

            swal("Sorry!", "Only " + max + " items available in stock.", "warning");
        }
    });


    $(document).on('click', '.btn-num-product-down', function () {

        var input = $(this).next();

        var numProduct = Number(input.val());

        var min = Number(input.attr('min')) || 1;

        if (numProduct > min) {

            input.val(numProduct - 1);
        }
    });
    /*==================================================================
    [ Rating ]*/
    $('.wrap-rating').each(function(){
        var item = $(this).find('.item-rating');
        var rated = -1;
        var input = $(this).find('input');
        $(input).val(0);

        $(item).on('mouseenter', function(){
            var index = item.index(this);
            var i = 0;
            for(i=0; i<=index; i++) {
                $(item[i]).removeClass('zmdi-star-outline');
                $(item[i]).addClass('zmdi-star');
            }

            for(var j=i; j<item.length; j++) {
                $(item[j]).addClass('zmdi-star-outline');
                $(item[j]).removeClass('zmdi-star');
            }
        });

        $(item).on('click', function(){
            var index = item.index(this);
            rated = index;
            $(input).val(index+1);
        });

        $(this).on('mouseleave', function(){
            var i = 0;
            for(i=0; i<=rated; i++) {
                $(item[i]).removeClass('zmdi-star-outline');
                $(item[i]).addClass('zmdi-star');
            }

            for(var j=i; j<item.length; j++) {
                $(item[j]).addClass('zmdi-star-outline');
                $(item[j]).removeClass('zmdi-star');
            }
        });
    });
    
    /*==================================================================*/
    window.loadMiniCart = function () {
        $("#mini-cart-container").load("/Cart/GetMiniCart");
    };


    /*==================================================================*/
   


})(jQuery);

// Delete product modal
$(document).on("click", ".delete-product-btn", function () {
    var productName = $(this).data("product-name");
    Notify.confirm({
        title: "Delete " + productName,
        message: "Are you sure you want to delete this product?",
        onConfirm: function () {
            $("#deleteProductForm").submit();
        }
    });
});

// Delete blog
$(document).on("click", ".delete-blog-btn", function () {
    var blogTitle = $(this).data("blog-title");
    Notify.confirm({
        title: "Delete " + blogTitle,
        message: "Are you sure you want to delete this blog?",
        onConfirm: function () {
            $("#deleteBlogForm").submit();
        }
    });
});


// Delete Comment 
$(document).on("click", ".delete-comment-btn", function () {
    var commentId = $(this).data("comment-id");
    var commentCard = $(this).closest(".comment-card");

    Notify.confirm({
        title: "Delete Comment",
        message: "Are you sure you want to delete this comment?",
        onConfirm: function () {
            $.ajax({
                url: "/Blog/DeleteComment",
                type: "POST",
                data: { id: commentId },
                success: function (response) {
                    if (!response.success) {
                        Notify.error(response.message);
                        return;
                    }
                    commentCard.fadeOut(300, function () {
                        $(this).remove();
                    });
                    Notify.success("Comment deleted successfully");
                },
                error: function () {
                    Notify.error("Something went wrong");
                }
            });
        }
    });
});


// delete prod category
$(document).on("click", ".delete-category-btn", function () {
    var categoryId = $(this).data("category-id");
    var categoryName = $(this).data("category-name");

    Notify.confirm({
        title: "Delete " + categoryName,
        message: "Are you sure you want to delete this category?",
        onConfirm: function () {
            var form = $("#deleteCategoryForm");
            form.attr("action", "/ProductCategory/Delete/" + categoryId);
            form.submit();
        }
    });
});


// delete prod SubCategory
$(document).on("click", ".delete-subcategory-btn", function () {
    var subCategoryId = $(this).data("subcategory-id");
    var subCategoryName = $(this).data("subcategory-name");

    Notify.confirm({
        title: "Delete " + subCategoryName,
        message: "Are you sure you want to delete this subcategory?",
        onConfirm: function () {
            var form = $("#deleteSubCategoryForm");
            form.attr("action", "/ProductSubCategory/Delete/" + subCategoryId);
            form.submit();
        }
    });
});

// delete permission
$(document).on("click", ".delete-permission-btn", function () {
    var permissionId = $(this).data("permission-id");
    var permissionName = $(this).data("permission-name");

    Notify.confirm({
        title: "Delete " + permissionName,
        message: "Are you sure you want to delete this permission? Any Sub-Admin currently using it will lose this access.",
        onConfirm: function () {
            var form = $("#deletePermissionForm");
            form.find("input[name='id']").remove();
            form.append('<input type="hidden" name="id" value="' + permissionId + '">');
            form.submit();
        }
    });
});


// Cancel Order modal
$(document).on("click", ".cancel-order-btn", function () {
    var productName = $(this).data("product-name");
    Notify.confirm({
        title: "Cancel Order",
        message: "Are you sure you want to cancel this Order?",
        onConfirm: function () {
            $("#cancelOrderConfirm").submit();
        }
    });
});

// delete coupon
$(document).on("click", ".delete-coupon-btn", function () {
    var couponId = $(this).data("coupon-id");
    var couponName = $(this).data("coupon-name");

    Notify.confirm({
        title: "Delete " + couponName,
        message: "Are you sure you want to delete this coupon?",
        onConfirm: function () {
            var form = $("#deleteCouponForm");
            form.find("input[name='id']").remove();
            form.append('<input type="hidden" name="id" value="' + couponId + '">');
            form.submit();
        }
    });
});


// Remove Item from cart
$(document).on("click", ".remove-item-btn", function () {
    var productId = $(this).data("product-id");
    var row = $(this).closest("tr");

    Notify.confirm({
        title: "Remove Item",
        message: "Are you sure you want to remove this item?",
        onConfirm: function () {
            $.ajax({
                url: "/Cart/RemoveFromCart",
                type: "POST",
                data: { productId: productId },
                success: function (response) {
                    if (response.success) {
                        row.fadeOut(300, function () {
                            $(this).remove();
                            updateStockWarningState();
                            if ($(".table_row").length === 0) {
                                location.reload();
                            }
                        });
                        showSuccess(response.message);
                        document.querySelector(".icon-header-noti")
                            ?.setAttribute("data-notify", response.cartCount);
                        loadMiniCart();
                    } else {
                        showError("Could not remove item");
                    }
                },
                error: function () {
                    showError("Something went wrong");
                }
            });
        }
    });
});
function updateStockWarningState() {
    var stillHasIssues = $("tr[data-stock-issue='true']").length > 0;

    if (stillHasIssues) {
        $("#stockWarningContainer").show();
        $("#checkoutBtn")
            .addClass("disabled")
            .attr("aria-disabled", "true")
            .attr("tabindex", "-1")
            .css({ "pointer-events": "none", "opacity": "0.5", "cursor": "not-allowed" });
    } else {
        $("#stockWarningContainer").hide();
        $("#checkoutBtn")
            .removeClass("disabled")
            .attr("aria-disabled", "false")
            .removeAttr("tabindex")
            .css({ "pointer-events": "auto", "opacity": "1", "cursor": "pointer" });
    }
}

// global search
let globalSearchTimeout = null;

$("#globalSearchInput").on("keyup", function () {
    var term = $(this).val().trim();

    clearTimeout(globalSearchTimeout);

    if (term.length < 2) {
        $("#globalSearchResults").hide();
        return;
    }

    globalSearchTimeout = setTimeout(function () {
        $.ajax({
            url: "/Home/GlobalSearch",
            type: "GET",
            data: { term: term },
            success: function (data) {
                var html = "";

                if (data.products.length > 0) {
                    html += `<div style="padding:10px 16px; font-size:11px; font-weight:600; color:#a0aec0; text-transform:uppercase; letter-spacing:.05em;">Products</div>`;
                    data.products.forEach(p => {
                        html += `
                        <a href="${p.url}" style="display:flex; align-items:center; gap:12px; padding:10px 16px; text-decoration:none; border-bottom:1px solid #f7fafc;">
                            <img src="${p.imageUrl || '/images/no-image.png'}" style="width:40px; height:40px; object-fit:cover; border-radius:6px;">
                            <div>
                                <div style="font-size:13px; font-weight:500; color:#2d3748;">${p.name}</div>
                                <div style="font-size:12px; color:#6c7ae0;">$${p.price}</div>
                            </div>
                        </a>`;
                    });
                }

                if (data.categories.length > 0) {
                    html += `<div style="padding:10px 16px; font-size:11px; font-weight:600; color:#a0aec0; text-transform:uppercase; letter-spacing:.05em;">Categories</div>`;
                    data.categories.forEach(c => {
                        html += `
                        <a href="${c.url}" style="display:flex; align-items:center; gap:12px; padding:10px 16px; text-decoration:none; border-bottom:1px solid #f7fafc;">
                            <i class="zmdi zmdi-folder" style="font-size:20px; color:#6c7ae0;"></i>
                            <div style="font-size:13px; font-weight:500; color:#2d3748;">${c.name}</div>
                        </a>`;
                    });
                }

                if (data.blogs.length > 0) {
                    html += `<div style="padding:10px 16px; font-size:11px; font-weight:600; color:#a0aec0; text-transform:uppercase; letter-spacing:.05em;">Blogs</div>`;
                    data.blogs.forEach(b => {
                        html += `
                        <a href="${b.url}" style="display:flex; align-items:center; gap:12px; padding:10px 16px; text-decoration:none; border-bottom:1px solid #f7fafc;">
                            <i class="zmdi zmdi-assignment" style="font-size:20px; color:#6c7ae0;"></i>
                            <div style="font-size:13px; font-weight:500; color:#2d3748;">${b.name}</div>
                        </a>`;
                    });
                }

                if (html === "") {
                    html = `<div style="padding:16px; text-align:center; color:#a0aec0; font-size:13px;">No results found</div>`;
                }

                $("#globalSearchResults").html(html).show();
            }
        });
    }, 300);
});

$(document).on("click", function (e) {
    if (!$(e.target).closest("#globalSearchInput, #globalSearchResults").length) {
        $("#globalSearchResults").hide();
    }
});

// Notify Me button
$(document).on("click", ".btn-notify-me", function () {
    var btn = $(this);
    var productId = btn.data("product-id");

    $.ajax({
        url: "/Product/NotifyMeWhenAvailable",
        type: "POST",
        data: { productId: productId },
        success: function (response) {
            if (response.requiresLogin) {
                showError(response.message);
                return;
            }
            if (response.success) {
                showSuccess(response.message);
                btn.prop("disabled", true)
                    .css({ opacity: 0.6, cursor: "not-allowed" })
                    .html('<i class="zmdi zmdi-check"></i> We\'ll notify you!');
            } else {
                showError(response.message);
            }
        },
        error: function () {
            showError("Something went wrong");
        }
    });
});