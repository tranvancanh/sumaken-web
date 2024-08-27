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

            [Required(ErrorMessage = "���[�U�[�R�[�h�͓��͕K�{���ڂł�")]
            [RegularExpression(@"[a-zA-Z0-9]+", ErrorMessage = "���[�U�[�R�[�h�͔��p�p�����̂ݓ��͂ł��܂�")]
            public string UserCode { get; set; }

            [Required(ErrorMessage = "�p�X���[�h�͓��͕K�{���ڂł�")]
            [RegularExpression(@"[a-zA-Z0-9]+", ErrorMessage = "�p�X���[�h�͔��p�p�����̂ݓ��͂ł��܂�")]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [Display(Name = "���O�C����Ԃ�ێ�")]
            public bool RememberMe { get; set; }

            public string CompanyName { get; set; }

            public string SecurityStamp { get; set; }

            public InputModel()
            {
                // ���O�C�����̕ێ��@�����l��False
                RememberMe = false;
            }

        }

    }
}
