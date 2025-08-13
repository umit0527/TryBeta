using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TryBeta.Models
{
    public class SimpleResume
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [Required]
        [JsonProperty("user_id")]
        public int UserId { get; set; }

        [Required]
        [JsonProperty("intro")]
        [MaxLength(2000)]
        public string Intro { get; set; }

        // 預設履歷：0 = 關閉，1 = 啟用
        [JsonProperty("is_active")]
        public int IsActive { get; set; } = 0;

        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [JsonProperty("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // 導覽屬性 (關聯 User)
        [ForeignKey("UserId")]
        public virtual Users User { get; set; }
    }
}