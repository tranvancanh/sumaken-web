function GetSystemSettingSubmitAjax(systemSettingCode) {
    let systemsettingmanagamentmodify = $('#systemsettingmanagamentmodify');
    $.ajax({
        url: ROOT + "/M_SystemSetting/Edit",
        type: 'GET',
        data: { systemSettingCode: systemSettingCode },
        success: function (response) {
            systemsettingmanagamentmodify.html(response);
            let myModal = document.getElementById("editsystemsettingmanagament");
            if (myModal) {
                systemsettingmanagamentmodify.find('.modal').modal('show');
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

function CancelSubmit() {
    let isDis = $('#editsystemsettingmanagament').is(':visible');
    if (isDis) {
        $("#editsystemsettingmanagament").modal("hide");
    };
}
//$("#iWarningSystemSetting").on('hidden.bs.modal', function () {
//    //alert("Esta accion se ejecuta al cerrar el modal");
//    window.location.href = ROOT + "/M_SystemSetting/Index";
//});