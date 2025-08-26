using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace TryBeta.Models
{
    public class Favorite
    {
        [Key]
        [JsonProperty("")]

        public int Id { get; set; }

        [Required]
        [JsonProperty("participant_id")]

        [ForeignKey("Participant")]
        public int ParticipantId { get; set; } // 體驗者ID

        [Required]
        [JsonProperty("program_plan_id")]
        [ForeignKey("ProgramPlan")]
        public int ProgramPlanId { get; set; } // 收藏的體驗計畫ID

        [JsonProperty("created_at")]

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [JsonProperty("canceled_at")]

        public DateTime CanceledAt { get; set; } = DateTime.Now;


        // 導覽屬性
        public virtual ParticipantInfoes Participant { get; set; }
        public virtual ProgramPlan ProgramPlan { get; set; }
    }
}