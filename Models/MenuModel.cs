using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using stock_management_system.Controllers;
using stock_management_system.common;
using System.Data.SqlClient;
using Dapper;

namespace stock_management_system.Models
{
    public class MenuModel:CommonModel
    {

        public List<Category> CategoryList()
        {
            var connectionString = new GetMasterConnectString().ConnectionString;
            using (var connection = new SqlConnection(connectionString))
            {
                //open-------------------------------------------------------------
                connection.Open();

                //SQLの準備
                var commandText = "";
                commandText = $@"SELECT
                                                 CategoryID
                                                ,CategoryName
                                            FROM M_WebMenuCategory
                                            ;";
                var selectCategoryList = connection.Query<Category>(commandText).ToList();
                return selectCategoryList;
            }
        }
    
        public CategoryMenuList GetCategoryMenuList(Category category)
        {
            var categoryMenuList = new CategoryMenuList();

            categoryMenuList.Category = category;
            categoryMenuList.MenuList = this.MenuList(category);

            return categoryMenuList;
        }

        public List<Menu> MenuList(Category category)
        {
            var menuList = new List<Menu>();

            if (Role != 0)
            {
                var userRole = Role;
                string userRoleName = "Role" + userRole;

                var categoryID = 0;
                string whereString = "";

                if (category == null)
                {
                    // カテゴリーの指定が無い場合は全Menuを返す
                }
                else
                {
                    categoryID = category.CategoryID;
                    whereString = $@"AND (A.CategoryID = @CategoryID)";
                }

                var ConnectionString = new GetMasterConnectString().ConnectionString;
                using (var connection = new SqlConnection(ConnectionString))
                {
                    //open-------------------------------------------------------------
                    connection.Open();

                    //SQLの準備
                    var commandText = "";
                    commandText = $@"SELECT
                                                    A.CategoryID,
                                                    A.MenuID,
                                                    A.MenuName,
                                                    B.Controller,
                                                    B.Action
                                                FROM M_WebMenu A
                                                LEFT OUTER JOIN M_WebMenuController B ON (A.CategoryID = B.CategoryID) AND (A.MenuID = B.MenuID)
                                                WHERE (1=1)
                                                    AND (A.CompanyID = @CompanyID)
                                                    AND (A.{userRoleName} = @RoleFlag)
                                                    {whereString}
                                                ORDER BY SortNumber ASC
                                                ;";

                    var param = new
                    {
                        CompanyID = CompanyID,
                        RoleFlag = 1,
                        CategoryID = categoryID
                    };
                    var selectMenuList = connection.Query<Menu>(commandText, param).ToList();

                    menuList = selectMenuList;
                }
            }

            return menuList;
        }

        /// <summary>
        /// 現在のコントローラー名から、カテゴリーコードとメニューコードを取得する
        /// </summary>
        /// <param name="controllerName"></param>
        /// <param name="actionName">指定しない場合はIndex</param>
        /// <returns></returns>
        public Menu GetCategoryAndMenuID(string controllerName, string actionName = "Index")
        {
            var menu = new Menu();
            if (!String.IsNullOrEmpty(controllerName))
            {
                var ConnectionString = new GetMasterConnectString().ConnectionString;
                using (var connection = new SqlConnection(ConnectionString))
                {
                    //open-------------------------------------------------------------
                    connection.Open();

                    //SQLの準備
                    var commandText = "";
                    commandText = $@"SELECT
                                                     A.CategoryID
                                                    ,A.MenuID
                                                  FROM M_WebMenuController AS A
                                                  WHERE (1=1)
                                                      AND (A.Controller = @Controller)
                                                      AND (A.Action = @Action)
                                            ;";

                    var param = new
                    {
                        Controller = controllerName,
                        Action = actionName
                    };

                    var selectMenu = connection.Query<Menu>(commandText, param).ToList().FirstOrDefault();
                    menu = selectMenu;

                }

            }

            return menu;

        }

    }

    public class CategoryMenuList
    {
        public Category Category { get; set; }
        public List<Menu> MenuList { get; set; }
    }

    public class Category
    {
        public int CategoryID { get; set; }
        public string CategoryName { get; set; }
    }

    public class Menu
    {
        public int CategoryID { get; set; }
        public int MenuID { get; set; }
        public string MenuName { get; set; }
        public string Controller { get; set; }
        public string Action { get; set; }
    }

}
