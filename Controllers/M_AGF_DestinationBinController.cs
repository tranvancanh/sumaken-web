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
using static stock_management_system.Models.M_AGF_DestinationBinModel;
using System.IO;
using System.Runtime.Serialization;
using System.Security.Principal;
using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.Drawing.Charts;
using CsvHelper;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.VisualBasic;
using System.Text.RegularExpressions;

namespace stock_management_system.Controllers
{
	/// <summary>
	/// AGF 行先と便マスタ
	/// </summary>
	[Authorize]
	public class M_AGF_DestinationBinController : BaseController
	{
		/// <summary>
		/// 行先と便マスタ画面表示
		/// </summary>
		/// <returns></returns>
		[HttpGet, HttpPost]
		public async Task<IActionResult> Index(string CustomerCodeSearch, string FinalDeliveryPlaceSearch, string DestinationSearch, string TruckBinName, string OldCustomerCode, string OldFinalDeliveryPlace, int OldTrucBinCode)
		{
			M_AGF_DestinationBinViewModel model = new M_AGF_DestinationBinViewModel();
			model.DestinationBinSearchList = await GetDestinationBinSearchList(CustomerCodeSearch, FinalDeliveryPlaceSearch, DestinationSearch, TruckBinName, OldCustomerCode, OldFinalDeliveryPlace, OldTrucBinCode);
			string db = UserDataList().DatabaseName;
			model.TruckBinNameList = model.TruckBinListCreate(db, "namesearch");

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
		/// 行先と便マスタ修正画面表示
		/// </summary>
		/// <param name="oldCustomerCode">得意先コード変更前データ</param>
		/// <param name="oldTruckBinCode">運送会社コード変更前データ</param>
		/// <returns></returns>
		[HttpGet]
		public IActionResult Edit(string customerCode, string destination, string finaldeliveryPlace, string truckbinCode, string oldCustomerCode, string oldTruckBinCode)
		{
			M_AGF_DestinationBinModel model = new M_AGF_DestinationBinModel();
			string db = UserDataList().DatabaseName;
			model.DestinationBinList = model.DestinationListCreate(db, "list");
			model.TruckBinList = model.TruckBinListCreate(db, "list");
			return View(model);
		}

		/// <summary>
		/// 行先と便マスタデータ修正画面内の保存・削除ボタン押下時処理
		/// </summary>
		/// <param name="model"></param>
		/// <returns></returns>
		[HttpPost]
		public async Task<IActionResult> Edit(M_AGF_DestinationBinModel model, string updata, string delete)
		{
			//保存ボタン押下時
			if (updata != null)
			{
				bool affectedRows = await EditDestinationBin(model);

				if (affectedRows)
				{
					TempData["Message"] = "データを更新しました";
				}
				else
				{
					TempData["Error"] = "データ更新に失敗しました";
				}
			}
			//削除ボタン押下時
			else if (delete != null)
			{
				bool affectedRows = await DeleteDestinationBin(model);

				if (affectedRows)
				{
					TempData["Message"] = "データを削除しました";
				}
				else
				{
					TempData["Error"] = "データ削除に失敗しました";
				}
			}
			return RedirectToAction("Index");
		}

		public new ClaimsPrincipal User { get; }

		/// <summary>
		/// 行先と便マスタ追加画面表示
		/// </summary>
		/// <returns></returns>
		[HttpGet]
		public IActionResult Import()
		{
			M_AGF_DestinationBinModel model = new M_AGF_DestinationBinModel();
			string db = UserDataList().DatabaseName;
			model.DestinationBinList = model.DestinationListCreate(db, "list");
			model.TruckBinList = model.TruckBinListCreate(db, "list");
			return View(model);
		}

		/// <summary>
		/// 行先と便マスタデータ追加画面の保存ボタン押下時処理
		/// </summary>
		/// <param name="model"></param>
		/// <returns></returns>
		[HttpPost]
		public async Task<IActionResult> Import(M_AGF_DestinationBinModel model)
		{
			//空欄チェック
			if (string.IsNullOrEmpty(model.CustomerCode))
			{
				TempData["Error"] = "得意先コードを入力してください";
			}
			else if (string.IsNullOrEmpty(model.FinalDeliveryPlace))
			{
				TempData["Error"] = "受入を入力してください";
			}
			else if (string.IsNullOrEmpty(model.Destination))
			{
				TempData["Error"] = "得意先名称を入力してください";
			}
			else if (model.TruckBinCode == 0)
			{
				TempData["Error"] = "運送会社を選択してください";
			}
			else
			{
				//半角英数字に変換
				model.CustomerCode = (Microsoft.VisualBasic.Strings.StrConv(model.CustomerCode, VbStrConv.Narrow, 0));
				model.FinalDeliveryPlace = (Microsoft.VisualBasic.Strings.StrConv(model.FinalDeliveryPlace, VbStrConv.Narrow, 0));

				//文字数チェック
				if (!Regex.IsMatch(model.CustomerCode, @"^[a-zA-Z0-9]{1,10}$"))
				{
					TempData["Error"] = "得意先コードは10桁以内の半角英数字で入力してください";
				}
				else if (!Regex.IsMatch(model.FinalDeliveryPlace, @"^[a-zA-Z0-9]{2}$"))
				{
					TempData["Error"] = "受入は2桁の半角英数字で入力してください";
				}
				else
				{
					bool affectedRows = await ImportDestinationBin(model);

					if (affectedRows)
					{
						TempData["Message"] = "データを登録しました";
					}
					else
					{
						TempData["Error"] = "データ登録に失敗しました";
					}
				}
			}

			return RedirectToAction("Index");
		}

		/// <summary>
		/// 行先と便マスタデータ取得
		/// </summary>
		/// <returns></returns>
		private async Task<List<M_AGF_DestinationBinModel>> GetDestinationBinSearchList(string customercodeSearch, string finaldeliveryplaceSearch, string destinationSearch, string truckbinName, string oldCustomerCode, string oldFinalDeliveryPlaceint, int oldTruckbinCode)
		{
			var list = new List<M_AGF_DestinationBinModel>();
			int depocode = Convert.ToInt32(UserDataList().MainDepoCode);

			string whereString = "WHERE (1=1)";
			if (depocode > 0)
			{
				whereString += $@"AND (A.depo_code = @DepoCode) ";
			}
			if (customercodeSearch != null)
			{
				whereString += $@"AND (A.customer_code LIKE @CustomerCodeSearch) ";
			}
			if (finaldeliveryplaceSearch != null)
			{
				whereString += $@"AND (A.final_delivery_place LIKE @FinalDeliveryPlaceSearch)";
			}
			if (destinationSearch != null)
			{
				whereString += $@"AND (C.destination LIKE @DestinationSearch)";
			}
			if (truckbinName != null)
			{
				whereString += $@"AND (D.truck_bin_name LIKE @TruckBinName)";
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
                                      ,A.customer_code AS CustomerCode
                                      ,A.final_delivery_place AS FinalDeliveryPlace
	                                  ,C.destination AS Destination
                                      ,A.truck_bin_code AS TruckBinCode
	                                  ,D.truck_bin_name AS TruckBinName
                                      ,FORMAT(A.update_date,'yyyy/MM/dd HH:mm:ss') AS LastUpdateDateTime
                                      ,B.UserDisplayName AS LastUpdateUserName
                                      , A.depo_code AS OldDepoCode
                                      ,A.customer_code AS OldCustomerCode
                                      ,A.truck_bin_code AS OldTruckBinCode
                                     FROM M_AGF_DestinationBin AS A
                                     LEFT OUTER JOIN M_User AS B ON A.update_user_id = B.UserID 
                                     INNER JOIN M_AGF_Destination AS C ON A.customer_code=C.customer_code AND A.final_delivery_place=C.final_delivery_place
                                     INNER JOIN M_AGF_TruckBin AS D ON A.truck_bin_code=D.truck_bin_code "
										+ whereString
										+ $@"ORDER BY depo_code ASC ";

					list = (await connection.QueryAsync<M_AGF_DestinationBinModel>(selectString, new
					{
						depocode,
						CustomerCodeSearch = "%" + customercodeSearch + "%",
						FinalDeliveryPlaceSearch = "%" + finaldeliveryplaceSearch + "%",
						DestinationSearch = "%" + destinationSearch + "%",
						truckbinName

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
		/// 行先と便マスタデータ修正処理
		/// </summary>
		/// <param name="model"></param>
		/// <returns></returns>
		private async Task<bool> EditDestinationBin(M_AGF_DestinationBinModel model)
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
					List<M_AGF_DestinationBinModel> list = new List<M_AGF_DestinationBinModel>();

					//重複チェック
					string check = $@"SELECT depo_code,customer_code, final_delivery_place, truck_bin_code
                   FROM M_AGF_DestinationBin
                   WHERE  depo_code = @DepoCode AND customer_code = @CustomerCode AND final_delivery_place = @FinalDeliveryPlace AND truck_bin_code = @TruckBinCode";

					list = (await connection.QueryAsync<M_AGF_DestinationBinModel>(check, new
					{
						depocode,
						model.CustomerCode,
						model.FinalDeliveryPlace,
						model.TruckBinCode
					})).ToList();

					string commandText = "";

					//重複データがない場合
					if (list.Count == 0)
					{
						commandText = $@"
                    UPDATE [M_AGF_DestinationBin]
                    SET 
                        depo_code = @DepoCode,
					    customer_code = @CustomerCode,
						final_delivery_place = @FinalDeliveryPlace,
						truck_bin_code = @TruckBinCode,
						update_date = @LastUpdateDateTime,
						update_user_id = @LastUpdateUserName
					WHERE depo_code = @DepoCode
                    AND  customer_code = @CustomerCode
					AND final_delivery_place = @FinalDeliveryPlace
					AND truck_bin_code = @OldTruckBinCode";
					}

					//重複データがある場合
					else if (list.Count == 1)
					{
						//変更中のデータを削除
						if (model.OldTruckBinCode != model.TruckBinCode.ToString())
						{
							commandText = $@"
                    DELETE FROM M_AGF_DestinationBin 
                    WHERE depo_code = @DepoCode
                    AND  customer_code = @CustomerCode
					AND final_delivery_place = @FinalDeliveryPlace
					AND truck_bin_code = @OldTruckBinCode";

							affectedRows = await connection.ExecuteAsync(commandText,
			   new
			   {
				   DepoCode = depocode,
				   model.CustomerCode,
				   model.OldFinalDeliveryPlace,
				   model.OldTruckBinCode

			   }
			   );
							//重複元データを更新
							if (affectedRows > 0)
							{
								commandText = $@"
                   UPDATE [M_AGF_DestinationBin]
                    SET 
                        depo_code = @DepoCode,
					    customer_code = @CustomerCode,
						final_delivery_place = @FinalDeliveryPlace,
						truck_bin_code = @TruckBinCode,
						update_date = @LastUpdateDateTime,
						update_user_id = @LastUpdateUserName
					WHERE depo_code = @DepoCode
                    AND  customer_code = @CustomerCode
					AND final_delivery_place = @FinalDeliveryPlace
					AND truck_bin_code = @TruckBinCode ";
							}
						}
						else
						{
							commandText = $@"
				   UPDATE [M_AGF_DestinationBin]
                    SET 
						update_date = @LastUpdateDateTime,
						update_user_id = @LastUpdateUserName
					WHERE depo_code = @DepoCode
                    AND  customer_code = @CustomerCode
					AND final_delivery_place = @FinalDeliveryPlace
					AND truck_bin_code = @TruckBinCode ";
						}
					}

					affectedRows = await connection.ExecuteAsync(commandText,
									new
									{
										DepoCode = depocode,
										model.CustomerCode,
										model.FinalDeliveryPlace,
										model.TruckBinCode,
										model.OldTruckBinCode,
										LastUpdateDateTime = DateTime.Now,
										LastUpdateUserName = UserDataList().UserID
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
		/// 行先と便マスタデータ削除処理
		/// </summary>
		/// <param name="model"></param>
		/// <returns></returns>
		private async Task<bool> DeleteDestinationBin(M_AGF_DestinationBinModel model)
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
                                       DELETE FROM M_AGF_DestinationBin 
                                       WHERE depo_code =  @DepoCode
                                       AND customer_code = @CustomerCode
									   AND final_delivery_place = @FinalDeliveryPlace
                                       AND truck_bin_code = @OldTruckBinCode ";

					affectedRows = await connection.ExecuteAsync(commandText,
						new
						{
							DepoCode = depocode,
							model.CustomerCode,
							model.FinalDeliveryPlace,
							model.OldTruckBinCode
						}
					);

					//行先マスタからも削除
					commandText = $@"
                                       DELETE FROM M_AGF_Destination
                                       WHERE customer_code = @CustomerCode
                                       AND final_delivery_place = @FinalDeliveryPlace
                                       AND destination = @Destination";

					affectedRows = await connection.ExecuteAsync(commandText,
						new
						{
							model.CustomerCode,
							model.FinalDeliveryPlace,
							model.Destination
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
		/// 行先と便マスタデータ追加処理
		/// </summary>
		/// <param name="model"></param>
		/// <returns></returns>
		private async Task<bool> ImportDestinationBin(M_AGF_DestinationBinModel model)
		{
			bool isEdit = false;
			int affectedRows = -99;

			string db = UserDataList().DatabaseName;
			int depocode = Convert.ToInt32(UserDataList().MainDepoCode);
			using (var connection = new SqlConnection(new GetConnectString(db).ConnectionString))
			{
				connection.Open();
				//行先マスタにもなければ追加
				try
				{
					string commandText = $@"
					MERGE INTO M_AGF_Destination AS A
					USING(SELECT
					@CustomerCode AS customer_code,
					@FinalDeliveryPlace AS final_delivery_place,
					@Destination AS destination) AS B
					ON A.customer_code = B.customer_code AND A.final_delivery_place = B.final_delivery_place
					WHEN MATCHED THEN
					UPDATE SET
					A.customer_code = @CustomerCode,
					A.final_delivery_place = @FinalDeliveryPlace,
					A.destination = @Destination,
					A.update_date = @LastUpdateDateTime,
					A.update_user_id = @LastUpdateUserName
					WHEN NOT MATCHED THEN
					INSERT(
					customer_code, final_delivery_place,destination,create_date, create_user_id)
					VALUES(
					@CustomerCode,
					@FinalDeliveryPlace,
					@Destination,
					@CreateDateTime,
					@CreateUserName);";

					affectedRows = await connection.ExecuteAsync(commandText,
						new
						{
							CustomerCode = model.CustomerCode.Trim(),
							FinalDeliveryPlace = model.FinalDeliveryPlace.Trim(),
							Destination = model.Destination.Trim(),
							LastUpdateDateTime = DateTime.Now,
							LastUpdateUserName = UserDataList().UserID,
							CreateDateTime = DateTime.Now,
							CreateUserName = UserDataList().UserID
						}
						);

					//行先と便マスタに追加
					commandText = $@"
					MERGE INTO M_AGF_DestinationBin AS A
					USING(SELECT
					@DepoCode AS depo_code,
					@CustomerCode AS customer_code,
					@FinalDeliveryPlace AS final_delivery_place,
					@TruckBinCode AS truck_bin_code) AS B
					ON A.depo_code = B.depo_code AND A.customer_code = B.customer_code AND A.final_delivery_place = B.final_delivery_place AND A.truck_bin_code = B.truck_bin_code
					WHEN MATCHED THEN
					UPDATE SET
					A.customer_code = @CustomerCode,
					A.final_delivery_place = @FinalDeliveryPlace,
					A.truck_bin_code = @TruckBinCode,
					A.update_date = @LastUpdateDateTime,
					A.update_user_id = @LastUpdateUserName
					WHEN NOT MATCHED THEN
					INSERT(
					depo_code, customer_code, final_delivery_place,truck_bin_code,create_date, create_user_id)
					VALUES(
					@DepoCode,
					@CustomerCode,
					@FinalDeliveryPlace,
					@TruckBinCode,
					@CreateDateTime,
					@CreateUserName); ";

					affectedRows = await connection.ExecuteAsync(commandText,
						new
						{
							DepoCode = depocode,
							CustomerCode = model.CustomerCode.Trim(),
							FinalDeliveryPlace = model.FinalDeliveryPlace.Trim(),
							model.TruckBinCode,
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