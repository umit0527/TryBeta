using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace TryBeta.Models
{
    public class ProgramStepDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }

    public class ProgramPlanDto : IValidatableObject
    {
        [JsonProperty("company_name")]
        public string CompanyName { get; set; }

        [JsonProperty("company_logo")]
        public string CompanyLogo { get; set; }

        [JsonProperty("company_cover")]
        public string CompanyCover { get; set; }

<<<<<<< HEAD
=======
        [Required]
        [StringLength(50)]
        [JsonProperty("serial_num")]
        public string SerialNum { get; set; }

>>>>>>> API-ParticipantEvaluation
        [Required(ErrorMessage = "請輸入名稱")]
        [MaxLength(200, ErrorMessage = "名稱最多200字")]
        [JsonProperty("name")]
        public string Name { get; set; }

        [Required(ErrorMessage = "請輸入介紹")]
        [MaxLength(1000, ErrorMessage = "介紹最多1000字")]
        [JsonProperty("intro")]
        public string Intro { get; set; }

        [Required(ErrorMessage = "請選擇產業")]
        [Range(1, int.MaxValue, ErrorMessage = "產業ID必須大於0")]
        [JsonProperty("industry_id")]
        public int IndustryId { get; set; }

        [Required(ErrorMessage = "請選擇職務")]
        [Range(1, int.MaxValue, ErrorMessage = "職務ID必須大於0")]
        [JsonProperty("job_title_id")]
        public int JobTitleId { get; set; }

        [Required(ErrorMessage = "請輸入體驗地址")]
        [MaxLength(500, ErrorMessage = "地址最多500字")]
        [JsonProperty("address")]
        public string Address { get; set; }

        [JsonProperty("address_map")]
        public string AddressMap { get; set; }

        [Required(ErrorMessage = "請輸入聯絡人名稱")]
        [MaxLength(100, ErrorMessage = "聯絡人最多100字")]
        [JsonProperty("contact_name")]
        public string ContactName { get; set; }

        [Required(ErrorMessage = "請輸入聯絡人電話號碼")]
        [Phone(ErrorMessage = "電話格式不正確")]
        [JsonProperty("contact_phone")]
        public string ContactPhone { get; set; }

        [Required(ErrorMessage = "請輸入聯絡人Email")]
        [EmailAddress(ErrorMessage = "Email格式不正確")]
        [JsonProperty("contact_email")]
        public string ContactEmail { get; set; }

        [Required(ErrorMessage = "請輸入最少體驗人數")]
        [Range(1, int.MaxValue, ErrorMessage = "最少體驗人數必須大於0")]
        [JsonProperty("min_people")]
        public int MinPeople { get; set; }

        [Required(ErrorMessage = "請輸入體驗人數上限")]
        [Range(1, int.MaxValue, ErrorMessage = "體驗人數上限必須大於0")]
        [JsonProperty("max_people")]
        public int MaxPeople { get; set; }

        [Required(ErrorMessage = "刊登開始日期為必填")]
        [JsonProperty("publish_start_date")]
        public DateTime PublishStartDate { get; set; }

        [Required(ErrorMessage = "刊登持續天數為必填")]
        [Range(1, 365, ErrorMessage = "刊登持續天數必須介於1到365")]
        [JsonProperty("publish_duration_days")]
        public int PublishDurationDays { get; set; }

        [JsonProperty("publish_end_date")]
        public DateTime PublishEndDate { get; set; }

        [Required(ErrorMessage = "請選擇體驗開始日期")]
        [JsonProperty("program_start_date")]
        public DateTime ProgramStartDate { get; set; }

        [Required(ErrorMessage = "請選擇體驗結束日期")]
        [JsonProperty("program_end_date")]
        public DateTime ProgramEndDate { get; set; }

        [JsonProperty("program_duration_days")]
        public int ProgramDurationDays { get; set; }

        [JsonProperty("status_id")]
        [ForeignKey("StatusId")]
        public int StatusId { get; set; }

        [JsonProperty("status_title")]
        public string StatusTitle { get; set; }

        // ------------ 熱門分數相關 ------------
        [JsonProperty("views_count")]
        public int ViewsCount { get; set; } = 0;

        [JsonProperty("favorites_count")]
        public int FavoritesCount { get; set; } = 0;

        [JsonProperty("applied_count")]
        public int AppliedCount { get; set; } = 0;

        [JsonProperty("score")]
        public int Score => ViewsCount * 1 + FavoritesCount * 3 + AppliedCount * 5;
        // ------------------------------------

        [JsonProperty("days_left")]
        public int? DaysLeft { get; set; }

        [JsonProperty("is_ongoing")]
        public bool? IsOngoing { get; set; }

        public SimpleEntityDto Industry { get; set; }
        public SimpleEntityDto JobTitle { get; set; }
        public SimpleEntityDto Status { get; set; }

        public List<string> Images { get; set; } = new List<string>();
        public List<ProgramStepDto> Steps { get; set; } = new List<ProgramStepDto>();

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var publishEndDate = PublishStartDate.AddDays(PublishDurationDays);
            var programEndDate = ProgramStartDate.AddDays(ProgramDurationDays);

            if (publishEndDate < PublishStartDate)
            {
                yield return new ValidationResult(
                    "刊登結束日期必須晚於刊登開始日期",
                    new[] { "PublishStartDate", "PublishDurationDays" });
            }

            if (programEndDate < ProgramStartDate)
            {
                yield return new ValidationResult(
                    "體驗結束日期必須晚於體驗開始日期",
                    new[] { "ProgramStartDate", "ProgramDurationDays" });
            }

            if (ProgramStartDate < PublishStartDate)
            {
                yield return new ValidationResult(
                    "體驗開始日期不能早於刊登開始日期",
                    new[] { "ProgramStartDate", "PublishStartDate" });
            }

            if (MaxPeople < MinPeople)
            {
                yield return new ValidationResult(
                "體驗人數上限不得小於最少體驗人數",
                new[] { nameof(MaxPeople), nameof(MinPeople) });
            }
        }

        public class SimpleEntityDto
        {
            public int Id { get; set; }
            public string Title { get; set; }
        }

        public class StepDto
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
        }

        //public class ImageDto
        //{
        //    public int Id { get; set; }
        //    public string Url { get; set; }
        //}
    }
}