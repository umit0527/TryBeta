using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TryBeta.Models
{
    public class PlanUsageStatus
    {
        [Key]
        [JsonProperty("id")]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        [JsonProperty("title")]
        public string Title { get; set; }  // active / expired / unused / full

        // 導航屬性
        public virtual ICollection<PlanUsage> PlanUsages { get; set; }
    }
}