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

        [Required]
        [JsonProperty("employee_num")]
        public string EmployeeNum { get; set; }

        [Required]
        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Required]
        [JsonProperty("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}