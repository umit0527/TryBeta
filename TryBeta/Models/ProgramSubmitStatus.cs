using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TryBeta.Models
{
    public class ProgramSubmitStatus
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        [JsonProperty("title")]
        public string Title { get; set; } // 狀態名稱，例如 "待審核"、"已通過"、"已拒絕"

        // 導航屬性：此狀態下的所有申請
        public virtual ICollection<ProgramSubmit> ProgramSubmits { get; set; } = new List<ProgramSubmit>();
    }
}