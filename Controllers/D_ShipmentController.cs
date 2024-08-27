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
using static stock_management_system.Models.D_ShipmentModel;
using static stock_management_system.Models.D_StockStatusModel;

namespace stock_management_system.Controllers
{
    public class D_ShipmentController : BaseController
    {
        // 検索条件セッションキーの設定
        private const string SESSIONKEY_PageNo = "PageNo";
        private const string SESSIONKEY_DepoCode = "DepoCode";
        private const string SESSIONKEY_ShipmentDate_Start = "ShipmentDateStart";
        private const string SESSIONKEY_ShipmentDate_End = "ShipmentDateEnd";
        private const string SESSIONKEY_ProductCode = "ProductCode";
        private const string SESSIONKEY_NextProcess1 = "NextProcess1";
        private const string SESSIONKEY_NextProcess2 = "NextProcess2";
        private const string SESSIONKEY_CustomerDeliverySlipNumber = "CustomerDeliverySlipNumber";
        private const string SESSIONKEY_CustomerNextProcess1 = "CustomerNextProcess1";

        // ページサイズの設定
        private const int pageSize = 50;

        // 新規作成行数
        private const int createRowCount = 10;

        public IActionResult Index()
        {
            var user = UserDataList();
            SessionReset();
            var search = new D_ShipmentSearchModel();
            search.DepoID = user.MainDepoID;
            return View(search);
        }

