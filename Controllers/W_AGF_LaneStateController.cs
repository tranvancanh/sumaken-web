using Dapper;
using DocumentFormat.OpenXml.Vml;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using stock_management_system.common;
using stock_management_system.Models;
using stock_management_system.Models.common;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using static stock_management_system.Models.common.Enums;
using static stock_management_system.Models.W_AGF_LaneStateModel;

namespace stock_management_system.Controllers
{
	/// <summary>
	/// AGF レーン状態
	/// </summary>
	[Authorize]
	public class W_AGF_LaneStateController : BaseController
	{
		/// <summary>
		/// レーン状態確認画面表示
		/// </summary>
		/// <param name="model"></param>
		/// <returns></returns>
		[HttpGet, HttpPost]
		public async Task<IActionResult> Index(W_AGF_LaneStateViewModel model)
		{
			W_AGF_LaneStateViewModel w_AGF_LaneStateViewModel = new W_AGF_LaneStateViewModel();
			try
			{
				// データベース名取得
				string db = UserDataList().DatabaseName;

				// 検索用リスト取得
				w_AGF_LaneStateViewModel.LaneNoList = w_AGF_LaneStateViewModel.LaneStateSearchListCreate(db, "laneno");
				w_AGF_LaneStateViewModel.StateList = w_AGF_LaneStateViewModel.LaneStateSearchListCreate(db, "state");
			
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
				return View(w_AGF_LaneStateViewModel);

			}
			catch(Exception ex)
			{
				ViewData["Error"] = ex;
				return View(w_AGF_LaneStateViewModel);
			}
		}

		/// <summary>
		/// レーン状態マスター取得
		/// </summary>
		/// <param name="searchModel">検索モデル</param>
		/// <returns></returns>
		public IActionResult SearchData(W_AGF_LaneStateViewModel searchModel)
		{
			var searchData = string.Empty;
			W_AGF_LaneStateViewModel w_AGF_LaneStateViewModel = new W_AGF_LaneStateViewModel();
			var laneStateSearchList =  GetLaneStateSearchList(searchModel);
			foreach (var item in laneStateSearchList)
			{
				// レーン状態が"0：荷物なし"でない場合は、使用中にチェックを入れる
				if (item.State != (int)Enums.LaneState.NoLuggage)
				{
					item.IsChecked = true;
				}

				//loking=1(使用不可)の場合、チェックボックスはdisabledにする
				searchData += $@"
					<tr>
						<td class='TdCheckbox'>
							<input type='checkbox' name='{item.IsChecked}' id='{item.LaneAddress}' {(item.IsChecked ? "checked " : "onchange = 'OnChangeCheckbox(this)'")}{(item.Loking == 1 ? "disabled " : "")}/>
						</td>
						<td class='LaneNo'>{item.LaneNo}</td>
						<td class='LaneAddress'>{item.LaneAddress}</td>
						<td class='StateName'>{item.StateName}</td>
                        <td class='LokingName' hidden>{item.LokingName}</td>
						<td class='Loking' hidden>{item.Loking}</td>
						<td class='LokingRadio' >
						<input  type='radio' class='text-center' name='{item.LaneAddress}' id='{item.LaneAddress}' value='0' {(item.Loking == 0 ? "checked" : "onchange = 'OnChangeRadio(this)'")}/>使用可
						<label>　</label>
						<input  type='radio' class='text-center' name='{item.LaneAddress}' id='{item.LaneAddress}' value='1' {(item.Loking == 1 ? "checked" : "onchange = 'OnChangeRadio(this)'")}/>使用不可
						</td>
                    </tr>
                ";
			}

			return Content(searchData);
		}


		/// <summary>
		/// レーン状態マスター更新
		/// </summary>
		/// <param name="laneStateSearchList"></param>
		/// <returns></returns>
		[HttpPost]
		public IActionResult Update([FromBody] List<W_AGF_LaneStateViewModel> laneStateSearchList)
		{
			try
			{
				bool affectedRows = EditLaneState(laneStateSearchList);

				if (!affectedRows)
				{
					return NotFound();
				}

				return Ok();
			}
			catch (Exception)
			{
				return BadRequest();
			}

		}

		public new ClaimsPrincipal User { get; }

