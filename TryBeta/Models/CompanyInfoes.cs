using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace TryBeta.Models
{
    public class CompanyInfoes
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        //企業名稱
        [Required]
        [JsonProperty("name")]
        [MaxLength(100)]
        public string Name { get; set; }

        //企業產業：規定於 configuration 裡 
        [Required]
        [JsonProperty("industry_id")]
        public int IndustryId { get; set; }
        // 導覽屬性 (導航到 Industry)
        public virtual Industry Industry { get; set; }

        //統一編號
        [JsonProperty("tax_id_number")]
        public string TaxIdNum { get; set; }

        //企業地址
        [Required]
        [JsonProperty("address")]
        [MaxLength(200)]
        public string Address { get; set; }

        //企業官網
        [JsonProperty("website")]
        public string Website { get; set; }

        //企業簡介
        [Required]
        [JsonProperty("intro")]
        [MaxLength(1000)]
        public string Intro { get; set; }

        //企業規模人數
        [Required]
        [JsonProperty("scale_id")]
        public int ScaleId { get; set; }
        // 導覽屬性 (導航到 CompanyScales)
        [ForeignKey("ScaleId")]
        public virtual CompanyScales Scales { get; set; }

        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [JsonProperty("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // 外鍵 UserId
        [Required] 
        [JsonProperty("user_id")]
        public int UserId { get; set; }
        // 導覽屬性 (導航到 User)
        [ForeignKey("UserId")]
        public virtual Users User { get; set; }

        // 雙邊導覽屬性 (ProgramPlans 導航到 CompanyInfoes)
        /// <summary>
        /// 一個企業會有多個體驗計畫，用 Collection 
        /// 兩邊都寫導覽屬性，方便雙向存取
        /// </summary>
        public virtual ICollection<ProgramPlan> ProgramPlans { get; set; } = new List<ProgramPlan>();

        // 雙邊導覽屬性 (CompanyContacts 導航到 CompanyInfoes)
        public virtual CompanyContacts CompanyContacts { get; set; }

        // 雙邊導覽屬性 (CompanyImages 導航到 CompanyInfoes)
        public virtual ICollection<CompanyImages> CompanyImages { get; set; }
    }
}