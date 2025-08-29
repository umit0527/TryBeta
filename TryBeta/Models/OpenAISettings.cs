using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;

namespace TryBeta.Models
{
    public class OpenAISettings
    {
        public string ApiKey { get; set; }
        public OpenAISettings LoadOpenAISettings()
        {
            var json = File.ReadAllText("openaisetting.json");
            var jObject = JObject.Parse(json);
            var apiKey = jObject["OpenAI"]["ApiKey"].ToString();
            return new OpenAISettings { ApiKey = apiKey };
        }
    }
}