using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace TryBeta.Models
{
    public class ParticipantEvaluation
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [JsonProperty("participant_id")]
        public int ParticipantId { get; set; }

        [Required]
        [JsonProperty("program_plan_id")]
        public int ProgramPlanId { get; set; }

        [Required]
        [JsonProperty("status_id")]
        public int StatusId { get; set; }

        [Required]
        [JsonProperty("score")]
        public int Score { get; set; }  // 評分，例如 1~5

        [Required]
        [JsonProperty("comment")]
        public string Comment { get; set; }

        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [JsonProperty("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        public virtual ParticipantInfoes Participant { get; set; }
        public virtual ProgramPlan Program { get; set; }

        ////導覽屬性
        //[ForeignKey("StatusId")]
        //public virtual ProgramPlanStatus Status { get; set; }
    }
}