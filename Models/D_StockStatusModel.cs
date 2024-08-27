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
            [Display(Name = "�Ώۓ�")]
            public string SearchDate { get; set; }

            [Display(Name = "�q��")]
            public int DepoID { get; set; }
            [Display(Name = "�q��")]
            public IEnumerable<SelectListItem> DepoList
            {
                get
                {
                    return DepoCodeSelectList;
                }
            }

            [Display(Name = "���i�R�[�h")]
            public string ProductCode { get; set; }

            [Display(Name = "�d����R�[�h")]
            public string SupplierCode { get; set; }
            [Display(Name = "�d���於")]
            public string SupplierName { get; set; }

            [Display(Name = "�ߏ��A���[�g")]
            public bool MinQuantityAlert { get; set; }

            [Display(Name = "�ߏ��A���[�g")]
            public bool MinPackingCountAlert { get; set; }

            [Display(Name = "�X�g�A�O����")]
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
            [Display(Name = "�ŏI���ɓ�")]
            public string LastStoreInDate { get; set; }
            [Display(Name = "�ŏI�o�ɓ�")]
            public string LastStoreOutDate { get; set; }
            [Display(Name = "�ŏ��݌ɐ�")]
            public int MinQuantity { get; set; }
            [Display(Name = "�ő�݌ɐ�")]
            public int MaxQuantity { get; set; }
            [Display(Name = "�ŏ�����")]
            public int MinPackingCount { get; set; }
            [Display(Name = "�ő唠��")]
            public int MaxPackingCount { get; set; }
            [Display(Name = "�ߏ����ʃA���[�g")]
            public bool MinQuantityAlert { get; set; }
            [Display(Name = "�ߑ吔�ʃA���[�g")]
            public bool MaxQuantityAlert { get; set; }
            [Display(Name = "�ߏ������A���[�g")]
            public bool MinPackingCountAlert { get; set; }
            [Display(Name = "�ߑ唠���A���[�g")]
            public bool MaxPackingCountAlert { get; set; }
            [Display(Name = "���N���o��")]
            public bool HalfYearNotShipment { get; set; }
            [Display(Name = "�P�N���o��")]
            public bool OneYearNotShipment { get; set; }

            public D_StockStatusExcelViewModel()
            {

            }

        }

        public class D_StockStatusViewModel : CommonModel
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
            [Display(Name = "������ɓ�")]
            public string FirstStoreInDate { get; set; }
            [Display(Name = "����o�ɓ�")]
            public string FirstStoreOutDate { get; set; }
            [Display(Name = "�ŏI���ɓ�")]
            public string LastStoreInDate { get; set; }
            [Display(Name = "�ŏI�o�ɓ�")]
            public string LastStoreOutDate { get; set; }
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
            [Display(Name = "�ŏ���")]
            public int MinQuantity { get; set; }
            [Display(Name = "�ő吔")]
            public int MaxQuantity { get; set; }
            [Display(Name = "�ŏ�����")]
            public int MinPackingCount { get; set; }
            [Display(Name = "�ő唠��")]
            public int MaxPackingCount { get; set; }
            [Display(Name = "�ߏ�")]
            public bool MinQuantityAlert { get; set; }
            [Display(Name = "�ߑ�")]
            public bool MaxQuantityAlert { get; set; }
            [Display(Name = "�ߏ�")]
            public bool MinPackingCountAlert { get; set; }
            [Display(Name = "�ߑ�")]
            public bool MaxPackingCountAlert { get; set; }
            [Display(Name = "���N���o��")]
            public bool HalfYearNotShipment { get; set; }
            [Display(Name = "�P�N���o��")]
            public bool OneYearNotShipment { get; set; }

        }
    }
 
}

