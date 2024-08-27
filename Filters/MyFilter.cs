using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using stock_management_system.Models;
using stock_management_system.Controllers;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using System.Linq;
using Microsoft.AspNetCore.Authentication;
using System;
using stock_management_system.common;

namespace stock_management_system.Filters
{
    public class MyFilter : IActionFilter
    {
        // IActionFilter参考：https://qiita.com/c-yan/items/c5d499cea7c93e0f5d70

        public static int CompanyID;
        public static int Role;

        public void OnActionExecuting(ActionExecutingContext context)
        {
            // アクセス制御
            // 会社と権限外の、表示されていないメニューでも、直接URLを叩けば閲覧できてしまう対応

            // これからアクセスしようとしているコントローラー名を取得
            var accessPath = context.RouteData.Values["controller"].ToString();
            var requestCon = accessPath.ToLower(); // 念のため小文字に統一しておく

            // 共通アクセス可能なControllerはアクセス制御から除外
            if (requestCon != "account" && requestCon != "home" && requestCon != "autocomplete")
            {
                // 参考：https://qiita.com/_meki/items/c82a132ccfef11ab3064
                CompanyID = int.Parse(context.HttpContext.User.FindFirstValue(CustomClaimTypes.ClaimType_CampanyID));
                Role = int.Parse(context.HttpContext.User.FindFirstValue(CustomClaimTypes.ClaimType_Role));

                var menuModel = new MenuModel();
                menuModel.CompanyID = CompanyID;
                menuModel.Role = Role;

                // 表示しているコントローラー名一覧を取得
                var viewMenuList = menuModel.MenuList(null);

                var accessOK = false;
                foreach (var viewMenu in viewMenuList)
                {
                    var viewCon = viewMenu.Controller.ToLower();

                    if (requestCon == viewCon)
                    {
                        accessOK = true;
                        break;
                    };
                }

                if (accessOK)
                {
//#if DEBUG

//#else
                    var controller = context.Controller as Controller;

                    // ログイン時に記憶したタイムスタンプが書き換わっていないか確認
                    var loginUserModel = new LoginUserModel();
                    var userID = Convert.ToInt32(context.HttpContext.User.Claims.Where(x => x.Type == CustomClaimTypes.ClaimType_UserID).First().Value);
                    var loginTimeStamp = context.HttpContext.User.Claims.Where(x => x.Type == CustomClaimTypes.ClaimType_TimeStamp).First().Value.ToString();

                    var returnLoginPage = true;
                    if (DateTime.TryParse(loginTimeStamp, out DateTime loginTime))
                    {
                        var db = controller.User.Claims.Where(x => x.Type == CustomClaimTypes.ClaimType_DatabaseName).First().Value;
                        var isLoginTimeValid = loginUserModel.IsLoginTimeValid(db, userID, loginTimeStamp);
                        if (isLoginTimeValid)
                        {
                            returnLoginPage = false;
                        }
                    }

                    // 書き換わっていたらログインページに戻す
                    if (returnLoginPage)
                    {
                        var viewResult = controller.RedirectToAction("Logout", "Account", new { param = 1000 });
                        context.Result = viewResult;
                        return;
                    }
                    
//#endif
                }
                else
                {
                    // 許可されていないページにアクセスしようとした場合
                    // 参考：https://atmarkit.itmedia.co.jp/ait/articles/0907/10/news109.html
                    var viewResult = new ViewResult
                    {
                        ViewName = "Error_AccessDenied"
                    };
                    // 参考:https://code-maze.com/action-filters-aspnetcore/
                    context.Result = viewResult;
                    return;
                }

            }

        }

        public void OnActionExecuted(ActionExecutedContext context)
        {

            var c = context.Controller as Controller;
            c.ViewData["test"] = null;

            // コントローラー名を取得
            var accessPath = context.RouteData.Values["controller"].ToString();
            var requestCon = accessPath.ToLower(); // 念のため小文字に統一しておく

            if (requestCon != "account")
            {
                // テスト環境だったら
                var db = c.User.Claims.Where(x => x.Type == CustomClaimTypes.ClaimType_DatabaseName).First().Value;
                var connectionString = new GetConnectString(db).ConnectionString;
                if (connectionString.Contains("0_test"))
                {
                    c.ViewData["test"] = "test";
                }
            }

            //// 画面のタイトルをセット
            //var commonModel = new CommonModel();
            //commonModel.WID = WID;
            //commonModel.Role = Role;
            //commonModel.ControllerName = c.RouteData.Values["controller"].ToString();
            //c.ViewData["Title"] = commonModel.ViewTitle;
        }
    }
}