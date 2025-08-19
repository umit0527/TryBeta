using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TryBeta.Models
{
    public class UserStatus
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [Required]
        [JsonProperty("title")]
        public string Title { get; set; }
    }
    public enum UserStatusEnum
    {
        Enabled = 1,     // 啟用
        Disabled = 2,      // 停用
    }
}