		/// <summary>
		/// レーン状態マスターデータ取得
		/// </summary>
		/// <param name="model"></param>
		/// <returns></returns>
		private  List<W_AGF_LaneStateViewModel> GetLaneStateSearchList(W_AGF_LaneStateViewModel model)
		{
			var list = new List<W_AGF_LaneStateViewModel>();
			int depoCode = Convert.ToInt32(UserDataList().MainDepoCode);
			int state = 0;

			string whereString = "WHERE (1=1) ";
			if (depoCode > 0)
			{
				whereString += $@"AND (depo_code = @depocode) ";
			}
			if (model.SearchLaneNo != null && model.SearchLaneNo != "0")
			{
				whereString += $@"AND (lane_no LIKE @SearchLaneNo) ";
			}
			if (model.SearchLaneAddress != null)
			{
				whereString += $@"AND (lane_address LIKE @SearchLaneAddress) ";
			}

			// 初期状態はレーン状態"全て"で検索
			// レーン状態が"全て"でない場合はWHERE句追加
			if (model.SelectedStateName != null && model.SelectedStateName != "0")
			{
				if (model.SelectedStateName == "荷物なし") { state = (int)Enums.LaneState.NoLuggage; }
				if (model.SelectedStateName == "荷物あり") { state = (int)Enums.LaneState.WithLuggage; }
				if (model.SelectedStateName == "禁止") { state = (int)Enums.LaneState.Prohibit; }
				whereString += $@"AND state = @state ";
			}

			string db = UserDataList().DatabaseName;
			using (var connection = new SqlConnection(new GetConnectString(db).ConnectionString))
			{
				connection.Open();
				try
				{
					string selectString = $@"
                                        SELECT
											depo_code AS DepoCode
											,lane_no AS LaneNo
											,lane_address AS LaneAddress
											,state AS State
                                            ,locking AS Loking
											,CASE 
												WHEN state = 0 THEN '0：荷物なし'
												WHEN state = 1 THEN '1：荷物あり'
												WHEN state = 2 THEN '2：禁止'
												ELSE ''
　　　　　　　　　　　　　　　END AS StateName
                                             ,CASE 
												WHEN locking = 0 THEN '使用可'
												WHEN locking = 1 THEN '使用不可'
												ELSE ''
                                              END AS LokingName
                                        FROM W_AGF_LaneState "
										+ whereString
										+ $@"ORDER BY depo_code ASC";

					list = connection.QueryAsync<W_AGF_LaneStateViewModel>(selectString, new
					{
						DepoCode = depoCode,
                        SearchLaneNo = model.SearchLaneNo,
                        SearchLaneAddress = "%" + model.SearchLaneAddress + "%",
						state = state
					}).Result.ToList();
				}
				catch (Exception)
				{
					throw;
				}
			}
			return list;
		}

		/// <summary>
		/// レーン状態マスターデータ更新
		/// </summary>
		/// <param name="laneStateSearchList"></param>
		/// <returns></returns>
		private bool EditLaneState(List<W_AGF_LaneStateViewModel> laneStateSearchList)
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
					foreach (var laneState in laneStateSearchList)
					{
						var newState = 1;
						var newLocking = laneState.LokingRadio;
						if (laneState.IsChecked)
						{
							//荷物状態が "禁止" の場合は、禁止のまま
							if (laneState.StateName.Contains("2"))
							{
								newState = 2;
							}
						}
						else
							newState = 0;

						//set_dateの設定
						DateTime setDate = DateTime.Now;
						if (newState == 0)
						{
							//荷物状態が "荷物なし" の場合は、set_dateを初期値に設定
							setDate = new DateTime(1900, 01, 01);
						}

							string commandText = $@"
						UPDATE [W_AGF_LaneState]
						SET 
							state = @State
							,locking = @Loking
							,update_date = @LastUpdateDateTime
							,update_user_id = @LastUpdateUserName
							,set_date = '{setDate}'
						 WHERE
							depo_code = @DepoCode
							AND lane_no = @LaneNo
							AND lane_address = @LaneAddress";

							affectedRows = connection.ExecuteAsync(commandText,
								new
								{
									State = newState,
									Loking = newLocking,
									DepoCode = depocode,
									laneState.LaneNo,
									laneState.LaneAddress,
									LastUpdateDateTime = DateTime.Now,
									LastUpdateUserName = UserDataList().UserID
								}
							).Result;
						}
				}
				catch (Exception)
				{
					throw;
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

