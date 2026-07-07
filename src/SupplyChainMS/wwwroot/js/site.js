// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// ============================================================
// Show any toast notifications on page load
// ============================================================
// Bootstrap toasts start hidden. This code finds every element with the
// "app-toast" class (our success/error flash messages) and tells Bootstrap
// to slide it into view. The toast then auto-hides after its data-bs-delay.
document.addEventListener('DOMContentLoaded', function () {
    var toastElements = document.querySelectorAll('.app-toast');
    toastElements.forEach(function (el) {
        var toast = new bootstrap.Toast(el);
        toast.show();
    });
});
