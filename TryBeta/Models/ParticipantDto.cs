using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TryBeta.Models
{
    public class ParticipantDto
    {

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("phone")]
        public string Phone { get; set; }

        [JsonProperty("birthday")]
        public DateTime Birthday { get; set; }

        [Url]

        [JsonProperty("headshot")]
        public string Headshot { get; set; }

        [JsonProperty("city_id")]
        public int CityId { get; set; }

        [JsonProperty("district_id")]
        public int DistrictId { get; set; }

        [JsonProperty("street")] 
        public string Street { get; set; }

        [JsonProperty("address")]
        public string Address { get; set; } // 後端組成 CityName + DistrictName + Street


        [JsonProperty("identity_id")]
        public int IdentityId { get; set; }

        [JsonProperty("identity_else")]
        public string IdentityElse { get; set; }

        [JsonProperty("identity_name")]
        public string IdentityName { get; set; }  // 從 Identity 表抓到的標準名稱

        [JsonIgnore]
        public bool Gender { get; set; }

        [JsonProperty("gender")]
        public string GenderString => Gender ? "男" : "女"; //女=0=false、男=1=true
    }
}