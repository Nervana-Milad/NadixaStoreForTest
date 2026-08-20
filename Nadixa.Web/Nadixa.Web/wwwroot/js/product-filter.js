$(document).on('click', '#productsWrapper .filter-link, #productsWrapper .how-pagination1[href]', function (e) {
    e.preventDefault();

    var url = $(this).attr('href');
    if (!url) return;

    loadProducts(url);
});

function loadProducts(url) {
    $.ajax({
        url: url,
        type: 'GET',
        headers: { 'X-Requested-With': 'XMLHttpRequest' },
        success: function (html) {
            $('#productsWrapper').html(html);
            window.history.pushState({}, '', url);

            // إعادة تفعيل الـ Isotope Layout للكاردز الجديدة
            if ($.fn.isotope) {
                $('#productsContainer').isotope('destroy');
                $('#productsContainer').isotope({
                    itemSelector: '.isotope-item',
                    layoutMode: 'fitRows'
                });
            }

            // تمرير سلس لأعلى الشبكة (اختياري، تجربة استخدام أفضل)
            $('html, body').animate({
                scrollTop: $('#productsWrapper').offset().top - 100
            }, 300);
        }
    });
}

// دعم زرار الرجوع في المتصفح (Back button)
window.addEventListener('popstate', function () {
    loadProducts(window.location.href);
});