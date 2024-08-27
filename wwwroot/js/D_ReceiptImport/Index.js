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

    let listFiles = document.querySelector("#FileImports");
    listFiles.addEventListener("change", getListFiles);

     // リロード時に行われます
    onchangeGetPatternName('PatternCode');
    // init
    errorMessenger = new Array();

    if (document.getElementById("NoPatternCode") !== null) {
        let noPatternCodeHtml = document.getElementById("NoPatternCode").innerHTML;
        let patternCodeValue = document.getElementById("PatternCode").value;
        if (patternCodeValue.trim().length <= 0) {
            if (noPatternCodeHtml.trim().length <= 0) {
                document.getElementById("NoPatternCode").innerHTML = "取込を行うには、入庫取込パターンマスタの登録が必要です。";
            }
        }
    }

    window.onload = function () {
        document.onkeydown = function (e) {
            if ((e.which == 116) || (e.keyCode == 116)) {
                window.location.href = ROOT + "/D_ReceiveImport/Index";
                //alert('enter F5');
            }
        };
    }
});

let errorMessenger = new Array();

function getListFiles() {
    let fileInput = document.querySelector("#FileImports");
    let files = fileInput.files;
    let fileListLength = files.length;
    errorMessenger = new Array();
    if (fileListLength > 0) {
        for (let i = 0; i < fileListLength; i++) {
            let file = files.item(i).name;
            //console.log(file);
        }
        // 条件チェック
        checkNumberFiles(fileInput);
        checkFileExtension(fileInput);
        checkOverlapFilenames(fileInput);
    }
    else {
        console.log('not file import');
        // set blank
        setDisplayBlankAllFiles();
    }

    setErrorMessengerBlank();
    console.log(errorMessenger);
    if (errorMessenger.length > 0) {
        console.log('start remove');
        removeFilesUploaded();

        // 同じなエラーをフィルタリングする
        errorMessenger = errorMessenger.filter((x, i, a) => a.indexOf(x) == i);
        // まず、古いエーラーメッセージを削除する
        // 新しいエーラーメッセージを作成
        let container = document.querySelector('#import-res');
        while (container.firstChild) {
            container.removeChild(container.firstChild);
        }

        for (let i = 0; i < errorMessenger.length; i++) {
            let li = document.createElement("li");
            let liText = document.createTextNode(errorMessenger[i]);
            li.appendChild(liText);
            document.getElementById("import-res").appendChild(li);
        }

        // set blank
        setDisplayBlankAllFiles();
    }
    else {
        // 
        console.log('bat dau ghi');
        wirteListFiles(files);
    }
}

function onSubmitFunction(load) {
    if (errorMessenger.length > 0) {
        return false;
    }
    let fileInput = document.querySelector("#FileImports");
    let files = fileInput.files;
    let fileListLength = files.length;
    if (fileListLength <= 0) {
        return false;
    }
    else {
        for (let i = 0; i < files.length; i++) {
            let file = files.item(i).name;
            let extension = file.substring(file.lastIndexOf('.')).toLowerCase();
            if (extension != '.xls' && extension != '.xlsx' && extension != '.csv') {
                return false;
            }
        }
    }
    let lenghConcat = document.getElementById("file0").innerHTML
        + document.getElementById("file1").innerHTML
        + document.getElementById("file2").innerHTML
        + document.getElementById("file3").innerHTML
        + document.getElementById("file4").innerHTML
        + document.getElementById("file5").innerHTML
        + document.getElementById("file6").innerHTML
        + document.getElementById("file7").innerHTML
        + document.getElementById("file8").innerHTML
        + document.getElementById("file9").innerHTML;
    if (lenghConcat.length <= 0) { return false; }
    let collection = document.getElementById("errormessenger").children.length;
    if (collection > 0) { return false; }
    let importRes = document.getElementById("import-res").children.length;
    if (importRes > 0) { return false; }
    if (document.getElementById("NoPatternCode") !== null) { return false; }
    if (load == "LOAD") {
        LoadingAnimationStart();
    }
    return true;

}

