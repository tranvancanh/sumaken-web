using Dapper;
using DocumentFormat.OpenXml.Office.CustomUI;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using stock_management_system.common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace stock_management_system.Models
{
	public class M_AGF_DestinationBinModel : CommonModel
	{
		public int DepoCode { get; set; }

		/// <summary>
		/// 得意先コード
		/// </summary>
		public string CustomerCode { get; set; }

		/// <summary>
		/// 受入
		/// </summary>
		public string FinalDeliveryPlace { get; set; }

		/// <summary>
		/// 運送会社名称
		/// </summary>
        public string Destination { get; set; }
		public List<SelectListItem> DestinationBinSelectList;
        public List<SelectListItem> DestinationBinList
		{
			set => DestinationBinSelectList = value;
			get
			{
				if (DestinationBinSelectList == null)
				{
					DestinationBinSelectList = new List<SelectListItem>()
					{
					new SelectListItem{ Value = "0", Text = "選択ユーザーなし"},
					};
				}
				{
					return DestinationBinSelectList;
				}
			}
		}
		public List<SelectListItem> CustomerCodeSelectList;
		public List<SelectListItem> CustomerCodeList
		{
			set => CustomerCodeSelectList = value;
			get
			{
				if (CustomerCodeSelectList == null)
				{
					CustomerCodeSelectList = new List<SelectListItem>()
					{
					new SelectListItem{ Value = "0", Text = "選択ユーザーなし"},
					};
				}
				{
					return CustomerCodeSelectList;
				}
			}
		}
		public List<SelectListItem> FinalDeliveryPlaceSelectList;
		public List<SelectListItem> FinalDeliveryPlaceList
		{
			set => FinalDeliveryPlaceSelectList = value;
			get
			{
				if (FinalDeliveryPlaceSelectList == null)
				{
					FinalDeliveryPlaceSelectList = new List<SelectListItem>()
					{
					new SelectListItem{ Value = "0", Text = "選択ユーザーなし"},
					};
				}
				{
					return FinalDeliveryPlaceSelectList;
				}
			}
		}
		public List<SelectListItem> DestinationSelectList;
		public List<SelectListItem> DestinationList
		{
			set => DestinationSelectList = value;
			get
			{
				if (DestinationSelectList == null)
				{
					DestinationSelectList = new List<SelectListItem>()
					{
					new SelectListItem{ Value = "0", Text = "選択ユーザーなし"},
					};
				}
				{
					return DestinationSelectList;
				}
			}
		}

		/// <summary>
		/// 運送会社コード
		/// </summary>
		public int TruckBinCode { get; set; }
		public string TruckBinName { get; set; }
		public List<SelectListItem> TruckBinSelectList;
		public List<SelectListItem> TruckBinList
		{
			set => TruckBinSelectList = value;
			get
			{
				if (TruckBinSelectList == null)
				{
					TruckBinSelectList = new List<SelectListItem>()
					{
					new SelectListItem{ Value = "0", Text = "選択ユーザーなし"},
					};
				}
				{
					return TruckBinSelectList;
				}
			}
		}
		public List<SelectListItem> TruckBinCodeSelectList;
		public List<SelectListItem> TruckBinCodeList
		{
			set => TruckBinCodeSelectList = value;
			get
			{
				if (TruckBinCodeSelectList == null)
				{
					TruckBinCodeSelectList = new List<SelectListItem>()
					{
					new SelectListItem{ Value = "0", Text = "選択ユーザーなし"},
					};
				}
				{
					return TruckBinCodeSelectList;
				}
			}
		}
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
		public string LastUpdateDateTime { get; set; }
		public string LastUpdateUserName { get; set; }
		public string CreateDateTime { get; set; }
		public string CreateUserName { get; set; }

		//データ更新時に使用する変更前データ
		public string OldCustomerCode { get; set; }
		public string OldFinalDeliveryPlace { get; set; }
		public string OldTruckBinCode { get; set; }

		public static implicit operator M_AGF_DestinationBinModel(List<M_AGF_DestinationBinModel> v)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// 検索結果のModel
		/// </summary>
		public class M_AGF_DestinationBinViewModel : M_AGF_DestinationBinModel
		{
			/// <summary>
			/// 検索用得意先コード
			/// </summary>
			public string CustomerCodeSearch { get; set; }

			/// <summary>
			/// 検索用受入
			/// </summary>
			public string FinalDeliveryPlaceSearch { get; set; }

			/// <summary>
			/// 検索用運送会社
			/// </summary>
			/// 
			public string DestinationSearch { get; set; }

			/// <summary>
			/// 運送会社検索結果リスト
			/// </summary>
			public List<M_AGF_DestinationBinModel> DestinationBinSearchList { get; set; }
		}

		/// <summary>
		/// 検索用得意先マスタデータ取得
		/// </summary>
		/// <param name="db"></param>
		/// <param name="version"></param>
		/// <returns></returns>
		public List<SelectListItem> DestinationListCreate(string db, string version)
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

					//編集・追加用リスト
					if (version == "list")
					{
						command.CommandText = $@"
                                        SELECT
	                                           A.customer_code AS CustomerCode
                                              ,A.final_delivery_place AS FinalDeliveryPlace
                                              ,A.destination AS Destination
                                        FROM M_AGF_Destination AS A
                                        ORDER BY customer_code ASC ";

						SqlDataReader reader = command.ExecuteReader();
						string Customer_Code = "";
						string Final_Delivery_Place = "";
						string Destination = "";
						int i = 0;
						while (reader.Read() == true)
						{
							Customer_Code = (string)reader.GetValue(0);
							Final_Delivery_Place = (string)reader.GetValue(1);
							Destination = (string)reader.GetValue(2);
							i += 1;

							ImportList.Add(new SelectListItem { Value = Customer_Code+":"+ Final_Delivery_Place, Text = Customer_Code + "：" + Final_Delivery_Place + "：" + Destination });
						}

						if (i == 0)
						{
							//ハンディユーザーの設定がない場合の初期値設定
							ImportList.Add(new SelectListItem { Value = "0", Text = "選択ユーザーなし" });
						}
					}

					//得意先コード検索用
					else if (version == "codesearch")
					{
						command.CommandText = $@"
                                        SELECT DISTINCT
	                                           A.customer_code AS CustomerCode
                                        FROM M_AGF_DestinationBin AS A
                                        ORDER BY customer_code ASC ";

						SqlDataReader reader = command.ExecuteReader();
						string Customer_Code = "";
						int i = 0;
						while (reader.Read() == true)
						{
							Customer_Code = (string)reader.GetValue(0);
							i += 1;

							ImportList.Add(new SelectListItem { Value = Customer_Code, Text = Customer_Code });
						}

						if (i == 0)
						{
							//ハンディユーザーの設定がない場合の初期値設定
							ImportList.Add(new SelectListItem { Value = "0", Text = "選択ユーザーなし" });
						}
					}

					//受入検索用
					else if (version == "placesearch")
					{
						command.CommandText = $@"
										SELECT DISTINCT
											  A.final_delivery_place AS FinalDeliveryPlace
										FROM M_AGF_DestinationBin AS A
										ORDER BY final_delivery_place ASC ";

						SqlDataReader reader = command.ExecuteReader();
						string Final_Delivery_Place = "";
						int i = 0;
						while (reader.Read() == true)
						{
							Final_Delivery_Place = (string)reader.GetValue(0);
							i += 1;

							ImportList.Add(new SelectListItem { Value = Final_Delivery_Place, Text = Final_Delivery_Place });
						}

						if (i == 0)
						{
							//ハンディユーザーの設定がない場合の初期値設定
							ImportList.Add(new SelectListItem { Value = "0", Text = "選択ユーザーなし" });
						}
					}

					//得意先名称検索用
					else if (version == "destinationsearch")
					{
						command.CommandText = $@"
										SELECT DISTINCT
                                        A.customer_code AS CustomerCode
                                       ,B.destination AS Destination
                                       FROM M_AGF_DestinationBin AS A
									   INNER JOIN M_AGF_Destination AS B ON A.customer_code=B.customer_code
								       ORDER BY A.customer_code ASC ";

						SqlDataReader reader = command.ExecuteReader();
						string Destination = "";
						int i = 0;
						while (reader.Read() == true)
						{
							Destination = (string)reader.GetValue(1);
							i += 1;

							ImportList.Add(new SelectListItem { Value = Destination, Text = Destination });
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

		/// <summary>
		/// 運送会社データ取得
		/// </summary>
		/// <param name="db"></param>
		/// <param name="version"></param>
		/// <returns></returns>
		public List<SelectListItem> TruckBinListCreate(string db,string version)
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

					//運送会社データ修正・追加用リスト
					if (version == "list")
					{
						command.CommandText = $@"
                                        SELECT
	                                           A.truck_bin_code AS TruckBinCode
                                              ,A.truck_bin_name AS TruckBinName
                                        FROM M_AGF_TruckBin AS A
                                        ORDER BY truck_bin_code ASC ";

						SqlDataReader reader = command.ExecuteReader();
						int Truck_Bin_Code = 0;
						string Truck_Bin_Name = "";
						int i = 0;
						while (reader.Read() == true)
						{
							Truck_Bin_Code = (int)reader.GetValue(0);
							Truck_Bin_Name = (string)reader.GetValue(1);
							i += 1;
							ImportList.Add(new SelectListItem { Value = Truck_Bin_Code.ToString(), Text = Truck_Bin_Code.ToString() + "：" + Truck_Bin_Name });
						}
						if (i == 0)
						{
							//ハンディユーザーの設定がない場合の初期値設定
							ImportList.Add(new SelectListItem { Value = "0", Text = "選択ユーザーなし" });
						}
					}

					//運送会社コード検索用
					else if (version == "codesearch")
					{
						command.CommandText = $@"
                                        SELECT DISTINCT
	                                           A.truck_bin_code AS TruckBinCode
                                        FROM M_AGF_DestinationBin AS A
                                        ORDER BY truck_bin_code ASC ";

						SqlDataReader reader = command.ExecuteReader();
						int Truck_Bin_Code = 0;
						int i = 0;
						while (reader.Read() == true)
						{
							Truck_Bin_Code = (int)reader.GetValue(0);
							i += 1;
							ImportList.Add(new SelectListItem { Value = Truck_Bin_Code.ToString(), Text = Truck_Bin_Code.ToString() });
						}
						if (i == 0)
						{
							//ハンディユーザーの設定がない場合の初期値設定
							ImportList.Add(new SelectListItem { Value = "0", Text = "選択ユーザーなし" });
						}
					}

					//運送会社名称検索用
					else if (version == "namesearch")
					{
						command.CommandText = $@"
                                        SELECT DISTINCT
                                        A.truck_bin_code AS TruckBinCode
	                                   ,B.truck_bin_name AS TruckBinName
                                       FROM M_AGF_DestinationBin AS A
                                       INNER JOIN M_AGF_TruckBin AS B ON A.truck_bin_code=B.truck_bin_code
                                       ORDER BY A.truck_bin_code ASC ";

						SqlDataReader reader = command.ExecuteReader();
						string Truck_Bin_Name = "";
						int i = 0;
						while (reader.Read() == true)
						{
							Truck_Bin_Name = (string)reader.GetValue(1);
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
	public static class AGF_DestinationBin
	{
		public static async Task<List<M_AGF_DestinationBinModel>> GetAGF_DestinationBinList(string db)
		{
			var systemSettingList = new List<M_AGF_DestinationBinModel>();

			// データベースから取得
			using (var connection = new SqlConnection(new GetConnectString(db).ConnectionString))
			{
				connection.Open();
				try
				{
					string selectString = string.Empty;
					selectString = $@"
                                          SELECT *
                                          FROM [M_AGF_DestinationBin]
                                          ORDER BY depo_code ASC
                                        ";
					systemSettingList = (await connection.QueryAsync<M_AGF_DestinationBinModel>(selectString)).ToList();

				}
				catch (Exception ex)
				{
					throw;
				}
			}
			return systemSettingList;
		}

	}
}

