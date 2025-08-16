using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TryBeta.Models
{
    public class ProgramPlanStatus
    {
        [Key]
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

    }
    public enum ProgramPlanStatusEnum
    {
        UnderReview = 1,     // 審核中
        SystemPass = 2,      // 系統通過
        SystemRejected = 3,  // 系統拒絕
        ManualPass = 4,      // 人工通過
        ManualRejected = 5,  // 人工拒絕
        Pending = 6,         // 待發布
        Published = 7        // 已發布
    }
}