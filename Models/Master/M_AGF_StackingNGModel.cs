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
    public class M_AGF_StackingNGModel : CommonModel
    {
        public int DepoCode { get; set; }

        /// <summary>
        /// 部品番号
        /// </summary>
        public string ProductCode { get; set; }
        public string LastUpdateDateTime { get; set; }
        public string LastUpdateUserName { get; set; }
        public string CreateDateTime { get; set; }
        public string CreateUserName { get; set; }

        //データ更新時に使用する変更前データ
        public string OldProductCode { get; set; }

        public static implicit operator M_AGF_StackingNGModel(List<M_AGF_StackingNGModel> v)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 検索結果のModel
        /// </summary>
        public class M_AGF_StackingNGViewModel : M_AGF_StackingNGModel
        {
			/// <summary>
			/// 部品番号検索用
			/// </summary>
			public string TextSearch { get; set; }

            public List<M_AGF_StackingNGModel> StackingNGSearchList { get; set; }

        }
        public static class AGF_StackingNG
        {
            public static async Task<List<M_AGF_StackingNGModel>> GetAGF_StackingNGList(string db)
            {
                var stackingNGList = new List<M_AGF_StackingNGModel>();

                // データベースから取得
                using (var connection = new SqlConnection(new GetConnectString(db).ConnectionString))
                {
                    connection.Open();
                    try
                    {
                        string selectString = string.Empty;
                        selectString = $@"
                                          SELECT *
                                          FROM [M_AGF_StackingNG]
                                          ORDER BY depo_code ASC
                                        ";
                        stackingNGList = (await connection.QueryAsync<M_AGF_StackingNGModel>(selectString)).ToList();

                    }
                    catch (Exception ex)
                    {
                        throw;
                    }
                }
                return stackingNGList;
            }

        }
    }
}

        