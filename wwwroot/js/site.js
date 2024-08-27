//var pathName = $(location).attr('pathname');
//var pathNameSpirit = pathName.split('/');
//var mainPathName = pathNameSpirit[1];
//var ROOT =
//    $(location).attr('protocol') + '//' + $(location).attr('host') + '/' + mainPathName + '/';

$(document).ready(function () {

    // 画面開いたら最初のInput要素にフォーカス
    //$('input[type!=hidden]:first').focus();

    // カレンダー
    $('.datepicker').on('click', function () {
        Datepicker(this);
    });
    // ブラウザのオートコンプリートをオフにする
    $('.datepicker').attr('autocomplete', 'off');
    // 数字が8文字なら日付形式に変換
    $('.datepicker').on('change blur', function () {
        var date = $.trim($(this).val());
        if (date.length === 8) {
            var year = date.substr(0, 4);
            var month = date.substr(4, 2);
            var day = date.substr(6, 2);
            var setDate = year + "/" + month + "/" + day;
            $(this).val(setDate);
        }
       
    });

    // 登録が完了しました、などのお知らせを消す
    $('input[type=text], select, input[type=checkbox], input[type=radio]').on('blur', function () {
        $('.message-area').text('');
    })

    // 検索枠の折りたたみ機能
    $('.serch-tab').on('click', function () {
        $(this).next().slideToggle();
        var close = $(this).hasClass('tab-close');
        if (close) {
            $(this).removeClass('tab-close');
        } else {
            $(this).addClass('tab-close');
        }
    });

    //// 一覧データの編集中、再度『検索』ボタンを押したときの警告
    //$('.search-btn').on('click', function () {
    //    var changeCount = $(document).find('[data-changing="true"]').length;
    //    if (changeCount > 0) {
    //        if (!confirm('未更新データが存在します。ページを更新してもよろしいですか？')) {
    //            return false;
    //        }
    //    }
    //})

    // 何らかの理由でリロードしたとき、取引先に値が入っていたら名称をセットする
    //var supplierCode = $.trim($('.Supplier_Autocomplete').val()).toString();
    //if (supplierCode.length) {
    //    var target = $('.Supplier_Autocomplete');
    //    SetSupplierName(supplierCode, target);
    //};
    //var customersCode = $.trim($('.CustomersCode_Autocomplete').val()).toString();
    //if (customersCode.length) {
    //    var target = $('.CustomersCode_Autocomplete');
    //    SetCustomerName(customersCode, target);
    //};

    //$('.Supplier_Autocomplete').on('change blur', function () {
    //    var supplierCode = $.trim($(this).val()).toString();
    //    SetSupplierName(supplierCode, this);
    //});

    //$('.CustomersCode_Autocomplete').on('change blur', function () {
    //    var customerCode = $.trim($(this).val()).toString();
    //    SetCustomerName(customerCode, this);
    //});

    $(document).find('.Product_Autocomplete').on('keydown', function () {
        Product_Autocomplete(this);
    });

    //SuppliersCodeEmptyAction($('.Supplier_Autocomplete'));

});

$('.Supplier_Autocomplete, .CustomersCode_Autocomplete').autocomplete({
    source: function (request, response) {
        var url = ROOT + "/AutoComplete/AutoCompleteSupplier";
        $.ajax({
            url: url,
            data: { "prefix": $.trim(request.term) },
            type: "POST",
            success: function (data) {
                console.log(data);
                if (data.length > 0) {
                    response($.map(data, function (item) {
                        var resLabel = item.val + "：" + item.label;
                        return resLabel;
                    }))
                }
                else {
                    return;
                }
            },
            error: function (response) {
                console.log('autoComplete error');
                alert(response.responseText);
            },
            failure: function (response) {
                console.log('autoComplete failure');
                alert(response.responseText);
            }
        });
    },
    select: function (e, i) {
        var selectLabel = i.item.label;
        var index = selectLabel.indexOf("：");
        var selectCode = selectLabel.substring(0, index);
        i.item.value = selectCode;

        var selectName = selectLabel.substring(index + 1);
        $(this).closest('tr').find('.Supplier_AutocompleteName_Table').val(selectName);
        $(this).closest('form').find('.Supplier_AutocompleteName_Form').val(selectName);
    },
    appendTo: "form",
    minLength: 1
});

