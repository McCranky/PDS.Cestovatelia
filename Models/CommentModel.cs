using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PDS.Cestovatelia.Models
{
    public class CommentModel
    {
        [Required]
        [StringLength(200)]
        public string Text { get; set; }
        [Required]
        public int UserId { get; set; }
        [Required]
        public int PostId { get; set; }
    }
}
