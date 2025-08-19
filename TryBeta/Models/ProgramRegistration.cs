using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using TryBeta.Models;

namespace TryBeta.Models
{
    public class ProgramRegistration
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey("ProgramPlan")]
        [JsonProperty("program_id")]
        public int ProgramId { get; set; }

        [Required]
        [ForeignKey("ParticipantInfoes")]
        [JsonProperty("user_id")]
        public int UserId { get; set; }

        [Required]
        [JsonProperty("status")]
        [StringLength(20)]
        public string Status { get; set; } // registered / canceled

        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // 導航屬性
        public virtual ProgramPlan ProgramPlan { get; set; }  //體驗計畫表
        public virtual ParticipantInfoes ParticipantInfoes { get; set; } // 體驗者表
    }
}