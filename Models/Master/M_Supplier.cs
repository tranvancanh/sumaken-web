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
    public class M_SupplierModel
    { 
        [Key]
        public int SupplierID { get; set; }

        public string SupplierCode { get; set; }
        public string SupplierName { get; set; }
        public string SupplierNameKana { get; set; }

    }
}