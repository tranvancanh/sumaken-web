using Dapper;
using stock_management_system.common;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace stock_management_system.Models
{
    public class M_SystemSettingModel
    {
        public int SystemSettingCode { get; set; }
        public int SystemSettingValue { get; set; }
        public string SystemSettingStringValue { get; set; }
        public string SystemSettingTitle { get; set; }
        public string SystemSettingDetail { get; set; }

        public string LastUpdateDateTime { get; set; }
        public string LastUpdateUserName { get; set; }

        public M_SystemSettingModel()
        {
            SystemSettingCode = 0;
            SystemSettingValue = 0;
            SystemSettingStringValue = "";
            SystemSettingTitle = string.Empty;
            SystemSettingDetail = string.Empty;
        }
    }

    public class M_SystemSettingViewModel : CommonModel
    {
        public string TextSearch { get; set; }
        public List<M_SystemSettingModel> SystemSettingSearchList { get; set; }
    }

    public static class SystemSetting
    {
        public static async Task<List<M_SystemSettingModel>> GetSystemSettingList(string db)
        {
            var systemSettingList = new List<M_SystemSettingModel>();

            // データベースから取得
            using (var connection = new SqlConnection(new GetConnectString(db).ConnectionString))
            {
                connection.Open();
                try
                {
                    string selectString = string.Empty;
                    selectString = $@"
                                          SELECT *
                                          FROM [M_SystemSetting]
                                          ORDER BY SystemSettingCode ASC
                                        ";
                    systemSettingList = (await connection.QueryAsync<M_SystemSettingModel>(selectString)).ToList();

                }
                catch (Exception ex)
                {
                    throw;
                }
            }
            return systemSettingList;
        }

    }

}
