using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TryBeta.Models
{
    public static class ReviewerIds
    {
        public const int AI = 0; // AI判斷時填身分的ID用
    }
    public enum ReviewTypeEnum
    {
        System = 1, //系統
        Manual = 2  //人工
    }
    public class EvaluationReview
    {
        [Key]
        [JsonProperty("id")]
        public int Id { get; set; }

        [Required]
        [JsonProperty("evaluation_id")]
        [ForeignKey("Evaluation")]
        public int EvaluationId { get; set; }
        public virtual ParticipantEvaluation Evaluation { get; set; }

        // 審核時間
        [Required]
        [JsonProperty("reviewed_at")]
        public DateTime ReviewedAt { get; set; } = DateTime.Now;

        // 審核人員
        [Required]
        [JsonProperty("reviewer_id")]
        [ForeignKey("Reviewer")]
        public int ReviewerId { get; set; }
        public virtual Users Reviewer { get; set; }

        // 狀態類型 
        [Required]
        [JsonProperty("review_type_id")]
        public ReviewTypeEnum ReviewTypeId { get; set; }

        // 審核意見
        [JsonProperty("comment")]
        [MaxLength(500)]
        public string Comment { get; set; }

        // 導覽屬性: 狀態
        [Required]
        [JsonProperty("status_id")]
        public int StatusId { get; set; }

        [ForeignKey("StatusId")]
        public virtual ProgramPlanStatus Status { get; set; }
    }
}