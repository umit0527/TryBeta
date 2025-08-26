using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TryBeta.Models
{
    public class ProgramSubmitReviewDto
    {
        [Required(ErrorMessage = "請選擇核准或婉拒")]
        [JsonProperty("status_id")]
        public int StatusId { get; set; }  // 審核狀態，通過或拒絕

        [Required(ErrorMessage = "請輸入訊息")]
        [JsonProperty("comment")]
        [MaxLength(1000, ErrorMessage = "最多 1000 字")]
        public string Comment { get; set; } // 給體驗者的訊息，通過訊息或拒絕理由
    }
}