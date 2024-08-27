using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;
using stock_management_system.common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Claims;
using X.PagedList;

namespace stock_management_system.Models
{
    public class D_ShipmentModel
    {
        public class D_ShipmentSearchModel : CommonModel
        {
            [Display(Name = "倉庫")]
            public int DepoID { get; set; }
            //[Display(Name = "社内仕入先コード")]
            //public string SupplierCode { get; set; }
            [Display(Name = "出荷日（始）")]
            public string ShipmentDateStart { get; set; }
            [Display(Name = "出荷日（終）")]
            public string ShipmentDateEnd { get; set; }
            [Display(Name = "社内仕入先名")]
            public string SupplierName { get; set; }
            [Display(Name = "社内商品コード")]
            public string ProductCode { get; set; }
            [Display(Name = "社内納入先1")]
            public string NextProcess1 { get; set; }
            [Display(Name = "社内納入先2")]
            public string NextProcess2 { get; set; }
            [Display(Name = "客先納品書番号")]
            public string CustomerDeliverySlipNumber { get; set; }
            [Display(Name = "客先納入先")]
            public string CustomerNextProcess1 { get; set; }

            [Display(Name = "倉庫")]
            public IEnumerable<SelectListItem> DepoList
            {
                get
                {
                    return DepoCodeSelectList;
                }
            }

            public IPagedList<D_ShipmentViewModel> ShipmentList { get; set; }

            public Page Page { get; set; }

            //public string startDay = DateTime.Now.AddDays(-7).Year.ToString() + "/" + DateTime.Now.AddDays(-7).Month.ToString("d2") + "/" + DateTime.Now.AddDays(-7).Day.ToString("d2");
            public string endDay = DateTime.Now.Year.ToString() + "/" + DateTime.Now.Month.ToString("d2") + "/" + DateTime.Now.Day.ToString("d2");

            public D_ShipmentSearchModel()
            {
                ShipmentDateStart = endDay;
                ShipmentDateEnd = endDay;
            }

        }

        public class D_ShipmentExcelViewModel
        {
            [Display(Name = "倉庫コード")]
            public string DepoCode { get; set; }
            [Display(Name = "倉庫名")]
            public string DepoName { get; set; }
            [Display(Name = "出荷日")]
            public string ShipmentDate { get; set; }

            [Display(Name = "社内納入日")]
            public string DeliveryDate { get; set; }
            [Display(Name = "社内納入便")]
            public string DeliveryTimeClass { get; set; }
            [Display(Name = "社内納品書番号")]
            public string DeliverySlipNumber { get; set; }
            [Display(Name = "社内納品書行番号")]
            public int DeliverySlipRowNumber { get; set; }
            [Display(Name = "社内仕入先コード")]
            public string SupplierCode { get; set; }
            [Display(Name = "社内仕入先名")]
            public string SupplierName { get; set; }
            [Display(Name = "社内商品コード")]
            public string ProductCode { get; set; }
            [Display(Name = "社内商品略番")]
            public string ProductAbbreviation { get; set; }
            [Display(Name = "社内商品管理区分")]
            public string ProductManagementClass { get; set; }
            [Display(Name = "社内商品名")]
            public string ProductName { get; set; }
            [Display(Name = "社内発行枝番")]
            public int ProductLabelBranchNumber { get; set; }
            [Display(Name = "社内納入先1")]
            public string NextProcess1 { get; set; }
            [Display(Name = "社内置き場1")]
            public string Location1 { get; set; }
            [Display(Name = "社内納入先2")]
            public string NextProcess2 { get; set; }
            [Display(Name = "社内置き場2")]
            public string Location2 { get; set; }