$('.Supplier_Autocomplete').on('change blur', function () {
    SupplierCodeEmptyAction(this);
});

$('.Product_Autocomplete').on('change blur', function () {
    ProductCodeEmptyAction(this);
});

function SupplierCodeEmptyAction(target) {
    var supplierCode = $.trim($(target).val()).toString();
    if (supplierCode.length == 0) {
        $(target).closest('tr').find('.Supplier_AutocompleteName_Table').val("");
        $(target).closest('form').find('.Supplier_AutocompleteName_Form').val("");
    }
}
function ProductCodeEmptyAction(target) {
    var productCode = $.trim($(target).val()).toString();
    if (productCode.length == 0) {
        $(target).closest('tr').find('.Product_AutocompleteName_Table').val("");
        $(target).closest('form').find('.Product_AutocompleteName_Form').val("");
    }
}
$('.Supplier_Autocomplete').on('blur', function () {
    //var target = $(this);
    var supplierCode = $(this).val();
    // オートコンプリートでテキストボックスに名称が残ってしまう場合の対応
    var supIndex = supplierCode.indexOf("：");
    if (supIndex != (-1)) {
        var sup_code = supplierCode.substring(0, supIndex);
        var sup_name = supplierCode.slice(supIndex + 1);
        $(this).val(sup_code);
        $(this).closest('tr').find('.Supplier_AutocompleteName_Table').val(sup_name);
        $(this).closest('form').find('.Supplier_AutocompleteName_Form').val(sup_name);
    }
});

// メモ:動的に追加したテキストボックスにもオートコンプリートを仕掛けたいので関数化しkeydownイベントで発火させる
function Product_Autocomplete(target) {
    $(target).autocomplete({
        source: function (request, response) {
            var depoId = 0;
            depoId = $.trim($(target).closest("form").find("#DepoID").val());
            if (depoId == 0) {
                depoId = $.trim($(target).closest('tr').find('.DepoID').val());
            }
            console.log(depoId);
            var url = ROOT + "/AutoComplete/AutoCompleteProduct";
            $.ajax({
                url: url,
                //data: { "prefixCode": $.trim(request.term), "depoCode": depoCd, "supplierCode": supCd },
                data: { "prefix": $.trim(request.term), "depoId": depoId },
                type: "POST",
                success: function (data) {
                    console.log(data);
                    if (data.length > 0) {
                        response($.map(data, function (item) {
                            var resLabel;

                            if (item.label.length > 0) {
                                resLabel = item.val + "：" + item.label;
                            }
                            else {
                                resLabel = item.val;
                            }
                            return resLabel;
                        }))
                    }
                    else {
                        return;
                    }
                },
                error: function (response) {
                    console.log('autoComplete error');
                    alert(response.responseText);
                },
                failure: function (response) {
                    console.log('autoComplete failure');
                    alert(response.responseText);
                }
            });
        },
        select: function (e, i) {
            var selectLabel = i.item.label;
            var index = selectLabel.indexOf("：");
            var selectCode;
            if (index >= 0) {
                selectCode = selectLabel.substring(0, index);
            }
            else {
                selectCode = selectLabel;
            }
            i.item.value = $.trim(selectCode);

            var selectName = selectLabel.substring(index + 1);
            $(this).closest('tr').find('.Product_AutocompleteName_Table').val(selectName);
            $(this).closest('form').find('.Product_AutocompleteName_Form').val(selectName);
        },
        appendTo: "form",
        minLength: 1
    })
};

