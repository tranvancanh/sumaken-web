function EditDestinationBinSubmitAjax(customerCode, destination ,finaldeliveryPlace, truckbinCode, oldCustomerCode, oldFinalDeliveryPlace, oldTruckBinCode) {
    let destinationbinmanagamentmodify = $('#destinationbinmanagamentmodify');
    $.ajax({
        url: ROOT + "/M_AGF_DestinationBin/Edit",
        type: 'GET',
        data: { customerCode: customerCode, destination:destination,finaldeliveryPlace: finaldeliveryPlace, truckbinCode: truckbinCode, oldCustomerCode: oldCustomerCode, oldFinalDeliveryPlace:oldFinalDeliveryPlace,oldTruckBinCode: oldTruckBinCode },
        success: function (response) {
            destinationbinmanagamentmodify.html(response);
            let myModal = document.getElementById("editdestinationbinmanagament");
            if (myModal) {
                destinationbinmanagamentmodify.find('.modal').modal('show');
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
function DeleteDestinationBin(customerCode, truckbinCode, oldCustomerCode, oldTruckBinCode) {
    let destinationbinmanagamentmodify = $('#destinationbinmanagamentmodify');
    $.ajax({
        url: ROOT + "/M_AGF_DestinationBin/Delete",
        type: 'GET',
        data: { customerCode: customerCode, truckbinCode: truckbinCode, oldCustomerCode: oldCustomerCode, oldTruckBinCode: oldTruckBinCode },
        success: function (response) {
            destinationbinmanagamentmodify.html(response);
            let myModal = document.getElementById("editdestinationbinmanagament");
            if (myModal) {
                destinationbinmanagamentmodify.find('.modal').modal('show');
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
function EditCancelSubmit() {
    let isDis = $('#editdestinationbinmanagament').is(':visible');
    if (isDis) {
        $("#editdestinationbinmanagament").modal("hide");
    };
}
//$("#iWarningSystemSetting").on('hidden.bs.modal', function () {
//    //alert("Esta accion se ejecuta al cerrar el modal");
//    window.location.href = ROOT + "/M_SystemSetting/Index";
//});

function ImportDestinationBinSubmitAjax() {
    let destinationbinmanagamentmodify = $('#destinationbinmanagamentmodify');
    $.ajax({
        url: ROOT + "/M_AGF_DestinationBin/Import",
        type: 'GET',
        data: {},
        success: function (response) {
            destinationbinmanagamentmodify.html(response);
            let myModal = document.getElementById("importdestinationbinmanagament");
            if (myModal) {
                destinationbinmanagamentmodify.find('.modal').modal('show');
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

function ImportCancelSubmit() {
    let isDis = $('#importdestinationbinmanagament').is(':visible');
    if (isDis) {
        $("#importdestinationbinmanagament").modal("hide");
    };
}
//$("#iWarningSystemSetting").on('hidden.bs.modal', function () {
//    //alert("Esta accion se ejecuta al cerrar el modal");
//    window.location.href = ROOT + "/M_SystemSetting/Index";
//});


