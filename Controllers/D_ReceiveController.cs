using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using stock_management_system.common;
using stock_management_system.Models;
using X.PagedList;
using Dapper;
using System.ComponentModel.DataAnnotations;
using System.IO;
using SpreadsheetLight;
using static stock_management_system.Models.D_ReceiveModel;

namespace stock_management_system.Controllers
{
    public class D_ReceiveController : BaseController
    {
        public static LoginUserModel UserModel;

        // 検索条件セッションキーの設定
        private const string SESSIONKEY_PageNo = "PageNo";
        private const string SESSIONKEY_DepoCode = "DepoCode";
        private const string SESSIONKEY_SupplierCode = "SupplierCode";
        private const string SESSIONKEY_SupplierName = "SupplierName";
        private const string SESSIONKEY_ProductCode = "ProductCode";
        //private const string SESSIONKEY_ProductName = "ProductName";
        private const string SESSIONKEY_NextProcess1 = "NextProcess1";
        private const string SESSIONKEY_NextProcess2 = "NextProcess2";
        private const string SESSIONKEY_ReceiveDate_Start = "ReceiveDateStart";
        private const string SESSIONKEY_ReceiveDate_End = "ReceiveDateEnd";

        // ページサイズの設定
        private const int pageSize = 50;

        // 新規作成行数
        private const int createRowCount = 10;

        public IActionResult Index()
        {
            UserModel = UserDataList();
            SessionReset();
            var search = new D_ReceiveSearchModel();
            search.DepoID = UserModel.MainDepoID;
            return View(search);
        }

        public IActionResult Search(D_ReceiveSearchModel search)
        {
            // 入庫日の日付形式チェック
            if (!String.IsNullOrEmpty(search.ReceiveDateStart))
            {
                if (!DateTime.TryParse(search.ReceiveDateStart, out DateTime receiveDateStart))
                {
                    ViewData["Error"] = "入荷日を日付形式に変更できません";
                    return View("Index", search);
                }
            }
            if (!String.IsNullOrEmpty(search.ReceiveDateEnd))
            {
                if (!DateTime.TryParse(search.ReceiveDateEnd, out DateTime receiveDateEnd))
                {
                    ViewData["Error"] = "入荷日を日付形式に変更できません";
                    return View("Index", search);
                }
            }

            int page = 1;

            // 検索条件をセッションにセット
            SessionSet(page, search);

            return Search(search, page);
        }

        public IActionResult SearchByPageChange(int page)
        {
            // ページャー移動のとき
            // 前回の検索条件をセッションから取得し一覧表示

            var session = SessionGet();
            var search = session.SerchModel;

            return Search(search, page);
        }

        public IActionResult SearchByPageBack()
        {
            var search = new D_ReceiveSearchModel();

             // 戻るボタンを押したとき
             // 前回の検索条件をセッションから取得し一覧表示

             var session = SessionGet();

            if (session.PageNo == 0)
            {
                // まだ一度も検索をしていない場合
                return View("Index", search);
            }

            search = session.SerchModel;
            int page = session.PageNo;

            // 編集データ表示エラーで戻った時
            if (TempData["Error"] != null)
            {
                var error = TempData["Error"].ToString();
                ViewData["Error"] = error;
            }

            return Search(search, page);
        }

