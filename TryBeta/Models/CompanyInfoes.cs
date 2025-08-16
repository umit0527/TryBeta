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

        [Required]
        [JsonProperty("name")]
        [MaxLength(50)]
        public string Name { get; set; }

        [Required]
        [JsonProperty("industry_id")]
        public int IndustryId { get; set; }

        [JsonProperty("tax_id_number")]
        public string TaxIdNum { get; set; }

        [Required]
        [JsonProperty("address")]
        [MaxLength(200)]
        public string Address { get; set; }

        [JsonProperty("website")]
        public string Website { get; set; }

        [Required]
        [JsonProperty("intro")]
        [MaxLength(1000)]
        public string Intro { get; set; }

        [Required]
        [JsonProperty("scale_id")]
        public int ScaleId { get; set; }  //企業規模人數

        // 外鍵 UserId
        [Required]
        [JsonProperty("user_id")]
        public int UserId { get; set; }

        // 導覽屬性 (導航到 User)
        [ForeignKey("UserId")]
        public virtual Users User { get; set; }

        //// 導覽屬性 (導航到 CompanyContacts)
        public virtual CompanyContacts CompanyContacts { get; set; }

        // 導覽屬性 (導航到 CompanyImages)
        public virtual ICollection<CompanyImages> CompanyImages { get; set; }

        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [JsonProperty("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}