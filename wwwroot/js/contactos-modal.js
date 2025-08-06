function openContactoModal(url, title) {
    $("#contactoModalLabel").text(title);
    $("#contactoModalBody").html('<div class="text-center p-4"><div class="spinner-border"></div></div>');
    $("#contactoModal").modal("show");
    $.get(url, function(data) {
        $("#contactoModalBody").html(data);
    });
}

function submitContactoForm(actionUrl, onSuccess) {
    var form = $("#contactoForm");
    var formData = form.serialize();
    $.post(actionUrl, formData, function(result) {
        if (result.success) {
            $("#contactoModal").modal("hide");
            location.reload();
        } else {
            $("#contactoModalBody").html(result.html);
        }
    });
}