// URLの「?」以降をクリア
function ClearUrlGet() {
    var get_param = window.location.href.substring(window.location.href.indexOf("?"));
    var default_url = window.location.href.replace(get_param, '');

    history.replaceState('', '', default_url);
}

//$(document).find('.Product_Autocomplete').autocomplete({
//    source: function (request, response) {
//        var loginUserId = $.trim($(document).find('.LoginUserId').val());
//        console.log('loginUserId', loginUserId);
//        var supCd = $.trim($(document).find('.Supplier_Autocomplete').val());
//        $.ajax({
//            url: ROOT + "AutoComplete/AutoComplete_Buhin",
//            data: { "prefixCode": $.trim(request.term), "supplierCode": supCd, "loginUserId": loginUserId },
//            type: "POST",
//            success: function (data) {
//                response($.map(data, function (item) {
//                    var resLabel = item.val + "：" + item.label;
//                    return resLabel;
//                }))
//            },
//            error: function (response) {
//                console.log('autoComplete error');
//                alert(response.responseText);
//            },
//            failure: function (response) {
//                console.log('autoComplete failure');
//                alert(response.responseText);
//            }
//        });
//    },
//    select: function (e, i) {
//        var selectLabel = i.item.label;
//        var index = selectLabel.indexOf("：");
//        var selectCode = selectLabel.substring(0, index);
//        i.item.value = $.trim(selectCode);
//    },
//    minLength: 1
//});
//$(document).on('blur', '.Product_Autocomplete', function () {
//    var self = $(this);
//    var Product = self.val();
//    var DepoCode = $.trim(self.closest('tr').find('.DepoCode').val());
//    if (DepoCode.length === 0) {
//        DepoCode = $.trim(self.closest('form').find('.DepoCode').val());
//    }
//    console.log(DepoCode);
//    var SuppliersCode = $.trim(self.closest('tr').find('.Supplier_Autocomplete').val());
//    if (SuppliersCode.length === 0) {
//        SuppliersCode = $.trim(self.closest('form').find('.Supplier_Autocomplete').val());
//    }
//    //オートコンプリートでテキストボックスに名称が残ってしまう場合の対応
//    var buhinIndex = Product.indexOf("：");
//    if (buhinIndex != (-1)) {
//        var buhin_no = $.trim(Product.substring(0, buhinIndex));
//        var buhin_name = $.trim(Product.slice(buhinIndex + 1));
//        $('.Product_Autocomplete').val(buhin_no);

//        $(self).closest('tr').find('.Product_AutocompleteName').val(buhin_name);
//    }
//    else {
//        SetBuhinName(Product, DepoCode, SuppliersCode, self);
//    }
//});

//$('.Supplier_Autocomplete').on('keydown', function () {
//    var serchKey = $(this).val();
//    if ($.type(serchKey) === "string") {
//        console.log('文字列');
//    }
//    else {
//        console.log('それ以外');
//    }
//});

//function SetCustomerName(code, target) {
//    var url = ROOT + "AutoComplete/GetJsonUnyName";
//    $.ajax({
//        url: url,
//        type: "POST",
//        data: { "suppliersCd": code }
//    }).done(function (supplierName) {
//        if (supplierName[0] === "OK") {
//            $(target).closest('tr').find('.CustomersCode_AutocompleteName_Table').val(supplierName[2]);
//            $(target).closest('form').find('.CustomersCode_AutocompleteName_Form').val(supplierName[2]);
//        }
//        else {
//            $(target).closest('tr').find('.CustomersCode_AutocompleteName_Table').val('');
//            $(target).closest('form').find('.CustomersCode_AutocompleteName_Form').val('');
//        }
//    });
//}

