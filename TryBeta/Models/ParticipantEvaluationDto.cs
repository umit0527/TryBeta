using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TryBeta.Models
{
    public enum ReviewStatus
    {
        Pending = 1,    // 待審核
        Approved = 2,   // 已通過
        Rejected = 3,   // 已拒絕（企業操作）
        Cancelled = 4   // 自行取消（體驗者操作）
    }
    public class ParticipantEvaluationDto
    {
        [JsonProperty("status_id")]
        public ReviewStatus StatusId { get; set; }  // 1: 待審核, 2: 已通過, 3: 已拒絕

        [Required(ErrorMessage ="請選擇評價分數")]
        [JsonProperty("score")]
        public int Score { get; set; }  // 評分，例如 1~5

        [Required(ErrorMessage = "請輸入評價內容")]
        [MaxLength(1000, ErrorMessage = "評價內容最多 1000 字")]
        [JsonProperty("comment")]
        public string Comment { get; set; }

        [JsonProperty("serial_num")]
        [MaxLength(50)]
        public string SerialNum { get; set; }

        [JsonProperty("program_name")]
        public string ProgramName { get; set; }

        [JsonProperty("program_start_date")]
        public DateTime ProgramStartDate { get; set; }

        [JsonProperty("program_end_date")]
        public DateTime ProgramEndDate { get; set; }

        [JsonProperty("company_name")]
        public string CompanyName { get; set; }

        [JsonProperty("company_logo")]
        public string CompanyLogo { get; set; }  // 企業 Logo (體驗者端的所有評價列表用)

        [JsonProperty("evaluation_date")]
        public DateTime EvaluationDate { get; set; }  // 評價日期 (體驗者端的所有評價列表用)

        //[JsonProperty("debug")]
        //public string Debug { get; set; }
        //[Required]
        //[JsonProperty("reviewed_at")]
        //public DateTime ReviewedAt { get; set; } = DateTime.Today;
    }
}