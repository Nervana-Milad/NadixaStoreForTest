const Notify = {
    success(message) {
        toastr.success(message);
    },
    error(message) {
        toastr.error(message);
    },
    warning(message) {
        toastr.warning(message);
    },

    loginRequired(message) {
        Swal.fire({
            icon: 'warning',
            title: 'Login Required',
            text: message,
            confirmButtonText: 'Login'
        }).then(() => {
            window.location.href = '/Auth/Login';
        });
    }
}