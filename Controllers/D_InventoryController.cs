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
using static stock_management_system.Models.D_InventoryModel;
using DocumentFormat.OpenXml.EMMA;
using stock_management_system.Models.common;
using System.Reflection;

namespace stock_management_system.Controllers
{
    public class D_InventoryController : D_StockStatusController
    {

        public IActionResult Index(D_InventorySearchModel viewModel)
        {
            var user = UserDataList();
            viewModel.DepoID = user.MainDepoID;

            var stockStatusModel = new D_StockStatusModel.D_StockStatusSearchModel();

            var stockStatus = GetStockList(stockStatusModel);

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ExcelOutput(D_InventorySearchModel searchModel)
        {
            try
            {
                var stockStatusModel = new D_StockStatusModel.D_StockStatusSearchModel();
                var stockList = GetStockList(stockStatusModel);

                // ファイル名
                var filename = "stock_status_data_" + DateTime.Now.ToString("yyyyMMddHHmmss");

                var headerNameList = new List<string>();

                // TODO:クラス化したい
                // 参考：https://stackoverflow.com/questions/7335629/get-displayattribute-attribute-from-propertyinfo
                var properties = typeof(D_InventoryExcelViewModel).GetProperties()
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
                    var data = stockList;
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

    }

}