        private IActionResult Search(D_ReceiveSearchModel search, int page)
        {
            try
            {
                var viewList = GetReceiveList(search);
                var _dataCount = viewList.Count;

                if (_dataCount > 0)
                {
                    search.ReceiveList = viewList.ToPagedList(page, pageSize);

                    // ページ関連情報セット
                    search.Page = new Page();
                    var pageData = Util.ComPageNoGet(page, pageSize, _dataCount);
                    search.Page.PageRowCount = _dataCount;
                    search.Page.PageRowStartNo = pageData.PageRowStartNo;
                    search.Page.PageRowEndNo = pageData.PageRowEndNo;
                }
                else
                {
                    ViewData["Error"] = "対象データが存在しません。";
                }

            }
            catch(Exception ex)
            {
                ViewData["Error"] = "データ検索に失敗しました。";
            }

            return View("Index", search);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ExcelOutput(D_ReceiveSearchModel searchModel)
        {
            try
            {
                var receiptList = GetReceiveList(searchModel);

                // ファイル名
                var filename = "receive_data_" + DateTime.Now.ToString("yyyyMMddHHmmss");

                var headerNameList = new List<string>();

                // TODO:クラス化したい
                // 参考：https://stackoverflow.com/questions/7335629/get-displayattribute-attribute-from-propertyinfo
                var properties = typeof(D_ReceiveExcelViewModel).GetProperties()
                    .Where(p => p.IsDefined(typeof(DisplayAttribute), false))
                    .Select(p => new
                    {
                        PropertyName = p.Name,
                        DisplayName = p.GetCustomAttributes(typeof(DisplayAttribute),
                                false).Cast<DisplayAttribute>().Single().Name
                    });

                foreach (var property in properties)
                {
                    headerNameList.Add(property.DisplayName);
                }

                MemoryStream ms = new MemoryStream();
                using (SLDocument sl = new SLDocument())
                {
                    // 太字
                    SLStyle keyStyle = sl.CreateStyle();
                    keyStyle.SetFontBold(true);

                    // 1行目：ヘッダーをセット
                    for (int i = 1; i < (headerNameList.Count() + 1); ++i)
                    {
                        //if (i == 1 || i == 2 || i == 3)
                        //{
                        //    sl.SetCellStyle(1, i, keyStyle);
                        //}
                        sl.SetCellStyle(1, i, keyStyle);

                        sl.SetCellValue(1, i, headerNameList[i - 1]);
                    }

                    // 2行目～：値をセット
                    var data = receiptList;
                    if (data != null && data.Count() > 0)
                    {
                        for (int col = 2; col < (data.Count() + 2); ++col)
                        {
                            int row = 0;
                            int _col = col - 2;
                            sl.SetCellValue(col, ++row, data[_col].DepoCode);
                            sl.SetCellValue(col, ++row, data[_col].DepoName);
                            sl.SetCellValue(col, ++row, data[_col].ReceiveDate);
                            sl.SetCellValue(col, ++row, data[_col].DeliveryDate);
                            sl.SetCellValue(col, ++row, data[_col].DeliveryTimeClass);
                            sl.SetCellValue(col, ++row, data[_col].DeliverySlipNumber);
                            sl.SetCellValue(col, ++row, data[_col].DeliverySlipRowNumber);
                            sl.SetCellValue(col, ++row, data[_col].SupplierCode);
                            sl.SetCellValue(col, ++row, data[_col].SupplierName);
                            sl.SetCellValue(col, ++row, data[_col].ProductLabelBranchNumber);
                            sl.SetCellValue(col, ++row, data[_col].NextProcess1);
                            sl.SetCellValue(col, ++row, data[_col].Location1);
                            sl.SetCellValue(col, ++row, data[_col].NextProcess2);
                            sl.SetCellValue(col, ++row, data[_col].Location2);
                            sl.SetCellValue(col, ++row, data[_col].ProductCode);
                            sl.SetCellValue(col, ++row, data[_col].ProductAbbreviation);
                            sl.SetCellValue(col, ++row, data[_col].ProductManagementClass);
                            sl.SetCellValue(col, ++row, data[_col].ProductName);
                            sl.SetCellValue(col, ++row, data[_col].LotQuantity);
                            sl.SetCellValue(col, ++row, data[_col].Packing);
                            sl.SetCellValue(col, ++row, data[_col].PackingCount);
                            sl.SetCellValue(col, ++row, data[_col].Quantity);
                            sl.SetCellValue(col, ++row, data[_col].FractionQuantity);
                            sl.SetCellValue(col, ++row, data[_col].Remark);
                            sl.SetCellValue(col, ++row, data[_col].CreateUserCode);
                            sl.SetCellValue(col, ++row, data[_col].CreateHandyUserCode);
                            sl.SetCellValue(col, ++row, data[_col].ScanTime);
                            sl.SetCellValue(col, ++row, data[_col].CreateDate);
                            //sl.SetCellValue(col, ++row, data[_col].UpdateDate);
                            //sl.SetCellValue(col, ++row, data[_col].UpdateUser);
                        }
                    }

                    sl.SaveAs(ms);
                }

                ms.Position = 0;

                return File(ms, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", filename + ".xlsx");

            }
            catch (Exception ex)
            {
                ViewData["Error"] = "データの取得に失敗しました。";
                return View("Index", searchModel);
            }

        }

        private List<D_ReceiveViewModel> GetReceiveList(D_ReceiveSearchModel search)
        {
            //var user = UserDataList();
            string whereString;

            var sb = new StringBuilder($@"WHERE (A.DepoID = @DepoID) AND (A.DeleteFlag = @DeleteFlag) ");

            if (!String.IsNullOrEmpty(search.SupplierCode))
            {
                sb.Append($@"AND (A.SupplierCode LIKE ('%'+@SupplierCode+'%')) ");
            }
            if (!String.IsNullOrEmpty(search.ProductCode))
            {
                sb.Append($@"AND (A.ProductCode LIKE ('%'+@ProductCode+'%')) ");
            }
            if (!String.IsNullOrEmpty(search.NextProcess1))
            {
                sb.Append($@"AND (A.NextProcess1 LIKE ('%'+@NextProcess1+'%')) ");
            }
            if (!String.IsNullOrEmpty(search.NextProcess2))
            {
                sb.Append($@"AND (A.NextProcess2 LIKE ('%'+@NextProcess2+'%')) ");
            }
            if (!String.IsNullOrEmpty(search.ReceiveDateStart))
            {
                sb.Append($@"AND (A.ReceiveDate >= @ReceiveDateStart) ");
            }
            if(!String.IsNullOrEmpty(search.ReceiveDateEnd))
            {
                sb.Append($@"AND (A.ReceiveDate <= @ReceiveDateEnd) ");
            }

            whereString = sb.ToString();

            var receiveViewModels = new List<D_ReceiveViewModel>();
            try
            {
                var db = User.Claims.Where(x => x.Type == CustomClaimTypes.ClaimType_DatabaseName).First().Value;
                var connectionString = new GetConnectString(db).ConnectionString;
                using (var connection = new SqlConnection(connectionString))
                {
                    //open
                    connection.Open();

                    //commmand
                    var commandText = $@"
                        SELECT 
                               A.ReceiveID
                              ,A.ScanRecordID
                              ,A.ReceiveScheduleDetailID
                              ,FORMAT(A.ReceiveDate,'yyyy/MM/dd') AS ReceiveDate
                              ,A.DepoID
                              ,B.DepoCode
                              ,B.DepoName
                              ,CASE WHEN FORMAT(A.DeliveryDate, 'yyyyMMdd') = '19000101' THEN '' ELSE FORMAT(A.DeliveryDate, 'yyyy/MM/dd') END AS DeliveryDate
                              ,A.DeliveryTimeClass
                              ,A.DeliverySlipNumber
                              ,A.DeliverySlipRowNumber
                              ,A.SupplierCode
                              ,C.SupplierName
                              ,A.CustomerCode
                              ,A.ProductCode
                              ,A.ProductAbbreviation
                              ,A.ProductManagementClass
                              ,A.ProductLabelBranchNumber
                              ,A.NextProcess1
                              ,A.Location1
                              ,A.NextProcess2
                              ,A.Location2
                              ,A.LotQuantity
                              ,A.FractionQuantity
                              ,A.DefectiveQuantity
                              ,A.Quantity
                              ,A.Packing
                              ,A.PackingCount
                              ,A.LotNumber
                              ,A.InvoiceNumber
                              ,A.OrderNumber
                              ,A.ExpirationDate
                              ,A.CostPrice
                              ,A.DeleteFlag
                              ,A.DeleteReceiveID
                              ,A.Remark
                              ,FORMAT(A.CreateDate,'yyyy/MM/dd HH:mm') AS CreateDate
                              ,A.CreateUserID
                              ,ISNULL(D.UserCode, '') AS CreateUserCode
                              ,ISNULL(E.HandyUserCode, '') AS CreateHandyUserCode
                              ,CASE WHEN FORMAT(F.ScanTime, 'yyyyMMdd') = '19000101' OR (F.ScanTime is null) THEN '' ELSE FORMAT(F.ScanTime, 'yyyy/MM/dd HH:mm') END AS ScanTime
                          FROM D_Receive AS A
                          LEFT OUTER JOIN M_Depo AS B ON A.DepoID = B.DepoID
                          LEFT OUTER JOIN M_Supplier AS C ON A.SupplierCode = C.SupplierCode
                          LEFT OUTER JOIN M_User AS D ON A.CreateUserID = D.UserID
                          LEFT OUTER JOIN M_HandyUser AS E ON A.CreateUserID = E.HandyUserID
                          LEFT OUTER JOIN D_ScanRecord AS F ON A.ScanRecordID = F.ScanRecordID
                    " + whereString + " ORDER BY A.UpdateDate DESC, ReceiveDate DESC, ProductCode ASC, SupplierCode ASC";

                    var param = new
                    {
                        search.DepoID,
                        DeleteFlag = false,
                        search.SupplierCode,
                        search.ProductCode,
                        search.NextProcess1,
                        search.NextProcess2,
                        search.ReceiveDateStart,
                        search.ReceiveDateEnd
                    };
                    receiveViewModels = connection.Query<D_ReceiveViewModel>(commandText, param).ToList();

                    if (receiveViewModels.Count() == 0) ViewData["Error"] = "検索条件に一致する出荷データはありません";
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            return receiveViewModels;
        }

        public IActionResult Create()
        {
            var receiveModel = new D_ReceiveViewModel();

            var receiptList = new List<D_ReceiveViewModel>();
            receiveModel.ReceiveList = receiptList;

            //var user = UserDataList();
            for (int i = 0; i < createRowCount; i++) 
            {
                var viewModel = new D_ReceiveViewModel();
                viewModel.DepoID = UserModel.MainDepoID;
                receiveModel.ReceiveList.Add(viewModel);
            }

            var temp = TempData["msg"];
            if (temp != null)
            {
                ViewData["Message"] = temp;
            }

            return View(receiveModel);
        }

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public IActionResult Create(D_ReceiveViewModel inputModel)
        //{
        //    var inputList = inputModel.ReceiveList;

        //    var user = UserDataList();

        //    var inputData = inputList.Where(x => !String.IsNullOrEmpty(x.ReceiveDate)).ToList();
        //    if (inputData.Count == 0)
        //    {
        //        ViewData["Error"] = "登録対象のデータが存在しません";
        //        return View(inputModel);
        //    }

        //    inputData = inputData.Select(s =>
        //    {
        //        s.ReceiveDate = (s.ReceiveDate ?? "").Trim();
        //        s.SupplierCode = (s.SupplierCode ?? "").Trim();
        //        s.NextProcess1 = (s.NextProcess1 ?? "").Trim();
        //        s.NextProcess2 = (s.NextProcess2 ?? "").Trim();
        //        s.SlipNumber = (s.SlipNumber ?? "").Trim();
        //        s.ProductCode = (s.ProductCode ?? "").Trim();
        //        s.Packing = (s.Packing ?? "").Trim();
        //        return s;
        //    }).ToList();

        //    for (int i = 0; i < inputData.Count(); ++i)
        //    {
        //        var rowNo = inputData[i].RowNo;

        //        if (String.IsNullOrEmpty(inputData[i].SupplierCode))
        //        {
        //            ViewData["Error"] = rowNo + "行目：仕入先コードを入力してください。";
        //            return View(inputModel);
        //        }
        //        else if (String.IsNullOrEmpty(inputData[i].NextProcess1))
        //        {
        //            ViewData["Error"] = rowNo + "行目：納入先を入力してください。";
        //            return View(inputModel);
        //        }
        //        else if (String.IsNullOrEmpty(inputData[i].NextProcess2))
        //        {
        //            ViewData["Error"] = rowNo + "行目：受入を入力してください。";
        //            return View(inputModel);
        //        }
        //        else if (String.IsNullOrEmpty(inputData[i].ProductCode))
        //        {
        //            ViewData["Error"] = rowNo + "行目：商品コードを入力してください。";
        //            return View(inputModel);
        //        }
        //        else if (inputData[i].ReceiveQuantity == 0)
        //        {
        //            ViewData["Error"] = rowNo + "行目：入数を入力してください。";
        //            return View(inputModel);
        //        }
        //        else if (inputData[i].PackingCount == 0)
        //        {
        //            ViewData["Error"] = rowNo + "行目：箱数を入力してください。";
        //            return View(inputModel);
        //        }

        //        // 合計数量を計算
        //        inputData[i].ReceiveTotalQuantity = inputData[i].ReceiveQuantity * inputData[i].PackingCount;

        //        //// 入庫日の日付形式チェック
        //        //if (!ErrorCheck.IsChangeDateTime(inputData[i].ReceiveDate))
        //        //{
        //        //    ViewData["Error"] = rowNo + "行目：入庫日を日付形式（yyyy/MM/dd）に変更できません";
        //        //    return View(inputModel);
        //        //}

        //        //// 仕入先の存在確認
        //        //var unyNameData = ErrorCheck.GetUnyName(inputData[i].SupplierCode);
        //        //if (unyNameData[i] == "NG")
        //        //{
        //        //    ViewData["Error"] = rowNo + "行目：仕入先コードが存在しません";
        //        //    return View(inputModel);
        //        //}

        //        //// 商品の存在確認
        //        //var buhinData = GetBuhinName(inputData[i].DepoCode, inputData[i].SupplierCode, inputData[i].ProductCode);
        //        //if (buhinData[0] == "NG")
        //        //{
        //        //    ViewData["Error"] = rowNo + "行目：商品コードが存在しません";
        //        //    return View(inputModel);
        //        //}

        //    }

        //    // INSERT処理
        //    try
        //    {
        //        var insertResult = ReceiveInsert(inputData);
        //        if (insertResult)
        //        {
        //            TempData["msg"] = "登録が完了しました";
        //            return RedirectToAction("Create");
        //        }
        //        else
        //        {
        //            ViewData["Error"] = "登録に失敗しました";
        //            return View(inputModel);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        ViewData["Error"] = "登録に失敗しました";
        //        return View(inputModel);
        //    }

        //}

        //private bool ReceiveInsert(List<D_ReceiveViewModel> insertList)
        //{
        //    //var user = UserDataList();
        //    var now = DateTime.Now;

        //    var db = User.Claims.Where(x => x.Type == CustomClaimTypes.ClaimType_DatabaseName).First().Value;
        //    var connectionString = new GetConnectString(db).ConnectionString;
        //    using (var connection = new SqlConnection(connectionString))
        //    {
        //        connection.Open();
        //        SqlTransaction transaction = null;
        //        transaction = connection.BeginTransaction();
        //        try
        //        {
        //            for (int i = 0; i < insertList.Count; ++i)
        //            {
        //                var insertCommandText = $@"
        //                    INSERT INTO D_Receive (
        //                           DepoCode
        //                          ,SupplierCode
        //                          ,ReceiveDate
        //                          ,DeliveryTimeClass
        //                          ,ProductCode
        //                          ,ProductAbbreviation
        //                          ,ProductManagementClass
        //                          ,NextProcess1
        //                          ,Location1
        //                          ,NextProcess2
        //                          ,Location2
        //                          ,ReceiveQuantity
        //                          ,ReceiveTotalQuantity
        //                          ,DefectiveQuantity
        //                          ,SlipNumber
        //                          ,SlipRowNumber
        //                          ,Remark
        //                          ,Packing
        //                          ,PackingCount
        //                          ,InvoiceNumber
        //                          ,ExpirationDate
        //                          ,LotNumber
        //                          ,CostPrice
        //                          ,Barcode
        //                          ,ProductLabelBranchNumber
        //                          ,BarCodeRead_CID
        //                          ,CreatedDate
        //                          ,CreatedUser
        //                          ,UpdatedDate
        //                          ,UpdatedUser
        //                     ) VALUES (
        //                           @DepoCode
        //                          ,@SupplierCode
        //                          ,@ReceiveDate
        //                          ,@DeliveryTimeClass
        //                          ,@ProductCode
        //                          ,@ProductAbbreviation
        //                          ,@ProductManagementClass
        //                          ,@NextProcess1
        //                          ,@Location1
        //                          ,@NextProcess2
        //                          ,@Location2
        //                          ,@ReceiveQuantity
        //                          ,@ReceiveTotalQuantity
        //                          ,@DefectiveQuantity
        //                          ,@SlipNumber
        //                          ,@SlipRowNumber
        //                          ,@Remark
        //                          ,@Packing
        //                          ,@PackingCount
        //                          ,@InvoiceNumber
        //                          ,@ExpirationDate
        //                          ,@LotNumber
        //                          ,@CostPrice
        //                          ,@Barcode
        //                          ,@ProductLabelBranchNumber
        //                          ,@BarCodeRead_CID
        //                          ,@CreatedDate
        //                          ,@CreatedUser
        //                          ,@UpdatedDate
        //                          ,@UpdatedUser
        //                     );";

        //                var insertParamModel = new D_ReceiveViewModel()
        //                {
        //                    DepoID = insertList[i].DepoID,
        //                    SupplierCode = insertList[i].SupplierCode,
        //                    ReceiveDate = insertList[i].ReceiveDate,
        //                    NextProcess1 = insertList[i].NextProcess1,
        //                    NextProcess2 = insertList[i].NextProcess2,
        //                    SlipNumber = insertList[i].SlipNumber,
        //                    ProductCode = insertList[i].ProductCode,
        //                    ReceiveQuantity = insertList[i].ReceiveQuantity,
        //                    ReceiveTotalQuantity = insertList[i].ReceiveTotalQuantity,
        //                    PackingCount = insertList[i].PackingCount,
        //                    Packing = insertList[i].Packing,
        //                    CreatedDate = now.ToString(),
        //                    CreatedUser = UserModel.UserCode,
        //                    UpdatedDate = now.ToString(),
        //                    UpdatedUser = UserModel.UserCode
        //                };
        //                var insertResult = connection.Execute(insertCommandText, insertParamModel, transaction);
        //            }

        //            transaction.Commit();

        //        }
        //        catch (Exception ex)
        //        {
        //            if (transaction != null)
        //            {
        //                transaction.Rollback();
        //            }
        //            throw;
        //        }
        //    }
        //    return true;
        //}

        //public IActionResult Edit(long id)
        //{
        //    var receiptModel = new D_ReceiveViewModel();
        //    try
        //    {
        //        //var user = UserDataList();
        //        receiptModel = GetUpdateData(UserModel.CompanyID, id);
        //        return View(receiptModel);
        //    }
        //    catch (Exception ex)
        //    {
        //        TempData["Error"] = "編集データの取得に失敗しました";
        //        return RedirectToAction("SearchByPageBack");
        //    }
        //}

        //private D_ReceiveViewModel GetUpdateData(int wid, long id)
        //{
        //    var editModel = new D_ReceiveViewModel();

        //    var db = User.Claims.Where(x => x.Type == CustomClaimTypes.ClaimType_DatabaseName).First().Value;
        //    var connectionString = new GetConnectString(db).ConnectionString;
        //    using (var connection = new SqlConnection(connectionString))
        //    {
        //        connection.Open();
        //        try
        //        {
        //            var commandText = $@"
        //                SELECT 
        //                      ID
        //                      ,A.WID
        //                      ,DepoCode
        //                      ,SupplierCode
	       //                   ,B.DEKANJ AS SupplierName
        //                      ,FORMAT(ReceiveDate,'yyyy/MM/dd') AS ReceiveDate
        //                      ,DeliveryTimeClass
        //                      ,ProductCode
	       //                   ,C.product_name AS ProductName
        //                      ,ProductAbbreviation
        //                      ,ProductManagementClass
        //                      ,NextProcess1
        //                      ,Location1
        //                      ,NextProcess2
        //                      ,Location2
        //                      ,ReceiveQuantity
        //                      ,DefectiveQuantity
        //                      ,SlipNumber
        //                      ,SlipRowNumber
        //                      ,Remark
        //                      ,A.Packing
        //                      ,PackingCount
        //                      ,InvoiceNumber
        //                      ,ExpirationDate
        //                      ,LotNumber
        //                      ,CostPrice
        //                      ,Barcode
        //                      ,ProductLabelBranchNumber
        //                      ,BarCodeRead_CID
        //                      ,FORMAT(CreatedDate,'yyyy/MM/dd HH:mm') AS CreatedDate
        //                      ,CreatedUser
        //                      ,FORMAT(UpdatedDate,'yyyy/MM/dd HH:mm') AS UpdatedDate
        //                      ,UpdatedUser
        //                  FROM D_Receive AS A
        //                  LEFT OUTER JOIN m_suppliers AS B ON A.WID = B.WID AND A.SupplierCode = B.DETRCD
        //                  LEFT OUTER JOIN m_product AS C ON A.WID = C.WID AND A.ProductCode = C.productCD AND A.SupplierCode = C.suppliersCD
        //                  WHERE 1=1
        //                      AND A.WID  = @WID
        //                      AND ID = @ID
        //                ";

        //            var param = new
        //            {
        //                WID = wid,
        //                ID = id
        //            };
        //            editModel = connection.Query<D_ReceiveViewModel>(commandText, param).FirstOrDefault();

        //            return editModel;

        //        }
        //        catch (Exception ex)
        //        {
        //            throw;
        //        }
        //    }
        //}

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public IActionResult Edit(D_ReceiveViewModel updateModel, bool isDelete)
        //{
        //    //var user = UserDataList();

        //    if (!isDelete)
        //    {
        //        updateModel.ReceiveDate = (updateModel.ReceiveDate ?? "").Trim();
        //        updateModel.SupplierCode = (updateModel.SupplierCode ?? "").Trim();
        //        updateModel.NextProcess1 = (updateModel.NextProcess1 ?? "").Trim();
        //        updateModel.NextProcess2 = (updateModel.NextProcess2 ?? "").Trim();
        //        updateModel.SlipNumber = (updateModel.SlipNumber ?? "").Trim();
        //        updateModel.ProductCode = (updateModel.ProductCode ?? "").Trim();
        //        updateModel.Packing = (updateModel.Packing ?? "").Trim();
        //        updateModel.Remark = (updateModel.Remark ?? "").Trim();

        //        if (String.IsNullOrEmpty(updateModel.ProductCode))
        //        {
        //            ViewData["Error"] = "商品コードを入力してください";
        //            return View(updateModel);
        //        }
        //        else if (updateModel.ReceiveQuantity == 0)
        //        {
        //            ViewData["Error"] = "入数を入力してください";
        //            return View(updateModel);
        //        }
        //        else if (updateModel.PackingCount == 0)
        //        {
        //            ViewData["Error"] = "箱数を入力してください";
        //            return View(updateModel);
        //        }

        //        // 合計数量を計算
        //        updateModel.ReceiveTotalQuantity = updateModel.ReceiveQuantity * updateModel.PackingCount;

        //        //// 仕入先の存在確認
        //        //var supplierCodeCheck = ErrorCheck.GetUnyName(updateModel.SupplierCode);
        //        //if (supplierCodeCheck == null || supplierCodeCheck[0] == "NG")
        //        //{
        //        //    ViewData["Error"] = "仕入先コードが存在しません";
        //        //    return View(updateModel);
        //        //}

        //        //var buhinData = GetBuhinName(updateModel.DepoCode, updateModel.SupplierCode, updateModel.ProductCode);
        //        //if (buhinData == null || buhinData[0] == "NG")
        //        //{
        //        //    ViewData["Error"] = "商品コードが存在しません";
        //        //    return View(updateModel);
        //        //}

        //        try
        //        {
        //            Update(updateModel);
        //            ViewData["Message"] = "更新が完了しました";
        //            return View(updateModel);
        //        }
        //        catch (Exception ex)
        //        {
        //            ViewData["Error"] = "更新に失敗しました";
        //            return View(updateModel);
        //        }
        //    }
        //    else
        //    {
        //        // 削除処理
        //        try
        //        {
        //            var deleteId = updateModel.ID;
        //            if (deleteId > 0)
        //            {
        //                Delete(deleteId);
        //                return RedirectToAction("SearchByPageBack");
        //            }
        //            else
        //            {
        //                ViewData["Error"] = "更新に失敗しました";
        //                return View(updateModel);
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            ViewData["Error"] = "更新に失敗しました";
        //            return View(updateModel);
        //        }
        //    }

        //}

        //private bool Update(D_ReceiveViewModel updateModel)
        //{
        //    var now = DateTime.Now;

        //    var db = User.Claims.Where(x => x.Type == CustomClaimTypes.ClaimType_DatabaseName).First().Value;
        //    var connectionString = new GetConnectString(db).ConnectionString;
        //    using (var connection = new SqlConnection(connectionString))
        //    {
        //        connection.Open();
        //        SqlTransaction transaction = null;
        //        transaction = connection.BeginTransaction();
        //        try
        //        {
        //            var updateCommandText = $@"
        //                    UPDATE D_Receive SET
        //                           DepoCode = @DepoCode
        //                          ,SupplierCode = @SupplierCode
        //                          ,ReceiveDate = @ReceiveDate
        //                          ,ProductCode = @ProductCode
        //                          ,NextProcess1 = @NextProcess1
        //                          ,NextProcess2 = @NextProcess2
        //                          ,ReceiveQuantity = @ReceiveQuantity
        //                          ,ReceiveTotalQuantity = @ReceiveTotalQuantity
        //                          ,SlipNumber = @SlipNumber
        //                          ,Remark = @Remark
        //                          ,Packing = @Packing
        //                          ,PackingCount = @PackingCount
        //                          ,UpdatedDate = @UpdatedDate
        //                          ,UpdatedUser = @UpdatedUser
        //                    WHERE 1 = 1
        //                        AND ID = @ID
        //                    ;";

        //            var updateParamModel = new D_ReceiveViewModel()
        //            {
        //                ID = updateModel.ID,
        //                DepoID = updateModel.DepoID,
        //                SupplierCode = updateModel.SupplierCode,
        //                ReceiveDate = updateModel.ReceiveDate,
        //                ProductCode = updateModel.ProductCode,
        //                NextProcess1 = updateModel.NextProcess1,
        //                NextProcess2 = updateModel.NextProcess2,
        //                ReceiveQuantity = updateModel.ReceiveQuantity,
        //                ReceiveTotalQuantity = updateModel.ReceiveTotalQuantity,
        //                SlipNumber = updateModel.SlipNumber,
        //                Remark = updateModel.Remark,
        //                Packing = updateModel.Packing,
        //                PackingCount = updateModel.PackingCount,
        //                UpdatedDate = now.ToString(),
        //                UpdatedUser = UserModel.UserCode
        //            };
        //            var updateResult = connection.Execute(updateCommandText, updateParamModel, transaction);

        //            if (updateResult != 1)
        //            {
        //                transaction.Rollback();
        //                return false;
        //            }

        //            transaction.Commit();
        //            return true;
        //        }
        //        catch (Exception ex)
        //        {
        //            if (transaction != null)
        //            {
        //                transaction.Rollback();
        //            }
        //            throw;
        //        }
        //    }

        //}

        //private bool Delete(long id)
        //{
        //    //var user = UserDataList();
        //    var db = User.Claims.Where(x => x.Type == CustomClaimTypes.ClaimType_DatabaseName).First().Value;
        //    var connectionString = new GetConnectString(db).ConnectionString;
        //    using (var connection = new SqlConnection(connectionString))
        //    {
        //        connection.Open();
        //        SqlTransaction transaction = null;
        //        transaction = connection.BeginTransaction();
        //        try
        //        {
        //            var deleteCommandText = $@"
        //                    DELETE FROM D_Receive
        //                    WHERE 1 = 1
        //                     AND ID = @ID
        //                    ;";

        //            var deleteParamModel = new D_ReceiveViewModel()
        //            {
        //                ID = id
        //            };
        //            var deleteResult = connection.Execute(deleteCommandText, deleteParamModel, transaction);
        //            if (deleteResult != 1)
        //            {
        //                transaction.Rollback();
        //                return false;
        //            }

        //            transaction.Commit();

        //        }
        //        catch (Exception ex)
        //        {
        //            if (transaction != null)
        //            {
        //                transaction.Rollback();
        //            }
        //            throw;
        //        }
        //    }
        //    return true;
        //}

        private void SessionSet(int? page, D_ReceiveSearchModel model)
        {
            HttpContext.Session.SetString(SESSIONKEY_PageNo, page == null ? "1" : page.ToString());
            HttpContext.Session.SetString(SESSIONKEY_DepoCode, model.DepoID.ToString());
            HttpContext.Session.SetString(SESSIONKEY_SupplierCode, model.SupplierCode == null ? "" : model.SupplierCode.ToString());
            HttpContext.Session.SetString(SESSIONKEY_SupplierName, model.SupplierName == null ? "" : model.SupplierName.ToString());
            HttpContext.Session.SetString(SESSIONKEY_ProductCode, model.ProductCode == null ? "" : model.ProductCode.ToString());
            //HttpContext.Session.SetString(SESSIONKEY_ProductName, model.ProductName == null ? "" : model.ProductName.ToString());
            HttpContext.Session.SetString(SESSIONKEY_NextProcess1, model.NextProcess1 == null ? "" : model.NextProcess1.ToString());
            HttpContext.Session.SetString(SESSIONKEY_NextProcess2, model.NextProcess2 == null ? "" : model.NextProcess2.ToString());
            HttpContext.Session.SetString(SESSIONKEY_ReceiveDate_Start, model.ReceiveDateStart == null ? "" : model.ReceiveDateStart.ToString());
            HttpContext.Session.SetString(SESSIONKEY_ReceiveDate_End, model.ReceiveDateEnd == null ? "" : model.ReceiveDateEnd.ToString());
        }

        private (int PageNo, D_ReceiveSearchModel SerchModel) SessionGet()
        {
            var search = new D_ReceiveSearchModel();

            int.TryParse(HttpContext.Session.GetString(SESSIONKEY_PageNo), out int pageNo);

            int.TryParse(HttpContext.Session.GetString(SESSIONKEY_DepoCode), out int depoCode);
            search.DepoID = depoCode;

            var suppliersCode = HttpContext.Session.GetString(SESSIONKEY_SupplierCode);
            search.SupplierCode = suppliersCode == "" ? null : suppliersCode;

            var suppliersName = HttpContext.Session.GetString(SESSIONKEY_SupplierName);
            search.SupplierName = suppliersName == "" ? null : suppliersName;

            var productCode = HttpContext.Session.GetString(SESSIONKEY_ProductCode);
            search.ProductCode = productCode == "" ? null : productCode;

            //var productName = HttpContext.Session.GetString(SESSIONKEY_ProductName);
            //search.ProductName = productName == "" ? null : productName;

            var nextProcess1 = HttpContext.Session.GetString(SESSIONKEY_NextProcess1);
            search.NextProcess1 = nextProcess1 == "" ? null : nextProcess1;

            var nextProcess2 = HttpContext.Session.GetString(SESSIONKEY_NextProcess2);
            search.NextProcess2 = nextProcess2 == "" ? null : nextProcess2;

            var receiptDate_Start = HttpContext.Session.GetString(SESSIONKEY_ReceiveDate_Start);
            search.ReceiveDateStart = receiptDate_Start == "" ? null : receiptDate_Start;

            var receiptDate_End = HttpContext.Session.GetString(SESSIONKEY_ReceiveDate_End);
            search.ReceiveDateEnd = receiptDate_End == "" ? null : receiptDate_End;

            return (pageNo, search);

        }
        private void SessionReset()
        {
            HttpContext.Session.Remove(SESSIONKEY_PageNo);
            HttpContext.Session.Remove(SESSIONKEY_DepoCode);
            HttpContext.Session.Remove(SESSIONKEY_SupplierCode);
            HttpContext.Session.Remove(SESSIONKEY_SupplierName);
            HttpContext.Session.Remove(SESSIONKEY_ProductCode);
            //HttpContext.Session.Remove(SESSIONKEY_ProductName);
            HttpContext.Session.Remove(SESSIONKEY_NextProcess1);
            HttpContext.Session.Remove(SESSIONKEY_NextProcess2);
            HttpContext.Session.Remove(SESSIONKEY_ReceiveDate_Start);
            HttpContext.Session.Remove(SESSIONKEY_ReceiveDate_End);
        }

    }

}