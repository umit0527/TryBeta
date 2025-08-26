using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TryBeta.Models
{
    public enum EducationStatus  //用enum，不建狀態表
    {
        Studying = 1,   // 在學中
        Graduated = 2,  // 已畢業
        DroppedOut = 3  // 肄業
    }
    public class ResumeEducation
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [Required]
        [JsonProperty("resume_id")]
        public int ResumeId { get; set; }

        [Required]
        [JsonProperty("school")]
        [MaxLength(200)]
        public string School { get; set; }

        [JsonProperty("major")]
        [MaxLength(100)]
        public string Major { get; set; }

        [JsonProperty("degree")]
        [MaxLength(50)]
        public string Degree { get; set; }  //程度

        [Required]
        [JsonProperty("start_date")]
        public DateTime StartDate { get; set; }

        [JsonProperty("end_date")]
        public DateTime? EndDate { get; set; } //在職或就學中可使用

        [Required]
        [JsonProperty("status")]
        public EducationStatus Status { get; set; } = EducationStatus.Studying;  // 在學/畢業/肄業

        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [JsonProperty("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // 導覽屬性，關聯到簡單履歷
        [ForeignKey("ResumeId")]
        public virtual SimpleResume Resume { get; set; }
    }
}