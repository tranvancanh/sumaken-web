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
using stock_management_system.Models.common;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using System.Transactions;
using DocumentFormat.OpenXml.Spreadsheet;
using static stock_management_system.Models.AccountViewModel;
using static stock_management_system.Models.D_StoreInModel;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using JwtBuilder;
using Microsoft.VisualStudio.Web.CodeGeneration.Contracts.Messaging;
using OfficeOpenXml.FormulaParsing.Excel.Functions.RefAndLookup;

namespace stock_management_system.Controllers
{
    public class D_StoreInController : BaseController
    {
        // 検索条件セッションキーの設定
        private const string SESSIONKEY_PageNo = "PageNo";
        private const string SESSIONKEY_DepoCode = "DepoCode";
        private const string SESSIONKEY_ProductCode = "ProductCode";
        private const string SESSIONKEY_StockLocation1 = "StockLocation1";
        private const string SESSIONKEY_StockLocation2 = "StockLocation2";
        private const string SESSIONKEY_StoreInDate_Start = "StoreInDateStart";
        private const string SESSIONKEY_StoreInDate_End = "StoreInDateEnd";

        // ページサイズの設定
        private const int pageSize = 50;

        // 新規作成行数
        private const int createRowCount = 10;

        public IActionResult Index()
        {
            var user = UserDataList();
            SessionReset();
            var search = new D_StoreInSearchModel();
            search.DepoID = user.MainDepoID;
            return View(search);
        }

