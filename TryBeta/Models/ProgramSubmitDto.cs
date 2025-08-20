using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TryBeta.Models
{
    public class ProgramSubmitDto
    {
        [Required(ErrorMessage = "體驗計畫 ID 為必填")]
        [JsonProperty("program_plan_id")]
        public int ProgramPlanId { get; set; }

        [Required(ErrorMessage = "姓名為必填")]
        [StringLength(100, ErrorMessage = "姓名最多 100 字")]
        [JsonProperty("participant_name")]
        public string ParticipantName { get; set; }

        [Required(ErrorMessage = "Email 為必填")]
        [EmailAddress(ErrorMessage = "Email 格式不正確")]
        [JsonProperty("participant_email")]
        public string ParticipantEmail { get; set; }

        [Required(ErrorMessage = "電話為必填")]
        [Phone(ErrorMessage = "電話格式不正確")]
        [JsonProperty("participant_phone")]
        public string ParticipantPhone { get; set; }

        [Required(ErrorMessage = "申請人數為必填")]
        [Range(1, int.MaxValue, ErrorMessage = "申請人數必須大於 0")]
        [JsonProperty("participants_count")]
        public int ParticipantsCount { get; set; } = 1;

        [StringLength(500, ErrorMessage = "備註最多 500 字")]
        [JsonProperty("note")]
        public string Note { get; set; }

        [JsonProperty("agree_terms")]
        public bool AgreeTerms { get; set; }

        //履歷部分
        [JsonProperty("participant_id")]
        public int ParticipantId { get; set; }

        [JsonProperty("resume_type")]
        public string ResumeType { get; set; } // simple/existing

        [JsonProperty("resume_id")]
        public int ResumeId { get; set; }     // 根據類型填 SimpleResumeId or ExistingResumeId
        
        [JsonProperty("motivation_content")]
        [StringLength(500, ErrorMessage = "最多 500 字")]
        public string MotivationContent { get; set; }
    }
}