        public IActionResult Search(D_ShipmentSearchModel search)
        {
            // 入庫日の日付形式チェック
            if (!String.IsNullOrEmpty(search.ShipmentDateStart))
            {
                if (!DateTime.TryParse(search.ShipmentDateStart, out DateTime shipmentDateStart))
                {
                    var displayName = search.GetDisplayName(typeof(D_ShipmentSearchModel).GetProperty("Name"), "ShipmentDateStart");
                    ViewData["Error"] = displayName + "を日付形式に変更できません";
                    return View("Index", search);
                }
            }
            if (!String.IsNullOrEmpty(search.ShipmentDateEnd))
            {
                if (!DateTime.TryParse(search.ShipmentDateEnd, out DateTime shipmentDateEnd))
                {
                    var displayName = search.GetDisplayName(typeof(D_ShipmentSearchModel).GetProperty("Name"), "ShipmentDateEnd");
                    ViewData["Error"] = displayName + "を日付形式に変更できません";
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
            var search = new D_ShipmentSearchModel();

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

        private IActionResult Search(D_ShipmentSearchModel search, int page)
        {
            try
            {
                var viewList = GetShipmentList(search);
                var _dataCount = viewList.Count;

                if (_dataCount > 0)
                {
                    search.ShipmentList = viewList.ToPagedList(page, pageSize);

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
        public IActionResult ExcelOutput(D_ShipmentSearchModel searchModel)
        {
            try
            {
                var receiptList = GetShipmentList(searchModel);

                // ファイル名
                var filename = "shipment_data_" + DateTime.Now.ToString("yyyyMMddHHmmss");

                var headerNameList = new List<string>();

                // TODO:クラス化したい
                // 参考：https://stackoverflow.com/questions/7335629/get-displayattribute-attribute-from-propertyinfo
                var properties = typeof(D_ShipmentExcelViewModel).GetProperties()
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
                            sl.SetCellValue(col, ++row, data[_col].ShipmentDate);

                            sl.SetCellValue(col, ++row, data[_col].DeliveryDate);
                            sl.SetCellValue(col, ++row, data[_col].DeliveryTimeClass);
                            sl.SetCellValue(col, ++row, data[_col].DeliverySlipNumber);
                            sl.SetCellValue(col, ++row, data[_col].DeliverySlipRowNumber);
                            sl.SetCellValue(col, ++row, data[_col].SupplierCode);
                            sl.SetCellValue(col, ++row, data[_col].SupplierName);
                            sl.SetCellValue(col, ++row, data[_col].ProductCode);
                            sl.SetCellValue(col, ++row, data[_col].ProductAbbreviation);
                            sl.SetCellValue(col, ++row, data[_col].ProductManagementClass);
                            sl.SetCellValue(col, ++row, data[_col].ProductName);
                            sl.SetCellValue(col, ++row, data[_col].ProductLabelBranchNumber);
                            sl.SetCellValue(col, ++row, data[_col].NextProcess1);
                            sl.SetCellValue(col, ++row, data[_col].Location1);
                            sl.SetCellValue(col, ++row, data[_col].NextProcess2);
                            sl.SetCellValue(col, ++row, data[_col].Location2);

                            sl.SetCellValue(col, ++row, data[_col].CustomerDeliveryDate);
                            sl.SetCellValue(col, ++row, data[_col].CustomerDeliveryTimeClass);
                            sl.SetCellValue(col, ++row, data[_col].CustomerDeliverySlipNumber);
                            sl.SetCellValue(col, ++row, data[_col].CustomerDeliverySlipRowNumber);
                            sl.SetCellValue(col, ++row, data[_col].CustomerCode);
                            sl.SetCellValue(col, ++row, data[_col].CustomerName);
                            sl.SetCellValue(col, ++row, data[_col].CustomerProductCode);
                            sl.SetCellValue(col, ++row, data[_col].CustomerProductAbbreviation);
                            sl.SetCellValue(col, ++row, data[_col].CustomerProductManagementClass);
                            sl.SetCellValue(col, ++row, data[_col].CustomerProductLabelBranchNumber);
                            sl.SetCellValue(col, ++row, data[_col].CustomerNextProcess1);
                            sl.SetCellValue(col, ++row, data[_col].CustomerLocation1);
                            sl.SetCellValue(col, ++row, data[_col].CustomerNextProcess2);
                            sl.SetCellValue(col, ++row, data[_col].CustomerLocation2);
                            sl.SetCellValue(col, ++row, data[_col].CustomerOrderNumber);
                            sl.SetCellValue(col, ++row, data[_col].CustomerOrderClass);

                            sl.SetCellValue(col, ++row, data[_col].LotQuantity);
                            sl.SetCellValue(col, ++row, data[_col].Quantity);
                            sl.SetCellValue(col, ++row, data[_col].FractionQuantity);
                            sl.SetCellValue(col, ++row, data[_col].DefectiveQuantity);
                            sl.SetCellValue(col, ++row, data[_col].Packing);
                            sl.SetCellValue(col, ++row, data[_col].PackingCount);

                            sl.SetCellValue(col, ++row, data[_col].LotNumber);
                            sl.SetCellValue(col, ++row, data[_col].InvoiceNumber);
                            sl.SetCellValue(col, ++row, data[_col].ExpirationDate);
                            sl.SetCellValue(col, ++row, data[_col].Remark);

                            sl.SetCellValue(col, ++row, data[_col].CreateDate);
                            sl.SetCellValue(col, ++row, data[_col].CreateUserCode);
                            sl.SetCellValue(col, ++row, data[_col].ScanTime);
                            sl.SetCellValue(col, ++row, data[_col].CreateHandyUserCode);

                            sl.SetCellValue(col, ++row, data[_col].ImportFileName);
                            sl.SetCellValue(col, ++row, data[_col].ImportLogHandyUserCode);
                            sl.SetCellValue(col, ++row, data[_col].ImportLogHandyUserName);
                            sl.SetCellValue(col, ++row, data[_col].ImportLogHandyScanDate);
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

        private List<D_ShipmentViewModel> GetShipmentList(D_ShipmentSearchModel search)
        {
            //var user = UserDataList();
            string whereString;

            var sb = new StringBuilder($@"WHERE (A.DepoID = @DepoID) AND (A.DeleteFlag = @DeleteFlag) ");

            //if (!String.IsNullOrEmpty(search.SupplierCode))
            //{
            //    sb.Append($@"AND (A.SupplierCode LIKE ('%'+@SupplierCode+'%')) ");
            //}
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
            if (!String.IsNullOrEmpty(search.ShipmentDateStart))
            {
                sb.Append($@"AND (A.ShipmentDate >= @ShipmentDateStart) ");
            }
            if(!String.IsNullOrEmpty(search.ShipmentDateEnd))
            {
                sb.Append($@"AND (A.ShipmentDate <= @ShipmentDateEnd) ");
            }
            if (!String.IsNullOrEmpty(search.CustomerDeliverySlipNumber))
            {
                sb.Append($@"AND (A.CustomerDeliverySlipNumber <= @CustomerDeliverySlipNumber) ");
            }
            if (!String.IsNullOrEmpty(search.CustomerNextProcess1))
            {
                sb.Append($@"AND (A.CustomerNextProcess1 <= @CustomerNextProcess1) ");
            }

            whereString = sb.ToString();

            var shipmentViewModels = new List<D_ShipmentViewModel>();
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
                               A.ShipmentID
                              ,A.ScanRecordID
                              ,A.ShipmentInstructionDetailID
                              ,FORMAT(A.ShipmentDate,'yyyy/MM/dd') AS ShipmentDate
                              ,A.DepoID
                              ,B.DepoCode
                              ,B.DepoName
                              ,CASE WHEN FORMAT(A.DeliveryDate, 'yyyyMMdd') = '19000101' THEN '' ELSE FORMAT(A.DeliveryDate, 'yyyy/MM/dd') END AS DeliveryDate
                              ,A.DeliveryTimeClass
                              ,A.DeliverySlipNumber
                              ,A.DeliverySlipRowNumber
                              ,A.SupplierCode
                              ,C.SupplierName
                              ,A.ProductCode
                              ,A.ProductAbbreviation
                              ,A.ProductManagementClass
                              ,A.ProductLabelBranchNumber
                              ,A.NextProcess1
                              ,A.Location1
                              ,A.NextProcess2
                              ,A.Location2
                              ,CASE WHEN FORMAT(A.CustomerDeliveryDate, 'yyyyMMdd') = '19000101' THEN '' ELSE FORMAT(A.CustomerDeliveryDate, 'yyyy/MM/dd') END AS CustomerDeliveryDate
                              ,A.CustomerDeliveryTimeClass
                              ,A.CustomerDeliverySlipNumber
                              ,A.CustomerDeliverySlipRowNumber
                              ,A.CustomerCode
                              ,A.CustomerName
                              ,A.CustomerProductCode
                              ,A.CustomerProductAbbreviation
                              ,A.CustomerProductManagementClass
                              ,A.CustomerProductLabelBranchNumber
                              ,A.CustomerProductCode
                              ,A.CustomerProductAbbreviation
                              ,A.CustomerProductLabelBranchNumber
                              ,A.CustomerNextProcess1
                              ,A.CustomerLocation1
                              ,A.CustomerNextProcess2
                              ,A.CustomerLocation2
                              ,A.CustomerOrderNumber
                              ,A.CustomerOrderClass
                              ,A.LotQuantity
                              ,A.FractionQuantity
                              ,A.Quantity
                              ,A.Packing
                              ,A.PackingCount
                              ,A.LotNumber
                              ,A.InvoiceNumber
                              ,FORMAT(A.ExpirationDate,'yyyy/MM/dd HH:mm') AS ExpirationDate
                              ,A.DeleteFlag
                              ,A.DeleteShipmentID
                              ,A.Remark
                              ,FORMAT(A.CreateDate,'yyyy/MM/dd HH:mm') AS CreateDate
                              ,A.CreateUserID
                              ,ISNULL(D.UserCode, '') AS CreateUserCode
                              ,ISNULL(E.HandyUserCode, '') AS CreateHandyUserCode
                              ,CASE
                                    WHEN FORMAT(F.ScanTime, 'yyyyMMdd') = '19000101' OR (F.ScanTime is null) THEN ''
                                    ELSE FORMAT(F.ScanTime, 'yyyy/MM/dd HH:mm')
                               END AS ScanTime
                              ,A.ImportFileName
                              ,ISNULL(A.ImportLogHandyUserCode, '') AS ImportLogHandyUserCode
                              ,ISNULL(A.ImportLogHandyUserName, '') AS ImportLogHandyUserName
                              ,CASE
                                    WHEN
                                        FORMAT(A.ImportLogHandyScanDate, 'yyyyMMdd') = '19000101' OR (A.ImportLogHandyScanDate is null)  THEN ''
                                    ELSE
                                        FORMAT(A.ImportLogHandyScanDate, 'yyyy/MM/dd HH:mm')
                               END AS ImportLogHandyScanDate
                          FROM D_Shipment AS A
                          LEFT OUTER JOIN M_Depo AS B ON A.DepoID = B.DepoID
                          LEFT OUTER JOIN M_Supplier AS C ON A.SupplierCode = C.SupplierCode
                          LEFT OUTER JOIN M_User AS D ON A.CreateUserID = D.UserID
                          LEFT OUTER JOIN M_HandyUser AS E ON A.CreateHandyUserID = E.HandyUserID
                          LEFT OUTER JOIN D_ScanRecord AS F ON A.ScanRecordID = F.ScanRecordID
                    " + whereString + " ORDER BY A.UpdateDate DESC, ShipmentDate DESC, ProductCode ASC, SupplierCode ASC";

                    var param = new
                    {
                        search.DepoID,
                        DeleteFlag = false,
                        //search.SupplierCode,
                        search.ProductCode,
                        search.NextProcess1,
                        search.NextProcess2,
                        search.ShipmentDateStart,
                        search.ShipmentDateEnd
                    };
                    shipmentViewModels = connection.Query<D_ShipmentViewModel>(commandText, param).ToList();

                    if (shipmentViewModels.Count() == 0) ViewData["Error"] = "検索条件に一致する出荷データはありません";
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            return shipmentViewModels;
        }

        public IActionResult Create()
        {
            var shipmentModel = new D_ShipmentViewModel();

            var receiptList = new List<D_ShipmentViewModel>();
            shipmentModel.ShipmentList = receiptList;

            var user = UserDataList();

            for (int i = 0; i < createRowCount; i++) 
            {
                var viewModel = new D_ShipmentViewModel();
                viewModel.DepoID = user.MainDepoID;
                shipmentModel.ShipmentList.Add(viewModel);
            }

            var temp = TempData["msg"];
            if (temp != null)
            {
                ViewData["Message"] = temp;
            }

            return View(shipmentModel);
        }

        private void SessionSet(int? page, D_ShipmentSearchModel model)
        {
            HttpContext.Session.SetString(SESSIONKEY_PageNo, page == null ? "1" : page.ToString());
            HttpContext.Session.SetString(SESSIONKEY_DepoCode, model.DepoID.ToString());
            HttpContext.Session.SetString(SESSIONKEY_ShipmentDate_Start, model.ShipmentDateStart == null ? "" : model.ShipmentDateStart.ToString());
            HttpContext.Session.SetString(SESSIONKEY_ShipmentDate_End, model.ShipmentDateEnd == null ? "" : model.ShipmentDateEnd.ToString());
            HttpContext.Session.SetString(SESSIONKEY_ProductCode, model.ProductCode == null ? "" : model.ProductCode.ToString());
            HttpContext.Session.SetString(SESSIONKEY_NextProcess1, model.NextProcess1 == null ? "" : model.NextProcess1.ToString());
            HttpContext.Session.SetString(SESSIONKEY_NextProcess2, model.NextProcess2 == null ? "" : model.NextProcess2.ToString());
            HttpContext.Session.SetString(SESSIONKEY_CustomerDeliverySlipNumber, model.CustomerDeliverySlipNumber == null ? "" : model.CustomerDeliverySlipNumber.ToString());
            HttpContext.Session.SetString(SESSIONKEY_CustomerNextProcess1, model.CustomerNextProcess1 == null ? "" : model.CustomerNextProcess1.ToString());
        }

        private (int PageNo, D_ShipmentSearchModel SerchModel) SessionGet()
        {
            var search = new D_ShipmentSearchModel();

            int.TryParse(HttpContext.Session.GetString(SESSIONKEY_PageNo), out int pageNo);

            int.TryParse(HttpContext.Session.GetString(SESSIONKEY_DepoCode), out int depoCode);
            search.DepoID = depoCode;

            var receiptDate_Start = HttpContext.Session.GetString(SESSIONKEY_ShipmentDate_Start);
            search.ShipmentDateStart = receiptDate_Start == "" ? null : receiptDate_Start;

            var receiptDate_End = HttpContext.Session.GetString(SESSIONKEY_ShipmentDate_End);
            search.ShipmentDateEnd = receiptDate_End == "" ? null : receiptDate_End;

            var productCode = HttpContext.Session.GetString(SESSIONKEY_ProductCode);
            search.ProductCode = productCode == "" ? null : productCode;

            var nextProcess1 = HttpContext.Session.GetString(SESSIONKEY_NextProcess1);
            search.NextProcess1 = nextProcess1 == "" ? null : nextProcess1;

            var nextProcess2 = HttpContext.Session.GetString(SESSIONKEY_NextProcess2);
            search.NextProcess2 = nextProcess2 == "" ? null : nextProcess2;

            var customerDeliverySlipNumber = HttpContext.Session.GetString(SESSIONKEY_CustomerDeliverySlipNumber);
            search.CustomerDeliverySlipNumber = customerDeliverySlipNumber == "" ? null : customerDeliverySlipNumber;

            var customerNextProcess1 = HttpContext.Session.GetString(SESSIONKEY_CustomerNextProcess1);
            search.CustomerNextProcess1 = customerNextProcess1 == "" ? null : customerNextProcess1;

            return (pageNo, search);

        }
        private void SessionReset()
        {
            HttpContext.Session.Remove(SESSIONKEY_PageNo);
            HttpContext.Session.Remove(SESSIONKEY_DepoCode);
            HttpContext.Session.Remove(SESSIONKEY_ShipmentDate_Start);
            HttpContext.Session.Remove(SESSIONKEY_ShipmentDate_End);
            HttpContext.Session.Remove(SESSIONKEY_ProductCode);
            HttpContext.Session.Remove(SESSIONKEY_NextProcess1);
            HttpContext.Session.Remove(SESSIONKEY_NextProcess2);
            HttpContext.Session.Remove(SESSIONKEY_CustomerDeliverySlipNumber);
            HttpContext.Session.Remove(SESSIONKEY_CustomerNextProcess1);
        }

    }

}