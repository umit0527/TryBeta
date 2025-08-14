using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TryBeta.Models
{
    public class ResumeSkill
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [Required]
        [JsonProperty("resume_id")]
        public int ResumeId { get; set; }

        [Required]
        [JsonProperty("skill_name")]
        [MaxLength(100)]
        public string SkillName { get; set; }

        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [JsonProperty("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // 導覽屬性，關聯到簡單履歷
        [ForeignKey("ResumeId")]
        public virtual SimpleResume Resume { get; set; }
    }
}