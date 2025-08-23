using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace TryBeta.Models
{
    public class ProgramSubmitReview
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [JsonProperty("program_submit_id")]
        public int ProgramSubmitId { get; set; }  // 對應 ProgramSubmits
        [ForeignKey("ProgramSubmitId")]
        public virtual ProgramSubmit ProgramSubmit { get; set; }

        [Required]
        [JsonProperty("status_id")]
        public int StatusId { get; set; }  // 通過或拒絕

        [Required]
        [JsonProperty("comment")]
        [MaxLength(1000)]
        public string Comment { get; set; }  // 統一存放通過訊息或拒絕理由

        [Required]
        [JsonProperty("reviewed_at")]
        public DateTime ReviewedAt { get; set; }  // 審核時間

        [Required]
        [JsonProperty("reviewer_id")]
        public int ReviewerId { get; set; }  // 企業或審核者ID
    }
}