using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TryBeta.Models
{
    public class ParticipantReview
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
        [JsonProperty("score")]
        public int Score { get; set; }  // 評分，例如 1~5

        [Required]
        [JsonProperty("comment")]
        public string Comment { get; set; }

        public virtual ParticipantInfoes Participant { get; set; }
        public virtual ProgramPlan Program { get; set; }
    }
}