using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using stock_management_system.common;
using stock_management_system.Models;
using Dapper;

namespace stock_management_system.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {

        // ログインビュー
        public IActionResult Index(int paramID)
        {
            if (paramID == 1000)
            {
                ViewData["ForcedError"] = "異なるログインを検出したため自動ログアウトされました";
            }

            var accountModel = new AccountViewModel();
            return View(accountModel);
        }

        //public IActionResult login()
        //{
        //    return RedirectToAction("Index");
        //}

        ////TODO:AuthorizeにRoles指定時　AccessDeniedに勝手に飛ばされるよ　苦肉の策　直すべし
        //public IActionResult AccessDenied()
        //{
        //    return RedirectToAction("Index");
        //}

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult OtherLoginCheck(AccountViewModel account)
        {
            // 既存ログインの確認をする

            var loginCheck = LoginCheck(account);

            if (!loginCheck.result)
            {
                return Json(new { result = false, message = loginCheck.message });
            }
            else
            {
                var isLoginData = GetIsLoginData(loginCheck.company.DatabaseName, loginCheck.userInfoList[0].UserID);
                int isLogin = isLoginData.value;
                if (isLoginData.result == false)
                {
                    return Json(new { result = false, message = "ログインに失敗しました。" });
                }
                else
                {
                    if (isLogin == 1)
                    {
                        // ログアウトしていないログインがあるとき
                        return Json(new { result = true, value = 1 });
                    }
                    else
                    {
                        return Json(new { result = true, value = 2 });
                    }
                }
            }

        }

