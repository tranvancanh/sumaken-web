using Dapper;
using DocumentFormat.OpenXml.Office.CustomUI;
using DocumentFormat.OpenXml.Office2010.ExcelAc;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Mvc.Rendering;
using stock_management_system.common;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using static stock_management_system.Models.D_ReceiveModel;
using X.PagedList;

namespace stock_management_system.Models
{
	public class D_ScanRecordModel : CommonModel
	{
		/// <summary>
		/// ハンディユーザー名称
		/// </summary>
		public int HandyUserID { get; set; }

		/// <summary>
		/// ハンディユーザー名称
		/// </summary>
		public string HandyUserName { get; set; }

		public int DepoID { get; set; }

		/// <summary>
		/// 読取日時
		/// </summary>
		public string ScanTime { get; set; }

		/// <summary>
		/// 読取日時
		/// </summary>
		public string ScanTimeEnd { get; set; }

		/// <summary>
		/// 送信日時
		/// </summary>
		public string CreateDate { get; set; }

		/// <summary>
		/// ハンディページ名称
		/// </summary>
		public int HandyPageID { get; set; }

		/// <summary>
		/// ハンディページ名称
		/// </summary>
		public string HandyPageName { get; set; }

		/// <summary>
		/// エラーコード
		/// </summary>
		public int HandyOperationClass { get; set; }

		/// <summary>
		/// エラーメッセージ
		/// </summary>
		public string HandyOperationMessage { get; set; }

		/// <summary>
		/// 棚番1
		/// </summary>
		public string ScanStoreAddress1 { get; set; }

		/// <summary>
		/// 棚番2
		/// </summary>
		public string ScanStoreAddress2 { get; set; }

		/// <summary>
		/// バーコード情報１
		/// AGF：荷取りST番号
		/// </summary>
		public string ScanString1 { get; set; }

		/// <summary>
		/// バーコード情報２
		/// AGF：出荷かんばんQR
		/// </summary>
		public string ScanString2 { get; set; }

		/// <summary>
		/// AGF：出荷レーン番号
		/// </summary>
		public string ScanString3 { get; set; }

		/// <summary>
		/// 入庫フラグ
		/// </summary>
		public bool StoreInFlag { get; set; }

		/// <summary>
		/// 出庫フラグ
		/// </summary>
		public bool StoreOutFlag { get; set; }

        public IPagedList<D_ScanRecordViewModel> ScanRecordList { get; set; }

        public Page Page { get; set; }

        

		public static implicit operator D_ScanRecordModel(List<D_ScanRecordModel> v)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// 検索結果Model
		/// </summary>
		public class D_ScanRecordViewModel : D_ScanRecordModel
		{
			/// <summary>
			/// 検索用ハンディページ
			/// </summary>
			public int SelectedHandyPageID { get; set; }

			/// <summary>
			/// 検索用読取日時(始)
			/// </summary>
			public string SelectedScanTime{ get; set; }

			/// <summary>
			/// 検索用検索用読取日時(終)
			/// </summary>
			public string SelectedScanTimeEnd { get; set; }
			

			/// <summary>
			/// 検索用レーン状態名
			/// </summary>
			public string SelectedCreateDate { get; set; }
			/// <summary>
			/// 検索用ハンディユーザー
			/// </summary>
			public int SelectedHandyUserID { get; set; }

			public IEnumerable<SelectListItem> HandyUserIDList { get; set; }

			public IEnumerable<SelectListItem> HandyPageIDList { get; set; }

		}

		/// <summary>
		/// ハンディ読取履歴データ取得
		/// </summary>
		/// <param name="db"></param>
		/// <param name="version"></param>
		/// <returns></returns>
		public List<SelectListItem> ScanRecordListCreate(string db, string version, int depoid)
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

					//検索用ハンディユーザーリスト
					if (version == "user")
					{
						//D_ScanRecordテーブルのハンディユーザー名称取得
						command.CommandText = $@"
						SELECT  DISTINCT A.HandyUserID,A.HandyUserName
						FROM M_HandyUser AS A
						WHERE A.DepoID = '{depoid}'
						ORDER BY A.HandyUserID";

						SqlDataReader reader = command.ExecuteReader();
						int Handy_User_ID = 0;
						string Handy_Page_Name = "";
						int i = 0;
						while (reader.Read() == true)
						{
							Handy_User_ID = (int)reader.GetValue(0);
							Handy_Page_Name = (string)reader.GetValue(1);
							i += 1;

							ImportList.Add(new SelectListItem { Value = Handy_User_ID.ToString(), Text = Handy_Page_Name });
						}

						if (i == 0)
						{
							//ハンディユーザーの設定がない場合の初期値設定
							ImportList.Add(new SelectListItem { Value = "0", Text = "選択ユーザーなし" });
						}
					}

					//検索用ハンディページ名称リスト
					else if (version == "page")
					{
						command.CommandText = $@"
						SELECT HandyPageID, HandyPageName
						FROM M_HandyPage
						WHERE DepoID = {depoid}
						ORDER BY HandyPageID";

						SqlDataReader reader = command.ExecuteReader();
						int Handy_Page_ID = 0;
						string Handy_Page_Name = "";
						int i = 0;
						while (reader.Read() == true)
						{
							Handy_Page_ID = (int)reader.GetValue(0);
							Handy_Page_Name = (string)reader.GetValue(1);
							i += 1;

							ImportList.Add(new SelectListItem { Value = Handy_Page_ID.ToString(), Text = Handy_Page_Name });
						}
						if (i == 0)
						{
							//ハンディユーザーの設定がない場合の初期値設定
							ImportList.Add(new SelectListItem { Value = "0", Text = "選択ユーザーなし" });
						}
					}
				}
			}
			catch (Exception ex)
			{
				ImportList.Add(new SelectListItem { Value = "0", Text = "選択ユーザーなし" });
			}
			return ImportList;
		}
	}



	public static class D_ScanRecord
	{
		public static async Task<List<D_ScanRecordModel>> GetScanRecordList(string db)
		{
			var motionList = new List<D_ScanRecordModel>();

			// データベースから取得
			using (var connection = new SqlConnection(new GetConnectString(db).ConnectionString))
			{
				connection.Open();
				try
				{
					string selectString = string.Empty;
					selectString = $@"
                                          SELECT *
                                          FROM [A_AGF_Motion]
                                          ORDER BY A_AGF_Motion_control_id ASC
                                        ";
					motionList = (await connection.QueryAsync<D_ScanRecordModel>(selectString)).ToList();

				}
				catch (Exception ex)
				{
					throw;
				}
			}
			return motionList;
		}

	}
}

