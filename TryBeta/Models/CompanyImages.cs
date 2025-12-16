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
    public class CompanyImages
    {
        [Key]
        [JsonProperty("id")]
        public int Id { get; set; }

        // 外鍵，連結到 CompanyInfoes
        [ForeignKey("CompanyInfo")]  //標在外鍵欄位上 → 參數填「導覽屬性名稱」
        [JsonProperty("company_id")]
        public int CompanyId { get; set; }
        // 導覽屬性，方便 Entity Framework 進行關聯
        public virtual CompanyInfoes CompanyInfo { get; set; }

        /// <summary>
        /// 圖片類型：logo / cover / environment
        /// </summary>
        [Required]
        [StringLength(100)]
        [JsonProperty("type")]
        public string Type { get; set; }

        /// <summary>
        /// 圖片儲存路徑或網址
        /// </summary>
        [Required]
        [JsonProperty("img_path")]
        public string ImgPath { get; set; }

        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [JsonProperty("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}