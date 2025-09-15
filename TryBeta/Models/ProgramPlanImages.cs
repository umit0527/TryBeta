using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TryBeta.Models
{
    public class ProgramPlanImage
    {
        [Key]
        [JsonProperty("id")]
        public int Id { get; set; }

        // 外鍵，連結到公司資料
        [ForeignKey("ProgramPlans")]
        [JsonProperty("programplan_id")]
        public int ProgramPlanId { get; set; }      

        /// <summary>
        /// 圖片儲存路徑或網址
        /// </summary>
        //[Required]
        [JsonProperty("img_path")]
        public string ImgPath { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [JsonProperty("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // 導覽屬性，方便 Entity Framework 進行關聯
        public virtual ProgramPlan ProgramPlans { get; set; }
    }
}