using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using stock_management_system.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using stock_management_system.common;
using System.Data.SqlClient;
using Dapper;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations.Schema;
using stock_management_system.Models.common;

namespace stock_management_system.Models
{
    public class M_ShipmentImportPattern
    {
        [Key]
        public int ShipmentImportPatternID { get; set; }

        public int DepoID { get; set; }

        public int RowStartNumber { get; set; }

        public bool IsStockOut { get; set; }
        public Enums.ShipmentImportDuplicationCheckType DuplicationCheckType { get; set; }

        public int MatchString1ColumnNumber { get; set; }
        public string MatchString1_1 { get; set; }
        public string MatchString1_2 { get; set; }
        public string MatchString1_3 { get; set; }

        public int MatchString2ColumnNumber { get; set; }
        public string MatchString2_1 { get; set; }
        public string MatchString2_2 { get; set; }
        public string MatchString2_3 { get; set; }
        public string MatchString2_4 { get; set; }
        public string MatchString2_5 { get; set; }

        public bool MatchString1ErrorFlag { get; set; }
        public bool MatchString2ErrorFlag { get; set; }

        public bool RowEndSignFlag { get; set; }
        public string RowEndSignString { get; set; }

        public int FixedValuePackingCount { get; set; }


        public int ColumnIndexHandyMatchClass { get; set; }
        public int ColumnIndexHandyMatchResult { get; set; }
        public int ColumnIndexShipmentDate { get; set; }
        public int ColumnIndexDeliveryDate { get; set; }
        public int ColumnIndexDeliveryTimeClass { get; set; }
        public int ColumnIndexDeliverySlipNumber { get; set; }
        public int ColumnIndexDeliverySlipRowNumber { get; set; }
        public int ColumnIndexSupplierCode { get; set; }
        public int ColumnIndexSupplierClass { get; set; }
        public int ColumnIndexCustomerCode { get; set; }
        public int ColumnIndexCustomerClass { get; set; }
        public int ColumnIndexCustomerName { get; set; }
        public int ColumnIndexProductCode { get; set; }
        public int ColumnIndexProductAbbreviation { get; set; }
        public int ColumnIndexProductManagementClass { get; set; }
        public int ColumnIndexProductLabelBranchNumber { get; set; }
        public int ColumnIndexNextProcess1 { get; set; }
        public int ColumnIndexLocation1 { get; set; }
        public int ColumnIndexNextProcess2 { get; set; }
        public int ColumnIndexLocation2 { get; set; }
        public int ColumnIndexCustomerProductCode { get; set; }
        public int ColumnIndexCustomerProductAbbreviation { get; set; }
        public int ColumnIndexCustomerProductManagementClass { get; set; }
        public int ColumnIndexCustomerProductLabelBranchNumber { get; set; }
        public int ColumnIndexCustomerNextProcess1 { get; set; }
        public int ColumnIndexCustomerLocation1 { get; set; }
        public int ColumnIndexCustomerNextProcess2 { get; set; }
        public int ColumnIndexCustomerLocation2 { get; set; }
        public int ColumnIndexCustomerOrderNumber { get; set; }
        public int ColumnIndexCustomerOrderClass { get; set; }
        public int ColumnIndexCustomerDeliveryDate { get; set; }
        public int ColumnIndexCustomerDeliveryTimeClass { get; set; }
        public int ColumnIndexCustomerDeliverySlipNumber { get; set; }
        public int ColumnIndexCustomerDeliverySlipRowNumber { get; set; }
        public int ColumnIndexLotQuantity { get; set; }
        public int ColumnIndexFractionQuantity { get; set; }
        public int ColumnIndexQuantity { get; set; }
        public int ColumnIndexPacking { get; set; }
        public int ColumnIndexPackingCount { get; set; }
        public int ColumnIndexLotNumber { get; set; }
        public int ColumnIndexInvoiceNumber { get; set; }
        public int ColumnIndexExpirationDate { get; set; }
        public int ColumnIndexRemark { get; set; }
        public int ColumnIndexHandyUserCode { get; set; }
        public int ColumnIndexHandyUserName { get; set; }
        public int ColumnIndexHandyScanDate { get; set; }
        public int ColumnIndexHandyScanTime { get; set; }

