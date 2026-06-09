$("#reviewForm").on('submit', function (event) {

	event.preventDefault();

	var rating = $("#RatingValue").val();
	var content = $("#Content").val();
	var productId = parseInt($("#ProductId").val());

	console.log(content, rating, productId);

	if (!content || content.trim() === "") {
		showError("Please write a review");
		return;
	}

	console.log("Rating:", rating);
	console.log("Content:", content);
	console.log("ProductId:", productId);

	$.ajax({
		url: "/Product/AddReview",
		type: 'POST',
		contentType: 'application/json',
		data: JSON.stringify({
			Content: content,
			Rating: parseInt(rating),
			ProductId: productId
		}),
		
		success: function (response) {

			if (!response.success) {
				showError(response.message);
				return;
			}

			$("#noReviewsMessage").remove();

			let count = parseInt($("#reviewsCount").text());
			$("#reviewsCount").text(count + 1);

			$('#reviewForm')[0].reset();
			$('#RatingValue').val("");
			$(".item-rating")
				.removeClass("zmdi-star")
				.addClass("zmdi-star-outline");


			$('#reviewSection').prepend(
				`<div class="flex-w flex-t p-b-68 review-card">
        <div class="wrap-pic-s size-109 bor0 of-hidden m-r-18 m-t-6">
            <img src="${response.userImage}" alt="AVATAR">
        </div>
        <div class="size-207">
            <div class="flex-w flex-sb-m p-b-17">
                <span class="mtext-107 cl2 p-r-20">${response.username}</span>
                <span class="fs-20 cl11">
                    ${'★'.repeat(response.rating)}${'☆'.repeat(5 - response.rating)}
                </span>
            </div>
            <div class="d-flex justify-content-between">
                <p class="stext-102 cl6">${response.content}</p>
                <button type="button"
                        class="delete-review-btn btn btn-sm text-danger border-0 bg-transparent p-0"
                        data-review-id="${response.id}"
                        title="Delete">
                    <i class="fa fa-trash fs-18"></i>
                </button>
            </div>
        </div>
    </div>`
			);

			showSuccess("Review added successfully");

			if ($("#avgRating").length) {
				$("#avgRating").text(
					Number(response.avgRating).toFixed(1)
				);
			}

			console.log("Success");
		},
		error: function () {

			showError("Something went wrong");
		}
	});

});

let reviewToDelete = null;
let cardToDelete = null;

$(document).on("click", ".delete-review-btn", function () {
    reviewToDelete = $(this).data("review-id");
    cardToDelete = $(this).closest(".review-card");
    Notify.confirm({
        title: "Delete Review",
        message: "Are you sure you want to delete this review?",
        onConfirm: function () {
            $.ajax({
                url: "/Product/DeleteReview",
                type: "POST",
                data: { id: reviewToDelete },
                success: function (response) {
                    if (!response.success) {
                        Notify.error(response.message);
                        return;
                    }
                    cardToDelete.fadeOut(300, function () {
                        $(this).remove();
                        if ($(".review-card").length === 0) {
                            $("#reviewSection").html(
                                '<p id="noReviewsMessage" class="text-muted mb-3 text-center">No Reviews yet, Be the first to Review</p>'
                            );
                        }
                    });
                    $("#reviewsCount").text(response.reviewsCount);
                    $("#avgRating").text(Number(response.avgRating).toFixed(1));
                    Notify.success("Review deleted successfully");
                },
                error: function () {
                    Notify.error("Something went wrong");
                }
            });
        }
    });
});