function EditTruckBinSubmitAjax(truckBinCode, laneNo, lanegroupID, oldLaneNo, oldLaneGroupID) {
    let truckbinmanagamentmodify = $('#truckbinmanagamentmodify');
    $.ajax({
        url: ROOT + "/M_AGF_TruckBinLane/Edit",
        type: 'GET',
        data: { truckBinCode: truckBinCode, laneNo: laneNo, lanegroupID: lanegroupID, oldLaneNo: oldLaneNo, oldLaneGroupID: oldLaneGroupID },
        success: function (response) {
            truckbinmanagamentmodify.html(response);
            let myModal = document.getElementById("edittruckbinmanagament");
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
function EditTruckBinCancelSubmit() {
    let isDis = $('#edittruckbinmanagament').is(':visible');
    if (isDis) {
        $("#edittruckbinmanagament").modal("hide");
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