        public IActionResult Search(D_StoreInSearchModel search)
        {
            // 入庫日の日付形式チェック
            if (!String.IsNullOrEmpty(search.StoreInDateStart))
            {
                if (!DateTime.TryParse(search.StoreInDateStart, out DateTime StoreInDateStart))
                {
                    ViewData["Error"] = "入庫日を日付形式（yyyy/MM/dd）に変更できません";
                    return View("Index", search);
                }
            }
            if (!String.IsNullOrEmpty(search.StoreInDateEnd))
            {
                if (!DateTime.TryParse(search.StoreInDateEnd, out DateTime StoreInDateEnd))
                {
                    ViewData["Error"] = "入庫日を日付形式（yyyy/MM/dd）に変更できません";
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
            var search = new D_StoreInSearchModel();

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

        private IActionResult Search(D_StoreInSearchModel search, int page)
        {
            try
            {
                var viewList = GetStoreInList(search);
                var _dataCount = viewList.Count;

                if (_dataCount > 0)
                {
                    search.StoreInList = viewList.ToPagedList(page, pageSize);

                    // ページ関連情報セット
                    search.Page = new Models.Page();
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
        public IActionResult ExcelOutput(D_StoreInSearchModel searchModel)
        {
            try
            {
                var storeInList = GetStoreInList(searchModel);

                // ファイル名
                var filename = "storein_data_" + DateTime.Now.ToString("yyyyMMddHHmmss");

                var headerNameList = new List<string>();

                // TODO:クラス化したい
                // 参考：https://stackoverflow.com/questions/7335629/get-displayattribute-attribute-from-propertyinfo
                var properties = typeof(D_StoreInExcelViewModel).GetProperties()
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
                    var data = storeInList;
                    if (data != null && data.Count() > 0)
                    {
                        for (int col = 2; col < (data.Count() + 2); ++col)
                        {
                            int row = 0;
                            int _col = col - 2;
                            sl.SetCellValue(col, ++row, data[_col].DepoCode);
                            sl.SetCellValue(col, ++row, data[_col].DepoName);
                            sl.SetCellValue(col, ++row, data[_col].StoreInDate);
                            sl.SetCellValue(col, ++row, data[_col].ProductCode);
                            //sl.SetCellValue(col, ++row, data[_col].Quantity);
                            sl.SetCellValue(col, ++row, data[_col].Quantity);
                            sl.SetCellValue(col, ++row, data[_col].PackingCount);
                            sl.SetCellValue(col, ++row, data[_col].Packing);
                            sl.SetCellValue(col, ++row, data[_col].StockLocation1);
                            sl.SetCellValue(col, ++row, data[_col].StockLocation2);
                            sl.SetCellValue(col, ++row, data[_col].Remark);
                            sl.SetCellValue(col, ++row, data[_col].RemarkDelete);
                            sl.SetCellValue(col, ++row, data[_col].CreateDate);
                            sl.SetCellValue(col, ++row, data[_col].CreateUserCode);
                            sl.SetCellValue(col, ++row, data[_col].CreateHandyUserCode);
                            sl.SetCellValue(col, ++row, data[_col].UpdateDate);
                            sl.SetCellValue(col, ++row, data[_col].UpdateUserCode);
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

        private List<D_StoreInViewModel> GetStoreInList(D_StoreInSearchModel search)
        {
            var user = UserDataList();
            string whereString;

            var sb = new StringBuilder($@"WHERE (A.DepoID = @DepoID) AND (A.DeleteFlag = @DeleteFlag) ");

            if (!String.IsNullOrEmpty(search.ProductCode))
            {
                sb.Append($@"AND (A.ProductCode LIKE ('%'+@ProductCode+'%')) ");
            }
            if (!String.IsNullOrEmpty(search.StockLocation1))
            {
                sb.Append($@"AND (A.StockLocation1 LIKE ('%'+@StockLocation1+'%')) ");
            }
            if (!String.IsNullOrEmpty(search.StockLocation2))
            {
                sb.Append($@"AND (A.StockLocation2 LIKE ('%'+@StockLocation2+'%')) ");
            }
            if (!String.IsNullOrEmpty(search.StoreInDateStart))
            {
                sb.Append($@"AND (A.StoreInDate >= @StoreInDateStart) ");
            }
            if(!String.IsNullOrEmpty(search.StoreInDateEnd))
            {
                sb.Append($@"AND (A.StoreInDate <= @StoreInDateEnd) ");
            }

            whereString = sb.ToString();

            var D_StoreInViewList = new List<D_StoreInViewModel>();
            try
            {
                var connectionString = new GetConnectString(user.DatabaseName).ConnectionString;
                using (var connection = new SqlConnection(connectionString))
                {
                    //open
                    connection.Open();

                    //commmand
                    var commandText = $@"
                        SELECT 
                               StoreInID
                              ,A.DepoID
                              ,B.DepoCode
                              ,B.DepoName
                              ,FORMAT(StoreInDate,'yyyy/MM/dd') AS StoreInDate
                              ,ProductCode
                              ,StockLocation1
                              ,StockLocation2
                              ,Packing
                              ,PackingCount
                              ,Quantity
                              ,Remark
                              ,RemarkDelete
                              ,FORMAT(A.CreateDate,'yyyy/MM/dd HH:mm') AS CreateDate
                              ,FORMAT(A.UpdateDate,'yyyy/MM/dd HH:mm') AS UpdateDate
                              ,ISNULL(C.UserCode, '') AS CreateUserCode
                              ,ISNULL(E.HandyUserCode, '') AS CreateHandyUserCode
                              ,ISNULL(D.UserCode, '') AS UpdateUserCode
                          FROM D_StoreIn AS A
                          LEFT OUTER JOIN M_Depo AS B ON A.DepoID = B.DepoID
                          LEFT OUTER JOIN M_User AS C ON A.CreateUserID = C.UserID
                          LEFT OUTER JOIN M_User AS D ON A.UpdateUserID = D.UserID
                          LEFT OUTER JOIN M_HandyUser AS E ON A.CreateUserID = E.HandyUserID "
                    + whereString
                    + $@"ORDER BY UpdateDate DESC, StoreInDate DESC, ProductCode ASC
                            ";

                    var param = new
                    {
                        search.DepoID,
                        search.ProductCode,
                        search.StockLocation1,
                        search.StockLocation2,
                        search.StoreInDateStart,
                        search.StoreInDateEnd,
                        DeleteFlag = false
                    };
                    D_StoreInViewList = connection.Query<D_StoreInViewModel>(commandText, param).ToList();

                    if (D_StoreInViewList.Count() == 0) ViewData["Error"] = "検索条件に一致する入庫データはありません";
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            return D_StoreInViewList;
        }

        public IActionResult Create()
        {
            var StoreInModel = new D_StoreInViewModel();

            var receiptList = new List<D_StoreInViewModel>();
            StoreInModel.StoreInList = receiptList;

            var user = UserDataList();

            //var user = UserDataList();
            for (int i = 0; i < createRowCount; i++) 
            {
                var viewModel = new D_StoreInViewModel();
                viewModel.DepoID = user.MainDepoID;
                StoreInModel.StoreInList.Add(viewModel);
            }

            var temp = TempData["msg"];
            if (temp != null)
            {
                ViewData["Message"] = temp;
            }

            return View(StoreInModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(D_StoreInViewModel inputModel)
        {
            var inputList = inputModel.StoreInList;

            var user = UserDataList();

            var inputData = inputList.Where(x => !String.IsNullOrEmpty(x.StoreInDate)).ToList();
            if (inputData.Count == 0)
            {
                ViewData["Error"] = "登録対象のデータが存在しません　※入庫日が入力されている行が登録対象です";
                return View(inputModel);
            }

            inputData = inputData.Select(s =>
            {
                s.StoreInDate = Util.ObjectToString(s.StoreInDate);
                s.ProductCode = Util.ObjectToString(s.ProductCode);
                s.Packing = Util.ObjectToString(s.Packing);
                s.StockLocation1 = Util.ObjectToString(s.StockLocation1);
                s.StockLocation2 = Util.ObjectToString(s.StockLocation2);
                s.Remark = Util.ObjectToString(s.Remark);
                return s;
            }).ToList();

            for (int i = 0; i < inputData.Count(); ++i)
            {
                var rowNo = inputData[i].RowNo;

                // 未入力チェック
                if (String.IsNullOrEmpty(inputData[i].ProductCode))
                {
                    ViewData["Error"] = rowNo + "行目：商品コードを入力してください。";
                    return View(inputModel);
                }
                
                // 数値の大きさチェック
                if (inputData[i].Quantity < 1)
                {
                    ViewData["Error"] = rowNo + "行目：ロット数は1以上で入力してください。";
                    return View(inputModel);
                }
                else if (inputData[i].PackingCount < 1)
                {
                    ViewData["Error"] = rowNo + "行目：箱数は1以上で入力してください。";
                    return View(inputModel);
                }

                // 入庫日の日付形式チェック
                if (!DateTime.TryParse(inputData[i].StoreInDate, out DateTime storeIndate))
                {
                    ViewData["Error"] = rowNo + "行目：入庫日を日付形式に変更できません";
                    return View(inputModel);
                }

                // 商品の存在確認
                var products = M_ProductModel.GetProductByProductCode(user.DatabaseName, inputData[i].DepoID ,inputData[i].ProductCode);
                if (products.Count == 0)
                {
                    ViewData["Error"] = rowNo + "行目：商品コードが存在しません";
                    return View(inputModel);
                }

                // ロケーションの存在＆組み合わせ確認
                // 商品マスタより
                int locationCount = M_ProductModel.GetProductByAddress1AndAddress2(user.DatabaseName, inputData[i].DepoID, inputData[i].StockLocation1, inputData[i].StockLocation2);
                // 仮番地より
                var temporaryStoreAddresses = M_TemporaryStoreAddressModel.GetTemporaryStoreAddress(user.DatabaseName, inputData[i].DepoID, inputData[i].StockLocation1, inputData[i].StockLocation2);

                if (locationCount == 0 && temporaryStoreAddresses.Count == 0)
                {
                    ViewData["Error"] = rowNo + @$"行目：商品マスタまたは仮番地マスタに、保管場所（ {inputData[i].StockLocation1} - {inputData[i].StockLocation2} ）が存在しません";
                    return View(inputModel);
                }

            }

            // INSERT処理
            try
            {
                var insertResult = StoreInInsert(user, inputData);
                if (insertResult)
                {
                    TempData["msg"] = "登録が完了しました　続けて入力できます";
                    return RedirectToAction("Create");
                }
                else
                {
                    ViewData["Error"] = "登録に失敗しました";
                    return View(inputModel);
                }
            }
            catch (Exception ex)
            {
                ViewData["Error"] = "登録に失敗しました";
                return View(inputModel);
            }

        }

        private bool StoreInInsert(LoginUserModel user, List<D_StoreInViewModel> insertList)
        {
            var now = DateTime.Now;

            var connectionString = new GetConnectString(user.DatabaseName).ConnectionString;
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlTransaction transaction = null;
                transaction = connection.BeginTransaction();
                try
                {
                    for (int i = 0; i < insertList.Count; ++i)
                    {
                        // 箱数分の処理を繰り返す
                        int packingCount = insertList[i].PackingCount;

                        for (int p = 0; p < packingCount; ++p)
                        {
                            var insertCommandText = $@"
                            INSERT INTO D_StoreIn (
                                   ScanRecordID
                                  ,ReceiveID
                                  ,DepoID
                                  ,StoreInDate
                                  ,ProductCode
                                  ,Quantity
                                  ,PackingCount
                                  ,Packing
                                  ,StockLocation1
                                  ,StockLocation2
                                  ,Remark
                                  ,RemarkDelete
                                  ,AdjustmentFlag
                                  ,DeleteFlag
                                  ,DeleteStoreInID
                                  ,CreateDate
                                  ,CreateUserID
                                  ,UpdateDate
                                  ,UpdateUserID
                             ) VALUES (
                                   @ScanRecordID
                                  ,@ReceiveID
                                  ,@DepoID
                                  ,@StoreInDate
                                  ,@ProductCode
                                  ,@Quantity
                                  ,@PackingCount
                                  ,@Packing
                                  ,@StockLocation1
                                  ,@StockLocation2
                                  ,@Remark
                                  ,@RemarkDelete
                                  ,@AdjustmentFlag
                                  ,@DeleteFlag
                                  ,@DeleteStoreInID
                                  ,@CreateDate
                                  ,@CreateUserID
                                  ,@UpdateDate
                                  ,@UpdateUserID
                             );";

                            var insertParamModel = new D_StoreInViewModel()
                            {
                                ScanRecordID = 0,
                                ReceiveID = 0,
                                DepoID = insertList[i].DepoID,
                                StoreInDate = insertList[i].StoreInDate,
                                ProductCode = insertList[i].ProductCode,
                                Quantity = insertList[i].Quantity,
                                PackingCount = 1,
                                Packing = insertList[i].Packing,
                                StockLocation1 = insertList[i].StockLocation1,
                                StockLocation2 = insertList[i].StockLocation2,
                                Remark = insertList[i].Remark,
                                RemarkDelete = insertList[i].RemarkDelete,
                                AdjustmentFlag = true,
                                DeleteFlag = false,
                                DeleteStoreInID = 0,
                                CreateDate = now.ToString(),
                                CreateUserID = user.UserID,
                                UpdateDate = now.ToString(),
                                UpdateUserID = user.UserID
                            };
                            var insertResult = connection.Execute(insertCommandText, insertParamModel, transaction);
                        }
                    }

                    transaction.Commit();

                }
                catch (Exception ex)
                {
                    if (transaction != null)
                    {
                        transaction.Rollback();
                    }
                    throw;
                }
            }
            return true;
        }

        [HttpGet]
        public IActionResult GetProductDataAjax(string productCode, int depoId)
        {
            try
            {
                var user = UserDataList();
                var productData = M_ProductModel.GetProductByProductCode(user.DatabaseName, depoId, productCode);

                var jsonResult = new
                {
                    result = "OK",
                    lotQuantity = productData[0].LotQuantity,
                    address1 = productData[0].StoreAddress1,
                    address2 = productData[0].StoreAddress2,
                    packing = productData[0].Packing,
                };

                return Json(jsonResult);
            }
            catch (Exception ex)
            {
                return Json(Util.ChangeJsonResultObject("NG", "商品情報の取得に失敗しました"));
            }
        }

        [HttpGet]
        public IActionResult Edit(long id)
        {
            var storeInModel = new D_StoreInViewModel();
            try
            {
                storeInModel = GetUpdateData(id);
                return View(storeInModel);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "データの取得に失敗しました";
                return RedirectToAction("SearchByPageBack");
            }
        }

        private D_StoreInViewModel GetUpdateData(long id)
        {
            var editModel = new D_StoreInViewModel();
            try
            {
                var editModels = GetStoreInByStoreInID(UserDataList().DatabaseName, id);
                editModel = editModels[0];
                return editModel;
            }
            catch (Exception ex)
            {
                throw;
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(D_StoreInViewModel updateModel, bool isDelete)
        {
            var user = UserDataList();

            if (!isDelete)
            {
                try
                {
                    var updateCheck = UpdateItemCheck(user,updateModel);

                    var newID = Update(user, updateModel);
                    return Json(Util.ChangeJsonResultObjectAndNewId("OK", "", newID));
                }
                catch (CustomExtention ex)
                {
                    return Json(Util.ChangeJsonResultObject("NG", ex.Message));
                }
                catch (Exception ex)
                {
                    return Json(Util.ChangeJsonResultObject("NG", "更新に失敗しました"));
                }
            }
            else
            {
                // 削除処理
                try
                {
                    var deleteId = updateModel.StoreInID;
                    if (deleteId > 0)
                    {
                        updateModel.RemarkDelete = Util.ObjectToString(updateModel.RemarkDelete);

                        // 備考の削除理由は必須
                        if (String.IsNullOrEmpty(updateModel.RemarkDelete))
                        {
                            return Json(Util.ChangeJsonResultObject("NG", "削除理由を入力してください"));
                        }
                        else
                        {
                            Delete(user, deleteId, updateModel.RemarkDelete);
                            return Json(Util.ChangeJsonResultObject("OK", ""));
                        }

                    }
                    else
                    {
                        return Json(Util.ChangeJsonResultObject("NG", "削除に失敗しました"));
                    }
                }
                catch (Exception ex)
                {
                    return Json(Util.ChangeJsonResultObject("NG", "削除に失敗しました"));
                }
            }

        }

        private bool UpdateItemCheck(LoginUserModel user, D_StoreInViewModel updateModel)
        {
            updateModel.StoreInDate = Util.ObjectToString(updateModel.StoreInDate);
            updateModel.ProductCode = Util.ObjectToString(updateModel.ProductCode);
            updateModel.Packing = Util.ObjectToString(updateModel.Packing);
            updateModel.StockLocation1 = Util.ObjectToString(updateModel.StockLocation1);
            updateModel.StockLocation2 = Util.ObjectToString(updateModel.StockLocation2);
            updateModel.Remark = Util.ObjectToString(updateModel.Remark);

            // 入庫日の日付形式チェック
            if (String.IsNullOrEmpty(updateModel.StoreInDate))
            {
                throw new CustomExtention("入庫日を入力してください");
            }

            if (String.IsNullOrEmpty(updateModel.ProductCode))
            {
                throw new CustomExtention("商品コードを入力してください");
            }
            else if (updateModel.Quantity < 1)
            {
                throw new CustomExtention("ロット数は1以上で入力してください");
            }
            else if (updateModel.PackingCount < 1)
            {
                throw new CustomExtention("箱数は1以上で入力してください");
            }

            // 入庫日の日付形式チェック
            if (!DateTime.TryParse(updateModel.StoreInDate, out DateTime storeIndate))
            {
                throw new CustomExtention("入庫日を日付形式に変換できません");
            }

            // 商品の存在確認
            var products = M_ProductModel.GetProductByProductCode(user.DatabaseName, updateModel.DepoID, updateModel.ProductCode);
            if (products.Count == 0)
            {
                throw new CustomExtention("商品コードが存在しません");
            }

            // ロケーションの存在＆組み合わせ確認
            // 商品マスタより
            int locationCount = M_ProductModel.GetProductByAddress1AndAddress2(user.DatabaseName, updateModel.DepoID, updateModel.StockLocation1, updateModel.StockLocation2);
            // 仮番地より
            var temporaryStoreAddresses = M_TemporaryStoreAddressModel.GetTemporaryStoreAddress(user.DatabaseName, updateModel.DepoID, updateModel.StockLocation1, updateModel.StockLocation2);

            if (locationCount == 0 && temporaryStoreAddresses.Count == 0)
            {
                throw new CustomExtention(@$"商品マスタまたは仮番地マスタに、保管場所（ {updateModel.StockLocation1} - {updateModel.StockLocation2} ）が存在しません");
            }

            return true;
        }

        private long Update(LoginUserModel user, D_StoreInViewModel updateModel)
        {
            var now = DateTime.Now.ToString();

            var connectionString = new GetConnectString(user.DatabaseName).ConnectionString;
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlTransaction tran = null;
                tran = connection.BeginTransaction();
                try
                {
                    var updateCommandText = $@"
                            UPDATE D_StoreIn SET
                                   DeleteFlag = @DeleteFlag
                                  ,UpdateDate = @UpdateDate
                                  ,UpdateUserID = @UpdateUserID
                            WHERE 1 = 1
                                AND StoreInID = @StoreInID
                            ;";

                    var updateParamModel = new D_StoreInViewModel()
                    {
                        StoreInID = updateModel.StoreInID,
                        DeleteFlag = true,
                        UpdateDate = now,
                        UpdateUserID = user.UserID
                    };
                    var updateResult = connection.Execute(updateCommandText, updateParamModel, tran);

                    if (updateResult != 1)
                    {
                        tran.Rollback();
                        throw new CustomExtention("更新に失敗しました");
                    }

                    var insertCommandText = $@"
                            INSERT INTO D_StoreIn (
                                   ScanRecordID
                                  ,ReceiveID
                                  ,DepoID
                                  ,StoreInDate
                                  ,ProductCode
                                  ,Quantity
                                  ,PackingCount
                                  ,Packing
                                  ,StockLocation1
                                  ,StockLocation2
                                  ,Remark
                                  ,DeleteFlag
                                  ,DeleteStoreInID
                                  ,CreateDate
                                  ,CreateUserID
                                  ,UpdateDate
                                  ,UpdateUserID
                            ) 
                            OUTPUT 
                                INSERTED.StoreInID
                            VALUES (
                                   @ScanRecordID
                                  ,@ReceiveID
                                  ,@DepoID
                                  ,@StoreInDate
                                  ,@ProductCode
                                  ,@Quantity
                                  ,@PackingCount
                                  ,@Packing
                                  ,@StockLocation1
                                  ,@StockLocation2
                                  ,@Remark
                                  ,@DeleteFlag
                                  ,@DeleteStoreInID
                                  ,@CreateDate
                                  ,@CreateUserID
                                  ,@UpdateDate
                                  ,@UpdateUserID
                             );";

                    var insertParamModel = new D_StoreInViewModel()
                    {
                        ScanRecordID = updateModel.ScanRecordID,
                        ReceiveID = updateModel.ReceiveID,
                        DepoID = updateModel.DepoID,
                        StoreInDate = updateModel.StoreInDate,
                        ProductCode = updateModel.ProductCode,
                        Quantity = updateModel.Quantity,
                        PackingCount = updateModel.PackingCount,
                        Packing = updateModel.Packing,
                        StockLocation1 = updateModel.StockLocation1,
                        StockLocation2 = updateModel.StockLocation2,
                        Remark = updateModel.Remark,
                        DeleteFlag = false,
                        DeleteStoreInID = updateModel.StoreInID,
                        CreateDate = updateModel.CreateDate,
                        CreateUserID = updateModel.CreateUserID,
                        UpdateDate = now,   // 現在日付
                        UpdateUserID = user.UserID   // ログインユーザーID
                    };
                    var insertStoreInID = connection.QuerySingle<long>(insertCommandText, insertParamModel, tran);

                    if (insertStoreInID == 0)
                    {
                        tran.Rollback();
                        throw new CustomExtention("更新に失敗しました");
                    }

                    tran.Commit();
                    return insertStoreInID;
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    throw;
                }
            }

        }

        private bool Delete(LoginUserModel user, long id, string remarkDelete)
        {
            var now = DateTime.Now.ToString();

            var connectionString = new GetConnectString(user.DatabaseName).ConnectionString;
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlTransaction transaction = null;
                transaction = connection.BeginTransaction();
                try
                {
                    var deleteCommandText = $@"
                            UPDATE D_StoreIn SET
                                   DeleteFlag = @DeleteFlag
                                  ,RemarkDelete = @RemarkDelete
                                  ,UpdateDate = @UpdateDate
                                  ,UpdateUserID = @UpdateUserID
                            WHERE 1 = 1
                                AND StoreInID = @StoreInID
                            ;";

                    var deleteParamModel = new D_StoreInViewModel()
                    {
                        StoreInID = id,
                        DeleteFlag = true,
                        RemarkDelete = remarkDelete,
                        UpdateDate = now,
                        UpdateUserID = user.UserID
                    };
                    var deleteResult = connection.Execute(deleteCommandText, deleteParamModel, transaction);
                    if (deleteResult != 1)
                    {
                        transaction.Rollback();
                        throw new CustomExtention("削除に失敗しました");
                    }

                    transaction.Commit();

                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw;
                }
            }
            return true;
        }

        private void SessionSet(int? page, D_StoreInSearchModel model)
        {
            HttpContext.Session.SetString(SESSIONKEY_PageNo, page == null ? "1" : page.ToString());
            HttpContext.Session.SetString(SESSIONKEY_DepoCode, model.DepoID.ToString());
            HttpContext.Session.SetString(SESSIONKEY_ProductCode, model.ProductCode == null ? "" : model.ProductCode.ToString());
            HttpContext.Session.SetString(SESSIONKEY_StockLocation1, model.StockLocation1 == null ? "" : model.StockLocation1.ToString());
            HttpContext.Session.SetString(SESSIONKEY_StockLocation2, model.StockLocation2 == null ? "" : model.StockLocation2.ToString());
            HttpContext.Session.SetString(SESSIONKEY_StoreInDate_Start, model.StoreInDateStart == null ? "" : model.StoreInDateStart.ToString());
            HttpContext.Session.SetString(SESSIONKEY_StoreInDate_End, model.StoreInDateEnd == null ? "" : model.StoreInDateEnd.ToString());
        }

        private (int PageNo, D_StoreInSearchModel SerchModel) SessionGet()
        {
            var search = new D_StoreInSearchModel();

            int.TryParse(HttpContext.Session.GetString(SESSIONKEY_PageNo), out int pageNo);

            int.TryParse(HttpContext.Session.GetString(SESSIONKEY_DepoCode), out int depoCode);
            search.DepoID = depoCode;

            var productCode = HttpContext.Session.GetString(SESSIONKEY_ProductCode);
            search.ProductCode = productCode == "" ? null : productCode;

            var stockLocation1 = HttpContext.Session.GetString(SESSIONKEY_StockLocation1);
            search.StockLocation1 = stockLocation1 == "" ? null : stockLocation1;

            var stockLocation2 = HttpContext.Session.GetString(SESSIONKEY_StockLocation2);
            search.StockLocation2 = stockLocation2 == "" ? null : stockLocation2;

            var receiptDate_Start = HttpContext.Session.GetString(SESSIONKEY_StoreInDate_Start);
            search.StoreInDateStart = receiptDate_Start == "" ? null : receiptDate_Start;

            var receiptDate_End = HttpContext.Session.GetString(SESSIONKEY_StoreInDate_End);
            search.StoreInDateEnd = receiptDate_End == "" ? null : receiptDate_End;

            return (pageNo, search);

        }
        private void SessionReset()
        {
            HttpContext.Session.Remove(SESSIONKEY_PageNo);
            HttpContext.Session.Remove(SESSIONKEY_DepoCode);
            HttpContext.Session.Remove(SESSIONKEY_ProductCode);
            HttpContext.Session.Remove(SESSIONKEY_StockLocation1);
            HttpContext.Session.Remove(SESSIONKEY_StockLocation2);
            HttpContext.Session.Remove(SESSIONKEY_StoreInDate_Start);
            HttpContext.Session.Remove(SESSIONKEY_StoreInDate_End);
        }

    }

}