//function SetSupplierName(code, target) {
//    var url = ROOT + "AutoComplete/GetJsonUnyName";
//    $.ajax({
//        url: url,
//        type: "POST",
//        data: { "suppliersCd": code }
//    }).done(function (supplierName) {
//        if (supplierName[0] === "OK") {
//            console.log(supplierName);
//            console.log($(target).closest('tr').find('.Supplier_AutocompleteName_Table'));
//            $(target).closest('tr').find('.Supplier_AutocompleteName_Table').val(supplierName[2]);
//            $(target).closest('form').find('.Supplier_AutocompleteName_Form').val(supplierName[2]);
//        }
//        else {
//            $(target).closest('tr').find('.Supplier_AutocompleteName_Table').val('');
//            $(target).closest('form').find('.Supplier_AutocompleteName_Form').val('');
//        }
//    });
//}

//function SetBuhinName(no, depo, code, target) {
//    var targetName = $(target).closest('tr').find('.Product_AutocompleteName');
//    if (targetName.length === 0) {
//        targetName = $(target).closest('form').find('.Product_AutocompleteName');
//    }
//    var url = ROOT + "AutoComplete/GetJsonBuhinName";
//    $.ajax({
//        url: url,
//        type: "POST",
//        data: { "buhinNo": no, "depoCd": depo, "suppliersCd": code}
//    }).done(function (buhinName) {
//        console.log(buhinName);
//        if (buhinName[0] === "OK") {
//            buhinName = $.trim(buhinName[2]);
//            $(targetName).val(buhinName);
//        }
//        else {
//            $(targetName).val('');
//        }
//    });
//}

function SelectListDisabled() {
    var selectList = $(document).find('select');
    $.each(selectList, function (index, select) {
        if ($(select).find('option').length === 0 || ($(select).find('option').val().length == 0 && $(select).find('option').text().length == 0)) {
            $(select).prop('disabled', true);
        }
    })
}

function Datepicker(textBox) {
    //datepickerオプション 参考：https://www.sejuku.net/blog/44165
    $(textBox).datepicker({
        showAnim: 'fadeIn',
        dateFormat: 'yy/mm/dd'
    })
    $(textBox).trigger('focus');
}


