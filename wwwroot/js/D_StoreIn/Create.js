//////////////////////////////////
// D_StoreIn / Create
//////////////////////////////////

$(function () {

    const productCodeTextBox = document.querySelectorAll('.ProductCode');

    productCodeTextBox.forEach(function (ProductCode) {
        ProductCode.addEventListener("focusout", function (event) {
            let productCodeTextLength = ProductCode.value.length;
            if (productCodeTextLength > 0) {

                // 親要素trを取得
                let targetTr = ProductCode.closest("tr");

                // デポIDを取得
                let depoId = targetTr.querySelector(".DepoID").value;

                // 商品コードを取得
                let productCode = ProductCode.value;

                SetProductDataAjax(productCode, depoId, targetTr);
            }
        });
    });

});

function SetProductDataAjax(productCode, depoId, targetTr) {

    let lotQuantity = 0;
    let address1 = "";
    let address2 = "";
    let packing = "";

    $.ajax({
        url: ROOT + "/D_StoreIn/GetProductDataAjax",
        type: 'GET',
        data: { productCode, depoId }
    })
        .done(function (response) {
            var data_stringify = JSON.stringify(response);
            var data_json = JSON.parse(data_stringify);

            // jsonデータから各データを取得
            var result = data_json["result"];
            if (result === "OK") {

                lotQuantity = data_json["lotQuantity"];
                address1 = data_json["address1"];
                address2 = data_json["address2"];
                packing = data_json["packing"];

                // LotQuantity
                targetTr.querySelector(".Quantity").value = lotQuantity;
                // StockLocation1
                targetTr.querySelector(".StockLocation1").value = address1;
                // StockLocation2
                targetTr.querySelector(".StockLocation2").value = address2;
                // Packing
                targetTr.querySelector(".Packing").value = packing;
            }

        })
        .fail(function (xhr) {
            //通信失敗時の処理
            //失敗したときに実行したいスクリプトを記載
            return false;
        })
        .always(function (xhr, msg) {
            //通信完了時の処理
            //結果に関わらず実行したいスクリプトを記載
            return false;
        });

}

