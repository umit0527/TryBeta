using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TryBeta.Models
{
    public class AdminLoginDto
    {
        [Required]
        public string Identifier { get; set; }  // 帳號或Email

        [Required]
        public string Password { get; set; }
    }
}