// Toast logic for Perfil
function showToast(message, type) {
    var toastId = 'toast-' + Date.now();
    var toastHtml = `<div id="${toastId}" class="toast align-items-center text-bg-${type} border-0 mb-2" role="alert" aria-live="assertive" aria-atomic="true" data-bs-delay="3500">` +
        `<div class="d-flex">` +
        `<div class="toast-body">${message}</div>` +
        `<button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>` +
        `</div></div>`;
    var $toast = $(toastHtml);
    $('#toast-container').append($toast);
    var toast = new bootstrap.Toast($toast[0]);
    toast.show();
    $toast.on('hidden.bs.toast', function(){ $toast.remove(); });
}

$(function(){
    if (window.perfilSuccess) {
        showToast(window.perfilSuccess, 'success');
    }
    if (window.perfilError) {
        showToast(window.perfilError, 'danger');
    }
});