            [Display(Name = "客先納入期日")]
            public string CustomerDeliveryDate { get; set; }
            [Display(Name = "客先納入便")]
            public string CustomerDeliveryTimeClass { get; set; }
            [Display(Name = "客先納品書番号")]
            public string CustomerDeliverySlipNumber { get; set; }
            [Display(Name = "客先納品書行番号")]
            public int CustomerDeliverySlipRowNumber { get; set; }
            [Display(Name = "客先コード")]
            public string CustomerCode { get; set; }
            [Display(Name = "客先名")]
            public string CustomerName { get; set; }
            [Display(Name = "客先商品コード")]
            public string CustomerProductCode { get; set; }
            [Display(Name = "客先商品略番")]
            public string CustomerProductAbbreviation { get; set; }
            [Display(Name = "客先商品管理区分")]
            public string CustomerProductManagementClass { get; set; }
            [Display(Name = "客先発行枝番")]
            public int CustomerProductLabelBranchNumber { get; set; }
            [Display(Name = "客先納入先1")]
            public string CustomerNextProcess1 { get; set; }
            [Display(Name = "客先置き場1")]
            public string CustomerLocation1 { get; set; }
            [Display(Name = "客先納入先2")]
            public string CustomerNextProcess2 { get; set; }
            [Display(Name = "客先置き場2")]
            public string CustomerLocation2 { get; set; }
            [Display(Name = "客先発注番号")]
            public string CustomerOrderNumber { get; set; }
            [Display(Name = "客先発注区分")]
            public string CustomerOrderClass { get; set; }

            [Display(Name = "ロット数")]
            public int LotQuantity { get; set; }
            [Display(Name = "数量")]
            public int Quantity { get; set; }
            [Display(Name = "端数")]
            public int FractionQuantity { get; set; }
            [Display(Name = "不良数")]
            public int DefectiveQuantity { get; set; }
            [Display(Name = "箱種")]
            public string Packing { get; set; }
            [Display(Name = "箱数")]
            public int PackingCount { get; set; }

            [Display(Name = "ロット番号")]
            public string LotNumber { get; set; }
            [Display(Name = "請求書番号")]
            public string InvoiceNumber { get; set; }
            [Display(Name = "有効期限")]
            public string ExpirationDate { get; set; }
            [Display(Name = "備考")]
            public string Remark { get; set; }

            [Display(Name = "登録日時")]
            public string CreateDate { get; set; }
            [Display(Name = "登録ユーザコード")]
            public string CreateUserCode { get; set; }
            [Display(Name = "登録ハンディスキャン日時")]
            public string ScanTime { get; set; }
            [Display(Name = "登録ハンディユーザコード")]
            public string CreateHandyUserCode { get; set; }

            [Display(Name = "取込ファイル名")]
            public string ImportFileName { get; set; }
            [Display(Name = "取込記録 登録ハンディユーザコード")]
            public string ImportLogHandyUserCode { get; set; }
            [Display(Name = "取込記録 登録ハンディユーザ名")]
            public string ImportLogHandyUserName { get; set; }
            [Display(Name = "取込記録 ハンディスキャン日時")]
            public string ImportLogHandyScanDate { get; set; }

            public D_ShipmentExcelViewModel()
            {

            }

        }

        public class D_ShipmentViewModel : CommonModel
        {
            public long ShipmentID { get; set; }
            public long ScanRecordID { get; set; }
            public long ShipmentInstructionDetailID { get; set; }
            public long StoreOutID { get; set; }
            public int DepoID { get; set; }

            [Display(Name = "出荷日")]
            public string ShipmentDate { get; set; }

            [Display(Name = "倉庫コード")]
            public string DepoCode { get; set; }

            [Display(Name = "倉庫名")]
            public string DepoName { get; set; }

            [Display(Name = "社内納入日")]
            public string DeliveryDate { get; set; }

            [Display(Name = "社内納入便")]
            public string DeliveryTimeClass { get; set; }

            [Display(Name = "社内納品書番号")]
            public string DeliverySlipNumber { get; set; }

            [Display(Name = "社内納品書行番号")]
            public int DeliverySlipRowNumber { get; set; }

            [Display(Name = "社内仕入先コード")]
            public string SupplierCode { get; set; }

            [Display(Name = "社内仕入先名")]
            public string SupplierName { get; set; }

            [Display(Name = "社内商品コード")]
            public string ProductCode { get; set; }

            [Display(Name = "社内商品略番")]
            public string ProductAbbreviation { get; set; }

            [Display(Name = "社内商品管理区分")]
            public string ProductManagementClass { get; set; }

            [Display(Name = "社内商品名")]
            public string ProductName { get; set; }

            [Display(Name = "社内発行枝番")]
            public int ProductLabelBranchNumber { get; set; }

