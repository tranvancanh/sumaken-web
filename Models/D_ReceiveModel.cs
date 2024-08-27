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
            [Display(Name = "�q��")]
            public int DepoID { get; set; }
            [Display(Name = "�d����R�[�h")]
            public string SupplierCode { get; set; }
            [Display(Name = "�d���於")]
            public string SupplierName { get; set; }
            [Display(Name = "���i�R�[�h")]
            public string ProductCode { get; set; }
            [Display(Name = "�[����1")]
            public string NextProcess1 { get; set; }
            [Display(Name = "�[����2")]
            public string NextProcess2 { get; set; }
            [Display(Name = "���ד��i�n�j")]
            public string ReceiveDateStart { get; set; }
            [Display(Name = "���ד��i�I�j")]
            public string ReceiveDateEnd { get; set; }

            [Display(Name = "�q��")]
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
            [Display(Name = "�q�ɃR�[�h")]
            public string DepoCode { get; set; }
            [Display(Name = "�q�ɖ�")]
            public string DepoName { get; set; }
            [Display(Name = "���ד�")]
            public string ReceiveDate { get; set; }
            [Display(Name = "�[������")]
            public string DeliveryDate { get; set; }
            [Display(Name = "�[����")]
            public string DeliveryTimeClass { get; set; }
            [Display(Name = "�[�i���ԍ�")]
            public string DeliverySlipNumber { get; set; }
            [Display(Name = "�[�i���s�ԍ�")]
            public int DeliverySlipRowNumber { get; set; }
            [Display(Name = "�d����R�[�h")]
            public string SupplierCode { get; set; }
            [Display(Name = "�d���於")]
            public string SupplierName { get; set; }
            [Display(Name = "���s�}��")]
            public int ProductLabelBranchNumber { get; set; }
            [Display(Name = "�[����1")]
            public string NextProcess1 { get; set; }
            [Display(Name = "�u����1")]
            public string Location1 { get; set; }
            [Display(Name = "�[����2")]
            public string NextProcess2 { get; set; }
            [Display(Name = "�u����2")]
            public string Location2 { get; set; }
            [Display(Name = "���i�R�[�h")]
            public string ProductCode { get; set; }
            [Display(Name = "���i����")]
            public string ProductAbbreviation { get; set; }
            [Display(Name = "���i�Ǘ��敪")]
            public string ProductManagementClass { get; set; }
            [Display(Name = "���i��")]
            public string ProductName { get; set; }
            [Display(Name = "���b�g��")]
            public int LotQuantity { get; set; }
            [Display(Name = "����")]
            public string Packing { get; set; }
            [Display(Name = "����")]
            public int PackingCount { get; set; }
            [Display(Name = "����")]
            public int Quantity { get; set; }
            [Display(Name = "�[��")]
            public int FractionQuantity { get; set; }
            //[Display(Name = "�s�ǐ�")]
            //public int DefectiveQuantity { get; set; }
            //[Display(Name = "���b�g�ԍ�")]
            //public string LotNumber { get; set; }
            //[Display(Name = "�������ԍ�")]
            //public string InvoiceNumber { get; set; }
            //[Display(Name = "�����ԍ�")]
            //public string OrderNumber { get; set; }
            //[Display(Name = "�������")]
            //public string ExpirationDate { get; set; }
            //[Display(Name = "�d�����i")]
            //public decimal CostPrice { get; set; }
            [Display(Name = "���l")]
            public string Remark { get; set; }
            [Display(Name = "�o�^���[�U")]
            public string CreateUserCode { get; set; }
            [Display(Name = "�o�^�n���f�B")]
            public string CreateHandyUserCode { get; set; }
            [Display(Name = "�X�L��������")]
            public string ScanTime { get; set; }
            [Display(Name = "�o�^����")]
            public string CreateDate { get; set; }
            //[Display(Name = "�X�V��")]
            //public string UpdateDate { get; set; }
            //[Display(Name = "�X�V��")]
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

            [Display(Name = "���ד�")]
            public string ReceiveDate { get; set; }

            [Display(Name = "�q�ɃR�[�h")]
            public string DepoCode { get; set; }

            [Display(Name = "�q�ɖ�")]
            public string DepoName { get; set; }

            [Display(Name = "�[������")]
            public string DeliveryDate { get; set; }

            [Display(Name = "�[����")]
            public string DeliveryTimeClass { get; set; }

            [Display(Name = "�[�i���ԍ�")]
            public string DeliverySlipNumber { get; set; }

            [Display(Name = "�[�i���s�ԍ�")]
            public int DeliverySlipRowNumber { get; set; }

            [Display(Name = "�d����R�[�h")]
            public string SupplierCode { get; set; }

            //[Display(Name = "�d����敪")]
            //public string SupplierClass { get; set; }

            [Display(Name = "�d���於")]
            public string SupplierName { get; set; }

            [Display(Name = "���s�}��")]
            public int ProductLabelBranchNumber { get; set; }

            [Display(Name = "�[����1")]
            public string NextProcess1 { get; set; }

            [Display(Name = "�u����1")]
            public string Location1 { get; set; }

            [Display(Name = "�[����2")]
            public string NextProcess2 { get; set; }

            [Display(Name = "�u����2")]
            public string Location2 { get; set; }

            [Display(Name = "���i�R�[�h")]
            public string ProductCode { get; set; }

            [Display(Name = "���i����")]
            public string ProductAbbreviation { get; set; }

            [Display(Name = "���i�Ǘ��敪")]
            public string ProductManagementClass { get; set; }

            [Display(Name = "���i��")]
            public string ProductName { get; set; }

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

            [Display(Name = "���b�g�ԍ�")]
            public string ExpirationDate { get; set; }

            [Display(Name = "�d�����i")]
            public decimal CostPrice { get; set; }

            [Display(Name = "���l")]
            public string Remark { get; set; }

            //[Display(Name = "�o�^��")]
            //public string CreateUser { get; set; }
            //[Display(Name = "�X�V��")]
            //public string UpdateDate { get; set; }
            //[Display(Name = "�X�V��")]
            //public string UpdateUser { get; set; }
            [Display(Name = "�o�^�҃��[�U")]
            public string CreateUserCode { get; set; }
            [Display(Name = "�o�^�n���f�B")]
            public string CreateHandyUserCode { get; set; }
            [Display(Name = "�X�L��������")]
            public string ScanTime { get; set; }
            [Display(Name = "�o�^����")]
            public string CreateDate { get; set; }

            public bool DeleteFlag { get; set; }
            public long DeleteReceiveID { get; set; }
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

            public List<D_ReceiveViewModel> ReceiveList { get; set; }

            public D_ReceiveViewModel()
            {

            }

        }
    }
 
}

