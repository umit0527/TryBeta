using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TryBeta.Models
{
    public class FavoriteDto
    {
        [JsonProperty("user_id")]

        public int UserId { get; set; }

        [JsonProperty("program_plan_id")]

        public int ProgramPlanId { get; set; }
    }
}