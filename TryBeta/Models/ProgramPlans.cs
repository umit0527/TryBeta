using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TryBeta.Models
{
    public class ProgramPlan
    {
        [Key]
        [JsonProperty("id")]
        public int Id { get; set; }

        [Required]
        [JsonProperty("company_id")]
        [ForeignKey("Company")]
        public int CompanyId { get; set; }

        [Required]
        [StringLength(255)]
        [JsonProperty("name")]
        public string Name { get; set; }

        [StringLength(1000)]
        [JsonProperty("intro")]
        public string Intro { get; set; }

        [JsonProperty("industry_id")]
        public int IndustryId { get; set; }

        [JsonProperty("job_title_id")]
        public int JobTitleId { get; set; }

        [StringLength(255)]
        [JsonProperty("address")]
        public string Address { get; set; }

        [StringLength(50)]
        [JsonProperty("contact_name")]
        public string ContactName { get; set; }

        [StringLength(50)]
        [JsonProperty("contact_phone")]
        public string ContactPhone { get; set; }

        [Required]
        [JsonProperty("min_people")]
        public int MinPeople { get; set; }   // 最少人數（成團標準）

        [Required]
        [JsonProperty("max_people")]
        public int MaxPeople { get; set; }   // 最多人數（上限）

        [JsonProperty("publish_start_date")]
        public DateTime PublishStartDate { get; set; }  //刊登開始日期

        [JsonProperty("publish_duration_days")]
        public int PublishDurationDays { get; set; }  //刊登持續期間

        [JsonProperty("publish_end_date")]
        public DateTime PublishEndDate { get; set; }  //刊登結束日期

        [JsonProperty("program_start_date")]
        public DateTime ProgramStartDate { get; set; }  //體驗執行開始日期

        [JsonProperty("program_end_date")]
        public DateTime ProgramEndDate { get; set; }  //體驗執行結束日期

        [JsonProperty("program_duration_days")]
        public int ProgramDurationDays { get; set; }  //體驗持續期間

        [Required]
        [StringLength(50)]
        [JsonProperty("status")]
        public string Status { get; set; } // 審核中:Under review, 系統通過:System Pass, 系統拒絕:System Rejection,
                                           // 人工通過:Manual pass, 人工拒絕:Manual rejection

        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [JsonProperty("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // 導航屬性
        public virtual CompanyInfoes Company { get; set; }
    }
}