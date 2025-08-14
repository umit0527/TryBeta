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
        [Key]
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("company_id")]
        public int CompanyId { get; set; }

        [Required]
        [JsonProperty("name")]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [JsonProperty("job_title")]
        [StringLength(100)]
        public string JobTitle { get; set; }

        [Required]
        [JsonProperty("email")]
        [StringLength(200)]
        public string Email { get; set; }

        [Required]
        [JsonProperty("phone")]
        [StringLength(50)]
        public string Phone { get; set; }

        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [JsonProperty("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // 導覽屬性，連結公司
        [ForeignKey("CompanyId")]
        public virtual CompanyInfoes CompanyInfo { get; set; }
    }
}