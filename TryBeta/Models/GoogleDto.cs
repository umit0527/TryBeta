using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TryBeta.Models
{
    public class GoogleDto
    {
        [JsonProperty("id_token")]
        public string Token { get; set; }
    }
}