        private (bool result, string message, M_CompanyModel company, List<LoginUserModel> userInfoList)LoginCheck(AccountViewModel account)
        {
            bool result = false;
            string message = "";
            var userInfoList = new List<LoginUserModel>();
            var company = new M_CompanyModel();

            company = GetCompany();
            if (company == null || String.IsNullOrEmpty(company.DatabaseName))
            {
                message = "ログインに失敗しました";
                return (result, message, company, userInfoList);
            }

            userInfoList = CheckLoginUser(company.DatabaseName, account.Input.UserCode, account.Input.Password);
            if (userInfoList.Count != 1)
            {
               message = "ログインID or パスワードが違います";
                return (result, message, company, userInfoList);
            }

            result = true;
            return (result, message, company, userInfoList);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(AccountViewModel account)
        {
            try
            {
                var loginCheck = LoginCheck(account);

                if (loginCheck.result)
                {
                    var company = loginCheck.company;
                    var userInfo = loginCheck.userInfoList[0];
                    //var now = Util.DatetimeToFFF(DateTime.Now);
                    var now = DateTime.Now.ToString();

                    // サインインに必要なプリンシパルを作る
                    Claim[] claims = new[] {
                                new Claim(CustomClaimTypes.ClaimType_DatabaseName, company.DatabaseName.ToString()),
                                new Claim(CustomClaimTypes.ClaimType_CampanyID, company.CompanyID.ToString()),
                                new Claim(CustomClaimTypes.ClaimType_CampanyName, company.CompanyName.ToString()),
                                new Claim(CustomClaimTypes.ClaimType_MainDepoID, userInfo.MainDepoID.ToString()),
                                new Claim(CustomClaimTypes.ClaimType_MainDepoCode, userInfo.MainDepoCode.ToString()),
                                new Claim(CustomClaimTypes.ClaimType_MainDepoName, userInfo.MainDepoName.ToString()),
                                new Claim(CustomClaimTypes.ClaimType_UserID, userInfo.UserID.ToString()),
                                new Claim(CustomClaimTypes.ClaimType_UserCode, userInfo.UserCode.ToString()),
                                new Claim(CustomClaimTypes.ClaimType_UserName, userInfo.UserName.ToString()),
                                new Claim(CustomClaimTypes.ClaimType_Role, userInfo.Role.ToString()),
                                new Claim(CustomClaimTypes.ClaimType_TimeStamp, now)
                        };
                    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var principal = new ClaimsPrincipal(identity);

                    var authProperties = new AuthenticationProperties
                    {
                        AllowRefresh = false,
                        ExpiresUtc = DateTimeOffset.Now.AddDays(1),
                        IsPersistent = account.Input.RememberMe
                    };

                    // サインイン
                    await HttpContext.SignInAsync(
                      CookieAuthenticationDefaults.AuthenticationScheme,
                      principal,
                      authProperties
                      );

                    // 最終ログイン情報を更新する
                    var updateLoginDataResult = UpdateLastLoginData(company.DatabaseName.ToString(), userInfo.UserID, now);

                    if (updateLoginDataResult)
                    {
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        ViewData["Error"] = "ログインに失敗しました";
                        return View(account);
                    }
                }
                else
                {
                    ViewData["Error"] = loginCheck.message;
                    return View(account);
                }

            }
            catch(Exception ex)
            {
                ViewData["Error"] = "ログインに失敗しました";
                return View(account);
            }


        }

        private M_CompanyModel GetCompany()
        {
            var company = new M_CompanyModel();
            try
            {
                var loginWebPathBase = HttpContext.Request.PathBase.ToString().Substring(1);
                if (loginWebPathBase != "")
                {
                    var connectionString = new GetMasterConnectString().ConnectionString;
                    // connection
                    using (var connection = new SqlConnection(connectionString))
                    {
                        // open
                        connection.Open();

                        // commmand
                        var commandText = $@"
                                                    SELECT
                                                        CompanyID,
                                                        CompanyCode,
                                                        CompanyName,
                                                        DatabaseName
                                                    FROM M_Company
                                                    WHERE (1=1)
                                                        AND CompanyWebPath = @CompanyWebPath
                                                        ;";

                        company = connection.Query<M_CompanyModel>(commandText, new { CompanyWebPath = loginWebPathBase }).FirstOrDefault();
                    }
                }
            }
            catch(Exception ex)
            {
                throw;
            }

            return company;
        }

        private List<LoginUserModel> CheckLoginUser(string databaseName, string userCode, string password)
        {
            var userList = new List<LoginUserModel>();
            var connectionString = new GetConnectString(databaseName).ConnectionString;
            try
            {
                //connection
                using (var connection = new SqlConnection(connectionString))
                {
                    //open
                    connection.Open();

                    //commmand
                    var commandText = $@"
                                SELECT
                                    A.UserID,
                                    A.UserCode,
                                    A.UserDisplayName AS UserName,
                                    A.Role,
                                    A.MainDepoID,
                                    B.DepoCode AS MainDepoCode,
                                    B.DepoName AS MainDepoName
                                FROM M_User AS A
                                LEFT JOIN M_Depo AS B
                                 ON B.DepoID = A.MainDepoID
                                WHERE 1=1
                                    AND A.UserCode    = @UserCode
                                    AND A.Password = @Password
                                ;";

                    userList = connection.Query<LoginUserModel>(commandText, new { UserCode = userCode, Password = password }).ToList();

                }

            }
            catch (Exception ex)
            {
                throw;
            }

            return userList;
        }

        private (bool result, int value) GetIsLoginData(string databaseName, int userID)
        {
            var result = false;
            var value = 0;
            try
            {
                var ConnectionString = new GetConnectString(databaseName).ConnectionString;

                //connection
                using (var connection = new SqlConnection(ConnectionString))
                {
                    //open
                    connection.Open();

                    //commmand
                    var commandText = $@"
                                                SELECT
                                                    IsLogin
                                                FROM M_User
                                                WHERE (1=1)
                                                    AND UserID = @UserID
                                                ;";
                    var param = new
                    {
                        UserID = userID
                    };

                    var isLoginData = connection.Query<AccountViewModel>(commandText, param).ToList();
                    if (isLoginData.Count == 1)
                    {
                        result = true;
                        value = isLoginData[0].IsLogin;

                        return (result, value);
                    }

                }

                return (result, value);

            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private bool UpdateLastLoginData(string db, int userID, string now)
        {
            try
            {
                //var db = User.Claims.Where(x => x.Type == CustomClaimTypes.ClaimType_DatabaseName).First().Value;
                var connectionString = new GetConnectString(db).ConnectionString;

                // connection
                using (var connection = new SqlConnection(connectionString))
                {
                    // open
                    connection.Open();

                    // commmand
                    var commandText = $@"
                                                UPDATE M_User
                                                    SET
                                                        LastLoginDate = @LastLoginDate,
                                                        IsLogin = @IsLogin
                                                WHERE (1=1)
                                                    AND UserID = @UserID
                                                ;";
                    var param = new
                    {
                        UserID = userID,
                        LastLoginDate = now,
                        IsLogin = 1
                    };

                    var updateResult = connection.Execute(commandText, param);
                    if (updateResult == 1)
                    {
                        return true;
                    }
                }

                return false;

            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private bool UpdateIsLogout(string userID)
        {

            try
            {
                var db = User.Claims.Where(x => x.Type == CustomClaimTypes.ClaimType_DatabaseName).First().Value;
                var connectionString = new GetConnectString(db).ConnectionString;

                //connection
                using (var connection = new SqlConnection(connectionString))
                {
                    //open
                    connection.Open();

                    //commmand
                    var commandText = $@"
                                                UPDATE M_User
                                                    SET
                                                        IsLogin = @IsLogin
                                                WHERE (1=1)
                                                    AND UserID = @UserID
                                                ;";
                    var param = new
                    {
                        UserID = userID,
                        IsLogin = 0
                    };

                    var updateResult = connection.Execute(commandText, param);
                    if (updateResult == 1)
                    {
                        return true;
                    }
                }

                return false;

            }
            catch (Exception ex)
            {
                return false;
            }
        }

        // ログアウト
        public async Task<IActionResult> Logout(int? param)
        {
            var claimsList = User.Claims.ToList();
            
            if (claimsList.Count > 0)
            {
                string userID = User.Claims.Where(x => x.Type == CustomClaimTypes.ClaimType_UserID).First().Value;

                var logoutUpdate = UpdateIsLogout(userID);
            }

            // サインアウト
            // 認証クッキーをレスポンスから削除
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // 別ログインがあったため強制ログアウトの場合
            if (param == 1000)
            {
                return RedirectToAction("Index", new { paramID = 1000 });
            }

            // ログインビューにリダイレクト
            return RedirectToAction("Index");
        }

    }
}
