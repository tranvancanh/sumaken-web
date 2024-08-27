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
    public class M_AGF_TruckBinLaneModel : CommonModel
    {
        public int DepoCode { get; set; }

        /// <summary>
        /// 運送会社コード
        /// </summary>
        public int TruckBinCode { get; set; }
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
        /// レーン
        /// </summary>
        public string LaneNo { get; set; }

        /// <summary>
        /// グループ番号
        /// </summary>
        public string LaneGroupID { get; set; }
        public string LastUpdateDateTime { get; set; }
        public string LastUpdateUserName { get; set; }
        public string CreateDateTime { get; set; }
        public string CreateUserName { get; set; }

        //データ更新時に使用する変更前データ
        public string OldLaneNo { get; set; }
        public int OldLaneGroupID { get; set; }

        /// <summary>
        /// 運送会社検索用リスト
        /// </summary>
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

        /// <summary>
        /// レーン検索用リスト
        /// </summary>
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

        public static implicit operator M_AGF_TruckBinLaneModel(List<M_AGF_TruckBinLaneModel> v)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 検索結果のModel
        /// </summary>
        public class M_AGF_TruckBinLaneViewModel : M_AGF_TruckBinLaneModel
        {
            public List<M_AGF_TruckBinLaneModel> TruckBinSearchList { get; set; }
        }

        /// <summary>
        /// 検索用運送会社とレーンマスタデータ取得
        /// </summary>
        /// <param name="db"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        public List<SelectListItem> TruckBinLaneListCreate(string db,string version)
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

                    //追加画面の運送会社リスト
                    if (version == "truckbin")
                    {
                        command.CommandText = $@"
                                        SELECT
                                            A.truck_bin_code AS TruckBinCode
                                           ,A.truck_bin_name AS TruckBinName
                                        FROM M_AGF_TruckBin AS A
                                        ORDER BY truck_bin_code ASC ";

                        SqlDataReader reader = command.ExecuteReader();
                        int Truck_Bin_Code = 0;
                        string TruckBin_Name = "";
                        int i = 0;
                        while (reader.Read() == true)
                        {
                            Truck_Bin_Code = (int)reader.GetValue(0);
                            TruckBin_Name = (string)reader.GetValue(1);
                            i += 1;

                            ImportList.Add(new SelectListItem { Value = Truck_Bin_Code.ToString(), Text = Truck_Bin_Code.ToString() + "：" + TruckBin_Name });
                        }
                        if (i == 0)
                        {
                            //ハンディユーザーの設定がない場合の初期値設定
                            ImportList.Add(new SelectListItem { Value = "0", Text = "選択ユーザーなし" });
                        }
                    }

                    //追加修正画面のレーンリスト
                    if (version == "lane")
                    {
                        command.CommandText = $@"
                                        SELECT
                                         A.lane_no AS LaneNo
                                        FROM M_AGF_Lane AS A
                                        ORDER BY lane_no ASC ";

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
                    if (version == "truckbinsearch")
                    {
                        command.CommandText = $@"
                                        SELECT
                                            A.truck_bin_code AS TruckBinCode
                                           ,A.truck_bin_name AS TruckBinName
                                        FROM M_AGF_TruckBin AS A
                                        ORDER BY truck_bin_code ASC ";

                        SqlDataReader reader = command.ExecuteReader();
                        int Truck_Bin_Code = 0;
                        string TruckBin_Name = "";
                        int i = 0;
                        while (reader.Read() == true)
                        {
                            Truck_Bin_Code = (int)reader.GetValue(0);
                            TruckBin_Name = (string)reader.GetValue(1);
                            i += 1;

                            ImportList.Add(new SelectListItem { Value = Truck_Bin_Code.ToString(), Text = /*Truck_Bin_Code.ToString() + "：" +*/ TruckBin_Name });
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

        //使用していない？
        public List<SelectListItem> LaneNoListCreate(string db)
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
                    command.CommandText = $@"
                                        SELECT
                                            A.lane_no AS LaneNo
                                        FROM M_AGF_Lane AS A
                                        ORDER BY lane_no ASC ";
                    
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
            }
            catch (Exception ex)
            {
                ImportList.Add(new SelectListItem { Value = "0", Text = "選択ユーザーなし" });
            }
            return ImportList;
        }

        //使用していない？
        public List<SelectListItem> SetMethodListCreate(string db)
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
                    command.CommandText = $@"
                                        SELECT
	                                           A.set_method AS SetMethod
                                        FROM M_AGF_TruckBinLane AS A 
                                        ORDER BY set_method DESC";

                    SqlDataReader reader = command.ExecuteReader();
                    bool Set_Method_bool ;
                    int Set_Method = 0;
                    string ListText = "";
                    int i = 0;
                    while (reader.Read() == true)
                    {
                        Set_Method_bool = (bool)reader.GetValue(0);
                        if(Set_Method_bool == true)
                        {
                            Set_Method = 1;
                            ListText = "段積み";
                        }
                        else
                        {
                            Set_Method = 0;
                            ListText = "平積み";
                        }
                        i += 1;
                        ImportList.Add(new SelectListItem { Value = Set_Method.ToString(), Text = "【"+Set_Method.ToString()+"】"+ListText });
                    }
                    if (i == 0)
                    {
                        //ハンディユーザーの設定がない場合の初期値設定
                        ImportList.Add(new SelectListItem { Value = "2", Text = "選択ユーザーなし" });
                    }
                }
            }
            catch (Exception ex)
            {
                ImportList.Add(new SelectListItem { Value = "2", Text = "選択ユーザーなし" });
            }
            return ImportList;
        }
    }
        public static class AGF_TruckBin
        {
            public static async Task<List<M_AGF_TruckBinLaneModel>> GetAGF_TruckBinList(string db)
            {
                var truckBinList = new List<M_AGF_TruckBinLaneModel>();

                // データベースから取得
                using (var connection = new SqlConnection(new GetConnectString(db).ConnectionString))
                {
                    connection.Open();
                    try
                    {
                        string selectString = string.Empty;
                        selectString = $@"
                                          SELECT *
                                          FROM [M_AGF_TruckBinLane]
                                          ORDER BY truck_bin_code ASC
                                        ";
                        truckBinList = (await connection.QueryAsync<M_AGF_TruckBinLaneModel>(selectString)).ToList();

                    }
                    catch (Exception ex)
                    {
                        throw;
                    }
                }
                return truckBinList;
            }

        }
    }

        