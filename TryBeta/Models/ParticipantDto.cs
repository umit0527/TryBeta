using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TryBeta.Models
{
    public class ParticipantDto
    {
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
        public string GenderString => Gender ? "男" : "女"; //女=0=false、男=1=true
    }
}