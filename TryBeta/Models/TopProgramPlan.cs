using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace TryBeta.Models
{
    public class TopProgramPlan
    {
        [Key]
        [JsonProperty("Id")]
        public int Id { get; set; }

        [JsonProperty("program_plan_id")]
        public int ProgramPlanId { get; set; }

        [JsonProperty("score")]
        public decimal Score { get; set; }

        [JsonProperty("updated_at")]
        public DateTime UpdatedAt { get; set; }

        // 導航屬性：導航到 ProgramPlanId
        [ForeignKey("ProgramPlanId")]
        public virtual ProgramPlan ProgramPlan { get; set; }
    }
}