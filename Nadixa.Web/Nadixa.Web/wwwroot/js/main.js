
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
    $('.js-show-cart').on('click',function(){
        $('.js-panel-cart').addClass('show-header-cart');
    });

    $('.js-hide-cart').on('click',function(){
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