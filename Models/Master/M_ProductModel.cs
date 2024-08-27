using System;
using System.ComponentModel.DataAnnotations;
using Dapper;
using stock_management_system.common;
using System.Data.SqlClient;
using System.Linq;
using System.Collections.Generic;
using stock_management_system.Models.common;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Logical;

namespace stock_management_system.Models
{
    public class M_ProductModel
    {
        public class M_Product
        {
            [Key]
            public int ProductID { get; set; }

            public string ProductCode { get; set; }
            public string ProductName { get; set; }
            public string ProductNameKana { get; set; }
            public int LotQuantity { get; set; }
            public string StoreAddress1 { get; set; }
            public string StoreAddress2 { get; set; }
            public string Packing { get; set; }

            /// <summary>
            /// まとめ入庫フラグ
            /// </summary>
            public bool BulkStoreInFlag { get; set; }

            public M_Product()
            {
                BulkStoreInFlag = false;
            }
        }

        public static List<M_Product> GetProduct(string db, int depoID)
        {
            var products = new List<M_Product>();
            string whereString = $@"AND DepoID = @DepoID AND NotUseFlag = @NotUseFlag ";

            try
            {
                var connectionString = new GetConnectString(db).ConnectionString;
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string commandText = $@"
                        SELECT
                               ProductID,
                               ProductCode,
                               ProductName,
                               ProductNameKana,
                               BulkStoreInFlag
                        FROM M_Product
                        WHERE (1=1)
                            {whereString}
                        ";
                    var param = new
                    {
                        DepoID = depoID,
                        NotUseFlag = 0
                    };
                    products = connection.Query<M_Product>(commandText, param).ToList();
                }
            }
            catch (Exception ex)
            {
                throw;
            }

            return products;
        }

        public static List<M_Product> GetProductByProductCode(string db, int depoID, string productCode)
        {
            var products = new List<M_Product>();

            if (String.IsNullOrEmpty(productCode))
            {
                throw new CustomExtention("商品コードは必須です");
            }

            string whereString = $@"AND DepoID = @DepoID AND NotUseFlag = @NotUseFlag AND ProductCode = @ProductCode ";

            try
            {
                var connectionString = new GetConnectString(db).ConnectionString;
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string commandText = $@"
                        SELECT
                               ProductID,
                               ProductCode,
                               ProductName,
                               ProductNameKana,
                               BulkStoreInFlag,
                               LotQuantity,
                               StoreAddress1,
                               StoreAddress2,
                               Packing
                        FROM M_Product
                        WHERE (1=1)
                            {whereString}
                        ";
                    var param = new
                    {
                        DepoID = depoID,
                        NotUseFlag = 0,
                        ProductCode = productCode
                    };
                    products = connection.Query<M_Product>(commandText, param).ToList();
                    if (products.Count > 1)
                    {
                        throw new CustomExtention("商品コードが重複しています");
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }

            return products;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="db"></param>
        /// <param name="depoID"></param>
        /// <param name="address1"></param>
        /// <param name="address2"></param>
        /// <returns></returns>
        /// <exception cref="CustomExtention"></exception>
        public static int GetProductByAddress1AndAddress2(string db, int depoID, string address1, string address2)
        {
            int dataCount = 0;

            string whereString = $@"
                                        AND DepoID = @DepoID
                                        AND NotUseFlag = @NotUseFlag
                                        AND StoreAddress1 = @StoreAddress1
                                        AND StoreAddress2 = @StoreAddress2 
                                        ";
            try
            {
                var connectionString = new GetConnectString(db).ConnectionString;
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string commandText = $@"
                        SELECT
                               COUNT(*)
                        FROM M_Product
                        WHERE (1=1)
                            {whereString}
                        ";
                    var param = new
                    {
                        DepoID = depoID,
                        NotUseFlag = 0,
                        StoreAddress1 = address1,
                        StoreAddress2 = address2
                    };
                    dataCount = connection.QueryFirst<int>(commandText, param);
                }
            }
            catch (Exception ex)
            {
                throw;
            }

            return dataCount;
        }

    }

}