function DeleteKigo(val) {
    if (val === null || val === undefined || val === "") {
        return "";
    }
    return val.replace(/[!"#$%&'()*+,.\/:;<=>?\[\\\]^`{|}~]/g, "");
}

function TableSortChange(self, dataCount, rowCount) {

    // <th>内に【<span class="sort" data-sort="種類"></span>】を追加する
    //　種類・・・数値ソート"num"　文字列ソート"str"

    var pageCount = Math.ceil(dataCount / rowCount);
    var lastPageRowCount = dataCount % rowCount;

    if (dataCount <= rowCount) {
        lastPageRowCount = dataCount;
    }

    var targetColumn = self;
    var sortColumn = targetColumn.find('span');
    var list = $('tbody > tr');

    var targetData = self.closest('table').find('td').eq(self.index());
    var targetDataClass = targetData.attr('class');

    if (!sortColumn.hasClass('sort-asc') && !sortColumn.hasClass('sort-desc')) {
        $(document).find('th span').removeClass('sort-asc');
        $(document).find('th span').removeClass('sort-desc');
        sortColumn.addClass('sort-asc');
    }
    else if (sortColumn.hasClass('sort-asc')) {
        $(document).find('th span').removeClass('sort-asc');
        $(document).find('th span').removeClass('sort-desc');
        sortColumn.addClass('sort-desc');
    }
    else if (sortColumn.hasClass('sort-desc')) {
        $(document).find('th span').removeClass('sort-asc');
        $(document).find('th span').removeClass('sort-desc');
        sortColumn.addClass('sort-asc');
    }

    var sortType = sortColumn.attr('data-sort');
    //テキストボックスのソート変更
    if (targetData.find('input[type=text]').length > 0) {
        if (sortType === 'num') {
            list.sort(function (a, b) {
                var a_val = DeleteKigo($(a).find('.' + targetDataClass + ' input').val());
                var b_val = DeleteKigo($(b).find('.' + targetDataClass + ' input').val());
                if (sortColumn.hasClass('sort-asc')) {
                    return Number(a_val) - Number(b_val);
                }
                else {
                    return Number(b_val) - Number(a_val);
                }
            });
        }
        else if (sortType === 'str') {
            list.sort(function (a, b) {
                var a_val = DeleteKigo($(a).find('.' + targetDataClass + ' input').val());
                var b_val = DeleteKigo($(b).find('.' + targetDataClass + ' input').val());
                if (sortColumn.hasClass('sort-asc')) {
                    if (a_val > b_val) {
                        return 1;
                    }
                    else {
                        return -1;
                    }
                }
                else {
                    if (a_val < b_val) {
                        return 1;
                    }
                    else {
                        return -1;
                    }
                }

            });
        }
    }
    //セレクトボックスのソート変更
    else if (targetData.find('select').length > 0) {
        if (sortType === 'num') {
            list.sort(function (a, b) {
                var a_val = DeleteKigo($(a).find('.' + targetDataClass + ' select').val());
                var b_val = DeleteKigo($(b).find('.' + targetDataClass + ' select').val());
                if (sortColumn.hasClass('sort-asc')) {
                    return Number(a_val) - Number(b_val);
                }
                else {
                    return Number(b_val) - Number(a_val);
                }
            });
        }
        else if (sortType === 'str') {
            list.sort(function (a, b) {
                var a_val = DeleteKigo($(a).find('.' + targetDataClass + ' select').val());
                var b_val = DeleteKigo($(b).find('.' + targetDataClass + ' select').val());
                if (sortColumn.hasClass('sort-asc')) {
                    if (a_val > b_val) {
                        return 1;
                    }
                    else {
                        return -1;
                    }
                }
                else {
                    if (a_val < b_val) {
                        return 1;
                    }
                    else {
                        return -1;
                    }
                }

            });
        }
    }
    //ラベルのソート変更
    else {
        if (sortType === 'num') {
            list.sort(function (a, b) {
                var a_text = DeleteKigo($(a).find('.' + targetDataClass + ' label').text());
                var b_text = DeleteKigo($(b).find('.' + targetDataClass + ' label').text());
                if (sortColumn.hasClass('sort-asc')) {
                    return Number(a_text) - Number(b_text);
                }
                else {
                    return Number(b_text) - Number(a_text);
                }

            });
        }
        else if (sortType === 'str') {
            list.sort(function (a, b) {
                var a_text = DeleteKigo($(a).find('.' + targetDataClass + ' label').text());
                var b_text = DeleteKigo($(b).find('.' + targetDataClass + ' label').text());
                if (sortColumn.hasClass('sort-asc')) {
                    if (a_text > b_text) {
                        return 1;
                    }
                    else {
                        return -1;
                    }
                }
                else {
                    if (a_text < b_text) {
                        return 1;
                    }
                    else {
                        return -1;
                    }
                }

            });
        }
    }

    var sortSetList = [];
    $.each(list, function (i, e) {
        //新しい並び順の配列に行番号をセットし直す
        $(e).find('.input-row-no').text(i + 1);
        sortSetList.push(e);
    })
    var targetPageNo = Number($('.active').text());
    var endNo = rowCount * targetPageNo;
    var startNo = endNo - rowCount + 1;
    if (targetPageNo === pageCount) {   //ページが最大値だったら
        endNo = startNo + lastPageRowCount - 1;
    }

    SortChange(sortSetList, startNo, endNo);

    function SortChange(list, startNo, endNo) {
        $(document).find('tbody tr').remove();
        var newList = [];
        $.each(list, function (index, element) {
            var newRowNo = Number($(element).find('.input-row-no').text());
            if (newRowNo >= startNo && newRowNo <= endNo) {
                $(element).css({ "display": "" });
                newList.push(element);
            }
            else {
                $(element).css({ "display": "none" });
                newList.push(element);
            }
        })
        $(document).find('tbody').append(newList);
    }
}

function PageChange(dataCount, rowCount) {

    var pageCount = Math.ceil(dataCount / rowCount);
    var lastPageRowCount = dataCount % rowCount;

    if (dataCount <= rowCount) {
        lastPageRowCount = dataCount;
    }

    // ページャーに番号セット
    var pageNoAdd = "";
    for (var i = 1; i <= pageCount; ++i) {
        pageNoAdd += ('<li><a class="paging-link paging-no" href="#">' + i + '</a></li>');
    }
    $('.paging-no-replace').replaceWith(pageNoAdd);

    $('.prev').addClass('disabled');
    $('.paging-no:first').addClass('active');

    // ページ情報ラベルのセット
    var pageCountlabel;
    if (dataCount < rowCount) {
        pageCountlabel = "1～" + dataCount + "件 / " + dataCount + "件中";
    } else {
        pageCountlabel = "1～" + rowCount + "件 / " + dataCount + "件中";
    }

    $('.paging-count-label').text(pageCountlabel);

    // ページ番号クリック処理
    $('.paging-link').on('click', function () {

        // ページャーのレイアウト切り替え
        var targetPageNo = Number($(this).text());
        var targetPageElement = $(this);
        var isPrev = $(this).hasClass('prev');
        var isNext = $(this).hasClass('next');

        var beforeActivePageNo = Number($('.active').text());

        if (isPrev) {
            if (beforeActivePageNo === 1) {
                return false;
            }
            targetPageNo = beforeActivePageNo - 1;
            targetPageElement = $('.active').parent().prev().children('a');
        }
        else if (isNext) {
            if (beforeActivePageNo === pageCount) {
                return false;
            }
            targetPageNo = beforeActivePageNo + 1;
            targetPageElement = $('.active').parent().next().children('a');
        }

        if (!(targetPageNo === beforeActivePageNo)) {
            $('.active').removeClass('active');
            targetPageElement.addClass('active');
        }

        var isDisabled_prev = $('.prev').hasClass('disabled');
        var isDisabled_next = $('.next').hasClass('disabled');

        if (targetPageNo === 1) {
            if (!isDisabled_prev) {
                $('.prev').addClass('disabled');
            }
            if (isDisabled_next) {
                $('.next').removeClass('disabled');
            }
        }
        else if (targetPageNo === pageCount) {
            if (isDisabled_prev) {
                $('.prev').removeClass('disabled');
            }
            if (!isDisabled_next) {
                $('.next').addClass('disabled');
            }
        }
        else {
            if (isDisabled_prev) {
                $('.prev').removeClass('disabled');
            }
            if (isDisabled_next) {
                $('.next').removeClass('disabled');
            }
        }

        // 表示するデータの配列番号をセット
        var row_end = rowCount * targetPageNo;
        var row_start = row_end - rowCount + 1;
        if (targetPageNo === pageCount) {   // ページが最大値だったら
            row_end = row_start + lastPageRowCount - 1;
        }

        // ページ情報ラベルのセット
        pageCountlabel = (row_start) + "～" + row_end + "件 / " + dataCount + "件中";
        $('.paging-count-label').text(pageCountlabel);

        var trList = $(document).find('tbody tr');
        $.each(trList, function (index, element) {
            var domPageNo = Number($(element).find('.input-row-no').text());
            if (row_start <= domPageNo && row_end >= domPageNo) {
                $(element).css({ "display": "" });
            }
            else {
                $(element).css({ "display": "none" });
            }
        })
    })
}
//------------------- DataTables ------------------//
// 日本語表示
const language_url = "https://cdn.datatables.net/plug-ins/1.11.5/i18n/ja.json";

// ID(2列目)昇順
$('.datatable-normal').DataTable({
    "language": {
        "url": language_url
    },
    paging: true,           // ページング
    lengthChange: true,     // 件数切替
    info: false,            // 総件数
    scrollX: true,          // 横スクロール可
    scrollCollapse: true,   // 縦スクロール表示
    order: [[1, "asc"]],    // ID昇順
    columnDefs: [
        { targets: 0, sortable: false },    // インデックス0列(アイコン列)のソート禁止
    ]
});