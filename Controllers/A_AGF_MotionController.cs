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
using static stock_management_system.Models.A_AGF_MotionModel;
using System.IO;
using System.Runtime.Serialization;
using System.Security.Principal;
using DocumentFormat.OpenXml.EMMA;
using static stock_management_system.Models.D_ReceiveModel;
using DocumentFormat.OpenXml.Wordprocessing;

namespace stock_management_system.Controllers
{
	/// <summary>
	/// AGF 動作ログ
	/// </summary>
	[Authorize]
	public class A_AGF_MotionController : BaseController
	{
		/// <summary>
		/// AGF動作ログ画面表示
		/// </summary>
		/// <returns></returns>
		[HttpGet, HttpPost]
		public async Task<IActionResult> Index(string search, string clear, string MotionDateStart , string MotionDateEnd , string LaneNo = null, string TruckBinName = null, string Destination = null)
		{
			A_AGF_MotionViewModel model = new A_AGF_MotionViewModel();
			string db = UserDataList().DatabaseName;

			//初期値設定
			if (MotionDateStart == null && MotionDateEnd == null)
			{
				//Javaでできなかったため応急処置
				MotionDateStart = DateTime.Now.ToString("yyyy/MM/dd");
				MotionDateEnd = DateTime.Now.ToString("yyyy/MM/dd");
			}
			
			model.AGFMotionSearchList = await GetAGFMotionSearchList(MotionDateStart, MotionDateEnd, LaneNo, TruckBinName, Destination);
			if(model.AGFMotionSearchList.Count == 0)
			{
				TempData["Error"] = "対象データが存在しません。";
			}

			//検索クリアボタン押下時
			if (clear != null)
			{
				model.AGFMotionSearchList = await GetAGFMotionSearchList(MotionDateStart, MotionDateEnd, null, null, null);
			}
			model.LaneNoList = model.AGFMotionListCreate(db, "laneno");
			model.TruckBinNameList = model.AGFMotionListCreate(db, "truckbinname");

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
		/// AGF動作ログデータ取得
		/// </summary>
		/// <returns></returns>
		private async Task<List<A_AGF_MotionModel>> GetAGFMotionSearchList(string motiondateStart, string motiondateEnd, string laneNo, string truckbinName, string destination)
		{
			A_AGF_MotionModel model = new A_AGF_MotionModel();
			var list = new List<A_AGF_MotionModel>();
			int depocode = Convert.ToInt32(UserDataList().MainDepoCode);
			DateTime datetime = DateTime.Now;

			string whereString = "WHERE (1=1)";
			if (depocode > 0)
			{
				whereString += $@"AND (A.depo_code = @DepoCode) ";
			}

			//動作日付(始)と動作日付(終)の両方選択
			if (motiondateStart != null && motiondateEnd != null)
			{
				//日付を比較する
				if (motiondateStart.CompareTo(motiondateEnd) > 0)
				{
					TempData["Error"] = "動作日付(終)を動作日付(始)よりも後の日付にしてください";
					return list;
				}
				else
				{
					//Dapperにて日付を指定する場合は、文字列で指定する？
					//whereString += $@"AND (Format(A.motion_date,'yyyy/MM/dd') BETWEEN @MotionDateStart AND @MotionDateEnd)";
					whereString += $@"AND (Format(A.motion_date,'yyyy/MM/dd') BETWEEN '" + motiondateStart + "' AND '" + motiondateEnd + "')";
				}

			}
			if (laneNo != null)
			{
				whereString += $@"AND (A.lane_no = @LaneNo)";
			}
			if (truckbinName != null)
			{
				whereString += $@"AND (A.truck_bin_name = @TruckBinName)";
			}
			if (destination != null)
			{
				whereString += $@"AND (A.destination LIKE @Destination)";
			}

			string db = UserDataList().DatabaseName;
			using (var connection = new SqlConnection(new GetConnectString(db).ConnectionString))
			{
				connection.Open();
				try
				{
					string selectString = $@"
                                        SELECT
                                                A.A_AGF_Motion_control_id AS ID
	                                           ,A.depo_code AS DepoCode
                                               ,FORMAT(A.create_date,'yyyy/MM/dd HH:mm:ss') AS CreateDateTime
                                              ,A.luggage_station AS LuggageStation
											  ,A.lane_no AS Laneno
											  ,A.lane_address AS LaneAddress
											  ,A.truck_bin_name AS TruckBinName
											  ,A.customer_code AS CustomerCode
											  ,A.final_delivery_place AS FinalDeliveryPlace
											  ,A.destination AS Destination
                                              ,A.product_code AS ProductCode
                                        FROM A_AGF_Motion AS A "
										+ whereString
										+ $@"ORDER BY A_AGF_Motion_control_id DESC ";

					list = (await connection.QueryAsync<A_AGF_MotionModel>(selectString, new
					{
						DepoCode = depocode,
						//MotionDateStart=motiondateStart + " 00:00:00",
						//MotionDateEnd=motiondateEnd + " 23:59:59",
						LaneNo = laneNo,
						TruckBinName = truckbinName,
						Destination = "%" + destination + "%"

					})).ToList();
				}
				catch (Exception ex)
				{
					throw;
				}
			}
			return list;
		}
	}
}

