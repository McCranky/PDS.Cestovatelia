using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PDS.Cestovatelia.Models
{
    public class ImageModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public byte[] Image { get; set; }
    }
}
