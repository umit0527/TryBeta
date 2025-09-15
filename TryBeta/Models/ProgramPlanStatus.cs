using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TryBeta.Models
{
    public class ProgramPlanStatus
    {
        [Key]
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

    }
    //public enum ProgramPlanStatusEnum
    //{
        //Under Review = 1,     // 審核中
        //System Pass = 2,      // 系統通過
        //System Rejected = 3,  // 系統拒絕
        //Manual Pass = 4,      // 人工通過
        //Manual Rejected = 5,  // 人工拒絕
        //Pending = 6,         // 待發布
        //Published = 7        // 已發布
        //All Pass = 15
        //All Rejected = 16
    //}
}