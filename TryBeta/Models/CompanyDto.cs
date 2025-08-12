using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TryBeta.Models
{
    public class CompanInfoDto
    {
        public class CompanyRegisterDto
        {
            // 公司資料
            [JsonProperty("name")]
            [Required(ErrorMessage = "請輸入企業名稱")]
            public string Name { get; set; }

            [JsonProperty("industry_id")]
            public int IndustryId { get; set; }

            [JsonProperty("tax_id_number")]
            [RegularExpression(@"^\d{8}$", ErrorMessage = "統一編號必須是 8 碼的數字")]
            public string TaxIdNum { get; set; }

            [JsonProperty("address")]
            [Required(ErrorMessage = "請輸入地址")]
            public string Address { get; set; }

            [JsonProperty("website")]
            public string Website { get; set; }

            [JsonProperty("intro")]
            public string Intro { get; set; }

            [JsonProperty("scale_id")]
            public int ScaleId { get; set; }

            // 使用者帳號資料
            [JsonProperty("account")]
            [Required(ErrorMessage = "請輸入帳號")]
            [RegularExpression(@"^[a-zA-Z0-9]+$", ErrorMessage = "帳號只能包含英文與數字")]
            public string Account { get; set; }

            [JsonProperty]
            [EmailAddress(ErrorMessage = "格式不正確，請重新輸入")]
            public string Email { get; set; }

            [JsonProperty("password")]
            [Required(ErrorMessage = "請輸入密碼")]
            [RegularExpression(@"^[\x21-\x7E]+$", ErrorMessage = "密碼只能包含英文、數字與符號")]
            public string Password { get; set; } //暫存前端輸入進來的密碼，非雜湊過

            // 新增聯絡人列表
            public List<CompanyContactDto> CompanyContact { get; set; }

            //圖片資料（logo / cover / 環境）
            public List<CompanyImgDto> CompanyImg { get; set; }

        }
        public class CompanyContactDto
        {
            [JsonProperty("name")]
            [Required(ErrorMessage = "請輸入聯絡人名稱")]
            public string Name { get; set; }

            [JsonProperty("job_title")]
            [Required(ErrorMessage = "請輸入職稱")]
            public string JobTitle { get; set; }

            [JsonProperty("email")]
            [Required(ErrorMessage = "請輸入Emial")]
            [EmailAddress(ErrorMessage = "格式不正確，請重新輸入")]
            public string Email { get; set; }

            [JsonProperty("phone")]
            [Required(ErrorMessage = "請輸入電話號碼")]
            [RegularExpression(@"^\d+$", ErrorMessage = "請輸入數字")]

            public string Phone { get; set; }
        }
        public class CompanyImgDto
        {
            [JsonProperty("type")]
            public string Type { get; set; }

            [JsonProperty("img_path")]
            public string ImgPath { get; set; }
        }
    }
}