$(document).ready(function () {
    $(function () {
        $('#ReceiveImportPattern-Add-Btn').on('click', function () {
            if (!confirm('変更を保存します。よろしいですか？')) {
                return false;
            } else {
                $(this).submit();
            }
        });
    });

    // リロード時に行われます
    let endRowSignFlag_Hidden = document.getElementById('EndRowSignFlag_Hidden').value;
    if (endRowSignFlag_Hidden == '0') {
        document.getElementById("EndRowSign").readOnly = true;
    }
    else {
        document.getElementById("EndRowSign").readOnly = false;
    }

    let fixedValueDepoCodeFlag_Hidden = document.getElementById('FixedValueDepoCodeFlag_Hidden').value;
    if (fixedValueDepoCodeFlag_Hidden == '0') {
        document.getElementById("FixedValueDepoCode").readOnly = true;
    }
    else {
        document.getElementById("FixedValueDepoCode").readOnly = false;
    }

    let fixedValueSupplierCodeFlag_Hidden = document.getElementById('FixedValueSupplierCodeFlag_Hidden').value;
    if (fixedValueSupplierCodeFlag_Hidden == '0') {
        document.getElementById("FixedValueSupplierCode").readOnly = true;
    }
    else {
        document.getElementById("FixedValueSupplierCode").readOnly = false;
    }

    let fixedValueDeliveryTimeClassFlag_Hidden = document.getElementById('FixedValueDeliveryTimeClassFlag_Hidden').value;
    if (fixedValueDeliveryTimeClassFlag_Hidden == '0') {
        document.getElementById("FixedValueDeliveryTimeClass").readOnly = true;
    }
    else {
        document.getElementById("FixedValueDeliveryTimeClass").readOnly = false;
    }
});



function onlyNumberKey(evt) {
    var ch = String.fromCharCode(evt.which);
    if (!(/[0-9]/.test(ch))) {
        evt.preventDefault();
    }
}

function change_viewport_0(id) {
    //alert(id);
    let value = document.getElementById(id).value;
    if (value.trim().length == 0) {
        document.getElementById(id).value = 0;
    }
}

function change_viewport_1(id) {
    //alert(id);
    let value = document.getElementById(id).value;
    if (value.trim().length == 0) {
        document.getElementById(id).value = 1;
    }
    else {
        if (value < 1) {
            document.getElementById(id).value = 1;
        }
    }
}

function change_viewport_default(id, originalValue) {
    //alert(id);
    let value = document.getElementById(id).value;
    if (value.trim().length == 0) {
        document.getElementById(id).value = originalValue;
    }
}


// チェックボックスが変更されたときに実行されます
$('input[type=radio][name=EndRowSignFlag]').change(function () {
    if ($(this).is(':checked')) {
        //alert(`${this.value} is checked`);
        document.getElementById('EndRowSignFlag_Hidden').value = this.value;
    }
    let valueGet = this.value;
    if (valueGet == '0') {
        document.getElementById("EndRowSign").readOnly = true;
    }
    else {
        document.getElementById("EndRowSign").readOnly = false;
    }
    //else {
    //    alert(`${this.value} is unchecked`);
    //}
});

$('input[type=radio][name=FixedValueDepoCodeFlag]').change(function () {
    if ($(this).is(':checked')) {
        //alert(`${this.value} is checked`);
        document.getElementById('FixedValueDepoCodeFlag_Hidden').value = this.value;
    }
    let valueGet = this.value;
    if (valueGet == '0') {
        document.getElementById("FixedValueDepoCode").readOnly = true;
    }
    else {
        document.getElementById("FixedValueDepoCode").readOnly = false;
    }
});

$('input[type=radio][name=FixedValueSupplierCodeFlag]').change(function () {
    if ($(this).is(':checked')) {
        //alert(`${this.value} is checked`);
        document.getElementById('FixedValueSupplierCodeFlag_Hidden').value = this.value;
    }
    let valueGet = this.value;
    if (valueGet == '0') {
        document.getElementById("FixedValueSupplierCode").readOnly = true;
    }
    else {
        document.getElementById("FixedValueSupplierCode").readOnly = false;
    }
});

$('input[type=radio][name=FixedValueDeliveryTimeClassFlag]').change(function () {
    if ($(this).is(':checked')) {
        //alert(`${this.value} is checked`);
        document.getElementById('FixedValueDeliveryTimeClassFlag_Hidden').value = this.value;
    }
    let valueGet = this.value;
    if (valueGet == '0') {
        document.getElementById("FixedValueDeliveryTimeClass").readOnly = true;
    }
    else {
        document.getElementById("FixedValueDeliveryTimeClass").readOnly = false;
    }
});

