using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PDS.Cestovatelia.Models
{
    public class PostModel
    {
        [StringLength(200)]
        public string Description { get; set; }
        [Required]
        public byte[] Picture{ get; set; }
        [Required]
        public string Nickname { get; set; }
        [Required]
        public int UserId { get; set; }
    }
}
