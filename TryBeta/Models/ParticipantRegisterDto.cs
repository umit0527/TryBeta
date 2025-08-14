using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TryBeta.Models
{
    public class ParticipantRegisterDto
    {
        [Required(ErrorMessage = "請輸入姓名")]
        [MaxLength(100)]
        public string Name { get; set; }

        [Required(ErrorMessage = "請輸入帳號")]
        [RegularExpression(@"^[a-zA-Z0-9]+$", ErrorMessage = "帳號只能包含英文與數字")]
        [MaxLength(50, ErrorMessage = "帳號不可超過 50 個字元")]
        public string Account { get; set; }

        [Required(ErrorMessage = "請輸入Email")]
        [EmailAddress(ErrorMessage = "格式不正確，請重新輸入")]
        [MaxLength(100, ErrorMessage = "Email 不可超過 100 個字元")]
        public string Email { get; set; }

        [JsonProperty("password")]
        [Required(ErrorMessage = "請輸入密碼")]
        [RegularExpression(@"^[\x21-\x7E]+$", ErrorMessage = "密碼只能包含英文、數字與符號")]
        [MinLength(6, ErrorMessage = "密碼至少需要 6 個字元")]
        [MaxLength(100, ErrorMessage = "密碼最多不可超過 100 個字元")]
        public string Password { get; set; }
    }
}