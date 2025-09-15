using Newtonsoft.Json;
using System;
using System.Collections.Generic;
<<<<<<< HEAD
=======
using System.ComponentModel.DataAnnotations;
>>>>>>> API-ParticipantEvaluation
using System.Linq;
using System.Web;

namespace TryBeta.Models
{
    public class ParticipantDto
    {
<<<<<<< HEAD
        public string Name { get; set; }
        public string Phone { get; set; }
        public DateTime Birthday { get; set; }
        public string Headshot { get; set; }
        public int CityId { get; set; }
        public int DistrictId { get; set; }
        public string Street { get; set; }
        public int IdentityId { get; set; }
        public string IdentityElse { get; set; }
        public string IdentityName { get; set; }  // 導覽屬性 (例如學生、上班族等)
        
        [JsonIgnore]
        public bool Gender { get; set; }

        [JsonProperty("Gender")]
=======

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
>>>>>>> API-ParticipantEvaluation
        public string GenderString => Gender ? "男" : "女"; //女=0=false、男=1=true
    }
}