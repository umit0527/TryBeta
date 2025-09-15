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
        public bool IsActive { get; set; }

        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [JsonProperty("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // 導覽屬性 (關聯 User)
        [ForeignKey("UserId")]
        public virtual Users User { get; set; }

        // 關聯到技巧(一對多)
        public virtual ICollection<ResumeSkill> Skills { get; set; } = new List<ResumeSkill>();

        // 導覽屬性，Portfolio 附件列表
        public virtual ICollection<PortfolioFiles> PortfolioFiles { get; set; } = new List<PortfolioFiles>();
    }
}