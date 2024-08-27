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
using static stock_management_system.Models.D_ScanRecordModel;
using System.IO;
using System.Runtime.Serialization;
using System.Security.Principal;
using DocumentFormat.OpenXml.EMMA;
using static stock_management_system.Models.D_ReceiveModel;
using DocumentFormat.OpenXml.Wordprocessing;
using static stock_management_system.Models.W_AGF_LaneStateModel;
using stock_management_system.Models.common;
using System.Drawing;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Logical;
using Microsoft.AspNetCore.Http;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;
using X.PagedList;

namespace stock_management_system.Controllers
{
	/// <summary>
	/// ハンディ読取履歴
	/// </summary>
	[Authorize]
	public class D_ScanRecordController : BaseController
	{
		// 検索条件セッションキーの設定
		private const string SESSIONKEY_PageNo = "PageNo";
		private const string SESSIONKEY_DepoCode = "DepoID";
		private const string SESSIONKEY_HandyUserName = "HandyUserName";
		private const string SESSIONKEY_HandyPageID = "HandyPageID";
		private const string SESSIONKEY_HandyPageName = "HandyPageName";
		private const string SESSIONKEY_ScanTime = "ScanTime";
		private const string SESSIONKEY_ScanTimeEnd = "ScanTimeEnd";
		private const string SESSIONKEY_HandyOperationClass = "HandyOperationClass";
		private const string SESSIONKEY_HandyOperationMessage = "HandyOperationMessage";
		private const string SESSIONKEY_ScanStoreAddress1 = "ScanStoreAddress1";
		private const string SESSIONKEY_ScanStoreAddress2 = "ScanStoreAddress2";
		private const string SESSIONKEY_ScanString1 = "ScanString1";
		private const string SESSIONKEY_ScanString2 = "ScanString2";
		private const string SESSIONKEY_ScanString3 = "ScanString3";
		private const string SESSIONKEY_SelectedScanTime = "SelectedScanTime";
		private const string SESSIONKEY_SelectedScanTimeEnd = "SelectedScanTimeEnd";
		private const string SESSIONKEY_SelectedHandyPageID = "SelectedHandyPageID";
		private const string SESSIONKEY_SelectedHandyUserName = "SelectedHandyUserID";


		// ページサイズの設定
		private const int pageSize = 50;

		// 新規作成行数
		private const int createRowCount = 10;

