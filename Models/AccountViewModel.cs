using Microsoft.AspNetCore.Mvc;
using System;
using System.ComponentModel.DataAnnotations;

namespace stock_management_system.Models
{
    public class AccountViewModel
    {
        [BindProperty]
        public InputModel Input { get; set; }

        public int IsLogin { get; set; }

        public AccountViewModel()
        {
            IsLogin = 0;
        }

        public class InputModel
        {
            public int WID { get; set; }

            [DataType(DataType.Password)]
            public string WPassword { get; set; }

            public int DepoCode { get; set; }

            [Required(ErrorMessage = "ユーザーコードは入力必須項目です")]
            [RegularExpression(@"[a-zA-Z0-9]+", ErrorMessage = "ユーザーコードは半角英数字のみ入力できます")]
            public string UserCode { get; set; }

            [Required(ErrorMessage = "パスワードは入力必須項目です")]
            [RegularExpression(@"[a-zA-Z0-9]+", ErrorMessage = "パスワードは半角英数字のみ入力できます")]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [Display(Name = "ログイン状態を保持")]
            public bool RememberMe { get; set; }

            public string CompanyName { get; set; }

            public string SecurityStamp { get; set; }

            public InputModel()
            {
                // ログイン情報の保持　初期値はFalse
                RememberMe = false;
            }

        }

    }
}
