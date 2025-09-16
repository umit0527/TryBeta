using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TryBeta.Models
{

    public class ParticipantDetailDto  //企業審核單一體驗者用
    {
        [JsonProperty("review_status_id")]
        public int ReviewStatusId { get; set; }

        [JsonProperty("review_status_name")]
        public string ReviewStatusName { get; set; } 

        // 第一個區塊（基本資料、教育與評價）
        [Required, MaxLength(50)]
        [JsonProperty("name")]
        public string Name { get; set; }

        [Required, Phone, MaxLength(20)]
        [JsonProperty("phone")]
        public string Phone { get; set; }

        [JsonProperty("age")]
        public int Age { get; set; } // 由 Birthday 計算

        [Required(ErrorMessage = "請選擇性別")]
        [JsonIgnore]
        public bool Gender { get; set; }

        [JsonProperty("gender")]
        public string GenderString => Gender ? "男" : "女"; // 女=0=false、男=1=true

        [Required]
        [JsonProperty("identity_id")]
        public int IdentityId { get; set; }

        [JsonProperty("identity_name")]
        public string IdentityName { get; set; }

        [Required, MaxLength(200)]
        [JsonProperty("address")]
        public string Address { get; set; }

        [Required, EmailAddress]
        [JsonProperty("email")]
        public string Email { get; set; }

        [Url]
        [JsonProperty("headshot")]
        public string Headshot { get; set; }

        // 申請資訊
        [Required, MaxLength(100)]
        [JsonProperty("participant_serial_num")]
        public string ParticipantSerialNum { get; set; }

        // 教育資訊 (從 ParticipantEducation 表)
        //[JsonProperty("education_level")]
        //public string EducationLevel { get; set; } // 高中/大學/碩士/博士

        [JsonProperty("school_name")]
        public string SchoolName { get; set; }

        [JsonProperty("major")]
        public string Major { get; set; }

        [JsonProperty("status_id")]
        public int StatusId { get; set; } // 1=畢業 2=肄業 3=在學

        [JsonProperty("status_name")]
        public string StatusName
        {
            get
            {
                switch (StatusId)
                {
                    case 1:
                        return "畢業";
                    case 2:
                        return "肄業";
                    case 3:
                        return "在學";
                    default:
                        return "未知";
                }
            }
        }

        //體驗者的過去評價次數
        [JsonProperty("review_count")]  
        public int ReviewCount { get; set; }

        //體驗者的所有評價分數平均
        [JsonProperty("average_score")]
        public double AverageScore { get; set; }


        // 第二個區塊（申請體驗計畫資訊）
        [JsonProperty("program_plan")]
        public ProgramInfoDto ProgramPlan { get; set; }

        [JsonProperty("motivation_content")]
        public string MotivationContent { get; set; }

        public class ProgramInfoDto
        {
            [JsonProperty("program_name")]
            public string Name { get; set; }

            [JsonProperty("serial_num")]
            public string SerialNum { get; set; }

            [JsonProperty("program_start_date")]
            public DateTime ProgramStartDate { get; set; }

            [JsonProperty("program_end_date")]
            public DateTime ProgramEndDate { get; set; }

            [JsonProperty("program_duration_days")]
            public int DurationDays { get; set; }

            [JsonProperty("address")]
            public string Address { get; set; }
        }

        //第三個區塊
        public List<string> Skills { get; set; } = new List<string>();

        //第四區塊
        public List<PortfolioFileDto> PortfolioFiles { get; set; } = new List<PortfolioFileDto>();

        //內部 DTO
        public class PortfolioFileDto
        {
            public int Id { get; set; }

            [JsonProperty("title")]
            public string Title { get; set; }

            [JsonProperty("portfolio_path")]
            public string PortfolioPath { get; set; }

            [JsonProperty("file_size")]
            public string FileSize { get; set; }
        }

        // 第五個區塊：過去參加的體驗計畫
        [JsonProperty("past_programs")]
        public List<PastProgramDto> PastPrograms { get; set; } = new List<PastProgramDto>();

        public class PastProgramDto
        {
            [JsonProperty("program_name")]
            public string ProgramName { get; set; }

            [JsonProperty("program_start_date")]
            public DateTime ProgramStartDate { get; set; }

            [JsonProperty("program_end_date")]
            public DateTime ProgramEndDate { get; set; }

            // 判斷體驗進行狀態
            [JsonProperty("participation_status")]
            public string ParticipationStatus { get; set; }

            // 若已取消，列出取消原因
            [JsonProperty("cancel_reason")]
            public string CancelReason { get; set; }

            [JsonProperty("review_score")]
            public double? ReviewScore { get; set; }
        }        
    }
}