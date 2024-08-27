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
    public class D_ReceiveModel
    {
        public class D_ReceiveSearchModel : CommonModel
        {
            [Display(Name = "倉庫")]
            public int DepoID { get; set; }
            [Display(Name = "仕入先コード")]
            public string SupplierCode { get; set; }
            [Display(Name = "仕入先名")]
            public string SupplierName { get; set; }
            [Display(Name = "商品コード")]
            public string ProductCode { get; set; }
            [Display(Name = "納入先1")]
            public string NextProcess1 { get; set; }
            [Display(Name = "納入先2")]
            public string NextProcess2 { get; set; }
            [Display(Name = "入荷日（始）")]
            public string ReceiveDateStart { get; set; }
            [Display(Name = "入荷日（終）")]
            public string ReceiveDateEnd { get; set; }

            [Display(Name = "倉庫")]
            public IEnumerable<SelectListItem> DepoList
            {
                get
                {
                    return DepoCodeSelectList;
                }
            }

            public IPagedList<D_ReceiveViewModel> ReceiveList { get; set; }

            public Page Page { get; set; }

            //public string startDay = DateTime.Now.AddDays(-7).Year.ToString() + "/" + DateTime.Now.AddDays(-7).Month.ToString("d2") + "/" + DateTime.Now.AddDays(-7).Day.ToString("d2");
            public string endDay = DateTime.Now.Year.ToString() + "/" + DateTime.Now.Month.ToString("d2") + "/" + DateTime.Now.Day.ToString("d2");

            public D_ReceiveSearchModel()
            {
                ReceiveDateStart = endDay;
                ReceiveDateEnd = endDay;
            }

        }

        public class D_ReceiveExcelViewModel
        {
            [Display(Name = "倉庫コード")]
            public string DepoCode { get; set; }
            [Display(Name = "倉庫名")]
            public string DepoName { get; set; }
            [Display(Name = "入荷日")]
            public string ReceiveDate { get; set; }
            [Display(Name = "納入期日")]
            public string DeliveryDate { get; set; }
            [Display(Name = "納入便")]
            public string DeliveryTimeClass { get; set; }
            [Display(Name = "納品書番号")]
            public string DeliverySlipNumber { get; set; }
            [Display(Name = "納品書行番号")]
            public int DeliverySlipRowNumber { get; set; }
            [Display(Name = "仕入先コード")]
            public string SupplierCode { get; set; }
            [Display(Name = "仕入先名")]
            public string SupplierName { get; set; }
            [Display(Name = "発行枝番")]
            public int ProductLabelBranchNumber { get; set; }
            [Display(Name = "納入先1")]
            public string NextProcess1 { get; set; }
            [Display(Name = "置き場1")]
            public string Location1 { get; set; }
            [Display(Name = "納入先2")]
            public string NextProcess2 { get; set; }
            [Display(Name = "置き場2")]
            public string Location2 { get; set; }
            [Display(Name = "商品コード")]
            public string ProductCode { get; set; }
            [Display(Name = "商品略番")]
            public string ProductAbbreviation { get; set; }
            [Display(Name = "商品管理区分")]
            public string ProductManagementClass { get; set; }
            [Display(Name = "商品名")]
            public string ProductName { get; set; }
            [Display(Name = "ロット数")]
            public int LotQuantity { get; set; }
            [Display(Name = "箱種")]
            public string Packing { get; set; }
            [Display(Name = "箱数")]
            public int PackingCount { get; set; }
            [Display(Name = "数量")]
            public int Quantity { get; set; }
            [Display(Name = "端数")]
            public int FractionQuantity { get; set; }
            //[Display(Name = "不良数")]
            //public int DefectiveQuantity { get; set; }
            //[Display(Name = "ロット番号")]
            //public string LotNumber { get; set; }
            //[Display(Name = "請求書番号")]
            //public string InvoiceNumber { get; set; }
            //[Display(Name = "注文番号")]
            //public string OrderNumber { get; set; }
            //[Display(Name = "消費期限")]
            //public string ExpirationDate { get; set; }
            //[Display(Name = "仕入価格")]
            //public decimal CostPrice { get; set; }
            [Display(Name = "備考")]
            public string Remark { get; set; }
            [Display(Name = "登録ユーザ")]
            public string CreateUserCode { get; set; }
            [Display(Name = "登録ハンディ")]
            public string CreateHandyUserCode { get; set; }
            [Display(Name = "スキャン日時")]
            public string ScanTime { get; set; }
            [Display(Name = "登録日時")]
            public string CreateDate { get; set; }
            //[Display(Name = "更新日")]
            //public string UpdateDate { get; set; }
            //[Display(Name = "更新者")]
            //public string UpdateUser { get; set; }

            public D_ReceiveExcelViewModel()
            {

            }

        }

        public class D_ReceiveViewModel : CommonModel
        {
            public long ReceiveID { get; set; }
            public long ScanRecordID { get; set; }
            public long ReceiveScheduleDetailID { get; set; }
            public int DepoID { get; set; }

            [Display(Name = "入荷日")]
            public string ReceiveDate { get; set; }

            [Display(Name = "倉庫コード")]
            public string DepoCode { get; set; }

            [Display(Name = "倉庫名")]
            public string DepoName { get; set; }

            [Display(Name = "納入期日")]
            public string DeliveryDate { get; set; }

            [Display(Name = "納入便")]
            public string DeliveryTimeClass { get; set; }

            [Display(Name = "納品書番号")]
            public string DeliverySlipNumber { get; set; }

            [Display(Name = "納品書行番号")]
            public int DeliverySlipRowNumber { get; set; }

            [Display(Name = "仕入先コード")]
            public string SupplierCode { get; set; }

            //[Display(Name = "仕入先区分")]
            //public string SupplierClass { get; set; }

            [Display(Name = "仕入先名")]
            public string SupplierName { get; set; }

            [Display(Name = "発行枝番")]
            public int ProductLabelBranchNumber { get; set; }

            [Display(Name = "納入先1")]
            public string NextProcess1 { get; set; }

            [Display(Name = "置き場1")]
            public string Location1 { get; set; }

            [Display(Name = "納入先2")]
            public string NextProcess2 { get; set; }

            [Display(Name = "置き場2")]
            public string Location2 { get; set; }

            [Display(Name = "商品コード")]
            public string ProductCode { get; set; }

            [Display(Name = "商品略番")]
            public string ProductAbbreviation { get; set; }

            [Display(Name = "商品管理区分")]
            public string ProductManagementClass { get; set; }

            [Display(Name = "商品名")]
            public string ProductName { get; set; }

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

            [Display(Name = "ロット番号")]
            public string ExpirationDate { get; set; }

            [Display(Name = "仕入価格")]
            public decimal CostPrice { get; set; }

            [Display(Name = "備考")]
            public string Remark { get; set; }

            //[Display(Name = "登録者")]
            //public string CreateUser { get; set; }
            //[Display(Name = "更新日")]
            //public string UpdateDate { get; set; }
            //[Display(Name = "更新者")]
            //public string UpdateUser { get; set; }
            [Display(Name = "登録者ユーザ")]
            public string CreateUserCode { get; set; }
            [Display(Name = "登録ハンディ")]
            public string CreateHandyUserCode { get; set; }
            [Display(Name = "スキャン日時")]
            public string ScanTime { get; set; }
            [Display(Name = "登録日時")]
            public string CreateDate { get; set; }

            public bool DeleteFlag { get; set; }
            public long DeleteReceiveID { get; set; }
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

            public List<D_ReceiveViewModel> ReceiveList { get; set; }

            public D_ReceiveViewModel()
            {

            }

        }
    }
 
}

