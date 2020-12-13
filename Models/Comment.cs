using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PDS.Cestovatelia.Models
{
    public class Comment
    {
        public string Text { get; set; }
        public int UserId { get; set; }
        public int CommentId { get; set; }
        public string Nickname { get; set; }
        public DateTime CreationDate { get; set; }
        public bool EditableByMe { get; set; }
    }
}
