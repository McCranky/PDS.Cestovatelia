using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PDS.Cestovatelia.Models.User
{
    public class UserRegisterRequest
    {
        [Required]
        [StringLength(50)]
        public string Name { get; set; }
        [Required]
        [StringLength(50)]
        public string Surname { get; set; }
        [Required]
        [StringLength(30, ErrorMessage = "Nickname is too long (30 characters max).")]
        public string Nickname { get; set; }
        [Required, StringLength(30)]
        public string Password { get; set; }
        [Required, Compare("Password", ErrorMessage = "Passwords have to match.")]
        public string ConfirmPassword { get; set; }
    }
}
