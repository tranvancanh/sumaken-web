let editView = false;
$(function () {

    $('#storeIn-edit-btn').on('click', function () {
        $("#StoreInEditModel .valid-area").hide();

        if (!confirm('更新します。よろしいですか？')) {
            return false;
        } else {
            //$(this).submit();
            var formData = new FormData($('#StoreInEditForm').get(0));
            GetEditAjax(formData);
        }
    });

    // 削除エリアの表示・非表示
    $('#storeIn-delete-btn').on('click', function () {
        var showTargetArea = $("#remark-delete-input-area");
        if (showTargetArea.css('display') == 'none') {
            $("#remark-delete-input-area").show();
        }
        else {
            $("#remark-delete-input-area").hide();
        }
    });

    // 削除実行
    $('#storeIn-delete-done-btn').on('click', function () {

        // ボタン非活性化
        $('#storeIn-delete-done-btn').prop("disabled", true);

        var formData = new FormData($('#StoreInEditForm').get(0));
        GetDeleteAjax(formData);
    });

    // URLを取得
    var url = new URL(window.location.href);
    var urlParam = $(url).attr('search');

    var updateidParamName = 'updateid';
    var deletedParamName = 'isDelete=true';

    if (urlParam.indexOf(updateidParamName) > 0) {
        // update完了
        ToastUpdateShow();

        var updateid = getParam(updateidParamName);
        var trid = "#tr" + updateid + " td";
        $(trid).addClass('table-update-row');

        // アドレスバーのURLからGETパラメータを削除
        history.replaceState('', '', url.pathname);
    }
    else if (urlParam.indexOf(deletedParamName) > 0) {
        // delete完了
        ToastDeleteShow();

        // アドレスバーのURLからGETパラメータを削除
        history.replaceState('', '', url.pathname);
    }

});

function EditView(id) {

    // 二重モーダル表示禁止
    if (!editView) {
        editView = true;
    }
    else {
        return false;
    }

    GetEditViewAjax(id);
}


function GetEditViewAjax(id) {
    let storeInEditModel = $('#StoreInModify');
    $.ajax({
        url: ROOT + "/D_StoreIn/Edit",
        type: 'GET',
        data: { id: id },
    })
        .done(function (response) {
            storeInEditModel.html(response);
            let myModal = document.getElementById("StoreInEditModel");
            if (myModal) {
                storeInEditModel.find('.modal').modal('show');
            }
        })
        .fail(function (xhr) {
            //通信失敗時の処理
            //失敗したときに実行したいスクリプトを記載
        })
        .always(function (xhr, msg) {
            //通信完了時の処理
            //結果に関わらず実行したいスクリプトを記載
            editView = false;
        });
}

function GetEditAjax(formData) {
    formData.append("isDelete", "false");
    $.ajax({
        url: ROOT + "/D_StoreIn/Edit",
        type: 'POST',
        data: formData,
        processData: false,
        contentType: false,
    })
        .done(function (response) {
            var data_stringify = JSON.stringify(response);
            var data_json = JSON.parse(data_stringify);

            // jsonデータから各データを取得
            var responseResult = data_json["result"];
            var responseMessage = data_json["message"];

            if (responseResult === "OK") {
                //$("#StoreInSerchBtn").click();
                var updateid = data_json["newid"];
                location.href = ROOT + "/D_StoreIn/SearchByPageBack?updateid=" + updateid;
            }
            else {
                $("#StoreInEditModel .valid-area").fadeIn();
                $("#StoreInEditModel").find(".valid-area span").text(responseMessage);
            }

            //storeInEditModel.html(response);
            //let myModal = document.getElementById("StoreInEditModel");
            //if (myModal) {
            //    storeInEditModel.find('.modal').modal('show');
            //}
        })
        .fail(function (xhr) {
            //通信失敗時の処理
            //失敗したときに実行したいスクリプトを記載
        })
        .always(function (xhr, msg) {
            //通信完了時の処理
            //結果に関わらず実行したいスクリプトを記載
        });
}

function GetDeleteAjax(formData) {
    formData.append("isDelete", "true");
    $.ajax({
        url: ROOT + "/D_StoreIn/Edit",
        type: 'POST',
        data: formData,
        processData: false,
        contentType: false,
    })
        .done(function (response) {
            var data_stringify = JSON.stringify(response);
            var data_json = JSON.parse(data_stringify);

            // jsonデータから各データを取得
            var responseResult = data_json["result"];
            var responseMessage = data_json["message"];

            if (responseResult === "OK") {
                location.href = ROOT + "/D_StoreIn/SearchByPageBack?isDelete=true";
            }
            else {
                $("#StoreInEditModel .valid-area").fadeIn();
                $("#StoreInEditModel").find(".valid-area span").text(responseMessage);
            }
        })
        .fail(function (xhr) {
            //通信失敗時の処理
            //失敗したときに実行したいスクリプトを記載
        })
        .always(function (xhr, msg) {
            //通信完了時の処理
            //結果に関わらず実行したいスクリプトを記載
        });
}

function CancelSubmit() {
    let isDis = $('#StoreInEditModel').is(':visible');
    if (isDis) {
        $("#StoreInEditModel").modal("hide");
    };
}

function getParam(name, url) {
    if (!url) url = window.location.href;
    name = name.replace(/[\[\]]/g, "\\$&");
    var regex = new RegExp("[?&]" + name + "(=([^&#]*)|&|#|$)"),
        results = regex.exec(url);
    if (!results) return null;
    if (!results[2]) return '';
    return decodeURIComponent(results[2].replace(/\+/g, " "));
}
//$("#iWarningSystemSetting").on('hidden.bs.modal', function () {
//    //alert("Esta accion se ejecuta al cerrar el modal");
//    window.location.href = ROOT + "/M_SystemSetting/Index";
//});


