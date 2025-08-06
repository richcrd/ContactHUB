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

function openEtiquetaModal(url, title) {
    $("#etiquetaModalLabel").text(title);
    $("#etiquetaModalBody").html('<div class="text-center p-4"><div class="spinner-border"></div></div>');
    $("#etiquetaModal").modal("show");
    $.get(url, function(data) {
        $("#etiquetaModalBody").html(data);
    });
}

function submitEtiquetaForm(actionUrl) {
    var form = $("#etiquetaModalBody form");
    var formData = form.serialize();
    $.ajax({
        url: actionUrl,
        method: 'POST',
        data: formData,
        success: function(result) {
            if (result.success) {
                var modal = $("#etiquetaModal");
                modal.modal("hide");
                modal.on('hidden.bs.modal', function () {
                    $("#etiquetaModalBody").html("");
                    modal.off('hidden.bs.modal');
                });
                $.get(window.location.href, function(pageHtml){
                    var newTbody = $(pageHtml).find('#tabla-etiquetas tbody').html();
                    $('#tabla-etiquetas tbody').html(newTbody);
                    showToast('Operación realizada correctamente.', 'success');
                });
            } else {
                $("#etiquetaModalBody").html(result.html);
                if (result.errors && result.errors.length > 0) {
                    result.errors.forEach(function(err) {
                        showToast(err, 'danger');
                    });
                }
            }
        }
    });
}

function applyDarkTableEtiquetas() {
    if (document.body.classList.contains('theme-dark')) {
        $('#tabla-etiquetas').addClass('table-dark');
        $('#thead-etiquetas').removeClass('table-light').addClass('table-dark');
        $('#tabla-etiquetas tbody tr').css('background-color', '');
    } else {
        $('#tabla-etiquetas').removeClass('table-dark');
        $('#thead-etiquetas').removeClass('table-dark').addClass('table-light');
        $('#tabla-etiquetas tbody tr').css('background-color', '');
    }
}

$(function(){
    if (window.etiquetaSuccess) {
        showToast(window.etiquetaSuccess, 'success');
    }
    if (window.etiquetaError) {
        showToast(window.etiquetaError, 'danger');
    }
    $('#btnNuevaEtiqueta').on('click', function(){
        openEtiquetaModal($('#btnNuevaEtiqueta').data('url'), 'Nueva Etiqueta');
    });
    $('#tabla-etiquetas').on('click', '.btn-editar', function(){
        var id = $(this).data('id');
        var editUrl = '/Admin/Etiquetas/Editar/' + id;
        openEtiquetaModal(editUrl, 'Editar Etiqueta');
    });
    $('#etiquetaModal').on('submit', 'form', function(e){
        e.preventDefault();
        var actionUrl = $(this).attr('action');
        submitEtiquetaForm(actionUrl);
    });
    applyDarkTableEtiquetas();
    const observerEtiquetas = new MutationObserver(applyDarkTableEtiquetas);
    observerEtiquetas.observe(document.body, { attributes: true, attributeFilter: ['class'] });
});
