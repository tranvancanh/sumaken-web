using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;
using stock_management_system.common;
using stock_management_system.Models.common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Claims;
using X.PagedList;

namespace stock_management_system.Models
{
    public class D_StoreInModel 
    {
        public class D_StoreInSearchModel : CommonModel
        {
            [Display(Name = "�q��")]
            public int DepoID { get; set; }
            [Display(Name = "���i�R�[�h")]
            public string ProductCode { get; set; }
            [Display(Name = "���i��")]
            public string ProductName { get; set; }
            [Display(Name = "�ۊǏꏊ1")]
            public string StockLocation1 { get; set; }
            [Display(Name = "�ۊǏꏊ2")]
            public string StockLocation2 { get; set; }
            [Display(Name = "���ɓ��i�n�j")]
            public string StoreInDateStart { get; set; }
            [Display(Name = "���ɓ��i�I�j")]
            public string StoreInDateEnd { get; set; }

            public IPagedList<D_StoreInViewModel> StoreInList { get; set; }
            public IEnumerable<SelectListItem> DepoList
            {
                get
                {
                    return DepoCodeSelectList;
                }
            }

            public Page Page { get; set; }

            //public string startDay = DateTime.Now.AddDays(-7).Year.ToString() + "/" + DateTime.Now.AddDays(-7).Month.ToString("d2") + "/" + DateTime.Now.AddDays(-7).Day.ToString("d2");
            public string endDay = DateTime.Now.Year.ToString() + "/" + DateTime.Now.Month.ToString("d2") + "/" + DateTime.Now.Day.ToString("d2");

            public D_StoreInSearchModel()
            {
                StoreInDateStart = endDay;
                StoreInDateEnd = endDay;
            }

        }

        public class D_StoreInViewModel : CommonModel
        {
            public long StoreInID { get; set; }
            public int RowNo { get; set; }

            public int DepoID { get; set; }
            [Display(Name = "�q�ɃR�[�h")]
            public string DepoCode { get; set; }
            [Display(Name = "�q�ɖ�")]
            public string DepoName { get; set; }

            [Required(ErrorMessage = "���ɓ��͕K�{���͂ł�")]
            [DataType(DataType.Date, ErrorMessage = "���ɓ������t�`���ł͂���܂���")]
            [Display(Name = "���ɓ�")]
            public string StoreInDate { get; set; }

            [Required(ErrorMessage = "���i�R�[�h�͕K�{���͂ł�")]
            [Display(Name = "���i�R�[�h")]
            public string ProductCode { get; set; }

            [Required(ErrorMessage = "���b�g���͕K�{���͂ł�")]
            [Display(Name = "���b�g��")]
            public int Quantity { get; set; }

            [Required(ErrorMessage = "�����͕K�{���͂ł�")]
            [Display(Name = "����")]
            public int PackingCount { get; set; }

            //[Display(Name = "����")]
            //public int Quantity { get; set; }

            [Display(Name = "����")]
            public string Packing { get; set; }

            [Display(Name = "�ۊǏꏊ1")]
            public string StockLocation1 { get; set; }
            [Display(Name = "�ۊǏꏊ2")]
            public string StockLocation2 { get; set; }

            [Display(Name = "���l")]
            public string Remark { get; set; }

            [Display(Name = "�폜���R")]
            public string RemarkDelete { get; set; }

            [Display(Name = "�o�^����")]
            public string CreateDate { get; set; }
            [Display(Name = "�o�^���[�U")]
            public string CreateUserCode { get; set; }
            [Display(Name = "�o�^�n���f�B")]
            public string CreateHandyUserCode { get; set; }
            [Display(Name = "�X�V����")]
            public string UpdateDate { get; set; }
            [Display(Name = "�X�V���[�U")]
            public string UpdateUserCode { get; set; }