function LoadingAnimationStart() {
    
    //document.querySelector("#tab_content").style.visibility = "hidden";

    $("#tab_content").addClass("disable-div");
    $("#tab_content").addClass("opacity-7");

    $("#divSpinner").show();
    document.querySelector("#divSpinner").style.visibility = "visible";
    document.getElementById("divSpinner").style.display = "contents";
    $("#divSpinner").addClass("opacity-10");

    //document.querySelector("#loader").style.visibility = "visible";
    //$("#loader").addClass("opacity-10");
}

function LoadingAnimationStop() {
    document.querySelector("#loader").style.display = "none";
    //document.querySelector("body").style.visibility = "visible";
}

function setErrorMessengerBlank() {
    let importres = document.querySelector('#import-res');
    while (importres.firstChild) {
        importres.removeChild(importres.firstChild);
    }

    let errormessenger = document.querySelector('#errormessenger');
    while (errormessenger.firstChild) {
        errormessenger.removeChild(errormessenger.firstChild);
    }
    //document.getElementById("import-res").textContent = '';
    //if (document.getElementById("NoPatternCode") !== null) {
    //    document.getElementById("NoPatternCode").textContent = '';
    //}
  
}

function setDisplayBlankAllFiles() {
    // set blank
    document.getElementById("file0").innerHTML = '';
    document.getElementById("file1").innerHTML = '';
    document.getElementById("file2").innerHTML = '';
    document.getElementById("file3").innerHTML = '';
    document.getElementById("file4").innerHTML = '';
    document.getElementById("file5").innerHTML = '';
    document.getElementById("file6").innerHTML = '';
    document.getElementById("file7").innerHTML = '';
    document.getElementById("file8").innerHTML = '';
    document.getElementById("file9").innerHTML = '';
}


function wirteListFiles(files) {
    if (files.length > 0) {
        // set blank
        setDisplayBlankAllFiles();

        // write file name
        for (let i = 0; i < files.length; i++) {
            let file = files.item(i).name;
            switch (i) {
                case 0: {
                    document.getElementById("file0").innerHTML = file;
                    break;
                }
                case 1: {
                    document.getElementById("file1").innerHTML = file;
                    break;
                }
                case 2: {
                    document.getElementById("file2").innerHTML = file;
                    break;
                }
                case 3: {
                    document.getElementById("file3").innerHTML = file;
                    break;
                }
                case 4: {
                    document.getElementById("file4").innerHTML = file;
                    break;
                }
                case 5: {
                    document.getElementById("file5").innerHTML = file;
                    break;
                }
                case 6: {
                    document.getElementById("file6").innerHTML = file;
                    break;
                }
                case 7: {
                    document.getElementById("file7").innerHTML = file;
                    break;
                }
                case 8: {
                    document.getElementById("file8").innerHTML = file;
                    break;
                }
                case 9: {
                    document.getElementById("file9").innerHTML = file;
                    break;
                }
                default: {
                    break;
                }
            }
        }
    }
    else {
        console.log('ko co file de ghi');
        // set blank
        setDisplayBlankAllFiles();
    }
}

function removeFilesUploaded() {
    let patternCode = document.getElementById('PatternCode').value;
    let patternName = document.getElementById('PatternName').value;
    console.log('start remove');
    document.getElementById("d_receiptmodel_upload_form").reset();
    document.getElementById('PatternCode').value = patternCode;
    document.getElementById('PatternName').value = patternName;
    console.log('finish remove');
}

// ●【選択時のチェック１】
// get numberfiles
function checkNumberFiles(fileInput) {
    let files = fileInput.files;
    let fileListLength = files.length;
    if (fileListLength > 10) {
        errorMessenger.push(comboboxShowMessage('選択できるファイルは10件までです。'));
    }
    if (fileListLength <= 0) {
        errorMessenger.push(comboboxShowMessage('選択できるファイルは1~10件までです。'));
    }
}

