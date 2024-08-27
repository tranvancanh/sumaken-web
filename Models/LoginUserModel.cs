using Microsoft.AspNetCore.Mvc;
using System;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using Dapper;
using stock_management_system.common;
using System.Data.SqlClient;
using System.Linq;

namespace stock_management_system.Models
{
    public class LoginUserModel
    {

        public string DatabaseName { get; set; }

        public int CompanyID { get; set; }
        public string CompanyName { get; set; }

        public int UserID { get; set; }
        public string UserCode { get; set; }
        public string UserName { get; set; }

        public int Role { get; set; }

        public int MainDepoID { get; set; }
        public string MainDepoCode { get; set; }
        public string MainDepoName { get; set; }

        private int _isLoginTimeValidCount;

        public bool IsLoginTimeValid(string db, int userID, string lastLoginDate)
        {
            try
            {
                var connectionString = new GetConnectString(db).ConnectionString;
                using (var connection = new SqlConnection(connectionString))
                {
                    //open
                    connection.Open();

                    //commmand
                    var commandText = $@"
                                                    SELECT
                                                        COUNT(*) AS _isLoginTimeValidCount
                                                    FROM M_User
                                                    WHERE (1=1)
                                                        AND UserID = @UserID
                                                        AND LastLoginDate = @LastLoginDate
                                                        ;";

                    var userList = connection.Query<LoginUserModel>(commandText, new { UserID = userID, LastLoginDate = lastLoginDate }).FirstOrDefault();

                    if (userList._isLoginTimeValidCount == 1)
                    {
                        return true;
                    }
                }

            }
            catch (Exception ex)
            {

            }

            return false;
        }

    }
}