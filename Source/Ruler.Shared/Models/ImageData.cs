using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ruler.Shared.Models
{
    public class ImageData
    {
        public byte[] RawData { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string Format { get; set; } // e.g., "PNG"
    }
}
