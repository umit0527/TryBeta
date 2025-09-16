using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TryBeta.Models
{
    public class CompanyScales
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        //企業人數：規定於 configuration 裡
        [Required]
        [JsonProperty("employee_num")]
        public string EmployeeNum { get; set; }

        [Required]
        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;  //讓寫進資料庫的時間為當下時間

        [Required]
        [JsonProperty("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}