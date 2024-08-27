using System;
using System.ComponentModel.DataAnnotations;
using Dapper;
using stock_management_system.common;
using System.Data.SqlClient;
using System.Linq;
using System.Collections.Generic;
using stock_management_system.Models.common;

namespace stock_management_system.Models
{
    public class M_TemporaryStoreAddressModel
    {
        public class M_TemporaryStoreAddress
        {
            [Key]
            public int TemporaryStoreAddressID { get; set; }

            public int DepoID { get; set; }
            public string TemporaryStoreAddress1 { get; set; }
            public string TemporaryStoreAddress2 { get; set; }

        }

        public static List<M_TemporaryStoreAddress> GetTemporaryStoreAddress(string db, int depoID, string address1, string address2)
        {
            var temporaryStoreAddresses = new List<M_TemporaryStoreAddress>();

            string whereString = $@"AND DepoID = @DepoID ";

            if (!String.IsNullOrEmpty(address1))
            {
                whereString += $@"AND TemporaryStoreAddress1 = @TemporaryStoreAddress1 ";
            }
            if (!String.IsNullOrEmpty(address2))
            {
                whereString += $@"AND TemporaryStoreAddress2 = @TemporaryStoreAddress2 ";
            }

            try
            {
                var connectionString = new GetConnectString(db).ConnectionString;
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string commandText = $@"
                        SELECT
                               TemporaryStoreAddressID,
                               DepoID,
                               TemporaryStoreAddress1,
                               TemporaryStoreAddress2
                        FROM M_TemporaryStoreAddress
                        WHERE (1=1)
                            {whereString}
                        ";
                    var param = new
                    {
                        DepoID = depoID,
                        TemporaryStoreAddress1 = address1,
                        TemporaryStoreAddress2 = address2
                    };
                    temporaryStoreAddresses = connection.Query<M_TemporaryStoreAddress>(commandText, param).ToList();
                }
            }
            catch (Exception ex)
            {
                throw;
            }

            return temporaryStoreAddresses;
        }


    }

}