using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TryBeta.Models
{
    public class HomePageDto
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("company_name")]
        public string CompanyName { get; set; }

        [MaxLength(200, ErrorMessage = "名稱最多200字")]
        [JsonProperty("name")]
        public string Name { get; set; }

        [MaxLength(1000, ErrorMessage = "介紹最多1000字")]
        [JsonProperty("intro")]
        public string Intro { get; set; }

        [MaxLength(500, ErrorMessage = "地址最多500字")]
        [JsonProperty("address")]
        public string Address { get; set; }

        [JsonProperty("program_start_date")]
        public DateTime ProgramStartDate { get; set; }

        [JsonProperty("program_end_date")]
        public DateTime ProgramEndDate { get; set; }

        [JsonProperty("days_left")]
        public int DaysLeft { get; set; }

        [JsonProperty("status_id")]
        [ForeignKey("StatusId")]
        public int StatusId { get; set; }

        [JsonProperty("status_title")]
        public string StatusTitle { get; set; }

        // ------------ 熱門分數相關 ------------
        [JsonProperty("views_count")]
        public int ViewsCount { get; set; }
        
        [JsonProperty("favorites_count")]
        public int FavoritesCount { get; set; }
        
        [JsonProperty("applied_count")]
        public int AppliedCount { get; set; }

        [JsonProperty("score")]
        public int Score { get; set; }

        // ------------ 體驗封面 ------------
        [JsonProperty("cover_id")]
        public int CoverId { get; set; }

        [JsonProperty("img_path")]
        public string ImgPath { get; set; }  // 照片路徑

        ////導覽屬性
        //public virtual ProgramPlanImage Images { get; set; }


    }
}