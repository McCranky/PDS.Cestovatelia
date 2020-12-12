using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PDS.Cestovatelia.Models
{
    public class Post
    {
        public int PostId { get; set; }
        public int UserId { get; set; }
        public string Nickname { get; set; }
        public string PictureSource { get; set; }
        public string Description { get; set; }
        public DateTime CreationDate { get; set; }
        public int Likes { get; set; }
        public bool LikedByMe { get; set; }
        public bool EditableByMe { get; set; }
    }
}
