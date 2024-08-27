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
using static stock_management_system.Models.M_AGF_TruckBinLaneModel;
using System.IO;
using System.Runtime.Serialization;
using System.Security.Principal;
using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.Drawing.Charts;
using System.Linq.Expressions;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Logical;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Text.RegularExpressions;
using JwtBuilder;

namespace stock_management_system.Controllers
{
	/// <summary>
	/// AGF 運送会社とレーンマスタ
	/// </summary>
	[Authorize]
	public class M_AGF_TruckBinLaneController : BaseController
	{
		/// <summary>
		/// 運送会社とレーンマスタ画面表示
		/// </summary>
		/// <returns></returns>
		[HttpGet, HttpPost]
		public async Task<IActionResult> Index(int TruckBinCode = 0, string searchlist = null)
		{
			var model = new M_AGF_TruckBinLaneViewModel();
			model.TruckBinSearchList = await GetTruckBinSearchList(TruckBinCode);
			string db = UserDataList().DatabaseName;
			model.TruckBinNameList = model.TruckBinLaneListCreate(db, "truckbinsearch");

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
		/// 運送会社とレーンマスタ修正画面表示
		/// </summary>
		/// <returns></returns>
		[HttpGet]
		public IActionResult Edit(int truckBinCode, string laneNo, string lanegroupID, string oldLaneNo,string oldLaneGroupID)
		{
			M_AGF_TruckBinLaneModel model = new M_AGF_TruckBinLaneModel();
			string db = UserDataList().DatabaseName;
			model.TruckBinList = model.TruckBinLaneListCreate(db, "truckbin");
			model.LaneNoList = model.TruckBinLaneListCreate(db, "lane");
			return View(model);
		}

		/// <summary>
		/// 運送会社とレーンマスタ 修正画面内の保存・削除ボタン押下時処理
		/// </summary>
		/// <param name="model"></param>
		/// <returns></returns>
		[HttpPost]
		public async Task<IActionResult> Edit(M_AGF_TruckBinLaneModel model, string updata, string delete)
		{
            //グループ番号の入力値チェック用
            int i = 0;
            bool result = int.TryParse(model.LaneGroupID, out i);

            //空欄チェック
            if (string.IsNullOrEmpty(model.LaneGroupID))
			{
				TempData["Error"] = "グループ番号を入力してください";
			}
			//グループ番号の入力値チェック
			else if (result==false)
			{
				TempData["Error"] = "グループ番号は半角数字で入力してください";
			}
			else
			{
				//保存ボタン押下時
				if (updata != null)
				{
					//更新処理
					bool affectedRows = await EditTruckBin(model);

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
					//削除処理
					bool affectedRows = await DeleteTruckBin(model);

					if (affectedRows)
					{
						TempData["Message"] = "データを削除しました";
					}
					else
					{
						TempData["Error"] = "データ削除に失敗しました";
					}
				}
			}
			return RedirectToAction("Index");
		}

		public new ClaimsPrincipal User { get; }

		/// <summary>
		/// 運送会社とレーンマスタ追加画面表示
		/// </summary>
		/// <returns></returns>
		[HttpGet]
		public IActionResult Import()
		{
			M_AGF_TruckBinLaneModel model = new M_AGF_TruckBinLaneModel();
			string db = UserDataList().DatabaseName;
			model.TruckBinList = model.TruckBinLaneListCreate(db, "truckbin");
			model.LaneNoList = model.TruckBinLaneListCreate(db, "lane");
			return View(model);
		}

		/// <summary>
		/// 運送会社とレーンマスタ 追加画面内の保存ボタン押下時処理
		/// </summary>
		/// <param name="model"></param>
		/// <returns></returns>
		[HttpPost]
		public async Task<IActionResult> Import(M_AGF_TruckBinLaneModel model)
		{
			//グループ番号の入力値チェック用
            int i = 0;
            bool result = int.TryParse(model.LaneGroupID, out i);

            //空欄チェック
            if (model.TruckBinCode==0)
			{
				TempData["Error"] = "運送会社を選択してください";
            }
            else if (string.IsNullOrEmpty(model.LaneNo))
            {
                TempData["Error"] = "レーンを選択してください";
            }
			else if (string.IsNullOrEmpty(model.LaneGroupID))
			{
				TempData["Error"] = "グループ番号を入力してください";
			}
			//グループ番号の入力値チェック
             else if (result==false)
			{
				TempData["Error"] = "グループ番号は半角数字で入力してください";
            }
			else
			{
                bool affectedRows = await ImportTruckBin(model);

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
		/// 運送会社とレーンマスタデータ取得
		/// </summary>
		/// <returns></returns>
		private async Task<List<M_AGF_TruckBinLaneModel>> GetTruckBinSearchList(int truckBinCode)
		{
			var list = new List<M_AGF_TruckBinLaneModel>();
			int depocode = Convert.ToInt32(UserDataList().MainDepoCode);

			string whereString = "WHERE (1=1)";
			if (depocode > 0)
			{
				whereString += $@"AND (depo_code = @DepoCode) ";
			}
			if (truckBinCode != 0)
			{
				whereString += $@"AND(A.truck_bin_code LIKE @TruckBinCode)";
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
                                              ,A.truck_bin_code AS TruckBinCode
                                              ,C.truck_bin_name AS TruckBinName
                                              ,A.lane_no AS LaneNo
                                              ,A.lane_group_id AS LaneGroupID
                                              ,FORMAT(A.update_date,'yyyy/MM/dd HH:mm:ss') AS LastUpdateDateTime
                                             ,B.UserDisplayName AS LastUpdateUserName
                                              ,A.truck_bin_code AS OldTruckBinCode
                                              ,A.lane_no AS OldLaneNo
                                             ,A.lane_group_id AS OldLaneGroupID
                                        FROM M_AGF_TruckBinLane AS A
                                        LEFT OUTER JOIN M_User AS B ON A.update_user_id = B.UserID 
                                        LEFT OUTER JOIN M_AGF_TruckBin AS C ON A.truck_bin_code = C.truck_bin_code "
										+ whereString
										+ $@"ORDER BY depo_code ASC ";

					list = (await connection.QueryAsync<M_AGF_TruckBinLaneModel>(selectString, new
					{
						depocode,
						truckBinCode,
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
		/// 運送会社とレーンマスタ更新処理
		/// </summary>
		/// <param name="model"></param>
		/// <returns></returns>
		private async Task<bool> EditTruckBin(M_AGF_TruckBinLaneModel model)
		{
			bool isEdit = false;
			int affectedRows = -99;

			string db = UserDataList().DatabaseName;
			int depocode = Convert.ToInt32(UserDataList().MainDepoCode);
			string laneno = model.LaneNo;
			using (var connection = new SqlConnection(new GetConnectString(db).ConnectionString))
			{
				connection.Open();
				try
				{
					List<M_AGF_TruckBinLaneModel> list = new List<M_AGF_TruckBinLaneModel>();

					//重複チェック
					string check = $@"SELECT depo_code,truck_bin_code, lane_no, lane_group_id
                                       FROM M_AGF_TruckBinLane 
                                       WHERE  depo_code = @DepoCode AND truck_bin_code = @TruckBinCode AND lane_no = @LaneNo AND lane_group_id=@LaneGroupID";

					list = (await connection.QueryAsync<M_AGF_TruckBinLaneModel>(check, new
					{
						depocode,
						model.TruckBinCode,
						model.LaneNo,
                        LaneGroupID=Convert.ToInt32(model.LaneGroupID)
					})).ToList();

					string commandText = "";

					//重複がない場合
					if (list.Count == 0)
					{
						commandText = $@"
                                        UPDATE [M_AGF_TruckBinLane]
                                        SET 
                                            depo_code = @DepoCode,
											truck_bin_code = @TruckBinCode,
											lane_no = @LaneNo,
											lane_group_id = @LaneGroupID,
                                            update_date =@LastUpdateDateTime,
                                            update_user_id =@LastUpdateUserName
                                         WHERE
                                            depo_code = @DepoCode
										   AND truck_bin_code = @TruckBinCode
										   AND lane_no = @OldLaneNo
                                           AND lane_group_id=@OldLaneGroupID";
					}

					//重複がある場合
					else if (list.Count == 1)
					{
						//変更中のデータを削除
						commandText = $@"
                                        DELETE FROM M_AGF_TruckBinLane 
                                        WHERE depo_code = @DepoCode
                                        AND truck_bin_code = @TruckBinCode
                                        AND lane_no = @OldLaneNo
                                        AND lane_group_id=@OldLaneGroupID";

						affectedRows = await connection.ExecuteAsync(commandText,
					   new
					   {
						   DepoCode = depocode,
						   model.TruckBinCode,
						   model.LaneNo,
						   LaneGroupID = model.LaneGroupID.Trim(),
						   model.OldLaneNo,
						   model.OldLaneGroupID
					   }
					   );
						if (affectedRows > 0)
						{
							//重複元データを更新
							commandText = $@"
                                        UPDATE [M_AGF_TruckBinLane]
                                        SET 
                                            depo_code = @DepoCode,
											truck_bin_code = @TruckBinCode,
											lane_no = @LaneNo,
											lane_group_id = @LaneGroupID,
                                            update_date =@LastUpdateDateTime,
                                            update_user_id =@LastUpdateUserName
                                         WHERE
                                            depo_code = @DepoCode
										   AND truck_bin_code = @TruckBinCode
										  AND lane_no = @LaneNo
                                           AND lane_group_id=@LaneGroupID ";
						}
					}

					affectedRows = await connection.ExecuteAsync(commandText,
						new
						{
							DepoCode = depocode,
							model.TruckBinCode,
							model.LaneNo,
							LaneGroupID = model.LaneGroupID.Trim(),
							LastUpdateDateTime = DateTime.Now,
							LastUpdateUserName = UserDataList().UserID,
							CreateDateTime = DateTime.Now,
							CreateUserName = UserDataList().UserID,
							model.OldLaneNo,
							model.OldLaneGroupID
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
		/// 運送会社とレーンマスタ削除処理
		/// </summary>
		/// <param name="model"></param>
		/// <returns></returns>
		private async Task<bool> DeleteTruckBin(M_AGF_TruckBinLaneModel model)
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
                                        DELETE FROM M_AGF_TruckBinLane 
                                        WHERE depo_code = @DepoCode
                                        AND truck_bin_code = @TruckBinCode
                                        AND lane_no = @OldLaneNo
                                        AND lane_group_id = @LaneGroupID";

					affectedRows = await connection.ExecuteAsync(commandText,
						new
						{
							DepoCode = depocode,
							model.TruckBinCode,
							model.OldLaneNo,
							model.LaneGroupID
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
		/// 運送会社とレーンマスタ追加処理
		/// </summary>
		/// <param name="model"></param>
		/// <returns></returns>
		private async Task<bool> ImportTruckBin(M_AGF_TruckBinLaneModel model)
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
					MERGE INTO M_AGF_TruckBinLane AS A
					USING(SELECT
					@DepoCode AS depo_code,
					@TruckBinCode AS truck_bin_code,
					@LaneNo AS lane_no,
					@LaneGroupID AS lane_group_id) AS B
					ON A.depo_code = B.depo_code AND A.truck_bin_code = B.truck_bin_code AND A.lane_no = B.lane_no
					WHEN MATCHED THEN
					UPDATE SET
					A.truck_bin_code = @TruckBinCode,
					A.lane_no = @LaneNo,
					A.lane_group_id = @LaneGroupID,
					A.update_date = @LastUpdateDateTime,
					A.update_user_id = @LastUpdateUserName
					WHEN NOT MATCHED THEN
					INSERT(
					depo_code, truck_bin_code, lane_no, lane_group_id, create_date, create_user_id)
					VALUES(
					@DepoCode,
					@TruckBinCode,
					@LaneNo,
					@LaneGroupID,
					@CreateDateTime,
					@CreateUserName); ";


					affectedRows = await connection.ExecuteAsync(commandText,
						new
						{
							DepoCode = depocode,
							model.TruckBinCode,
							model.LaneNo,
							LaneGroupID = model.LaneGroupID.Trim(),
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

