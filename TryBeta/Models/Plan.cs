using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TryBeta.Models
{
    public class Plan
    {

        [JsonProperty("id")]
        public int Id { get; set; }                // 方案 ID

        [Required(ErrorMessage = "方案名稱為必填")]
        [JsonProperty("name")]
        public string Name { get; set; }           // 方案名稱

        [JsonProperty("description")]
        public string Description { get; set; }    // 方案描述

        [Range(0, 999999, ErrorMessage = "價格必須為正數")]
        [JsonProperty("price")]
        public decimal Price { get; set; }         // 價格（建議用 decimal 儲存金額）

        [Range(1, 365, ErrorMessage = "天數必須在 1 到 365 之間")]
        [JsonProperty("duration_days")]            
        public int DurationDays { get; set; }      // 使用期限（天數）

        [Range(1, int.MaxValue, ErrorMessage = "體驗人數上限必須大於0")]
        [JsonProperty("max_participants")]
        public int MaxParticipants { get; set; }   // 最大體驗人數

        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;    // 建立時間

        [JsonProperty("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;    // 更新時間

    }
}