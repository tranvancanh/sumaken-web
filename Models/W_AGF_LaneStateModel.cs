using Dapper;
using DocumentFormat.OpenXml.Office.CustomUI;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using stock_management_system.common;
using stock_management_system.Models.common;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace stock_management_system.Models
{
	public class W_AGF_LaneStateModel : CommonModel
	{
		public int DepoCode { get; set; }
		
		/// <summary>
		/// レーン番号
		/// </summary>
		public string LaneNo { get; set; }
		
		public List<SelectListItem> LaneNoSelectList;
		public List<SelectListItem> LaneNoList
		{
			set => LaneNoSelectList = value;
			get
			{
				if (LaneNoSelectList == null)
				{
					LaneNoSelectList = new List<SelectListItem>()
					{
					new SelectListItem{ Value = "", Text = "選択ユーザーなし"},
					};
				}
				{
					return LaneNoSelectList;
				}
			}
		}

		/// <summary>
		/// レーン番地
		/// </summary>
		public string LaneAddress { get; set; }

		public int State { get; set; }
		public string StateName { get; set; }
		public int Loking { get; set; }
		public string LokingName { get; set; }
		public bool IsChecked { get; set; }

		public int LokingRadio { get; set; }

		//checkboxの状態を保持するためのリスト
		public W_AGF_LaneStateModel()
		{
			Items = new List<bool>();
		}
		public IList<bool> Items { get; set; }

		public List<SelectListItem> StateSelectList;
		public List<SelectListItem> StateList
		{
			set => StateSelectList = value;
			get
			{
				if (StateSelectList == null)
				{
					StateSelectList = new List<SelectListItem>()
					{
					new SelectListItem{ Value = "", Text = "選択ユーザーなし"},
					};
				}
				{
					return StateSelectList;
				}
			}
		}
		public string LastUpdateDateTime { get; set; }
		public string LastUpdateUserName { get; set; }
		public string CreateDateTime { get; set; }
		public string CreateUserName { get; set; }

		public static implicit operator W_AGF_LaneStateModel(List<W_AGF_LaneStateModel> v)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// 検索結果のModel
		/// </summary>
		public class W_AGF_LaneStateViewModel : W_AGF_LaneStateModel
		{
			/// <summary>
			/// 検索用レーン番地
			/// </summary>
			public string SearchLaneAddress { get; set; }

			/// <summary>
			/// 検索用レーン番号
			/// </summary>
			public string SearchLaneNo { get; set; }

            /// <summary>
            /// 検索用レーン状態名
            /// </summary>
            public string SelectedStateName { get; set; }
			
		}


		/// <summary>
		/// レーン状態データ検索結果リスト
		/// </summary>
		public List<W_AGF_LaneStateViewModel> LaneStateSearchList { get; set; }

		public List<W_AGF_LaneStateModel>LaneStateUpdateList { get; set; }

		/// <summary>
		/// 検索用レーン状態データ取得
		/// </summary>
		/// <param name="db"></param>
		/// <param name="version"></param>
		/// <returns></returns>
		public List<SelectListItem> LaneStateSearchListCreate(string db, string version)
		{
			List<SelectListItem> ImportList = new List<SelectListItem>();
			try
			{
				var ConnectionString = new GetConnectString(db).ConnectionString;
				using (var connection = new SqlConnection(ConnectionString))
				using (var command = connection.CreateCommand())
				{
					connection.Open();
					command.Parameters.Clear();
					if (version == "laneno")
					{
						command.CommandText = $@"
                                        SELECT
	                                           A.lane_no AS LaneNo
                                        FROM M_AGF_Lane AS A";

						SqlDataReader reader = command.ExecuteReader();
						string Lane_No = "";
						int i = 0;
						while (reader.Read() == true)
						{
							Lane_No = (string)reader.GetValue(0);
							i += 1;
							ImportList.Add(new SelectListItem { Value = Lane_No, Text = Lane_No });
						}
						if (i == 0)
						{
							// ハンディユーザーの設定がない場合の初期値設定
							ImportList.Add(new SelectListItem { Value = "", Text = "選択ユーザーなし" });
						}
					}

					if (version == "state")
					{
						// 検索用レーン状態リスト追加
						ImportList.Add(new SelectListItem { Value = "荷物なし", Text = (int)Enums.LaneState.NoLuggage + "：荷物なし", Selected = false });
						ImportList.Add(new SelectListItem { Value = "荷物あり", Text = (int)Enums.LaneState.WithLuggage + "：荷物あり", Selected = false });
						ImportList.Add(new SelectListItem { Value = "禁止", Text = (int)Enums.LaneState.Prohibit + "：禁止", Selected = false });
					}
				}
			}
			catch (Exception)
			{
				ImportList.Add(new SelectListItem { Value = "", Text = "選択ユーザーなし" });
			}
			return ImportList;
		}
		
		public static class AGF_LaneState
		{
			public static async Task<List<W_AGF_LaneStateModel>> GetAGF_LaneStateList(string db)
			{
				var lanestateList = new List<W_AGF_LaneStateModel>();

				// データベースから取得
				using (var connection = new SqlConnection(new GetConnectString(db).ConnectionString))
				{
					connection.Open();
					try
					{
						string selectString = string.Empty;
						selectString = $@"
                                          SELECT *
                                          FROM [W_AGF_LaneState]
                                          ORDER BY depo_code ASC
                                        ";
						lanestateList = (await connection.QueryAsync<W_AGF_LaneStateModel>(selectString)).ToList();

					}
					catch (Exception)
					{
						throw;
					}
				}
				return lanestateList;
			}
		}
	}
}


