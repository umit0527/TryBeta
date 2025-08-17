using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using static Antlr.Runtime.Tree.TreeWizard;

namespace TryBeta.Models
{
    public class ProgramSubmit
    {
        [Key]
        [JsonProperty("id")]
        public int Id { get; set; }

        // 對應體驗計畫
        [Required]
        [ForeignKey("ProgramPlan")]
        [JsonProperty("program_plan_id")]
        public int ProgramPlanId { get; set; }
        public virtual ProgramPlan ProgramPlan { get; set; }

        // 體驗者
        [Required]
        [ForeignKey("Participant")]
        [JsonProperty("participant_id")]
        public int ParticipantId { get; set; }
        public virtual ParticipantInfoes Participant { get; set; }

        // 申請人數
        [Required]
        [JsonProperty("participants_count")]
        public int ParticipantsCount { get; set; }

        // 備註 (選填)
        [StringLength(500)]
        [JsonProperty("note")]
        public string Note { get; set; }

        // 申請日期
        [Required]
        [JsonProperty("submit_at")]
        public DateTime SubmitAt { get; set; } = DateTime.Now;

        // 申請狀態
        [Required]
        [JsonProperty("status_id")]
        public int StatusId { get; set; } = 1; // 1=待審核, 2=通過, 3=拒絕

        // 履歷類型: simple/existing
        [Required]
        [JsonProperty("resume_type")]
        [StringLength(50)]
        public string ResumeType { get; set; }

        // 如果選 existing → 存上傳履歷 Id
        [JsonProperty("existing_resume_id")]
        [ForeignKey("ExistingResume")]
        public int? ExistingResumeId { get; set; }
        public virtual ExistingResume ExistingResume { get; set; }

        // 如果選簡單履歷 → 存簡單履歷 Id
        [JsonProperty("simple_resume_id")]
        [ForeignKey("SimpleResume")]
        public int? SimpleResumeId { get; set; }
        public virtual SimpleResume SimpleResume { get; set; }

        // 申請動機
        [JsonProperty("motivation_content")]
        public string MotivationContent { get; set; }

        // 是否同意條款
        [JsonProperty("agree_terms")]
        public bool AgreeTerms { get; set; }


    }
}