		/// <summary>
		/// ハンディ履歴画面表示
		/// </summary>
		/// <returns></returns>
		[HttpGet, HttpPost]
		public IActionResult Index()
		{
			SessionReset();
			D_ScanRecordViewModel model = new D_ScanRecordViewModel();
			try
			{
				// データベース名取得
				string db = UserDataList().DatabaseName;
				//デポコード取得
				int depoCode = Convert.ToInt32(UserDataList().MainDepoCode);

				//初期値設定
				if (model.SelectedScanTime == null && model.SelectedScanTimeEnd == null)
				{
					//Javaでできなかったため応急処置
					model.SelectedScanTime = DateTime.Now.ToString("yyyy/MM/dd");
					model.SelectedScanTimeEnd = DateTime.Now.ToString("yyyy/MM/dd");
				}

				// 検索用リスト取得
				model.HandyUserIDList = model.ScanRecordListCreate(db, "user", depoCode);
				model.HandyPageIDList = model.ScanRecordListCreate(db, "page", depoCode);

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

			catch (Exception ex)
			{
				ViewData["Error"] = ex;
				return View(model);
			}
		}

		/// <summary>
		/// ハンディ読取履歴マスター取得
		/// </summary>
		/// <param name="searchModel">検索モデル</param>
		/// <returns></returns>
		public IActionResult SearchData(D_ScanRecordViewModel search)
		{
			var searchData = string.Empty;
			//D_ScanRecordViewModel d_ScanRecordViewModel = new D_ScanRecordViewModel();
			//var laneStateSearchList = GetScanRecordSearchList(search);

			int page = 1;

			// 検索条件をセッションにセット
			SessionSet(page, search);

			//return Content(searchData);
			return Search(search, page);
		}

		/// <summary>
		/// ページ遷移
		/// </summary>
		/// <param name="page"></param>
		/// <returns></returns>
		public IActionResult SearchByPageChange(int page)
		{
			// ページャー移動のとき
			// 前回の検索条件をセッションから取得し一覧表示

			var session = SessionGet();
			var search = session.SerchModel;

			return Search(search, page);
		}

		private IActionResult Search(D_ScanRecordViewModel search, int page)
		{
			try
			{
				var viewList = GetScanRecordSearchList(search);
				var _dataCount = viewList.Count;

				if (_dataCount > 0)
				{
					search.ScanRecordList = viewList.ToPagedList(page, pageSize);

					// ページ関連情報セット
					search.Page = new Page();
					var pageData = Util.ComPageNoGet(page, pageSize, _dataCount);
					search.Page.PageRowCount = _dataCount;
					search.Page.PageRowStartNo = pageData.PageRowStartNo;
					search.Page.PageRowEndNo = pageData.PageRowEndNo;
				}
				else
				{
					ViewData["Error"] = "対象データが存在しません。";
				}

			}
			catch (Exception ex)
			{
				ViewData["Error"] = "データ検索に失敗しました。";
			}

			// 検索条件をセッションにセット
			// データベース名取得
			string db = UserDataList().DatabaseName;
			//デポコード取得
			int depoCode = Convert.ToInt32(UserDataList().MainDepoCode);
			// 検索用リスト取得
			search.HandyUserIDList = search.ScanRecordListCreate(db, "user", depoCode);
			search.HandyPageIDList = search.ScanRecordListCreate(db, "page", depoCode);

			return View("Index", search);
			//return Content(searchData);
		}
		/// <summary>
		/// ハンディ読取履歴データ取得
		/// </summary>
		/// <returns></returns>
		private List<D_ScanRecordViewModel> GetScanRecordSearchList(D_ScanRecordViewModel model)
		{
			var list = new List<D_ScanRecordViewModel>();
			int depocode = Convert.ToInt32(UserDataList().MainDepoCode);
			DateTime datetime = DateTime.Now;

			string whereString = "WHERE (1=1)";
			if (depocode > 0)
			{
				whereString += $@"AND (A.DepoID = @DepoID) ";
			}
			//ハンディページ
			if (model.SelectedHandyPageID != 0)
			{
				whereString += $@"AND (A.HandyPageID = @SelectedHandyPageID) ";
			}
			//読取日時
			if (model.SelectedScanTime != null && model.SelectedScanTimeEnd != null)
			{
				//Dapperにて日付を指定する場合は、文字列で指定する？
				//whereString += $@"AND (A.ScanTime = @SelectedScanTime) ";
				whereString += $@"AND(Format(A.ScanTime, 'yyyy/MM/dd') BETWEEN '{model.SelectedScanTime}' AND  '{model.SelectedScanTimeEnd}')";
				
			}
			////送信日時
			//if (model.SelectedCreateDate != null)
			//{
			//	//Dapperにて日付を指定する場合は、文字列で指定する？
			//	//whereString += $@"AND (A.CreateDate = @SelectedCreateDate) ";
			//	whereString += $@"AND(Format(A.CreateDate, 'yyyy/MM/dd') BETWEEN '{model.SelectedCreateDate}' AND  '{model.SelectedCreateDate}')";
			//}
			//ハンディユーザー
			if (model.SelectedHandyUserID != 0)
			{
				whereString += $@"AND (A.HandyUserID = @SelectedHandyUserID) ";
			}

			string db = UserDataList().DatabaseName;
			using (var connection = new SqlConnection(new GetConnectString(db).ConnectionString))
			{
				connection.Open();
				try
				{
					//AGF指示ページ番号
					int agfPageid = (int)Enums.HandyPageID.AGFInstruction;
					//FROMテーブル名取得
					string fromtable = "";
					//スキャンレコードID取得
					string scanRecordID = "";
					//スキャン棚番1
					string ScanStoreAddress1 = " '' ";
					//スキャン棚番2
					string ScanStoreAddress2 = " '' ";
					//AGF出荷レーン
					string ScanString3 = " '' ";

					if (model.SelectedHandyPageID < agfPageid)
					{
						fromtable = "D_ScanRecord";
						scanRecordID = "ScanRecordID";
						ScanStoreAddress1 = "ScanStoreAddress1";
						ScanStoreAddress2 = "ScanStoreAddress2";
					}
					else
					{
						fromtable = "D_AGF_ScanRecord";
						scanRecordID="AGF_ScanRecordID";
						ScanString3 = "ScanString3";
					}

					string selectString = $@"
                                        SELECT 
											A.DepoID
											,C.HandyUserName
											,A.HandyPageID
											,B.HandyPageName
											,FORMAT(A.ScanTime,'yyyy/MM/dd HH:mm:ss') AS ScanTime
											,FORMAT(A.CreateDate,'yyyy/MM/dd HH:mm:ss') AS CreateDate
											,A.HandyOperationClass
											,A.HandyOperationMessage
											,{ScanStoreAddress1} AS ScanStoreAddress1
											,{ScanStoreAddress2} AS ScanStoreAddress2
											,A.ScanString1
											,A.ScanString2
											,{ScanString3} AS ScanString3
											 FROM {fromtable} AS A
											 LEFT OUTER JOIN M_HandyPage AS B ON A.HandyPageID=B.HandyPageID
											 LEFT OUTER JOIN M_HandyUser AS C ON A.HandyUserID=C.HandyUserID "
											+ whereString
											+ $@"ORDER BY A.ScanTime DESC";

					list = (connection.QueryAsync<D_ScanRecordViewModel>(selectString, new
					{
						DepoID = depocode,
						SelectedHandyPageID = model.SelectedHandyPageID,
						//SelectedScanTime = model.SelectedScanTime,
						//SelectedCreateDate = model.SelectedCreateDate,
						SelectedHandyUserID = model.SelectedHandyUserID

					})).Result.ToList();

				}
				catch (Exception ex)
				{
					throw;
				}
			}
			return list;
		}

		private void SessionSet(int? page, D_ScanRecordViewModel model)
		{
			HttpContext.Session.SetString(SESSIONKEY_PageNo, page == null ? "1" : page.ToString());
			HttpContext.Session.SetString(SESSIONKEY_DepoCode, model.DepoID.ToString());
			HttpContext.Session.SetString(SESSIONKEY_HandyUserName, model.HandyUserName == null ? "" : model.HandyUserName.ToString());
			HttpContext.Session.SetString(SESSIONKEY_HandyPageID, model.HandyPageID == 0 ? "" : model.HandyPageID.ToString());
			HttpContext.Session.SetString(SESSIONKEY_HandyPageName, model.HandyPageName == null ? "" : model.HandyPageName.ToString());
			HttpContext.Session.SetString(SESSIONKEY_ScanTime, model.ScanTime == null ? "" : model.ScanTime.ToString());
			HttpContext.Session.SetString(SESSIONKEY_ScanTime, model.ScanTimeEnd == null ? "" : model.ScanTimeEnd.ToString());
			HttpContext.Session.SetString(SESSIONKEY_HandyOperationClass, model.HandyOperationClass == 0 ? "" : model.HandyOperationClass.ToString());
			HttpContext.Session.SetString(SESSIONKEY_HandyOperationMessage, model.HandyOperationMessage == null ? "" : model.HandyOperationMessage.ToString());
			HttpContext.Session.SetString(SESSIONKEY_ScanStoreAddress1, model.ScanStoreAddress1 == null ? "" : model.ScanStoreAddress1.ToString());
			HttpContext.Session.SetString(SESSIONKEY_ScanStoreAddress2, model.ScanStoreAddress2 == null ? "" : model.ScanStoreAddress2.ToString());
			HttpContext.Session.SetString(SESSIONKEY_ScanString1, model.ScanString1 == null ? "" : model.ScanString1.ToString());
			HttpContext.Session.SetString(SESSIONKEY_ScanString2, model.ScanString2 == null ? "" : model.ScanString2.ToString());
			HttpContext.Session.SetString(SESSIONKEY_ScanString3, model.ScanString3 == null ? "" : model.ScanString3.ToString());
			HttpContext.Session.SetString(SESSIONKEY_SelectedScanTime, model.SelectedScanTime == null ? "" : model.SelectedScanTime.ToString());
			HttpContext.Session.SetString(SESSIONKEY_SelectedScanTimeEnd, model.SelectedScanTimeEnd == null ? "" : model.SelectedScanTimeEnd.ToString());
			HttpContext.Session.SetString(SESSIONKEY_SelectedHandyPageID, model.SelectedHandyPageID == 0 ? "" : model.SelectedHandyPageID.ToString());
			HttpContext.Session.SetString(SESSIONKEY_SelectedHandyUserName, model.SelectedHandyUserID == 0 ? "" : model.SelectedHandyUserID.ToString());


		}

		private (int PageNo, D_ScanRecordViewModel SerchModel) SessionGet()
		{
			var search = new D_ScanRecordViewModel();
			int.TryParse(HttpContext.Session.GetString(SESSIONKEY_PageNo), out int pageNo);
			int.TryParse(HttpContext.Session.GetString(SESSIONKEY_DepoCode), out int depoCode);
			int.TryParse(HttpContext.Session.GetString(SESSIONKEY_HandyPageID), out int handyPageID);
			int.TryParse(HttpContext.Session.GetString(SESSIONKEY_HandyOperationClass), out int handyOperationClass);

			search.DepoID = depoCode;
			search.HandyPageID = handyPageID;
			search.HandyOperationClass = handyOperationClass;

			var handyUserName = HttpContext.Session.GetString(SESSIONKEY_HandyUserName);
			search.HandyUserName = handyUserName == "" ? null : handyUserName;

			var handyPageName = HttpContext.Session.GetString(SESSIONKEY_HandyPageName);
			search.HandyPageName = handyPageName == "" ? null : handyPageName;

			var scanTime = HttpContext.Session.GetString(SESSIONKEY_ScanTime);
			search.ScanTime = scanTime == "" ? null : scanTime;

			var scanTimeEnd = HttpContext.Session.GetString(SESSIONKEY_ScanTimeEnd);
			search.ScanTimeEnd = scanTime == "" ? null : scanTime;

			var handyOperationMessage = HttpContext.Session.GetString(SESSIONKEY_HandyOperationMessage);
			search.HandyOperationMessage = handyOperationMessage == "" ? null : handyOperationMessage;

			var scanStoreAddress1 = HttpContext.Session.GetString(SESSIONKEY_ScanStoreAddress1);
			search.ScanStoreAddress1 = scanStoreAddress1 == "" ? null : scanStoreAddress1;

			var scanStoreAddress2 = HttpContext.Session.GetString(SESSIONKEY_ScanStoreAddress2);
			search.ScanStoreAddress2 = scanStoreAddress2 == "" ? null : scanStoreAddress2;

			var scanString1 = HttpContext.Session.GetString(SESSIONKEY_ScanString1);
			search.ScanString1 = scanString1 == "" ? null : scanString1;

			var scanString2 = HttpContext.Session.GetString(SESSIONKEY_ScanString2);
			search.ScanString2 = scanString2 == "" ? null : scanString2;

			var scanString3 = HttpContext.Session.GetString(SESSIONKEY_ScanString3);
			search.ScanString3 = scanString3 == "" ? null : scanString3;

			var selectedScanTime = HttpContext.Session.GetString(SESSIONKEY_SelectedScanTime);
			search.SelectedScanTime = selectedScanTime == "" ? null : selectedScanTime;

			var selectedScanTimeEnd = HttpContext.Session.GetString(SESSIONKEY_SelectedScanTimeEnd);
			search.SelectedScanTimeEnd = selectedScanTimeEnd == "" ? null : selectedScanTimeEnd;

			var selectedHandyPageID = HttpContext.Session.GetString(SESSIONKEY_SelectedHandyPageID);
			search.SelectedHandyPageID = selectedHandyPageID == "" ? 0 : int.Parse(selectedHandyPageID);

			var selectedHandyUserName = HttpContext.Session.GetString(SESSIONKEY_SelectedHandyUserName);
			search.SelectedHandyUserID = selectedHandyUserName == "" ? 0 : int.Parse(selectedHandyUserName);


			return (pageNo, search);

		}
		private void SessionReset()
		{
			HttpContext.Session.Remove(SESSIONKEY_PageNo);
			HttpContext.Session.Remove(SESSIONKEY_DepoCode);
			HttpContext.Session.Remove(SESSIONKEY_HandyUserName);
			HttpContext.Session.Remove(SESSIONKEY_HandyPageID);
			HttpContext.Session.Remove(SESSIONKEY_HandyPageName);
			HttpContext.Session.Remove(SESSIONKEY_ScanTime);
			HttpContext.Session.Remove(SESSIONKEY_ScanTimeEnd);
			HttpContext.Session.Remove(SESSIONKEY_HandyOperationClass);
			HttpContext.Session.Remove(SESSIONKEY_HandyOperationMessage);
			HttpContext.Session.Remove(SESSIONKEY_ScanStoreAddress1);
			HttpContext.Session.Remove(SESSIONKEY_ScanStoreAddress2);
			HttpContext.Session.Remove(SESSIONKEY_ScanString1);
			HttpContext.Session.Remove(SESSIONKEY_ScanString2);
			HttpContext.Session.Remove(SESSIONKEY_ScanString3);
			HttpContext.Session.Remove(SESSIONKEY_SelectedScanTime);
			HttpContext.Session.Remove(SESSIONKEY_SelectedScanTimeEnd);
			HttpContext.Session.Remove(SESSIONKEY_SelectedHandyPageID);
			HttpContext.Session.Remove(SESSIONKEY_SelectedHandyUserName);


		}

	}
}