        public static M_ShipmentImportPattern GetShipmentImportPattern(string db, int patternID)
        {
            var shipmentImportPattern = new M_ShipmentImportPattern();

            var connectionString = new GetConnectString(db).ConnectionString;
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                try
                {
                    string selectString = $@"
                                                SELECT
                                                   ShipmentImportPatternID
                                                  ,PatternCode
                                                  ,DepoID
                                                  ,PatternName
                                                  ,RowStartNumber
                                                  ,IsStockOut
                                                  ,DuplicationCheckType
                                                  ,MatchString1ColumnNumber
                                                  ,MatchString1_1
                                                  ,MatchString1_2
                                                  ,MatchString1_3
                                                  ,MatchString2ColumnNumber
                                                  ,MatchString2_1
                                                  ,MatchString2_2
                                                  ,MatchString2_3
                                                  ,MatchString2_4
                                                  ,MatchString2_5
                                                  ,MatchString1ErrorFlag
                                                  ,MatchString2ErrorFlag
                                                  ,RowEndSignFlag
                                                  ,RowEndSignString
                                                  ,FixedValuePackingCount
                                                  ,ColumnIndexHandyMatchClass
                                                  ,ColumnIndexHandyMatchResult
                                                  ,ColumnIndexShipmentDate
                                                  ,ColumnIndexDeliveryDate
                                                  ,ColumnIndexDeliveryTimeClass
                                                  ,ColumnIndexDeliverySlipNumber
                                                  ,ColumnIndexDeliverySlipRowNumber
                                                  ,ColumnIndexSupplierCode
                                                  ,ColumnIndexSupplierClass
                                                  ,ColumnIndexProductCode
                                                  ,ColumnIndexProductAbbreviation
                                                  ,ColumnIndexProductManagementClass
                                                  ,ColumnIndexProductLabelBranchNumber
                                                  ,ColumnIndexNextProcess1
                                                  ,ColumnIndexLocation1
                                                  ,ColumnIndexNextProcess2
                                                  ,ColumnIndexLocation2
                                                  ,ColumnIndexCustomerCode
                                                  ,ColumnIndexCustomerClass
                                                  ,ColumnIndexCustomerName
                                                  ,ColumnIndexCustomerProductCode
                                                  ,ColumnIndexCustomerProductAbbreviation
                                                  ,ColumnIndexCustomerProductManagementClass
                                                  ,ColumnIndexCustomerProductLabelBranchNumber
                                                  ,ColumnIndexCustomerDeliveryDate
                                                  ,ColumnIndexCustomerDeliveryTimeClass
                                                  ,ColumnIndexCustomerDeliverySlipNumber
                                                  ,ColumnIndexCustomerDeliverySlipRowNumber
                                                  ,ColumnIndexCustomerNextProcess1
                                                  ,ColumnIndexCustomerLocation1
                                                  ,ColumnIndexCustomerNextProcess2
                                                  ,ColumnIndexCustomerLocation2
                                                  ,ColumnIndexCustomerOrderNumber
                                                  ,ColumnIndexCustomerOrderClass
                                                  ,ColumnIndexLotQuantity
                                                  ,ColumnIndexFractionQuantity
                                                  ,ColumnIndexQuantity
                                                  ,ColumnIndexPacking
                                                  ,ColumnIndexPackingCount
                                                  ,ColumnIndexLotNumber
                                                  ,ColumnIndexInvoiceNumber
                                                  ,ColumnIndexExpirationDate
                                                  ,ColumnIndexRemark
                                                  ,ColumnIndexHandyUserCode
                                                  ,ColumnIndexHandyUserName
                                                  ,ColumnIndexHandyScanDate
                                                  ,ColumnIndexHandyScanTime
                                                  ,CreateDate
                                                  ,CreateUserID
                                                  ,UpdateDate
                                                  ,UpdateUserID
                                                FROM M_ShipmentImportPattern
                                                WHERE 1=1
                                                    AND ShipmentImportPatternID = @ShipmentImportPatternID
                                                ;";
                    var param = new
                    {
                        ShipmentImportPatternID = patternID
                    };
                    var shipmentImportPatternList = connection.Query<M_ShipmentImportPattern>(selectString, param).ToList();

                    if (shipmentImportPatternList.Count() > 0)
                    {
                        // OK
                        shipmentImportPattern = shipmentImportPatternList[0];
                    }
                    else
                    {

                    }

                }
                catch (Exception ex)
                {
                    throw;
                }

            }
            return shipmentImportPattern;
        }

        public class ShipmentImportPattern 
        { 
            public int ShipmentImportPatternID { get; set; }
            public string PatternCode { get; set; }
            public string PatternName { get; set; }
        }

        public static IEnumerable<SelectListItem> GetShipmentImportPatternList(string db)
        {
            var selectListItems = new List<SelectListItem>();
            var shipmentImportPatterns = new List<ShipmentImportPattern>();

            try
            {
                var connectionString = new GetConnectString(db).ConnectionString;
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string commandText = $@"
                        SELECT
                               ShipmentImportPatternID,
                               PatternCode,
                               PatternName
                        FROM M_ShipmentImportPattern
                        ";
                    shipmentImportPatterns = connection.Query<ShipmentImportPattern>(commandText).ToList();

                    foreach (var pattern in shipmentImportPatterns)
                    {
                        var item = new SelectListItem { Value = pattern.ShipmentImportPatternID.ToString(), Text = pattern.PatternCode.ToString() + "：" + pattern.PatternName.ToString() };
                        selectListItems.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                //DB取得エラー
            }
            return selectListItems;

        }

    }

}



