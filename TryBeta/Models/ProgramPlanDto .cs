using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TryBeta.Models
{
    public class ProgramPlanDto : IValidatableObject
    {
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

        [Required(ErrorMessage = "請輸入聯絡人")]
        [MaxLength(100, ErrorMessage = "聯絡人最多100字")]
        [JsonProperty("contact_name")]
        public string ContactName { get; set; }

        [Required(ErrorMessage = "請輸入電話號碼")]
        [Phone(ErrorMessage = "電話格式不正確")]
        [JsonProperty("contact_phone")]
        public string ContactPhone { get; set; }

        [Required(ErrorMessage = "Step ID為必填")]
        [Range(1, int.MaxValue, ErrorMessage = "Step ID必須大於0")]
        [JsonProperty("step_id")]
        public int StepId { get; set; }

        [Required(ErrorMessage = "體驗人數為必填")]
        [Range(1, int.MaxValue, ErrorMessage = "體驗人數必須大於0")]
        [JsonProperty("program_count")]
        public int ProgramCount { get; set; }

        [Required(ErrorMessage = "刊登開始日期為必填")]
        [JsonProperty("publish_start_date")]
        public DateTime PublishStartDate { get; set; }

        [Required(ErrorMessage = "刊登持續天數為必填")]
        [Range(1, 365, ErrorMessage = "刊登持續天數必須介於1到365")]
        [JsonProperty("publish_duration_days")]
        public int PublishDurationDays { get; set; }

        [Required(ErrorMessage = "體驗開始日期為必填")]
        [JsonProperty("program_start_date")]
        public DateTime ProgramStartDate { get; set; }

        [Required(ErrorMessage = "體驗持續天數為必填")]
        [Range(1, 365, ErrorMessage = "體驗持續天數必須介於1到365")]
        [JsonProperty("program_duration_days")]
        public int ProgramDurationDays { get; set; }


        /// <summary>
        /// 自動驗證日期邏輯
        /// </summary>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // 計算結束日期
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
        }
    }
}