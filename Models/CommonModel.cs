using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using stock_management_system.common;
using System.Threading.Tasks;
using System.CodeDom;
using System.Data;
using Microsoft.AspNetCore.Mvc.Rendering;
using stock_management_system.Controllers;
using System.Security.Claims;
using Dapper;
using System.ComponentModel;
using System.Reflection;

namespace stock_management_system.Models
{
    public class CommonModel
    {
        public int CompanyID { get; set; }
        public string DataBaseName { get; set; }
        public int Role { get; set; }
        public int UserID { get; set; }
        public string ControllerName { get; set; }
        public string ViewTitle { get; set; }
        public List<M_SystemSettingModel> SystemSettingList { get; set; }
        public IEnumerable<SelectListItem> DepoCodeSelectList { get; set; }
        //AGF追加
        //public IEnumerable<SelectListItem> CustomerCodeSelectList { get; set; }
        //public IEnumerable<SelectListItem> FinalDeliveryPlaceSelectList { get; set; }
        //public IEnumerable<SelectListItem> TruckBinCodeSelectList { get; set; }

        public void GetBaseView(ClaimsPrincipal claimsPrincipal, ViewContext viewContext)
        {
            CompanyID = Convert.ToInt32(claimsPrincipal.Claims.Where(x => x.Type == CustomClaimTypes.ClaimType_CampanyID).First().Value);
            DataBaseName = claimsPrincipal.Claims.Where(x => x.Type == CustomClaimTypes.ClaimType_DatabaseName).First().Value;
            UserID = Convert.ToInt32(claimsPrincipal.Claims.Where(x => x.Type == CustomClaimTypes.ClaimType_UserID).First().Value);
            Role = Convert.ToInt32(claimsPrincipal.Claims.Where(x => x.Type == CustomClaimTypes.ClaimType_Role).First().Value);
            ControllerName = viewContext.RouteData.Values["controller"].ToString();
            SystemSettingList = GetSystemSettingList();
            DepoCodeSelectList = GetDepoCodeSelectList();
            ViewTitle = GetViewTitle();
        }

        public string GetDisplayName(MemberInfo info, string propatyName)
        {
            string displayName = "";

            var attribute = info.GetCustomAttributes(typeof(DisplayNameAttribute), true).Cast<DisplayNameAttribute>().Where(x => x.DisplayName == "propatyName").FirstOrDefault();
            displayName = attribute.DisplayName;

            return displayName;
        }

        public IEnumerable<SelectListItem> GetDepoCodeSelectList()
        {
            var depoSelectList = new List<SelectListItem>();
            var depoList = new List<M_DepoModel>();

            try
            {
                var connectionString = new GetConnectString(DataBaseName).ConnectionString;
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string commandText = $@"
                        SELECT
                               A.DepoID,
                               B.DepoCode,
                               B.DepoName,
                               ISNULL(C.MainDepoID, 0) AS MainDepoID
                        FROM M_UserDepo AS A
                        LEFT OUTER JOIN M_Depo AS B
                            ON B.DepoID = A.DepoID
                        LEFT OUTER JOIN M_User AS C
                            ON C.UserID = A.UserID AND C.MainDepoID = A.DepoID
                        WHERE (1=1)
                            AND A.UserID = @UserID
                            AND A.NotUseFlag = @NotUseFlag
                        ORDER BY MainDepoID DESC, DepoID Asc
                        ";
                    var param = new
                    {
                        UserID = UserID,
                        NotUseFlag = 0
                    };
                    depoList = connection.Query<M_DepoModel>(commandText, param).ToList();

                    foreach(var depo in depoList)
                    {
                        var item = new SelectListItem { Value = depo.DepoID.ToString(), Text = depo.DepoCode.ToString() + "：" + depo.DepoName.ToString() };
                        depoSelectList.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                //DB取得エラー
            }
            return depoSelectList;

        }

        public string GetViewTitle()
        {
            string pageTitle = "";

            try
            {
                var connectionString = new GetMasterConnectString().ConnectionString;
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string commandText = $@"
                              SELECT
	                            A.MenuName AS MenuName
                              FROM M_WebMenu AS A
                              LEFT OUTER JOIN M_WebMenuController AS B ON  (A.CategoryID = B.CategoryID AND A.MenuID = B.MenuID)
                              WHERE 1=1
                                  AND A.CompanyID = @CompanyID
                                  AND B.Controller    = @Controller
                              ORDER BY SortNumber Asc
                        ";

                    var param = new
                    {
                        CompanyID = CompanyID,
                        Controller = ControllerName
                    };
                    pageTitle = connection.ExecuteScalar<string>(commandText, param);
                }
            }
            catch (Exception ex)
            {
                //DB取得エラー
                return pageTitle;
            }

            return pageTitle;
        }

        public bool PowerUser
        {
            get
            {
                // 強制入出庫などの権限をもつユーザー
                bool returnValue = false;

                // 基本はUser1のみ
                if (Role == 1)
                {
                    returnValue = true;
                }

                return returnValue;

            }
            set { }
        }

        /// <summary>
        /// システム設定一覧
        /// </summary>
        public List<M_SystemSettingModel> GetSystemSettingList()
        {
            var data = new List<M_SystemSettingModel>();
            try
            {
                var task = Task.Run(async () =>
                {
                    data = await SystemSetting.GetSystemSettingList(DataBaseName);
                });
                task.Wait();
                return data;
            }
            catch (Exception ex)
            {
                return data;
            }

        }

        /// <summary>
        /// 行先と便マスター画面
        /// </summary>
        public List<M_AGF_DestinationBinModel> GetAGF_DestinationBinList()
        {
            var data = new List<M_AGF_DestinationBinModel>();
            try
            {
                var task = Task.Run(async () =>
                {
                    data = await AGF_DestinationBin.GetAGF_DestinationBinList(DataBaseName);
                });
                task.Wait();
                return data;
            }
            catch (Exception ex)
            {
                return data;
            }

        }

        /// <summary>
        /// システム設定値の取得
        /// </summary>
        public (int Value, string StringValue) SystemSettingValue(int systemSettingCode)
        {
            try
            {
                var systemSetting = SystemSettingList.Where(x => x.SystemSettingCode == systemSettingCode).ToList().FirstOrDefault();

                if (systemSetting == null)
                {
                    return (0, "");
                }

                var systemSettingValue = systemSetting.SystemSettingValue;
                var systemSettingStringValue = systemSetting.SystemSettingStringValue;

                return (systemSettingValue, systemSettingStringValue);
            }
            catch (Exception ex)
            {
                return (0, "");
            }

        }


    }
}
