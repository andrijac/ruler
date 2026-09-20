using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ruler.Shared.Models
{
    public class UpdatePackageInfo
    {
        public string ManifestUrl { get; set; }
        public string SigUrl { get; set; }
        public string ZipUrl { get; set; }
    }
}
