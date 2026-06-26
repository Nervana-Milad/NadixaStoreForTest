toastr.options = {
    positionClass: "toast-bottom-right", // أو أي مكان تانية
};


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
        confirmButtonText: "Login",
        confirmButtonColor: "#6c7ae0",
        showCancelButton: true,
        cancelButtonText: "Cancel",
        background: "#fff",
        borderRadius: "16px",
        
    }).then((result) => {
        if (result.isConfirmed) {
            let loginUrl = "/Auth/Login";
            if (returnUrl) {
                loginUrl += "?returnUrl=" + encodeURIComponent(returnUrl);
            }
            window.location.href = loginUrl;
        }
    });
}