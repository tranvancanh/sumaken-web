function EditLaneStateSubmitAjax(laneNo, laneAddress, state) {
    let lanestatemanagamentmodify = $('#lanestatemanagamentmodify');
    $.ajax({
        url: ROOT + "/W_AGF_LaneState/Edit",
        type: 'GET',
        data: { laneNo:laneNo, laneAddress:laneAddress, state:state },
        success: function (response) {
            lanestatemanagamentmodify.html(response);
            let myModal = document.getElementById("editlanestatemanagament");
            if (myModal) {
                lanestatemanagamentmodify.find('.modal').modal('show');
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
function EditLaneStateCancelSubmit() {
    let isDis = $('#editlanestatemanagament').is(':visible');
    if (isDis) {
        $("#editlanestatemanagament").modal("hide");
    };
}
//$("#iWarningSystemSetting").on('hidden.bs.modal', function () {
//    //alert("Esta accion se ejecuta al cerrar el modal");
//    window.location.href = ROOT + "/M_SystemSetting/Index";
//});

function ImportTruckBinSubmitAjax() {
    let truckbinmanagamentmodify = $('#truckbinmanagamentmodify');
    $.ajax({
        url: ROOT + "/M_AGF_TruckBinLane/Import",
        type: 'GET',
        data: {},
        success: function (response) {
            truckbinmanagamentmodify.html(response);
            let myModal = document.getElementById("importtruckbinmanagament");
            if (myModal) {
                truckbinmanagamentmodify.find('.modal').modal('show');
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

function ImportTruckBinCancelSubmit() {
    let isDis = $('#importtruckbinmanagament').is(':visible');
    if (isDis) {
        $("#importtruckbinmanagament").modal("hide");
    };
}
//$("#iWarningSystemSetting").on('hidden.bs.modal', function () {
//    //alert("Esta accion se ejecuta al cerrar el modal");
//    window.location.href = ROOT + "/M_SystemSetting/Index";
//});


