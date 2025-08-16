using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace TryBeta.Models
{
    public class PlanUsage
    {
        [Key]
        [JsonProperty("id")]
        public int Id { get; set; }

        [Required]
        [ForeignKey("Company")]
        [JsonProperty("company_id")]
        public int CompanyId { get; set; }

        [Required]
        [ForeignKey("Plan")]
        [JsonProperty("plan_id")]
        public int PlanId { get; set; }

        [Required]
        [JsonProperty("start_date")]
        public DateTime StartDate { get; set; }

        [Required]
        [JsonProperty("end_date")]
        public DateTime? EndDate { get; set; }

        [Required]
        [JsonProperty("remaining_people")]
        public int RemainingPeople { get; set; }

        [Required]
        [JsonProperty("status")]
        [StringLength(20)]
        public string Status { get; set; } // active / expired / unused

        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [JsonProperty("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // 導航屬性
        public virtual CompanyInfoes Company { get; set; }
        public virtual Plan Plan { get; set; }
    }
}