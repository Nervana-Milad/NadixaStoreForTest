function showSuccess(message, title = "Success") {
    toastr.success(message, title);
}

function showError(message, title = "Error") {
    toastr.error(message, title);
}

function showInfo(message, title = "Info") {
    toastr.info(message, title);
}

function showLoginRequired(message, returnUrl = null) {

    Swal.fire({
        icon: "warning",
        title: "Login Required",
        text: message,
        confirmButtonText: "Login"
    }).then(() => {

        let loginUrl = "/Auth/Login";

        if (returnUrl) {
            loginUrl +=
                "?returnUrl=" +
                encodeURIComponent(returnUrl);
        }

        window.location.href = loginUrl;
    });
}