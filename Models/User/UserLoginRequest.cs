using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PDS.Cestovatelia.Models.User
{
    public class UserLoginRequest
    {
        [Required]
        [StringLength(30)]
        public string Nickname { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
