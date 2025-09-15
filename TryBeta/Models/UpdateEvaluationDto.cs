using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TryBeta.Models
{
    /// <summary>
    /// 平台管理員審核 評價用 DTO
    /// </summary>
    public class UpdateEvaluationDto
    {
        [Required]
        [JsonProperty("status_id")]
        public int StatusId { get; set; } // 2=核准, 3=拒絕

        [JsonProperty("comment")]
        [MaxLength(500)]
        public string Comment { get; set; }
    }
}