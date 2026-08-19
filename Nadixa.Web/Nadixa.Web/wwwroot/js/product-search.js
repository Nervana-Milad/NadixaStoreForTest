
let timeout = null;

$("#searchInput").on("keyup", function () {
    let query = $(this).val();
    clearTimeout(timeout);
    timeout = setTimeout(function () {
        $.ajax({
            url: "/Product/Search",
            type: "GET",
            data: { term: query },
            success: function (html) {
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
