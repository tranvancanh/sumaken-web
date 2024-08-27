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
using System.Security.Cryptography.X509Certificates;
using X.PagedList;

namespace stock_management_system.Models
{
    public class D_StockStatusModel
    {
        public class D_StockStatusSearchModel : CommonModel
        {
            [Display(Name = "対象日")]
            public string SearchDate { get; set; }

            [Display(Name = "倉庫")]
            public int DepoID { get; set; }
            [Display(Name = "倉庫")]
            public IEnumerable<SelectListItem> DepoList
            {
                get
                {
                    return DepoCodeSelectList;
                }
            }

            [Display(Name = "商品コード")]
            public string ProductCode { get; set; }

            [Display(Name = "仕入先コード")]
            public string SupplierCode { get; set; }
            [Display(Name = "仕入先名")]
            public string SupplierName { get; set; }

            [Display(Name = "過少アラート")]
            public bool MinQuantityAlert { get; set; }

            [Display(Name = "過少アラート")]
            public bool MinPackingCountAlert { get; set; }

            [Display(Name = "ストア外あり")]
            public bool IsTemporaryStoreAddress { get; set; }

            public IPagedList<D_StockStatusViewModel> StockList { get; set; }

            public Page Page { get; set; }

            public D_StockStatusSearchModel()
            {
                SearchDate = DateTime.Now.ToString("yyyy/MM/dd");
            }

        }

        public class D_StockStatusExcelViewModel
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
            [Display(Name = "最終入庫日")]
            public string LastStoreInDate { get; set; }
            [Display(Name = "最終出庫日")]
            public string LastStoreOutDate { get; set; }
            [Display(Name = "最小在庫数")]
            public int MinQuantity { get; set; }
            [Display(Name = "最大在庫数")]
            public int MaxQuantity { get; set; }
            [Display(Name = "最小箱数")]
            public int MinPackingCount { get; set; }
            [Display(Name = "最大箱数")]
            public int MaxPackingCount { get; set; }
            [Display(Name = "過少数量アラート")]
            public bool MinQuantityAlert { get; set; }
            [Display(Name = "過大数量アラート")]
            public bool MaxQuantityAlert { get; set; }
            [Display(Name = "過少箱数アラート")]
            public bool MinPackingCountAlert { get; set; }
            [Display(Name = "過大箱数アラート")]
            public bool MaxPackingCountAlert { get; set; }
            [Display(Name = "半年未出庫")]
            public bool HalfYearNotShipment { get; set; }
            [Display(Name = "１年未出庫")]
            public bool OneYearNotShipment { get; set; }

            public D_StockStatusExcelViewModel()
            {

            }

        }

        public class D_StockStatusViewModel : CommonModel
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
            [Display(Name = "初回入庫日")]
            public string FirstStoreInDate { get; set; }
            [Display(Name = "初回出庫日")]
            public string FirstStoreOutDate { get; set; }
            [Display(Name = "最終入庫日")]
            public string LastStoreInDate { get; set; }
            [Display(Name = "最終出庫日")]
            public string LastStoreOutDate { get; set; }
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
            [Display(Name = "最小数")]
            public int MinQuantity { get; set; }
            [Display(Name = "最大数")]
            public int MaxQuantity { get; set; }
            [Display(Name = "最小箱数")]
            public int MinPackingCount { get; set; }
            [Display(Name = "最大箱数")]
            public int MaxPackingCount { get; set; }
            [Display(Name = "過少")]
            public bool MinQuantityAlert { get; set; }
            [Display(Name = "過大")]
            public bool MaxQuantityAlert { get; set; }
            [Display(Name = "過少")]
            public bool MinPackingCountAlert { get; set; }
            [Display(Name = "過大")]
            public bool MaxPackingCountAlert { get; set; }
            [Display(Name = "半年未出庫")]
            public bool HalfYearNotShipment { get; set; }
            [Display(Name = "１年未出庫")]
            public bool OneYearNotShipment { get; set; }

        }
    }
 
}