            public long ScanRecordID { get; set; }
            public long ReceiveID { get; set; }
            public bool AdjustmentFlag { get; set; }
            public bool DeleteFlag { get; set; }
            public long DeleteStoreInID { get; set; }
            public int CreateUserID { get; set; }
            public int UpdateUserID { get; set; }

            [Display(Name = "�q��")]
            public IEnumerable<SelectListItem> DepoList
            {
                get
                {
                    return GetDepoCodeSelectList();
                }
            }

            public List<D_StoreInViewModel> StoreInList { get; set; }

            public D_StoreInViewModel()
            {
                Packing = "";
                StockLocation1 = "";
                StockLocation2 = "";
                Remark = "";
                RemarkDelete = "";
            }

        }

        public class D_StoreInExcelViewModel
        {
            [Display(Name = "�q�ɃR�[�h")]
            public string DepoCode { get; set; }
            [Display(Name = "�q�ɖ�")]
            public string DepoName { get; set; }
            [Display(Name = "���ɓ�")]
            public string StoreInDate { get; set; }
            [Display(Name = "���i�R�[�h")]
            public string ProductCode { get; set; }
            [Display(Name = "���b�g��")]
            public int Quantity { get; set; }
            [Display(Name = "����")]
            public int PackingCount { get; set; }
            [Display(Name = "����")]
            public string Packing { get; set; }
            [Display(Name = "�ۊǏꏊ1")]
            public string StockLocation1 { get; set; }
            [Display(Name = "�ۊǏꏊ2")]
            public string StockLocation2 { get; set; }
            [Display(Name = "���l")]
            public string Remark { get; set; }
            [Display(Name = "�폜���l")]
            public string RemarkDelete { get; set; }
            [Display(Name = "�o�^����")]
            public string CreateDate { get; set; }
            [Display(Name = "�o�^���[�U")]
            public string CreateUserCode { get; set; }
            [Display(Name = "�o�^�n���f�B")]
            public string CreateHandyUserCode { get; set; }
            [Display(Name = "�X�V����")]
            public string UpdateDate { get; set; }
            [Display(Name = "�X�V���[�U")]
            public string UpdateUserCode { get; set; }

            public D_StoreInExcelViewModel()
            {

            }
        }

        public static List<D_StoreInViewModel> GetStoreInByStoreInID(string db, long storeInID)
        {
            var storeInDataList = new List<D_StoreInViewModel>();

            try
            {
                var connectionString = new GetConnectString(db).ConnectionString;
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string commandText = $@"
                        SELECT 
                               StoreInID
                              ,ScanRecordID
                              ,ReceiveID
                              ,A.DepoID
                              ,B.DepoCode
                              ,B.DepoName
                              ,FORMAT(StoreInDate,'yyyy/MM/dd') AS StoreInDate
                              ,ProductCode
                              ,StockLocation1
                              ,StockLocation2
                              ,Packing
                              ,PackingCount
                              ,Quantity
                              ,Remark
                              ,RemarkDelete
                              ,A.CreateUserID
                              ,FORMAT(A.CreateDate,'yyyy/MM/dd HH:mm') AS CreateDate
                          FROM D_StoreIn AS A
                          LEFT OUTER JOIN M_Depo AS B ON A.DepoID = B.DepoID
                          LEFT OUTER JOIN M_HandyUser AS C ON A.CreateUserID = C.HandyUserID
                          LEFT OUTER JOIN M_HandyUser AS D ON A.UpdateUserID = D.HandyUserID
                          WHERE (1=1)
                              AND StoreInID = @StoreInID
                              AND DeleteFlag = @DeleteFlag
                        ";
                    var param = new
                    {
                        StoreInID = storeInID,
                        DeleteFlag = false
                    };
                    storeInDataList = connection.Query<D_StoreInViewModel>(commandText, param).ToList();
                    if (storeInDataList.Count > 1)
                    {
                        throw new CustomExtention("�d���f�[�^�����݂��܂�");
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }

            return storeInDataList;
        }

    }


}

