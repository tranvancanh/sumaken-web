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
            [Display(Name = "倉庫")]
            public int DepoID { get; set; }
            [Display(Name = "商品コード")]
            public string ProductCode { get; set; }
            [Display(Name = "商品名")]
            public string ProductName { get; set; }
            [Display(Name = "保管場所1")]
            public string StockLocation1 { get; set; }
            [Display(Name = "保管場所2")]
            public string StockLocation2 { get; set; }
            [Display(Name = "入庫日（始）")]
            public string StoreInDateStart { get; set; }
            [Display(Name = "入庫日（終）")]
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
            [Display(Name = "倉庫コード")]
            public string DepoCode { get; set; }
            [Display(Name = "倉庫名")]
            public string DepoName { get; set; }

            [Required(ErrorMessage = "入庫日は必須入力です")]
            [DataType(DataType.Date, ErrorMessage = "入庫日が日付形式ではありません")]
            [Display(Name = "入庫日")]
            public string StoreInDate { get; set; }

            [Required(ErrorMessage = "商品コードは必須入力です")]
            [Display(Name = "商品コード")]
            public string ProductCode { get; set; }

            [Required(ErrorMessage = "ロット数は必須入力です")]
            [Display(Name = "ロット数")]
            public int Quantity { get; set; }

            [Required(ErrorMessage = "箱数は必須入力です")]
            [Display(Name = "箱数")]
            public int PackingCount { get; set; }

            //[Display(Name = "数量")]
            //public int Quantity { get; set; }

            [Display(Name = "箱種")]
            public string Packing { get; set; }

            [Display(Name = "保管場所1")]
            public string StockLocation1 { get; set; }
            [Display(Name = "保管場所2")]
            public string StockLocation2 { get; set; }

            [Display(Name = "備考")]
            public string Remark { get; set; }

            [Display(Name = "削除理由")]
            public string RemarkDelete { get; set; }

            [Display(Name = "登録日時")]
            public string CreateDate { get; set; }
            [Display(Name = "登録ユーザ")]
            public string CreateUserCode { get; set; }
            [Display(Name = "登録ハンディ")]
            public string CreateHandyUserCode { get; set; }
            [Display(Name = "更新日時")]
            public string UpdateDate { get; set; }
            [Display(Name = "更新ユーザ")]
            public string UpdateUserCode { get; set; }

            public long ScanRecordID { get; set; }
            public long ReceiveID { get; set; }
            public bool AdjustmentFlag { get; set; }
            public bool DeleteFlag { get; set; }
            public long DeleteStoreInID { get; set; }
            public int CreateUserID { get; set; }
            public int UpdateUserID { get; set; }

            [Display(Name = "倉庫")]
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
            [Display(Name = "倉庫コード")]
            public string DepoCode { get; set; }
            [Display(Name = "倉庫名")]
            public string DepoName { get; set; }
            [Display(Name = "入庫日")]
            public string StoreInDate { get; set; }
            [Display(Name = "商品コード")]
            public string ProductCode { get; set; }
            [Display(Name = "ロット数")]
            public int Quantity { get; set; }
            [Display(Name = "箱数")]
            public int PackingCount { get; set; }
            [Display(Name = "箱種")]
            public string Packing { get; set; }
            [Display(Name = "保管場所1")]
            public string StockLocation1 { get; set; }
            [Display(Name = "保管場所2")]
            public string StockLocation2 { get; set; }
            [Display(Name = "備考")]
            public string Remark { get; set; }
            [Display(Name = "削除備考")]
            public string RemarkDelete { get; set; }
            [Display(Name = "登録日時")]
            public string CreateDate { get; set; }
            [Display(Name = "登録ユーザ")]
            public string CreateUserCode { get; set; }
            [Display(Name = "登録ハンディ")]
            public string CreateHandyUserCode { get; set; }
            [Display(Name = "更新日時")]
            public string UpdateDate { get; set; }
            [Display(Name = "更新ユーザ")]
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
                        throw new CustomExtention("重複データが存在します");
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

