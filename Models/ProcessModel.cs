using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using stock_management_system.Controllers;
using stock_management_system.common;
using System.Data.SqlClient;
using Dapper;
using stock_management_system.Models.common;
using DocumentFormat.OpenXml.Spreadsheet;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using static stock_management_system.Models.D_StoreInModel;
using static stock_management_system.Models.common.Enums;

namespace stock_management_system.Models
{
    public class ProcessModel : CommonModel
    {
        public static (bool processFlag, DateTime startDate) ProcessGet(LoginUserModel user, int processID)
        {
            var now = DateTime.Now.ToString();

            try
            {
                var connectionString = new GetConnectString(user.DatabaseName).ConnectionString;
                using (var connection = new SqlConnection(connectionString))
                {
                    //open-------------------------------------------------------------
                    connection.Open();

                    string selectString = $@"
                                SELECT
                                   ProcessFlag
                                  ,ProcessStartDate
                                FROM D_Process
                                WHERE (1=1)
                                    AND ProcessID = @ProcessID
                                ;";

                    var selectAdjustmentStoreInIDParam = new
                    {
                        ProcessID = processID
                    };

                    var result = connection.Query<(bool processFlag, DateTime startDate)>(selectString, selectAdjustmentStoreInIDParam).ToList().FirstOrDefault();

                    return result;
                }
            }
            catch (Exception ex)
            {
                throw new CustomExtention("処理状態の取得に失敗しました。");
            }
        }

        public static bool ProcessSet(LoginUserModel user, int processID)
        {
            var now = DateTime.Now.ToString();

            try
            {
                var connectionString = new GetConnectString(user.DatabaseName).ConnectionString;
                using (var connection = new SqlConnection(connectionString))
                {
                    //open-------------------------------------------------------------
                    connection.Open();

                    //SQLの準備
                    var updateCommandText = $@"
                            UPDATE D_Process SET
                                   ProcessFlag = @ProcessFlag
                                  ,ProcessStartDate = @ProcessStartDate
                                  ,ProcessExecutionUserID = @ProcessExecutionUserID
                            WHERE 1 = 1
                                AND ProcessID = @ProcessID
                            ;";

                    var updateParamModel = new
                    {
                        ProcessID = processID,
                        ProcessFlag = true,
                        ProcessStartDate = now,
                        ProcessExecutionUserID = user.UserID
                    };
                    var updateResult = connection.Execute(updateCommandText, updateParamModel);

                    return true;
                }
            }
            catch (Exception ex)
            {
                throw new CustomExtention("処理開始フラグセットに失敗しました。");
            }
        }

        public static bool ProcessEnd(LoginUserModel user, int processID)
        {
            var now = DateTime.Now.ToString();

            try
            {
                var connectionString = new GetConnectString(user.DatabaseName).ConnectionString;
                using (var connection = new SqlConnection(connectionString))
                {
                    //open-------------------------------------------------------------
                    connection.Open();

                    //SQLの準備
                    var updateCommandText = $@"
                            UPDATE D_Process SET
                                   ProcessFlag = @ProcessFlag
                                  ,ProcessEndDate = @ProcessEndDate
                            WHERE 1 = 1
                                AND ProcessID = @ProcessID
                            ;";

                    var updateParamModel = new
                    {
                        ProcessID = processID,
                        ProcessFlag = false,
                        ProcessEndDate = now
                    };
                    var updateResult = connection.Execute(updateCommandText, updateParamModel);

                    return true;
                }
            }
            catch (Exception ex)
            {
                throw new CustomExtention("処理終了フラグセットに失敗しました。");
            }
        }
    }
}
