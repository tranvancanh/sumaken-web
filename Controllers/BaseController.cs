using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using stock_management_system.common;
using stock_management_system.Models;
using JsonSerializer = System.Text.Json.JsonSerializer;
using Dapper;
using static stock_management_system.Models.M_ProductModel;

namespace stock_management_system.Controllers
{

    public class BaseController : Controller
    {

        // ログイン中ユーザー情報取得
        public LoginUserModel UserDataList()
        {

            var userData = User.Claims.ToList();
            var userModel = new LoginUserModel
            {
                DatabaseName = userData.Where(x => x.Type == CustomClaimTypes.ClaimType_DatabaseName).First().Value,
                CompanyID = Convert.ToInt32(userData.Where(x => x.Type == CustomClaimTypes.ClaimType_CampanyID).First().Value),
                MainDepoID = Convert.ToInt32(userData.Where(x => x.Type == CustomClaimTypes.ClaimType_MainDepoID).First().Value),
                MainDepoCode = userData.Where(x => x.Type == CustomClaimTypes.ClaimType_MainDepoCode).First().Value,
                MainDepoName = userData.Where(x => x.Type == CustomClaimTypes.ClaimType_MainDepoName).First().Value,
                UserID = Convert.ToInt32(userData.Where(x => x.Type == CustomClaimTypes.ClaimType_UserID).First().Value),
                UserCode = userData.Where(x => x.Type == CustomClaimTypes.ClaimType_UserCode).First().Value,
                UserName = userData.Where(x => x.Type == CustomClaimTypes.ClaimType_UserName).First().Value,
                Role = Convert.ToInt32(userData.Where(x => x.Type == CustomClaimTypes.ClaimType_Role).First().Value)
            };

            return userModel;

        }

    }

}