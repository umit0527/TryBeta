using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TryBeta.Models
{
    public class ParticipantInfoes
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [Required]
        [JsonProperty("name")]
        [MaxLength(50)]
        public string Name { get; set; }

        [Required]
        [JsonProperty("phone")]
        [MaxLength(50)]
        public string Phone { get; set; }

        [Required]
        [JsonProperty("birthday")]
        public DateTime Birthday { get; set; }

        [Required]
        [JsonProperty("headshot")]
        public string Headshot { get; set; }

        [Required]
        [JsonProperty("city_id")]
        public int CityId { get; set; }

        [Required]
        [JsonProperty("district_id")]
        public int DistrictId { get; set; }

        [Required]
        [JsonProperty("street")]
        [MaxLength(100)]
        public string Street { get; set; }

        [Required]
        [JsonProperty("identity_id")]
        public int IdentityId { get; set; }

        [JsonProperty("identity_else")]
        [MaxLength(50)]
        public string IdentityElse { get; set; }

        //導覽到 Identity 表
        [ForeignKey("IdentityId")]
        public virtual Identity Identity { get; set; }

        [Required]
        [JsonProperty("gender")]
        public bool Gender { get; set; }  

        // 外鍵 UserId
        [Required]
        [JsonProperty("user_id")]
        public int UserId { get; set; }

        // 導覽屬性 (導航到 User)
        [ForeignKey("UserId")]
        public virtual Users User { get; set; }

        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [JsonProperty("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}