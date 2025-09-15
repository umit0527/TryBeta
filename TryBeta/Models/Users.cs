using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace TryBeta.Models
{
    public class Users
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [Required]
        [JsonProperty("role")]
        [MaxLength(20)]
        public string Role { get; set; }  // Company / Participant / Admin 等

        [Required]
        [JsonProperty("account")]
        [MaxLength(50)]
        public string Account { get; set; }

        [Required]
        [JsonProperty("email")]
        public string Email { get; set; }

        [Required]
        [JsonProperty("password_hash")]
        [MaxLength(200)]
        public string PasswordHash { get; set; }

        [Required]
        [JsonProperty("status_id")]
        [ForeignKey("UserStatus")]
        public int StatusId { get; set; }  // 停用, 啟用

        [Required]
        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Required]
        [JsonProperty("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // EF 導航屬性
        [ForeignKey("StatusId")]
        public virtual UserStatus UserStatus { get; set; }
    }
}