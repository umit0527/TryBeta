using Newtonsoft.Json;
using OpenAI.Moderations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TryBeta.Models
{
        // Moderation API 回傳強型別
        public class ModerationResponse
        {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("model")]
        public string Model { get; set; }

        [JsonProperty("results")]
        public List<ModerationResult> Results { get; set; }
    }

        public class ModerationResult
        {
        [JsonProperty("flagged")]
        public bool Flagged { get; set; }

        [JsonProperty("categories")]
        public Dictionary<string, bool> Categories { get; set; }

        [JsonProperty("category_scores")]
        public Dictionary<string, double> CategoryScores { get; set; }
    }
    
}