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
    public class M_CompanyModel
    { 
        [Key]
        public int CompanyID { get; set; }

        public string CompanyCode { get; set; }
        public string CompanyName { get; set; }
        public string DatabaseName { get; set; }

    }
}