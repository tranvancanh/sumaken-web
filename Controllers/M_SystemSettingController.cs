using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using stock_management_system.common;
using stock_management_system.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace stock_management_system.Controllers
{
    [Authorize]
    public class M_SystemSettingController : BaseController
    {
        [HttpGet, HttpPost]
        public async Task<IActionResult> Index(string TextSearch = null)
        {
            var model = new M_SystemSettingViewModel();
            model.SystemSettingSearchList = await GetSystemSettingList(0, TextSearch);

            var tempMessage = TempData["Message"];
            if (tempMessage != null)
            {
                ViewData["Message"] = tempMessage;
            }

            var tempError = TempData["Error"];
            if (tempError != null)
            {
                ViewData["Error"] = tempError;
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int systemSettingCode)
        {
            M_SystemSettingModel model = (await GetSystemSettingList(systemSettingCode, null)).FirstOrDefault();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(M_SystemSettingModel model)
        {
            bool affectedRows = await EditSystemSetting(model);

            if (affectedRows)
            {
                TempData["Message"] = "データを更新しました";
            }
            else
            {
                TempData["Error"] = "データ更新に失敗しました";
            }

            return RedirectToAction("Index");
        }

        private async Task<List<M_SystemSettingModel>> GetSystemSettingList(int settingCode, string textSearch)
        {
            var list = new List<M_SystemSettingModel>();

            string whereString = $@"WHERE (1=1) AND (HideFlag = @HideFlag) ";
            if (settingCode > 0)
            {
                whereString += $@"AND (SystemSettingCode = @SystemSettingCode) ";
            }
            else if(textSearch != null)
            {
                whereString += $@"AND (SystemSettingCode LIKE @TextSearch) OR (SystemSettingTitle LIKE @TextSearch) ";
            }

            string db = UserDataList().DatabaseName;
            using (var connection = new SqlConnection(new GetConnectString(db).ConnectionString))
            {
                connection.Open();
                try
                {
                    string selectString = $@"
                                        SELECT
	                                           A.SystemSettingCode
                                              ,A.SystemSettingValue
                                              ,A.SystemSettingStringValue
                                              ,A.SystemSettingTitle
                                              ,A.SystemSettingDetail
                                              ,FORMAT(A.UpdateDate,'yyyy/MM/dd HH:mm:ss') AS LastUpdateDateTime
                                              ,B.UserDisplayName AS LastUpdateUserName
                                        FROM M_SystemSetting AS A
                                        LEFT OUTER JOIN M_User AS B ON A.UpdateUserID = B.UserID "
                                        + whereString
                                        + $@"ORDER BY SystemSettingCode ASC;";

                    list = (await connection.QueryAsync<M_SystemSettingModel>(selectString, new {
                        HideFlag = false,
                        SystemSettingCode = settingCode,
                        TextSearch = "%" + textSearch + "%"
                    })).ToList();
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
            return list;
        }

        private async Task<bool> EditSystemSetting(M_SystemSettingModel model)
        {
            var isEdit = false;
            var affectedRows = -99;

            string db = UserDataList().DatabaseName;
            using (var connection = new SqlConnection(new GetConnectString(db).ConnectionString))
            {
                connection.Open();
                try
                {
                    string commandText = $@"
                                        UPDATE [M_SystemSetting]
                                        SET 
                                            SystemSettingValue = @SystemSettingValue,
                                            SystemSettingStringValue =@SystemSettingStringValue,
                                            UpdateDate =@UpdateDate,
                                            UpdateUserID =@UpdateUserID
                                        WHERE SystemSettingCode = @SystemSettingCode
                                          ;
                                      ";

                    affectedRows = await connection.ExecuteAsync(commandText,
                        new
                        {
                            model.SystemSettingCode,
                            model.SystemSettingValue,
                            SystemSettingStringValue = model.SystemSettingStringValue ?? "",
                            UpdateDate = DateTime.Now,
                            UpdateUserID = UserDataList().UserID
                        }
                    );
                }
                catch (Exception ex)
                {
                    isEdit = false;
                }
            }
            if(affectedRows > 0)
                isEdit = true;
            else
                isEdit = false;
            return isEdit;
        }

    }
}
