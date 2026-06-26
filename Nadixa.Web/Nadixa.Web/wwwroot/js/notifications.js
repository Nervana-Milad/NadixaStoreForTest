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
    },

    confirm(options) {
        Swal.fire({
            icon: 'warning',
            title: options.title || 'Confirm Delete',
            text: options.message || 'Are you sure?',
            showCancelButton: true,
            confirmButtonText: 'Yes, Sure',
            cancelButtonText: 'No',
            confirmButtonColor: '#d33'
        }).then((result) => {
            if (result.isConfirmed) {
                options.onConfirm();
            }
        });
    }
}