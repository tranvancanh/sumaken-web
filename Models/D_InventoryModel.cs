using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using X.PagedList;

namespace stock_management_system.Models
{
    public class D_InventoryModel
    {
        public class D_InventorySearchModel : CommonModel
        {
            [Display(Name = "棚卸日")]
            public string InventoryDate { get; set; }

            [Display(Name = "対象倉庫")]
            public int DepoID { get; set; }
            [Display(Name = "対象倉庫")]
            public IEnumerable<SelectListItem> DepoList
            {
                get
                {
                    return DepoCodeSelectList;
                }
            }

            public List<D_InventoryViewModel> InventoryStockList { get; set; }

            public D_InventorySearchModel()
            {
                InventoryDate = DateTime.Now.ToString("yyyy/MM/dd");
            }

        }

        public class D_InventoryExcelViewModel
        {
            [Display(Name = "倉庫コード")]
            public string DepoCode { get; set; }
            [Display(Name = "倉庫名")]
            public string DepoName { get; set; }
            [Display(Name = "商品コード")]
            public string ProductCode { get; set; }
            [Display(Name = "商品名")]
            public string ProductName { get; set; }
            [Display(Name = "商品略称")]
            public string ProductAbbreviation { get; set; }
            [Display(Name = "仕入先コード")]
            public string SupplierCode { get; set; }
            [Display(Name = "仕入先名")]
            public string SupplierName { get; set; }
            [Display(Name = "最終入庫日時")]
            public DateTime LastStoreInDate { get; set; }
            [Display(Name = "最終出庫日時")]
            public DateTime LastStoreOutDate { get; set; }
            [Display(Name = "箱種")]
            public string Packing { get; set; }
            [Display(Name = "番地1")]
            public string StoreAddress1 { get; set; }
            [Display(Name = "番地2")]
            public string StoreAddress2 { get; set; }
            [Display(Name = "ロット数")]
            public int LotQuantity { get; set; }
            [Display(Name = "ストア内箱数")]
            public int FormalStoreAddressPackingCount
            {
                get
                {
                    var totalPackingCount = TotalPackingCount - TemporaryStoreAddressPackingCount;
                    return totalPackingCount;
                }
            }
            [Display(Name = "ストア外箱数")]
            public int TemporaryStoreAddressPackingCount { get; set; }

            [Display(Name = "総箱数")]
            public int TotalPackingCount { get; set; }
            [Display(Name = "総数")]
            public int StockQuantity { get; set; }

            [Display(Name = "入庫調整アラート")]
            public bool StoreInAdjustmentAlert { get; set; }
            [Display(Name = "出庫調整アラート")]
            public bool StoreOutAdjustmentAlert { get; set; }

            public D_InventoryExcelViewModel()
            {

            }

        }

        public class D_InventoryViewModel : CommonModel
        {
            public int DepoID { get; set; }

            [Display(Name = "倉庫コード")]
            public string DepoCode { get; set; }
            [Display(Name = "倉庫名")]
            public string DepoName { get; set; }
            [Display(Name = "商品コード")]
            public string ProductCode { get; set; }
            [Display(Name = "商品名")]
            public string ProductName { get; set; }
            [Display(Name = "商品略称")]
            public string ProductAbbreviation { get; set; }
            [Display(Name = "仕入先コード")]
            public string SupplierCode { get; set; }
            [Display(Name = "仕入先名")]
            public string SupplierName { get; set; }
            [Display(Name = "箱種")]
            public string Packing { get; set; }
            [Display(Name = "ロット数")]
            public int LotQuantity { get; set; }
            [Display(Name = "番地1")]
            public string StoreAddress1 { get; set; }
            [Display(Name = "番地2")]
            public string StoreAddress2 { get; set; }
            [Display(Name = "ストア内箱数")]
            public int FormalStoreAddressPackingCount
            {
                get
                {
                    var totalPackingCount = TotalPackingCount - TemporaryStoreAddressPackingCount;
                    return totalPackingCount;
                }
            }
            [Display(Name = "ストア外箱数")]
            public int TemporaryStoreAddressPackingCount { get; set; }
            [Display(Name = "総箱数")]
            public int TotalPackingCount { get; set; }
            [Display(Name = "総数")]
            public int StockQuantity { get; set; }
            [Display(Name = "最終入庫日時")]
            public DateTime LastStoreInDate { get; set; }
            [Display(Name = "最終出庫日時")]
            public DateTime LastStoreOutDate { get; set; }
            [Display(Name = "入庫調整アラート")]
            public bool StoreInAdjustmentAlert { get; set; }
            [Display(Name = "出庫調整アラート")]
            public bool StoreOutAdjustmentAlert { get; set; }

        }
    }
 
}

