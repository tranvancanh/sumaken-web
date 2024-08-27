function EditStackingNGSubmitAjax(depoCode, productCode) {
    let stackingngmanagamentmodify = $('#stackingngmanagamentmodify');
    $.ajax({
        url: ROOT + "/M_AGF_StackingNG/Edit",
        type: 'GET',
        data: { depoCode: depoCode, productCode: productCode },
        success: function (response) {
            stackingngmanagamentmodify.html(response);
            let myModal = document.getElementById("editstackingngmanagament");
            if (myModal) {
                stackingngmanagamentmodify.find('.modal').modal('show');
            }
        },
        failure: function (response) {
            return null;
        },
        error: function (response) {
            return null;
        },
        processData: true
    });

}
function EditStackingNGCancelSubmit() {
    let isDis = $('#editstackingngmanagament').is(':visible');
    if (isDis) {
        $("#editstackingngmanagament").modal("hide");
    };
}
//$("#iWarningSystemSetting").on('hidden.bs.modal', function () {
//    //alert("Esta accion se ejecuta al cerrar el modal");
//    window.location.href = ROOT + "/M_SystemSetting/Index";
//});

function DeleteStackingNG(depoCode, oldproductCode,) {
    let stackingngmanagamentmodify = $('#stackingngmanagamentmodify');
    $.ajax({
        url: ROOT + "/M_AGF_StackingNG/Delete",
        type: 'GET',
        data: { depoCode: depoCode, oldproductCode: oldproductCode },
        success: function (response) {
            stackingngmanagamentmodify.html(response);
            let myModal = document.getElementById("editstackingngmanagament");
            if (myModal) {
                stackingngmanagamentmodify.find('.modal').modal('show');
            }
        },
        failure: function (response) {
            return null;
        },
        error: function (response) {
            return null;
        },
        processData: true
    });

}

function ImportStackingNGSubmitAjax() {
    let stackingngmanagamentmodify = $('#stackingngmanagamentmodify');
    $.ajax({
        url: ROOT + "/M_AGF_StackingNG/Import",
        type: 'GET',
        data: { },
        success: function (response) {
            stackingngmanagamentmodify.html(response);
            let myModal = document.getElementById("importstackingngmanagament");
            if (myModal) {
                stackingngmanagamentmodify.find('.modal').modal('show');
            }
        },
        failure: function (response) {
            return null;
        },
        error: function (response) {
            return null;
        },
        processData: true
    });

}

function ImportStackingNGCancelSubmit() {
    let isDis = $('#importstackingngmanagament').is(':visible');
    if (isDis) {
        $("#importstackingngmanagament").modal("hide");
    };
}
//$("#iWarningSystemSetting").on('hidden.bs.modal', function () {
//    //alert("Esta accion se ejecuta al cerrar el modal");
//    window.location.href = ROOT + "/M_SystemSetting/Index";
//});