function setChecked(id) {
    switch (id) {
        // 読込終了合図
        case 'EndRowSignFlag_0': {
            document.getElementById('EndRowSignFlag_Hidden').value = 0;
            document.getElementById('EndRowSignFlag_Radio_0').checked = true;
            document.getElementById("EndRowSign").readOnly = true;

            let child_0 = document.getElementById('EndRowSignFlag_Radio_0');
            child_0.parentElement.classList.add('checked');

            let child_1 = document.getElementById('EndRowSignFlag_Radio_1');
            child_1.parentElement.classList.remove('checked');

            break;
        }
        case 'EndRowSignFlag_1': {
            document.getElementById('EndRowSignFlag_Hidden').value = 1;
            document.getElementById('EndRowSignFlag_Radio_1').checked = true;
            document.getElementById("EndRowSign").readOnly = false;

            let child_0 = document.getElementById('EndRowSignFlag_Radio_0');
            child_0.parentElement.classList.remove('checked');

            let child_1 = document.getElementById('EndRowSignFlag_Radio_1');
            //alert(child_1);
            child_1.parentElement.classList.add('checked');

            break;
        }

        // 倉庫コード
        case 'FixedValueDepoCodeFlag_0': {
            document.getElementById('FixedValueDepoCodeFlag_Hidden').value = 0;
            document.getElementById('FixedValueDepoCodeFlag_Radio_0').checked = true;
            document.getElementById("FixedValueDepoCode").readOnly = true;

            let child_0 = document.getElementById('FixedValueDepoCodeFlag_Radio_0');
            child_0.parentElement.classList.add('checked');

            let child_1 = document.getElementById('FixedValueDepoCodeFlag_Radio_1');
            child_1.parentElement.classList.remove('checked');

            break;
        }
        case 'FixedValueDepoCodeFlag_1': {
            document.getElementById('FixedValueDepoCodeFlag_Hidden').value = 1;
            document.getElementById('FixedValueDepoCodeFlag_Radio_1').checked = true;
            document.getElementById("FixedValueDepoCode").readOnly = false;

            let child_0 = document.getElementById('FixedValueDepoCodeFlag_Radio_0');
            child_0.parentElement.classList.remove('checked');

            let child_1 = document.getElementById('FixedValueDepoCodeFlag_Radio_1');
            child_1.parentElement.classList.add('checked'); 

            break;
        }

        // 仕入先コード
        case 'FixedValueSupplierCodeFlag_0': {
            document.getElementById('FixedValueSupplierCodeFlag_Hidden').value = 0;
            document.getElementById('FixedValueSupplierCodeFlag_Radio_0').checked = true;
            document.getElementById("FixedValueSupplierCode").readOnly = true;

            let child_0 = document.getElementById('FixedValueSupplierCodeFlag_Radio_0');
            child_0.parentElement.classList.add('checked');

            let child_1 = document.getElementById('FixedValueSupplierCodeFlag_Radio_1');
            child_1.parentElement.classList.remove('checked');

            break;
        }
        case 'FixedValueSupplierCodeFlag_1': {
            document.getElementById('FixedValueSupplierCodeFlag_Hidden').value = 1;
            document.getElementById('FixedValueSupplierCodeFlag_Radio_1').checked = true;
            document.getElementById("FixedValueSupplierCode").readOnly = false;

            let child_0 = document.getElementById('FixedValueSupplierCodeFlag_Radio_0');
            child_0.parentElement.classList.remove('checked');

            let child_1 = document.getElementById('FixedValueSupplierCodeFlag_Radio_1');
            child_1.parentElement.classList.add('checked'); 

            break;
        }

        // 納入時間区分
        case 'FixedValueDeliveryTimeClassFlag_0': {
            document.getElementById('FixedValueDeliveryTimeClassFlag_Hidden').value = 0;
            document.getElementById('FixedValueDeliveryTimeClassFlag_Radio_0').checked = true;
            document.getElementById("FixedValueDeliveryTimeClass").readOnly = true;

            let child_0 = document.getElementById('FixedValueDeliveryTimeClassFlag_Radio_0');
            child_0.parentElement.classList.add('checked');

            let child_1 = document.getElementById('FixedValueDeliveryTimeClassFlag_Radio_1');
            child_1.parentElement.classList.remove('checked');

            break;
        }
        case 'FixedValueDeliveryTimeClassFlag_1': {
            document.getElementById('FixedValueDeliveryTimeClassFlag_Hidden').value = 1;
            document.getElementById('FixedValueDeliveryTimeClassFlag_Radio_1').checked = true;
            document.getElementById("FixedValueDeliveryTimeClass").readOnly = false;

            let child_0 = document.getElementById('FixedValueDeliveryTimeClassFlag_Radio_0');
            child_0.parentElement.classList.remove('checked');

            let child_1 = document.getElementById('FixedValueDeliveryTimeClassFlag_Radio_1');
            child_1.parentElement.classList.add('checked');

            break;
        }
    }
}


function onlynum() {
    //alert('vao day roi  ');
    var ip = document.getElementById("Start_Row_Number");
    var res = ip.value;

    if (res != '') {
        if (isNaN(res)) {
            alert('vao day roi  ');
            // Set input value empty
            ip.value = "";
            return false;
        } else {
            return true
        }
    }
}

function CombackPage() {
    setTimeout(function () {
        window.location.href = ROOT + "/M_ReceiveImportPattern/Index";
    }, 000);
}


//window.onhashchange = function () {
//    alert('vao tan day');
//    CombackPage();
//}