using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using stock_management_system.common;
using stock_management_system.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using System.Data;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using static stock_management_system.Models.M_AGF_StackingNGModel;
using System.IO;
using System.Runtime.Serialization;
using System.Security.Principal;
using DocumentFormat.OpenXml.EMMA;

namespace stock_management_system.Controllers
{
    [Authorize]
    public class M_AGF_StackingNGController : BaseController
    {
        [HttpGet, HttpPost]
        public async Task<IActionResult> Index(string TextSearch = null)
        {
            var model = new M_AGF_StackingNGViewModel();
            model.StackingNGSearchList = await GetStackingNGSearchList(0,"",0,"", TextSearch);

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
        public async Task<IActionResult> Edit(int depoCode,string productCode,int oldDepoCode, string oldProductCode)
        {
            M_AGF_StackingNGModel model = new M_AGF_StackingNGModel();
            string db = UserDataList().DatabaseName;
            model.AGFDepoCodeList = model.AGFDepoCodeListCreate(db);
            model.ProductCodeList = model.ProductCodeListCreate(db);
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(M_AGF_StackingNGModel model)
        {
            bool affectedRows = await EditStackingNG(model);

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
        public new ClaimsPrincipal User { get; }
        [HttpGet]
        public IActionResult Import()
        {
            M_AGF_StackingNGModel model = new M_AGF_StackingNGModel();
            string db = UserDataList().DatabaseName;
            model.AGFDepoCodeList = model.AGFDepoCodeListCreate(db);
            model.ProductCodeList = model.ProductCodeListCreate(db);
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Import(M_AGF_StackingNGModel model)
        {
            bool affectedRows = await ImportStackingNG(model);

            if (affectedRows)
            {
                TempData["Message"] = "データを登録しました";
            }
            else
            {
                TempData["Error"] = "データ登録に失敗しました";
            }

            return RedirectToAction("Index");
        }

        private async Task<List<M_AGF_StackingNGModel>> GetStackingNGSearchList(int depocode, string productCode,int olddepocode, string oldproductCode,string textSearch)
        {
            var list = new List<M_AGF_StackingNGModel>();
            depocode = UserDataList().MainDepoID;
            //test用
            if(depocode == 1)
            {
                depocode = 0;
            }

            string whereString = "WHERE (1=1)";
            if (depocode > 0)
            {
                whereString += $@"AND (depo_code = @DepoCode) ";
            }
            else if (textSearch != null && depocode == 0)
            {
                whereString += $@"AND (depo_code LIKE @TextSearch) OR (product_code LIKE @TextSearch)";
            }
            else if (textSearch != null && depocode > 0)
            {
                whereString += $@"AND (product_code LIKE @TextSearch)";
            }

            string db = UserDataList().DatabaseName;
            using (var connection = new SqlConnection(new GetConnectString(db).ConnectionString))
            {
                connection.Open();
                try
                {
                    string selectString = $@"
                                        SELECT
	                                           A.depo_code AS DepoCode
                                              ,A.product_code AS ProductCode
                                              ,FORMAT(A.update_date,'yyyy/MM/dd HH:mm:ss') AS LastUpdateDateTime
                                             ,B.UserDisplayName AS LastUpdateUserName
                                              , A.depo_code AS OldDepoCode
                                              ,A.product_code AS OldProductCode
                                        FROM M_AGF_StackingNG AS A
                                        LEFT OUTER JOIN M_User AS B ON A.update_user_id = B.UserID "
                                        + whereString
                                        + $@"ORDER BY depo_code ASC ";

                    list = (await connection.QueryAsync<M_AGF_StackingNGModel>(selectString, new
                    {
                        depocode,
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

        private async Task<bool> EditStackingNG(M_AGF_StackingNGModel model)
        {
            bool isEdit = false;
            int affectedRows = -99;

            string db = UserDataList().DatabaseName;
            using (var connection = new SqlConnection(new GetConnectString(db).ConnectionString))
            {
                connection.Open();
                try
                {
                    string commandText = $@"
                                        UPDATE [M_AGF_StackingNG]
                                        SET 
                                            depo_code = @DepoCode,
                                            product_code =@ProductCode,
                                            update_date =@LastUpdateDateTime,
                                            update_user_id =@LastUpdateUserName
                                         WHERE
                                            depo_code = @OldDepoCode
                                            AND product_code = @OldProductCode ";

                    affectedRows = await connection.ExecuteAsync(commandText,
                        new
                        {
                            model.DepoCode,
                            model.ProductCode,
                            LastUpdateDateTime = DateTime.Now,
                            LastUpdateUserName = UserDataList().UserID,
                            model.OldDepoCode,
                            model.OldProductCode
                        }
                    );
                }
                catch (Exception ex)
                {
                    isEdit = false;
                }
            }
            if (affectedRows > 0)
                isEdit = true;
            else
                isEdit = false;
            return isEdit;
        }

        private async Task<bool> ImportStackingNG(M_AGF_StackingNGModel model)
        {
            bool isEdit = false;
            int affectedRows = -99;

            string db = UserDataList().DatabaseName;
            using (var connection = new SqlConnection(new GetConnectString(db).ConnectionString))
            {
                connection.Open();
                try
                {
                    string commandText = $@"
                                        INSERT INTO M_AGF_StackingNG(depo_code,product_code,create_date,create_user_id)
                                        VALUES(@DepoCode,@ProductCode,@CreateDateTime,@CreateUserName)";

                    //int DepoCode = Convert.ToInt32(model.DepoCode);
                    //int TruckBinCode = Convert.ToInt32(model.TruckBinCode);

                    affectedRows = await connection.ExecuteAsync(commandText,
                        new
                        {
                            model.DepoCode,
                            model.ProductCode,
                            CreateDateTime = DateTime.Now,
                            CreateUserName = UserDataList().UserID
                        }
                    );
                }
                catch (Exception ex)
                {
                    isEdit = false;
                }
            }
            if (affectedRows > 0)
                isEdit = true;
            else
                isEdit = false;
            return isEdit;
        }
    }
}

