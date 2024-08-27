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
using static stock_management_system.Models.D_StockStatusModel;
using DocumentFormat.OpenXml.EMMA;
using stock_management_system.Models.common;
using System.Reflection;

namespace stock_management_system.Controllers
{
    public class D_StockStatusController : BaseController
    {
        // 検索条件セッションキーの設定
        private const string SESSIONKEY_PageNo = "PageNo";
        private const string SESSIONKEY_DepoCode = "DepoCode";
        private const string SESSIONKEY_SupplierCode = "SupplierCode";
        private const string SESSIONKEY_SupplierName = "SupplierName";
        private const string SESSIONKEY_ProductCode = "ProductCode";
        private const string SESSIONKEY_MinQuantityAlert = "MinQuantityAlert";

        // ページサイズの設定
        private const int pageSize = 100;

        public IActionResult Index()
        {
            LoginUserModel user = UserDataList();
            SessionReset();
            D_StockStatusSearchModel search = new D_StockStatusSearchModel();
            search.DepoID = user.MainDepoID;
            return Search(search);
            //return View(search);
        }

        public IActionResult Search(D_StockStatusSearchModel search)
        {
            //// 入庫日の日付形式チェック
            //if (!String.IsNullOrEmpty(search.SearchDate))
            //{
            //    if (!DateTime.TryParse(search.SearchDate, out DateTime SearchDate))
            //    {
            //        ViewData["Error"] = "検索日を日付形式に変更できません";
            //        return View("Index", search);
            //    }
            //}

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
            var search = new D_StockStatusSearchModel();

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

        private IActionResult Search(D_StockStatusSearchModel search, int page)
        {
            try
            {
                List<D_StockStatusViewModel> viewList = GetStockList(search);
                int _dataCount = viewList.Count;

                if (_dataCount > 0)
                {
                    search.StockList = viewList.ToPagedList(page, pageSize);

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
            catch (CustomExtention ex)
            {
                ViewData["Error"] = ex.Message;
            }
            catch (Exception ex)
            {
                ViewData["Error"] = "データ検索に失敗しました。";
            }

            return View("Index", search);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ExcelOutput(D_StockStatusSearchModel searchModel)
        {
            try
            {
                var receiptList = GetStockList(searchModel);

                // ファイル名
                var filename = "stock_status_data_" + DateTime.Now.ToString("yyyyMMddHHmmss");

                var headerNameList = new List<string>();

                // TODO:クラス化したい
                // 参考：https://stackoverflow.com/questions/7335629/get-displayattribute-attribute-from-propertyinfo
                var properties = typeof(D_StockStatusExcelViewModel).GetProperties()
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
                            sl.SetCellValue(col, ++row, data[_col].ProductCode);
                            sl.SetCellValue(col, ++row, data[_col].ProductName);
                            sl.SetCellValue(col, ++row, data[_col].ProductAbbreviation);
                            sl.SetCellValue(col, ++row, data[_col].SupplierCode);
                            sl.SetCellValue(col, ++row, data[_col].SupplierName);
                            sl.SetCellValue(col, ++row, data[_col].Packing);
                            sl.SetCellValue(col, ++row, data[_col].StoreAddress1);
                            sl.SetCellValue(col, ++row, data[_col].StoreAddress2);
                            sl.SetCellValue(col, ++row, data[_col].LotQuantity);
                            sl.SetCellValue(col, ++row, data[_col].FormalStoreAddressPackingCount);
                            sl.SetCellValue(col, ++row, data[_col].TemporaryStoreAddressPackingCount);
                            sl.SetCellValue(col, ++row, data[_col].TotalPackingCount);
                            sl.SetCellValue(col, ++row, data[_col].StockQuantity);
                            sl.SetCellValue(col, ++row, data[_col].LastStoreInDate);
                            sl.SetCellValue(col, ++row, data[_col].LastStoreOutDate);
                            sl.SetCellValue(col, ++row, data[_col].MinQuantity);
                            sl.SetCellValue(col, ++row, data[_col].MaxQuantity);
                            sl.SetCellValue(col, ++row, data[_col].MinPackingCount);
                            sl.SetCellValue(col, ++row, data[_col].MaxPackingCount);
                            sl.SetCellValue(col, ++row, data[_col].MinQuantityAlert);
                            sl.SetCellValue(col, ++row, data[_col].MaxQuantityAlert);
                            sl.SetCellValue(col, ++row, data[_col].MinPackingCountAlert);
                            sl.SetCellValue(col, ++row, data[_col].MaxPackingCountAlert);
                            sl.SetCellValue(col, ++row, data[_col].HalfYearNotShipment);
                            sl.SetCellValue(col, ++row, data[_col].OneYearNotShipment);
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

        public List<D_StockStatusViewModel> GetStockList(D_StockStatusSearchModel search)
        {
            search.ProductCode = Util.ObjectToString(search.ProductCode);
            search.SupplierCode = Util.ObjectToString(search.SupplierCode);

            //var user = UserDataList();
            string whereString;

            var sb = new StringBuilder($@"WHERE (1=1) ");

            if (!String.IsNullOrEmpty(search.ProductCode))
            {
                sb.Append($@"AND ((AAA.ProductCode LIKE ('%'+@ProductCode+'%')) OR (M_Product.ProductCode LIKE ('%'+@ProductCode+'%'))) ");
            }
            if (!String.IsNullOrEmpty(search.SupplierCode))
            {
                sb.Append($@"AND (M_Supplier.SupplierCode LIKE ('%'+@SupplierCode+'%')) ");
            }

            whereString = sb.ToString();

            DateTime searchDate;
            if (!DateTime.TryParse(search.SearchDate, out searchDate))
            {
                var displayName = search.GetDisplayName(typeof(D_StockStatusSearchModel).GetProperty("Name"), "SearchDate");
                throw new CustomExtention(displayName + "を日付に変換できません");
            }
            //string searchDate = DateTime.Now.ToString("yyyy/MM/dd");

            var stockViewModels = new List<D_StockStatusViewModel>();
            try
            {
                var db = User.Claims.Where(x => x.Type == CustomClaimTypes.ClaimType_DatabaseName).First().Value;
                var connectionString = new GetConnectString(db).ConnectionString;
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    var commandText = $@"
                                                with StoreInData as--入庫データ
                                                (
                                                    SELECT
                                                        A.DepoID,
                                                        A.ProductCode,
                                                        A.Quantity AS LotQuantity,
                                                        MIN(A.StoreInDate) AS FirstStoreInDate,
                                                        MAX(A.StoreInDate) AS LastStoreInDate,
                                                        SUM(A.PackingCount) AS StoreInPackingCount,
                                                        SUM(A.Quantity) AS StoreInQuantity
                                                    FROM D_StoreIn AS A
                                                    LEFT OUTER JOIN M_Product AS B
                                                        ON  A.ProductCode = B.ProductCode
                                                    WHERE(1=1)
                                                        AND A.DepoID = @DepoID
                                                        AND A.StoreInDate <= @SearchDate
                                                        AND A.DeleteFlag = @DeleteFlag
                                                        AND (
                                                                B.NotStockFlag IS NULL
                                                            OR  B.NotStockFlag = @NotStockFlag
                                                            )
                                                    GROUP BY
                                                        A.DepoID,
                                                        A.ProductCode,
                                                        A.Quantity
			                                    )
                                                , StoreOutData AS --出庫データ,
                                                (
                                                    SELECT
                                                        A.DepoID,
                                                        A.ProductCode,
                                                        A.Quantity AS LotQuantity,
                                                        MIN(A.StoreOutDate) AS FirstStoreOutDate,
                                                        MAX(A.StoreOutDate) AS LastStoreOutDate,
                                                        SUM(A.PackingCount) AS StoreOutPackingCount,
                                                        SUM(A.Quantity) AS StoreOutQuantity
                                                    FROM D_StoreOut AS A
                                                    LEFT OUTER JOIN M_Product AS B
                                                        ON  A.ProductCode = B.ProductCode
                                                    WHERE(1=1)
                                                        AND A.DepoID = @DepoID
                                                        AND A.StoreOutDate <= @SearchDate
                                                        AND A.DeleteFlag = @DeleteFlag
                                                        AND (
                                                                B.NotStockFlag IS NULL
                                                            OR  B.NotStockFlag = @NotStockFlag
                                                        )
                                                    GROUP BY
                                                        A.DepoID,
                                                        A.ProductCode,
                                                        A.Quantity
                                                )

		                                                SELECT
			                                                ISNULL(M_Depo.DepoCode, AAA.DepoID) AS DepoCode,
			                                                ISNULL(M_Depo.DepoName,'') AS DepoName,
			                                                ISNULL(M_Product.ProductID, 0) AS ProductID,
			                                                ISNULL(M_Product.ProductCode, AAA.ProductCode) AS ProductCode,--商品マスタに存在しない場合は、実績から商品コードを表示する
			                                                ISNULL(M_Product.ProductAbbreviation, '') AS ProductAbbreviation,
			                                                ISNULL(M_Product.ProductName, '') AS ProductName,
			                                                ISNULL(M_Supplier.SupplierCode, '') AS SupplierCode,
			                                                ISNULL(M_Supplier.SupplierName, '') AS SupplierName,
			                                                ISNULL(M_Product.Packing, '') AS Packing,
			                                                ISNULL(M_Product.LotQuantity, AAA.LotQuantity) AS LotQuantity,
			                                                ISNULL(M_Product.StoreAddress1, '') AS StoreAddress1,
			                                                ISNULL(M_Product.StoreAddress2, '') AS StoreAddress2,
			                                                ISNULL(StoreInPackingCount, 0) AS StoreInPackingCount,
			                                                ISNULL(StoreOutPackingCount, 0) AS StoreOutPackingCount,
			                                                (
				                                                SELECT
					                                                SUM(A.PackingCount)
				                                                FROM
					                                                D_StoreIn AS A
				                                                WHERE(1=1)
				                                                AND A.ProductCode = AAA.ProductCode
				                                                AND A.DeleteFlag = @DeleteFlag
				                                                --未出庫
				                                                AND EXISTS(
					                                                --仮番地マスタに存在しているか確認
					                                                SELECT
						                                                *
					                                                FROM
						                                                M_TemporaryStoreAddress AS tempAd
					                                                WHERE(1=1)
					                                                AND A.DepoID = @DepoID
					                                                AND A.StockLocation1 = tempAd.TemporaryStoreAddress1
					                                                AND A.StockLocation2 = tempAd.TemporaryStoreAddress2
				                                                )
			                                                ) AS TemporaryStoreAddressPackingCount,
			                                                (ISNULL(StoreInPackingCount, 0) - ISNULL(StoreOutPackingCount, 0)) AS TotalPackingCount,
			                                                ISNULL(StoreInQuantity, 0) AS StoreInQuantity,
			                                                ISNULL(StoreOutQuantity, 0) AS StoreOutQuantity,
                                                            ISNULL(M_Product.MinimumQuantity, 0) AS MinQuantity,
                                                            CASE
                                                                WHEN (StoreInQuantity - StoreOutQuantity) - ISNULL(M_Product.MinimumQuantity, 0) < 0 THEN 1
                                                                ELSE 0
                                                            END AS MinQuantityAlert,
                                                            ISNULL(M_Product.MaximumQuantity, 99999999) AS MaxQuantity,
                                                            CASE
                                                                WHEN (StoreInQuantity - StoreOutQuantity) - ISNULL(M_Product.MaximumQuantity, 99999999) > 0 THEN 1
                                                                ELSE 0
                                                            END AS MaxQuantityAlert,
                                                            ISNULL(M_Product.MinimumPackingCount, 0) AS MinPackingCount,
                                                            CASE
                                                                WHEN (StoreInPackingCount - StoreOutPackingCount) - ISNULL(M_Product.MinimumPackingCount, 0) < 0 THEN 1
                                                                ELSE 0
                                                            END AS MinPackingCountAlert,
                                                            ISNULL(M_Product.MaximumPackingCount, 99999999) AS MaxPackingCount,
                                                            CASE
                                                                WHEN (StoreInPackingCount - StoreOutPackingCount) - ISNULL(M_Product.MaximumPackingCount, 99999999) > 0 THEN 1
                                                                ELSE 0
                                                            END AS MaxPackingCountAlert,
			                                                FORMAT(ISNULL(FirstStoreInDate, ''), 'yyyy/MM/dd') AS FirstStoreInDate,
			                                                FORMAT(ISNULL(LastStoreInDate, ''), 'yyyy/MM/dd') AS LastStoreInDate,
			                                                FORMAT(ISNULL(FirstStoreOutDate, ''), 'yyyy/MM/dd') AS FirstStoreOutDate,
			                                                FORMAT(ISNULL(LastStoreOutDate, ''), 'yyyy/MM/dd') AS LastStoreOutDate
		                                                FROM(
			                                                ---StoreInDataとStoreOutDataをフル結合してベースの入出庫データを作成
			                                                SELECT
				                                                ISNULL(A.DepoID, B.DepoID) AS DepoID,
				                                                ISNULL(A.ProductCode, B.ProductCode) AS ProductCode,
				                                                ISNULL(A.LotQuantity, B.LotQuantity) AS LotQuantity,
				                                                StoreInPackingCount,
				                                                StoreOutPackingCount,
				                                                StoreInQuantity,
				                                                StoreOutQuantity,
				                                                FirstStoreInDate,
				                                                LastStoreInDate,
				                                                FirstStoreOutDate,
				                                                LastStoreOutDate
			                                                FROM StoreInData AS A
			                                                FULL OUTER JOIN StoreOutData AS B
				                                                ON A.DepoID = B.DepoID
				                                                AND A.ProductCode = B.ProductCode
				                                                AND A.LotQuantity = B.LotQuantity
				                                            ) AAA
		                                                FULL OUTER JOIN
			                                                M_Product
		                                                ON   AAA.ProductCode = M_Product.ProductCode
		                                                AND AAA.DepoID = M_Product.DepoID
		                                                AND AAA.LotQuantity = M_Product.LotQuantity
		                                                LEFT OUTER JOIN
			                                                M_Depo
		                                                ON  M_Product.DepoID = M_Depo.DepoID
		                                                LEFT OUTER JOIN
			                                                M_Supplier
		                                                ON  M_Product.SupplierCode = M_Supplier.SupplierCode
                                                        " + whereString +
                                                        $@"
                                                            ORDER BY
                                                                M_Supplier.SupplierCode,
                                                                AAA.ProductCode
                                                                              ;";

                    var param = new
                    {
                        SearchDate = searchDate,
                        //search.MinQuantityAlert,
                        NotStockFlag = 0,
                        DeleteFlag = 0,
                        search.DepoID,
                        search.SupplierCode,
                        search.ProductCode
                    };
                    stockViewModels = connection.Query<D_StockStatusViewModel>(commandText, param).ToList();

                    if (search.MinQuantityAlert)
                    {
                        stockViewModels = stockViewModels.Where(x => x.MinQuantityAlert == true).ToList();
                    }

                    if (search.MinPackingCountAlert)
                    {
                        stockViewModels = stockViewModels.Where(x => x.MinPackingCountAlert == true).ToList();
                    }

                    if (search.IsTemporaryStoreAddress)
                    {
                        stockViewModels = stockViewModels.Where(x => x.TemporaryStoreAddressPackingCount > 0).ToList();
                    }

                    foreach (var viewModel in stockViewModels)
                    {
                        if (viewModel.LastStoreOutDate != "1900/01/01" && Convert.ToDateTime(viewModel.LastStoreOutDate) < DateTime.Now.AddMonths(-6))
                        {
                            viewModel.HalfYearNotShipment = true;
                        }
                        if (viewModel.LastStoreOutDate != "1900/01/01" && Convert.ToDateTime(viewModel.LastStoreOutDate) < DateTime.Now.AddYears(-1))
                        {
                            viewModel.HalfYearNotShipment = false;
                            viewModel.OneYearNotShipment = true;
                        }
                    }

                    if (stockViewModels.Count() == 0) ViewData["Error"] = "検索条件に一致するデータはありません";
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            return stockViewModels;
        }

        private void SessionSet(int? page, D_StockStatusSearchModel model)
        {
            HttpContext.Session.SetString(SESSIONKEY_PageNo, page == null ? "1" : page.ToString());
            HttpContext.Session.SetString(SESSIONKEY_DepoCode, model.DepoID.ToString());
            HttpContext.Session.SetString(SESSIONKEY_SupplierCode, model.SupplierCode == null ? "" : model.SupplierCode.ToString());
            HttpContext.Session.SetString(SESSIONKEY_SupplierName, model.SupplierName == null ? "" : model.SupplierName.ToString());
            HttpContext.Session.SetString(SESSIONKEY_ProductCode, model.ProductCode == null ? "" : model.ProductCode.ToString());
            HttpContext.Session.SetString(SESSIONKEY_MinQuantityAlert, model.MinQuantityAlert.ToString());
        }

        private (int PageNo, D_StockStatusSearchModel SerchModel) SessionGet()
        {
            D_StockStatusSearchModel search = new D_StockStatusSearchModel();

            int.TryParse(HttpContext.Session.GetString(SESSIONKEY_PageNo), out int pageNo);

            int.TryParse(HttpContext.Session.GetString(SESSIONKEY_DepoCode), out int depoCode);
            search.DepoID = depoCode;

            string suppliersCode = HttpContext.Session.GetString(SESSIONKEY_SupplierCode);
            search.SupplierCode = suppliersCode == "" ? null : suppliersCode;

            string suppliersName = HttpContext.Session.GetString(SESSIONKEY_SupplierName);
            search.SupplierName = suppliersName == "" ? null : suppliersName;

            string productCode = HttpContext.Session.GetString(SESSIONKEY_ProductCode);
            search.ProductCode = productCode == "" ? null : productCode;

            search.MinQuantityAlert = Convert.ToBoolean(HttpContext.Session.GetString(SESSIONKEY_MinQuantityAlert) ?? "true");

            return (pageNo, search);

        }
        private void SessionReset()
        {
            HttpContext.Session.Remove(SESSIONKEY_PageNo);
            HttpContext.Session.Remove(SESSIONKEY_DepoCode);
            HttpContext.Session.Remove(SESSIONKEY_SupplierCode);
            HttpContext.Session.Remove(SESSIONKEY_SupplierName);
            HttpContext.Session.Remove(SESSIONKEY_ProductCode);
            HttpContext.Session.Remove(SESSIONKEY_MinQuantityAlert);
        }

    }

}