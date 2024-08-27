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
    public class M_DepoModel
    { 
        [Key]
        public int DepoID { get; set; }

        public string DepoCode { get; set; }
        public string DepoName { get; set; }
    }
}