            [Display(Name = "社内納入先1")]
            public string NextProcess1 { get; set; }

            [Display(Name = "社内置き場1")]
            public string Location1 { get; set; }

            [Display(Name = "社内納入先2")]
            public string NextProcess2 { get; set; }

            [Display(Name = "社内置き場2")]
            public string Location2 { get; set; }

            [Display(Name = "客先納入期日")]
            public string CustomerDeliveryDate { get; set; }

            [Display(Name = "客先納入便")]
            public string CustomerDeliveryTimeClass { get; set; }

            [Display(Name = "客先納品書番号")]
            public string CustomerDeliverySlipNumber { get; set; }

            [Display(Name = "客先納品書行番号")]
            public int CustomerDeliverySlipRowNumber { get; set; }

            [Display(Name = "客先コード")]
            public string CustomerCode { get; set; }

            [Display(Name = "客先名")]
            public string CustomerName { get; set; }

            [Display(Name = "客先商品コード")]
            public string CustomerProductCode { get; set; }

            [Display(Name = "客先商品略番")]
            public string CustomerProductAbbreviation { get; set; }

            [Display(Name = "客先商品管理区分")]
            public string CustomerProductManagementClass { get; set; }

            [Display(Name = "客先商品名")]
            public string CustomerProductName { get; set; }

            [Display(Name = "客先発行枝番")]
            public int CustomerProductLabelBranchNumber { get; set; }

            [Display(Name = "客先納入先1")]
            public string CustomerNextProcess1 { get; set; }

            [Display(Name = "客先置き場1")]
            public string CustomerLocation1 { get; set; }

            [Display(Name = "客先納入先2")]
            public string CustomerNextProcess2 { get; set; }

            [Display(Name = "客先置き場2")]
            public string CustomerLocation2 { get; set; }

            [Display(Name = "客先発注番号")]
            public string CustomerOrderNumber { get; set; }

            [Display(Name = "客先発注区分")]
            public string CustomerOrderClass { get; set; }

            [Display(Name = "ロット数")]
            public int LotQuantity { get; set; }

            [Display(Name = "数量")]
            public int Quantity { get; set; }

            [Display(Name = "端数")]
            public int FractionQuantity { get; set; }

            [Display(Name = "不良数")]
            public int DefectiveQuantity { get; set; }

            [Display(Name = "箱種")]
            public string Packing { get; set; }

            [Display(Name = "箱数")]
            public int PackingCount { get; set; }

            [Display(Name = "ロット番号")]
            public string LotNumber { get; set; }

            [Display(Name = "請求書番号")]
            public string InvoiceNumber { get; set; }

            [Display(Name = "注文番号")]
            public string OrderNumber { get; set; }

            [Display(Name = "有効期限")]
            public string ExpirationDate { get; set; }

            [Display(Name = "備考")]
            public string Remark { get; set; }

            [Display(Name = "登録日時")]
            public string CreateDate { get; set; }
            [Display(Name = "登録ユーザコード")]
            public string CreateUserCode { get; set; }
            [Display(Name = "登録ハンディスキャン日時")]
            public string ScanTime { get; set; }
            [Display(Name = "登録ハンディユーザコード")]
            public string CreateHandyUserCode { get; set; }

            [Display(Name = "取込ファイル名")]
            public string ImportFileName { get; set; }
            [Display(Name = "取込記録 登録ハンディユーザコード")]
            public string ImportLogHandyUserCode { get; set; }
            [Display(Name = "取込記録 登録ハンディユーザ名")]
            public string ImportLogHandyUserName { get; set; }
            [Display(Name = "取込記録 ハンディスキャン日時")]
            public string ImportLogHandyScanDate { get; set; }

            public bool DeleteFlag { get; set; }
            public long DeleteShipmentID { get; set; }
            public int CreateUserID { get; set; }
            public int UpdateUserID { get; set; }

            public int RowNo { get; set; } // 未使用　新規登録で使う

            [Display(Name = "倉庫")]
            public IEnumerable<SelectListItem> DepoList
            {
                get
                {
                    return GetDepoCodeSelectList();
                }
            }

            public List<D_ShipmentViewModel> ShipmentList { get; set; }

            public D_ShipmentViewModel()
            {

            }

        }
    }
 
}

