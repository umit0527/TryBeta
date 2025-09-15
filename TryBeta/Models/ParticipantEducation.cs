using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TryBeta.Models
{
    public class ParticipantEducation  //體驗者填寫最高學歷，給企業審核單一體驗者用
    {
        [Key, ForeignKey("ParticipantInfo")]
        [JsonProperty("participant_id")]
        public int ParticipantId { get; set; }  // 用 ParticipantId 當主鍵，每個 participant 只會有一筆教育資訊

        // 學校名稱
        [Required, MaxLength(100)]
        [JsonProperty("school_name")]
        public string SchoolName { get; set; }

        [MaxLength(100)]
        [JsonProperty("major")]
        public string Major { get; set; }

        // 狀態：1=畢業 2=肄業 3=在學
        [Required]
        [JsonProperty("status_id")]
        public int StatusId { get; set; }

        // 導覽屬性
        public virtual ParticipantInfoes ParticipantInfo { get; set; }
    }
}