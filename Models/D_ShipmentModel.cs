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
            [Display(Name = "�q��")]
            public int DepoID { get; set; }
            //[Display(Name = "�Г��d����R�[�h")]
            //public string SupplierCode { get; set; }
            [Display(Name = "�o�ד��i�n�j")]
            public string ShipmentDateStart { get; set; }
            [Display(Name = "�o�ד��i�I�j")]
            public string ShipmentDateEnd { get; set; }
            [Display(Name = "�Г��d���於")]
            public string SupplierName { get; set; }
            [Display(Name = "�Г����i�R�[�h")]
            public string ProductCode { get; set; }
            [Display(Name = "�Г��[����1")]
            public string NextProcess1 { get; set; }
            [Display(Name = "�Г��[����2")]
            public string NextProcess2 { get; set; }
            [Display(Name = "�q��[�i���ԍ�")]
            public string CustomerDeliverySlipNumber { get; set; }
            [Display(Name = "�q��[����")]
            public string CustomerNextProcess1 { get; set; }

            [Display(Name = "�q��")]
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
            [Display(Name = "�q�ɃR�[�h")]
            public string DepoCode { get; set; }
            [Display(Name = "�q�ɖ�")]
            public string DepoName { get; set; }
            [Display(Name = "�o�ד�")]
            public string ShipmentDate { get; set; }

            [Display(Name = "�Г��[����")]
            public string DeliveryDate { get; set; }
            [Display(Name = "�Г��[����")]
            public string DeliveryTimeClass { get; set; }
            [Display(Name = "�Г��[�i���ԍ�")]
            public string DeliverySlipNumber { get; set; }
            [Display(Name = "�Г��[�i���s�ԍ�")]
            public int DeliverySlipRowNumber { get; set; }
            [Display(Name = "�Г��d����R�[�h")]
            public string SupplierCode { get; set; }
            [Display(Name = "�Г��d���於")]
            public string SupplierName { get; set; }
            [Display(Name = "�Г����i�R�[�h")]
            public string ProductCode { get; set; }
            [Display(Name = "�Г����i����")]
            public string ProductAbbreviation { get; set; }
            [Display(Name = "�Г����i�Ǘ��敪")]
            public string ProductManagementClass { get; set; }
            [Display(Name = "�Г����i��")]
            public string ProductName { get; set; }
            [Display(Name = "�Г����s�}��")]
            public int ProductLabelBranchNumber { get; set; }
            [Display(Name = "�Г��[����1")]
            public string NextProcess1 { get; set; }
            [Display(Name = "�Г��u����1")]
            public string Location1 { get; set; }
            [Display(Name = "�Г��[����2")]
            public string NextProcess2 { get; set; }
            [Display(Name = "�Г��u����2")]
            public string Location2 { get; set; }

            [Display(Name = "�q��[������")]
            public string CustomerDeliveryDate { get; set; }
            [Display(Name = "�q��[����")]
            public string CustomerDeliveryTimeClass { get; set; }
            [Display(Name = "�q��[�i���ԍ�")]
            public string CustomerDeliverySlipNumber { get; set; }
            [Display(Name = "�q��[�i���s�ԍ�")]
            public int CustomerDeliverySlipRowNumber { get; set; }
            [Display(Name = "�q��R�[�h")]
            public string CustomerCode { get; set; }
            [Display(Name = "�q�於")]
            public string CustomerName { get; set; }
            [Display(Name = "�q�揤�i�R�[�h")]
            public string CustomerProductCode { get; set; }
            [Display(Name = "�q�揤�i����")]
            public string CustomerProductAbbreviation { get; set; }
            [Display(Name = "�q�揤�i�Ǘ��敪")]
            public string CustomerProductManagementClass { get; set; }
            [Display(Name = "�q�攭�s�}��")]
            public int CustomerProductLabelBranchNumber { get; set; }
            [Display(Name = "�q��[����1")]
            public string CustomerNextProcess1 { get; set; }
            [Display(Name = "�q��u����1")]
            public string CustomerLocation1 { get; set; }
            [Display(Name = "�q��[����2")]
            public string CustomerNextProcess2 { get; set; }
            [Display(Name = "�q��u����2")]
            public string CustomerLocation2 { get; set; }
            [Display(Name = "�q�攭���ԍ�")]
            public string CustomerOrderNumber { get; set; }
            [Display(Name = "�q�攭���敪")]
            public string CustomerOrderClass { get; set; }

            [Display(Name = "���b�g��")]
            public int LotQuantity { get; set; }
            [Display(Name = "����")]
            public int Quantity { get; set; }
            [Display(Name = "�[��")]
            public int FractionQuantity { get; set; }
            [Display(Name = "�s�ǐ�")]
            public int DefectiveQuantity { get; set; }
            [Display(Name = "����")]
            public string Packing { get; set; }
            [Display(Name = "����")]
            public int PackingCount { get; set; }

            [Display(Name = "���b�g�ԍ�")]
            public string LotNumber { get; set; }
            [Display(Name = "�������ԍ�")]
            public string InvoiceNumber { get; set; }
            [Display(Name = "�L������")]
            public string ExpirationDate { get; set; }
            [Display(Name = "���l")]
            public string Remark { get; set; }

            [Display(Name = "�o�^����")]
            public string CreateDate { get; set; }
            [Display(Name = "�o�^���[�U�R�[�h")]
            public string CreateUserCode { get; set; }
            [Display(Name = "�o�^�n���f�B�X�L��������")]
            public string ScanTime { get; set; }
            [Display(Name = "�o�^�n���f�B���[�U�R�[�h")]
            public string CreateHandyUserCode { get; set; }

            [Display(Name = "�捞�t�@�C����")]
            public string ImportFileName { get; set; }
            [Display(Name = "�捞�L�^ �o�^�n���f�B���[�U�R�[�h")]
            public string ImportLogHandyUserCode { get; set; }
            [Display(Name = "�捞�L�^ �o�^�n���f�B���[�U��")]
            public string ImportLogHandyUserName { get; set; }
            [Display(Name = "�捞�L�^ �n���f�B�X�L��������")]
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

            [Display(Name = "�o�ד�")]
            public string ShipmentDate { get; set; }

            [Display(Name = "�q�ɃR�[�h")]
            public string DepoCode { get; set; }

            [Display(Name = "�q�ɖ�")]
            public string DepoName { get; set; }

            [Display(Name = "�Г��[����")]
            public string DeliveryDate { get; set; }

            [Display(Name = "�Г��[����")]
            public string DeliveryTimeClass { get; set; }

            [Display(Name = "�Г��[�i���ԍ�")]
            public string DeliverySlipNumber { get; set; }

            [Display(Name = "�Г��[�i���s�ԍ�")]
            public int DeliverySlipRowNumber { get; set; }

            [Display(Name = "�Г��d����R�[�h")]
            public string SupplierCode { get; set; }

            [Display(Name = "�Г��d���於")]
            public string SupplierName { get; set; }

            [Display(Name = "�Г����i�R�[�h")]
            public string ProductCode { get; set; }

            [Display(Name = "�Г����i����")]
            public string ProductAbbreviation { get; set; }

            [Display(Name = "�Г����i�Ǘ��敪")]
            public string ProductManagementClass { get; set; }

            [Display(Name = "�Г����i��")]
            public string ProductName { get; set; }

            [Display(Name = "�Г����s�}��")]
            public int ProductLabelBranchNumber { get; set; }

            [Display(Name = "�Г��[����1")]
            public string NextProcess1 { get; set; }

            [Display(Name = "�Г��u����1")]
            public string Location1 { get; set; }

            [Display(Name = "�Г��[����2")]
            public string NextProcess2 { get; set; }

            [Display(Name = "�Г��u����2")]
            public string Location2 { get; set; }

            [Display(Name = "�q��[������")]
            public string CustomerDeliveryDate { get; set; }

            [Display(Name = "�q��[����")]
            public string CustomerDeliveryTimeClass { get; set; }

            [Display(Name = "�q��[�i���ԍ�")]
            public string CustomerDeliverySlipNumber { get; set; }

            [Display(Name = "�q��[�i���s�ԍ�")]
            public int CustomerDeliverySlipRowNumber { get; set; }

            [Display(Name = "�q��R�[�h")]
            public string CustomerCode { get; set; }

            [Display(Name = "�q�於")]
            public string CustomerName { get; set; }

            [Display(Name = "�q�揤�i�R�[�h")]
            public string CustomerProductCode { get; set; }

            [Display(Name = "�q�揤�i����")]
            public string CustomerProductAbbreviation { get; set; }

            [Display(Name = "�q�揤�i�Ǘ��敪")]
            public string CustomerProductManagementClass { get; set; }

            [Display(Name = "�q�揤�i��")]
            public string CustomerProductName { get; set; }

            [Display(Name = "�q�攭�s�}��")]
            public int CustomerProductLabelBranchNumber { get; set; }

            [Display(Name = "�q��[����1")]
            public string CustomerNextProcess1 { get; set; }

            [Display(Name = "�q��u����1")]
            public string CustomerLocation1 { get; set; }

            [Display(Name = "�q��[����2")]
            public string CustomerNextProcess2 { get; set; }

            [Display(Name = "�q��u����2")]
            public string CustomerLocation2 { get; set; }

            [Display(Name = "�q�攭���ԍ�")]
            public string CustomerOrderNumber { get; set; }

            [Display(Name = "�q�攭���敪")]
            public string CustomerOrderClass { get; set; }

            [Display(Name = "���b�g��")]
            public int LotQuantity { get; set; }

            [Display(Name = "����")]
            public int Quantity { get; set; }

            [Display(Name = "�[��")]
            public int FractionQuantity { get; set; }

            [Display(Name = "�s�ǐ�")]
            public int DefectiveQuantity { get; set; }

            [Display(Name = "����")]
            public string Packing { get; set; }

            [Display(Name = "����")]
            public int PackingCount { get; set; }

            [Display(Name = "���b�g�ԍ�")]
            public string LotNumber { get; set; }

            [Display(Name = "�������ԍ�")]
            public string InvoiceNumber { get; set; }

            [Display(Name = "�����ԍ�")]
            public string OrderNumber { get; set; }

            [Display(Name = "�L������")]
            public string ExpirationDate { get; set; }

            [Display(Name = "���l")]
            public string Remark { get; set; }

            [Display(Name = "�o�^����")]
            public string CreateDate { get; set; }
            [Display(Name = "�o�^���[�U�R�[�h")]
            public string CreateUserCode { get; set; }
            [Display(Name = "�o�^�n���f�B�X�L��������")]
            public string ScanTime { get; set; }
            [Display(Name = "�o�^�n���f�B���[�U�R�[�h")]
            public string CreateHandyUserCode { get; set; }

            [Display(Name = "�捞�t�@�C����")]
            public string ImportFileName { get; set; }
            [Display(Name = "�捞�L�^ �o�^�n���f�B���[�U�R�[�h")]
            public string ImportLogHandyUserCode { get; set; }
            [Display(Name = "�捞�L�^ �o�^�n���f�B���[�U��")]
            public string ImportLogHandyUserName { get; set; }
            [Display(Name = "�捞�L�^ �n���f�B�X�L��������")]
            public string ImportLogHandyScanDate { get; set; }

            public bool DeleteFlag { get; set; }
            public long DeleteShipmentID { get; set; }
            public int CreateUserID { get; set; }
            public int UpdateUserID { get; set; }

            public int RowNo { get; set; } // ���g�p�@�V�K�o�^�Ŏg��

            [Display(Name = "�q��")]
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