// ●【選択時のチェック２】
// get file extensions
function checkFileExtension(fileInput) {
    let files = fileInput.files;
    if (files.length > 0) {
        for (let i = 0; i < files.length; i++) {
            let file = files.item(i).name;
            let extension = file.substring(file.lastIndexOf('.')).toLowerCase();
            if (extension != '.xls' && extension != '.xlsx' && extension != '.csv') {
                errorMessenger.push(comboboxShowMessage('選択できる拡張子は .xls／.xlsx／.csv のみです。'));
                console.log(file);
            }
        }
    }
}

// ●【選択時のチェック3】
// file name overlap check
function checkOverlapFilenames(fileInput) {
    let files = fileInput.files;
    let fileNames = new Array();
    if (files.length > 0) {
        for (let i = 0; i < files.length; i++) {
            let file = files.item(i).name;
            let newFile = file.substring(0, file.lastIndexOf('.'));
            fileNames.push(newFile);
        }
    }
    let uniqueFileSingle = fileNames.filter((x, i, a) => a.indexOf(x) == i);
    if (files.length != uniqueFileSingle.length) {
        errorMessenger.push(comboboxShowMessage('ファイル名に重複があります。'));
    }
}


function onchangeGetPatternName(id) {
    let patternCode = document.getElementById(id).value;
    $.ajax({
        url: ROOT + '/D_ReceiveImport/GetPatternName',
        type: "POST",
        dataType: 'json',
        data: { wid: wid, patternCode: patternCode },
        success: function (res) {
            if (String(res.statusCode) == "true") {
                document.getElementById('PatternName').value = res.patternName;
            }
            else {
                document.getElementById('PatternName').value = res.patternName;
            }
        },
        processData: true
    });
}

function onclickChooseFile(id) {
    console.log('============ click choose file before =============='); 
    $('#file-select-form input').trigger('click');
    //document.getElementById("FileImports").value = null;

    //let fileInput = document.querySelector("#FileImports");
    //let files = fileInput.files;
    //console.log(files);
}

function formUploads(formId) {
    // alert(formId);
    //console.log('formId : ' + formId);
    //console.log('start change');
    //var form = new FormData(document.querySelector(`#${formId}`));
    //const fileInput = document.querySelector("#FileImports");
    //console.log(fileInput);

    //console.log(form);

    //const input = document.querySelector('#FileImports');

    //// Listen for file selection event
    //input.addEventListener('change', (e) => {
    //    fileUpload(input.files[0]);
    //});
    console.log('thay doi trong from');
}

function comboboxShowMessage(mess) {
    return mess;
}
// Function that handles file upload using XHR
const fileUpload = (file) => {
    // Create FormData instance
    let fd = new FormData();
    fd.append('avatar', file);
    console.log(fd);
};

function preCheckImport() {
    let isFormCheck = onSubmitFunction('NO');
    let value = document.getElementById('PatternCode').value;
    if (isFormCheck) {
        if (window.FormData == undefined)
            alert("Error: FormData is undefined");

        else {
            let fileUpload = $("#FileImports").get(0);
            let files = fileUpload.files;
            let fileData = new FormData();
            for (var i = 0; i !== files.length; i++) {
                fileData.append('Files', files[i]);
            }
            fileData.append('PatternCode', value);
            let data = {
                PatternCode: value,
                Files: fileData
            };
            console.log('data : ', data );
            $.ajax({
                url: ROOT + '/D_ReceiveImport/PreCheckUploadFiles',
                type: 'post',
                datatype: 'json',
                contentType: false,
                processData: false,
                async: false,
                data: fileData,
                success: function (response) {
                    console.log(response);
                }
            });
        }
    }
}

function onlyNumberKey(evt) {
    let ch = String.fromCharCode(evt.which);
    if (!(/[0-9]/.test(ch))) {
        evt.preventDefault();
    }
}
