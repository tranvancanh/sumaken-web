using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using stock_management_system.common;
using stock_management_system.Models;
using Dapper;
using static stock_management_system.Models.M_ProductModel;

namespace stock_management_system.Controllers
{
    public class AutoCompleteController : BaseController
    {

        [HttpPost]
        public JsonResult AutoCompleteSupplier(string prefix)
        {
            if (prefix != null && prefix != "")
            {
                var suppliersList = new List<M_SupplierModel>();
                try
                {
                    var db = UserDataList().DatabaseName;
                    var connectionString = new GetConnectString(db).ConnectionString;
                    using (var connection = new SqlConnection(connectionString))
                    {
                        connection.Open();
                        string commandText = $@"
                                SELECT
                                     SupplierCode
                                    ,SupplierName
                                    ,SupplierNameKana
                                  FROM M_Supplier
                                  WHERE (1=1)
	                                AND NotUseFlag = @NotUseFlag
                        ";

                        var param = new
                        {
                            NotUseFlag = 0
                        };
                        suppliersList = connection.Query<M_SupplierModel>(commandText, param).ToList();
                    }
                }
                catch (Exception ex)
                {
                }

                var values = (from supplier in suppliersList
                                 where supplier.SupplierCode.StartsWith(prefix)
                                 //ひらがなで入力しても半角ｶﾅに変更して検索する
                                 || supplier.SupplierNameKana.StartsWith(Util.ToHankakuKatakana(prefix), StringComparison.OrdinalIgnoreCase)
                                 //カナやコードの大文字小文字半角全角を無視して検索する
                                 || supplier.SupplierNameKana.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
                                 select new
                                 {
                                     val = supplier.SupplierCode,
                                     label = supplier.SupplierName,
                                 }).ToList();

                return Json(values);
            }
            return null;
        }

        [HttpPost]
        public JsonResult AutoCompleteProduct(string prefix, int depoId)
        {
            if (prefix != null && prefix != "" && depoId > 0)
            {
                var productList= new List<M_Product>();
                try
                {
                    var db = UserDataList().DatabaseName;
                    var connectionString = new GetConnectString(db).ConnectionString;
                    using (var connection = new SqlConnection(connectionString))
                    {
                        connection.Open();
                        string commandText = $@"
                                                        SELECT
                                                               ProductID,
                                                               ProductCode,
                                                               ProductName,
                                                               ProductNameKana
                                                        FROM M_Product
                                                        WHERE (1=1)
	                                                        AND NotUseFlag = @NotUseFlag
	                                                        AND DepoID = @DepoID
                                                                ";

                        var param = new
                        {
                            NotUseFlag = 0,
                            DepoID = depoId
                        };
                        productList = connection.Query<M_Product>(commandText, param).ToList();
                    }
                }
                catch (Exception ex)
                {
                }

                var values = (from product in productList
                                 where product.ProductCode.StartsWith(prefix)
                                 //ひらがなで入力しても半角ｶﾅに変更して検索する
                                 || product.ProductNameKana.StartsWith(Util.ToHankakuKatakana(prefix), StringComparison.OrdinalIgnoreCase)
                                 //カナやコードの大文字小文字半角全角を無視して検索する
                                 || product.ProductNameKana.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
                                 select new
                                 {
                                     val = product.ProductCode,
                                     label = product.ProductName,
                                 }).ToList();

                return Json(values);
            }
            return Json("");
        }

    }

}