using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TryBeta.Models
{
    public class ProgramStep
    {
        [Key]
        [JsonProperty("id")]
        public int Id { get; set; }

        [Required(ErrorMessage = "階段名稱必填")]
        [MaxLength(100, ErrorMessage = "階段名稱最多100字")]
        [JsonProperty("name")]
        public string Name { get; set; } // 階段名稱

        [JsonProperty("description")]
        [MaxLength(1000, ErrorMessage = "階段說明最多1000字")]
        public string Description { get; set; } // 階段說明

        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [JsonProperty("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}