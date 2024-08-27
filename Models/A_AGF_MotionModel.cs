using Dapper;
using DocumentFormat.OpenXml.Office.CustomUI;
using Microsoft.AspNetCore.Mvc.Rendering;
using stock_management_system.common;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace stock_management_system.Models
{
    public class A_AGF_MotionModel : CommonModel
    {
		/// <summary>
		/// ログID
		/// </summary>
        public int ID { get; set; }

        public int DepoCode { get; set; }

		/// <summary>
		/// 動作日付
		/// </summary>
        public string MotionDate { get; set; }

		/// <summary>
		/// 動作日付(始)
		/// </summary>
        public string MotionDateStart { get; set; }

		/// <summary>
		/// 動作日付(終)
		/// </summary>
        public string MotionDateEnd { get; set; }

		/// <summary>
		/// 部品番号
		/// </summary>
        public string ProductCode { get; set; }

		/// <summary>
		/// 荷取りST
		/// </summary>
        public string LuggageStation { get; set; }

		/// <summary>
		/// レーン
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
			 new SelectListItem{ Value = "0", Text = "選択ユーザーなし"},
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

		/// <summary>
		/// 運送会社名称
		/// </summary>
        public string TruckBinName { get; set; }
		public List<SelectListItem> TruckBinNameSelectList;
		public List<SelectListItem> TruckBinNameList
		{
			set => TruckBinNameSelectList = value;
			get
			{
				if (TruckBinNameSelectList == null)
				{
					TruckBinNameSelectList = new List<SelectListItem>()
			{
			new SelectListItem{ Value = "0", Text = "選択ユーザーなし"},
			};
				}
				{
					return TruckBinNameSelectList;
				}
			}
		}

		/// <summary>
		/// 行先名称
		/// </summary>
		public string Destination { get; set; }

		/// <summary>
		/// 得意先コード
		/// </summary>
        public string CustomerCode { get; set; }

		/// <summary>
		/// 受入
		/// </summary>
		public string FinalDeliveryPlace { get; set; }

        public string LastUpdateDateTime { get; set; }
        public string LastUpdateUserName { get; set; }
        public string CreateDateTime { get; set; }
        public string CreateUserName { get; set; }

        public static implicit operator A_AGF_MotionModel(List<A_AGF_MotionModel> v)
        {
            throw new NotImplementedException();
        }

		/// <summary>
		/// 検索結果Model
		/// </summary>
        public class A_AGF_MotionViewModel : A_AGF_MotionModel
		{
            public List<A_AGF_MotionModel> AGFMotionSearchList { get; set; }
        }

		//public string endDay = DateTime.Now.Year.ToString() + "/" + DateTime.Now.Month.ToString("d2") + "/" + DateTime.Now.Day.ToString("d2");

		//public A_AGF_MotionModel()
		//{
		//	MotionDateStart = endDay;
		//	MotionDateEnd = endDay;
		//}

		/// <summary>
		/// AGF動作ログデータ取得
		/// </summary>
		/// <param name="db"></param>
		/// <param name="version"></param>
		/// <returns></returns>
		public List<SelectListItem> AGFMotionListCreate(string db, string version)
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

					//検索用レーンリスト
					if (version == "laneno")
					{
						command.CommandText = $@"
                    SELECT
                        A.lane_no AS LaneNo
                    FROM M_AGF_TruckBinLane AS A 
                    ORDER BY lane_no";

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
							//ハンディユーザーの設定がない場合の初期値設定
							ImportList.Add(new SelectListItem { Value = "0", Text = "選択ユーザーなし" });
						}
					}

					//検索用運送会社リスト
					else if (version == "truckbinname")
					{
						command.CommandText = $@"
					SELECT
						A.truck_bin_name AS TruckBinName
					FROM M_AGF_TruckBin AS A
					ORDER BY truck_bin_code ASC ";

						SqlDataReader reader = command.ExecuteReader();
						string Truck_Bin_Name = "";
						int i = 0;
						while (reader.Read() == true)
						{
							Truck_Bin_Name = (string)reader.GetValue(0);
							i += 1;

							ImportList.Add(new SelectListItem { Value = Truck_Bin_Name, Text = Truck_Bin_Name });
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
	


		public static class AGF_Motion
        {
            public static async Task<List<A_AGF_MotionModel>> GetAGF_MotionList(string db)
            {
                var motionList = new List<A_AGF_MotionModel>();

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
                    motionList = (await connection.QueryAsync<A_AGF_MotionModel>(selectString)).ToList();

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

        