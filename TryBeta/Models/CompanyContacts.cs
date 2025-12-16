using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using System.Globalization;

namespace TryBeta.Models
{
    public class CompanyContacts
    {
        /// <summary>
        /// CompanyId 當作主鍵和外鍵，一間公司只有一個"帳號的聯絡人"
        /// </summary>
        [Key, ForeignKey("CompanyInfo")]  //標在外鍵欄位上 → 參數填「導覽屬性名稱」
        [JsonProperty("company_id")]
        public int CompanyId { get; set; }
        // 導覽屬性，連結公司
        public virtual CompanyInfoes CompanyInfo { get; set; }

        //聯絡人姓名
        [Required]
        [JsonProperty("name")]
        [StringLength(100)]
        public string Name { get; set; }

        //聯絡人職稱
        [Required]
        [JsonProperty("job_title")]
        [StringLength(100)]
        public string JobTitle { get; set; }

        //聯絡人email
        [Required]
        [JsonProperty("email")]
        [StringLength(200)]
        public string Email { get; set; }

        //聯絡人電話
        [Required]
        [JsonProperty("phone")]
        [StringLength(50)]
        public string Phone { get; set; }

        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [JsonProperty("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.Now; 
    }
}