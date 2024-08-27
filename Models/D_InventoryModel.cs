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
            [Display(Name = "�I����")]
            public string InventoryDate { get; set; }

            [Display(Name = "�Ώۑq��")]
            public int DepoID { get; set; }
            [Display(Name = "�Ώۑq��")]
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
            [Display(Name = "�q�ɃR�[�h")]
            public string DepoCode { get; set; }
            [Display(Name = "�q�ɖ�")]
            public string DepoName { get; set; }
            [Display(Name = "���i�R�[�h")]
            public string ProductCode { get; set; }
            [Display(Name = "���i��")]
            public string ProductName { get; set; }
            [Display(Name = "���i����")]
            public string ProductAbbreviation { get; set; }
            [Display(Name = "�d����R�[�h")]
            public string SupplierCode { get; set; }
            [Display(Name = "�d���於")]
            public string SupplierName { get; set; }
            [Display(Name = "�ŏI���ɓ���")]
            public DateTime LastStoreInDate { get; set; }
            [Display(Name = "�ŏI�o�ɓ���")]
            public DateTime LastStoreOutDate { get; set; }
            [Display(Name = "����")]
            public string Packing { get; set; }
            [Display(Name = "�Ԓn1")]
            public string StoreAddress1 { get; set; }
            [Display(Name = "�Ԓn2")]
            public string StoreAddress2 { get; set; }
            [Display(Name = "���b�g��")]
            public int LotQuantity { get; set; }
            [Display(Name = "�X�g�A������")]
            public int FormalStoreAddressPackingCount
            {
                get
                {
                    var totalPackingCount = TotalPackingCount - TemporaryStoreAddressPackingCount;
                    return totalPackingCount;
                }
            }
            [Display(Name = "�X�g�A�O����")]
            public int TemporaryStoreAddressPackingCount { get; set; }

            [Display(Name = "������")]
            public int TotalPackingCount { get; set; }
            [Display(Name = "����")]
            public int StockQuantity { get; set; }

            [Display(Name = "���ɒ����A���[�g")]
            public bool StoreInAdjustmentAlert { get; set; }
            [Display(Name = "�o�ɒ����A���[�g")]
            public bool StoreOutAdjustmentAlert { get; set; }

            public D_InventoryExcelViewModel()
            {

            }

        }

        public class D_InventoryViewModel : CommonModel
        {
            public int DepoID { get; set; }

            [Display(Name = "�q�ɃR�[�h")]
            public string DepoCode { get; set; }
            [Display(Name = "�q�ɖ�")]
            public string DepoName { get; set; }
            [Display(Name = "���i�R�[�h")]
            public string ProductCode { get; set; }
            [Display(Name = "���i��")]
            public string ProductName { get; set; }
            [Display(Name = "���i����")]
            public string ProductAbbreviation { get; set; }
            [Display(Name = "�d����R�[�h")]
            public string SupplierCode { get; set; }
            [Display(Name = "�d���於")]
            public string SupplierName { get; set; }
            [Display(Name = "����")]
            public string Packing { get; set; }
            [Display(Name = "���b�g��")]
            public int LotQuantity { get; set; }
            [Display(Name = "�Ԓn1")]
            public string StoreAddress1 { get; set; }
            [Display(Name = "�Ԓn2")]
            public string StoreAddress2 { get; set; }
            [Display(Name = "�X�g�A������")]
            public int FormalStoreAddressPackingCount
            {
                get
                {
                    var totalPackingCount = TotalPackingCount - TemporaryStoreAddressPackingCount;
                    return totalPackingCount;
                }
            }
            [Display(Name = "�X�g�A�O����")]
            public int TemporaryStoreAddressPackingCount { get; set; }
            [Display(Name = "������")]
            public int TotalPackingCount { get; set; }
            [Display(Name = "����")]
            public int StockQuantity { get; set; }
            [Display(Name = "�ŏI���ɓ���")]
            public DateTime LastStoreInDate { get; set; }
            [Display(Name = "�ŏI�o�ɓ���")]
            public DateTime LastStoreOutDate { get; set; }
            [Display(Name = "���ɒ����A���[�g")]
            public bool StoreInAdjustmentAlert { get; set; }
            [Display(Name = "�o�ɒ����A���[�g")]
            public bool StoreOutAdjustmentAlert { get; set; }

        }
    }
 
}

