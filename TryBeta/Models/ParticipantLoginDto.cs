using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TryBeta.Models
{
    public class ParticipantLoginDto
    {
        [Required(ErrorMessage = "請輸入帳號或 Email")]
        [RegularExpression(@"^[\x21-\x7E]+$", ErrorMessage = "帳號或 Email 只能包含英文、數字與符號")]
        public string Identifier { get; set; }  // 可為帳號或 Email


        [Required(ErrorMessage = "請輸入密碼")]
        [RegularExpression(@"^[\x21-\x7E]+$", ErrorMessage = "密碼只能包含英文、數字與符號")]
        [MinLength(6, ErrorMessage = "密碼至少需要 6 個字元")]
        [MaxLength(100, ErrorMessage = "密碼最多不可超過 100 個字元")]
        public string Password { get; set; }
    }
}