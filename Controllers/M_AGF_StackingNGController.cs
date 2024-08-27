using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.VisualBasic;
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
using System.Text.RegularExpressions;
using Org.BouncyCastle.Utilities;

namespace stock_management_system.Controllers
{
    /// <summary>
    /// AGF 段積み禁止マスタ
    /// </summary>
    [Authorize]
    public class M_AGF_StackingNGController : BaseController
    {
        /// <summary>
        /// 段積み禁止マスタ画面表示
        /// </summary>
        /// <returns></returns>
        [HttpGet, HttpPost]
        public async Task<IActionResult> Index(int DepoCode=0,string TextSearch = null,  string OldProductCode = null)
        {
            var model = new M_AGF_StackingNGViewModel();
            string db = UserDataList().DatabaseName;
            DepoCode = Convert.ToInt32(UserDataList().MainDepoCode);
            model.StackingNGSearchList = await GetStackingNGSearchList(DepoCode, TextSearch, OldProductCode);

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

		/// <summary>
		/// 段積み禁止マスタ削除画面表示
		/// </summary>
		/// <returns></returns>
		[HttpGet]
        public IActionResult Edit(int depoCode, string productCode, string oldProductCode)
        {
            M_AGF_StackingNGModel model = new M_AGF_StackingNGModel();
            return View(model);
        }

        /// <summary>
        /// 段積み禁止マスタ削除画面の削除ボタン押下時処理
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Edit(M_AGF_StackingNGModel model,string updata, string delete)

        {
                bool affectedRows = await DeleteStackingNG(model);

                if (affectedRows)
                {
                    TempData["Message"] = "データを削除しました";
                }
                else
                {
                    TempData["Error"] = "データ削除に失敗しました";
                }

            return RedirectToAction("Index");

        }
        public new ClaimsPrincipal User { get; }

        /// <summary>
        /// 段積み禁止マスタ追加画面表示
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Import()
        {
            M_AGF_StackingNGModel model = new M_AGF_StackingNGModel();
            return View(model);
        }

        /// <summary>
        /// 段積み禁止マスタ追加画面の保存ボタン押下時処理
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Import(M_AGF_StackingNGModel model)
        {
			//空欄チェック
			if (string.IsNullOrEmpty(model.ProductCode))
			{
				TempData["Error"] = "部品番号を入力してください";
			}
            //全角から半角に変換
			else if (Regex.IsMatch(model.ProductCode, @"[^\x01-\x7E]"))
            {
                model.ProductCode = (Microsoft.VisualBasic.Strings.StrConv(model.ProductCode, VbStrConv.Narrow, 0));
			}

			//最終入力チェック
			if (!Regex.IsMatch(model.ProductCode, @"^\d{5}-\d{5}-$"))
			{
				TempData["Error"] = "部品番号が正しい入力形式ではありません";
			}
			else
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
            }

            return RedirectToAction("Index");
        }

        /// <summary>
        /// 段積み禁止マスタデータ取得
        /// </summary>
        /// <returns></returns>
        private async Task<List<M_AGF_StackingNGModel>> GetStackingNGSearchList(int depocode, string textSearch,  string oldproductCode)
        {
            var list = new List<M_AGF_StackingNGModel>();
            string whereString = "WHERE (1=1)";
            if (depocode > 0)
            {
                whereString += $@"AND (depo_code = @DepoCode) ";
            }

            //検索条件
            if (textSearch != null)
            {
                whereString += $@"AND (product_code LIKE @ProductCodeSearch) ";
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
                                              ,A.product_code AS OldProductCode
                                        FROM M_AGF_StackingNG AS A
                                        LEFT OUTER JOIN M_User AS B ON A.update_user_id = B.UserID "
                                        + whereString
                                        + $@"ORDER BY depo_code ASC ";

                    list = (await connection.QueryAsync<M_AGF_StackingNGModel>(selectString, new
                    {
                        depocode,
                        ProductCodeSearch = textSearch + "%"
                    })).ToList();
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
            return list;
        }

        /// <summary>
        /// 段積み禁止マスタ削除処理
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private async Task<bool> DeleteStackingNG(M_AGF_StackingNGModel model)
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
                               DELETE FROM M_AGF_StackingNG
                               WHERE depo_code = @DepoCode
                               AND product_code = @ProductCode";

                    affectedRows = await connection.ExecuteAsync(commandText,
                        new
                        {
                            model.DepoCode,
                            model.ProductCode
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

        /// <summary>
        /// 段積み禁止マスタ追加処理
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private async Task<bool> ImportStackingNG(M_AGF_StackingNGModel model)
        {
            bool isEdit = false;
            int affectedRows = -99;

            string db = UserDataList().DatabaseName;
            int depocode = Convert.ToInt32(UserDataList().MainDepoCode);
            using (var connection = new SqlConnection(new GetConnectString(db).ConnectionString))
            {
                connection.Open();
                try
                {
                        string commandText = $@"
                        MERGE INTO M_AGF_StackingNG AS A
                        USING(SELECT
                        @DepoCode AS depo_code,
                        @ProductCode AS product_code) AS B
                        ON A.depo_code = B.depo_code AND A.product_code = B.product_code 
                        WHEN MATCHED THEN
                        UPDATE SET
                        A.product_code = @ProductCode,
                        A.update_date = @LastUpdateDateTime,
                        A.update_user_id =@LastUpdateUserName
                        WHEN NOT MATCHED THEN
                        INSERT(
                        depo_code,product_code,create_date,create_user_id )
                        VALUES(
                        @DepoCode,
                        @ProductCode,
                        @CreateDateTime,
                        @CreateUserName); ";

                    affectedRows = await connection.ExecuteAsync(commandText,
                        new
                        {
                            DepoCode=depocode,
                            model.ProductCode,
                            LastUpdateDateTime = DateTime.Now,
                            LastUpdateUserName = UserDataList().UserID,
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

