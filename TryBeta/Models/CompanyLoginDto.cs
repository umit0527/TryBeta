using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TryBeta.Models
{
    public class CompanyLoginDto
    {
        [Required(ErrorMessage = "請輸入帳號或 Email")]
        public string Identifier { get; set; }  // 可為帳號或 Email


        [Required(ErrorMessage = "請輸入密碼")]
        public string Password { get; set; }
    }
}