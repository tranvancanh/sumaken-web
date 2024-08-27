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

namespace stock_management_system.Models
{
    public class D_ShipmentImportViewModel : CommonModel
    {
        public IFormFile PostedFile { get; set; }

        public int PatternID { get; set; }

        [Display(Name = "取込パターン")]
        public IEnumerable<SelectListItem> PatternList
        { 
            get
            {
                return M_ShipmentImportPattern.GetShipmentImportPatternList(DataBaseName);
            } 
        }

        //public int DepoID { get; set; }
        //[Display(Name = "倉庫")]
        //public IEnumerable<SelectListItem> DepoList
        //{
        //    get
        //    {
        //        return DepoCodeSelectList;
        //    }
        //}

        [Display(Name = "固定箱数")]
        public int PackingCount { get; set; }


        public List<ImportedData> ImportedDatas { get; set; }

        public D_ShipmentImportViewModel()
        {

        }
    }

    public class ImportedData
    {
        [Display(Name = "倉庫名")]
        public string DepoName { get; set; }
        [Display(Name = "取込ファイル名")]
        public string ImportFileName { get; set; }
        [Display(Name = "登録数")]
        public int DataCount { get; set; }
        [Display(Name = "最初のスキャン日時")]
        public DateTime FirstImportLogHandyScanDate { get; set; }
        [Display(Name = "最後のスキャン日時")]
        public DateTime LastImportLogHandyScanDate { get; set; }
    }

    public class ShipmentDuplicationCheckData
    {
        public DateTime CustomerDeliveryDate { get; set; }
        public string CustomerDeliverySlipNumber { get; set; }
        public string ProductCode { get; set; }
        public int Quantity { get; set; }
        public int ProductLabelBranchNumber { get; set; }
        public int CustomerProductLabelBranchNumber { get; set; }
    }

    public class D_ShipmentImportModel
    {
        public long ShipmentID { get; set; }
        public long ShipmentInstructionDetailID { get; set; }
        public long ScanRecordID { get; set; }
        public long StoreOutID { get; set; }
        public string HandyMatchClass { get; set; }
        public string HandyMatchResult { get; set; }
        public DateTime ShipmentDate { get; set; }
        public int DepoID { get; set; }
        public DateTime DeliveryDate { get; set; }
        public string DeliveryTimeClass { get; set; }
        public string DeliverySlipNumber { get; set; }
        public int DeliverySlipRowNumber { get; set; }
        public string SupplierCode { get; set; }
        public string SupplierClass { get; set; }
        public string SupplierName { get; set; }
        public string ProductCode { get; set; }
        public string ProductAbbreviation { get; set; }
        public string ProductManagementClass { get; set; }
        public int ProductLabelBranchNumber { get; set; }
        public string NextProcess1 { get; set; }
        public string Location1 { get; set; }
        public string NextProcess2 { get; set; }
        public string Location2 { get; set; }
        public string CustomerProductCode { get; set; }
        public string CustomerProductAbbreviation { get; set; }
        public string CustomerProductManagementClass { get; set; }
        public int CustomerProductLabelBranchNumber { get; set; }
        public string CustomerCode { get; set; }
        public string CustomerClass { get; set; }
        public string CustomerName { get; set; }
        public string CustomerNextProcess1 { get; set; }
        public string CustomerLocation1 { get; set; }
        public string CustomerNextProcess2 { get; set; }
        public string CustomerLocation2 { get; set; }
        public string CustomerOrderNumber { get; set; }
        public string CustomerOrderClass { get; set; }
        public DateTime CustomerDeliveryDate { get; set; }
        public string CustomerDeliveryTimeClass { get; set; }
        public string CustomerDeliverySlipNumber { get; set; }
        public int CustomerDeliverySlipRowNumber { get; set; }
        public int LotQuantity { get; set; }
        public int FractionQuantity { get; set; }
        public int Quantity { get; set; }
        public string Packing { get; set; }
        public int PackingCount { get; set; }
        public string LotNumber { get; set; }
        public string InvoiceNumber { get; set; }
        public string ExpirationDate { get; set; }
        public bool DeleteFlag { get; set; }
        public long DeleteShipmentID { get; set; }
        public string Remark { get; set; }

        public string ImportLogHandyUserCode { get; set; }
        public string ImportLogHandyUserName { get; set; }

        public DateTime ImportLogHandyScanDate { get; set; }
        public string ImportLogHandyScanTime { get; set; }
        public DateTime ImportLogHandyScanDateAndTime { get; set; }

        string importFileName = "";
        public string ImportFileName
        {
            get
            {
                if (importFileName.Length > 500)
                {
                    importFileName.Substring(0, 500);
                }
                return importFileName;
            }
            set
            { importFileName = value; }
        }

        public DateTime CreateDate { get; set; }
        public int CreateUserID { get; set; }
        public int CreateHandyUserID { get; set; }
        public DateTime UpdateDate { get; set; }
        public int UpdateUserID { get; set; }

        public D_ShipmentImportModel()
        {
            ShipmentID = 0;
            ShipmentInstructionDetailID = 0;
            ScanRecordID = 0;
            StoreOutID = 0;
            HandyMatchClass = "";
            HandyMatchResult = "";
            ShipmentDate = Convert.ToDateTime("1900/01/01");
            DepoID = 0;
            DeliveryDate = Convert.ToDateTime("1900/01/01");
            DeliveryTimeClass = "";
            DeliverySlipNumber = "";
            DeliveryDate = Convert.ToDateTime("1900/01/01");
            CustomerCode = "";
            CustomerClass = "";
            CustomerName = "";
            SupplierCode = "";
            SupplierClass = "";
            SupplierName = "";
            ProductCode = "";
            ProductAbbreviation = "";
            ProductManagementClass = "";
            ProductLabelBranchNumber = 0;
            NextProcess1 = "";
            Location1 = "";
            NextProcess2 = "";
            Location2 = "";
            CustomerProductCode = "";
            CustomerProductAbbreviation = "";
            CustomerProductManagementClass = "";
            CustomerProductLabelBranchNumber = 0;
            CustomerOrderNumber = "";
            CustomerOrderClass = "";
            CustomerDeliveryDate = Convert.ToDateTime("1900/01/01");
            CustomerDeliveryTimeClass = "";
            CustomerDeliverySlipNumber = "";
            CustomerDeliveryDate = Convert.ToDateTime("1900/01/01");
            CustomerNextProcess1 = "";
            CustomerLocation1 = "";
            CustomerNextProcess2 = "";
            CustomerLocation2 = "";
            LotQuantity = 0;
            Quantity = 0;
            Packing = "";
            PackingCount = 0;
            LotNumber = "";
            Remark = "";
            ImportLogHandyUserCode = "";
            ImportLogHandyUserName = "";
            ImportLogHandyScanDate = Convert.ToDateTime("1900/01/01");
            ImportLogHandyScanTime = "";
            ImportLogHandyScanDateAndTime = Convert.ToDateTime("1900/01/01");
            DeleteFlag = false;
            DeleteShipmentID = 0;
            CreateHandyUserID = 0;
        }
    }

}



