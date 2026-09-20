using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ruler.Shared.Models
{
    public class UpdateManifest
    {
        public string ReleaseVersion { get; set; }
        public string ZipSignature { get; set; } // RSA signature of the entire .zip package

        // Detailed metadata for every critical file in the zip
        public List<FileSignature> Files { get; set; } = new List<FileSignature>();

        public string ReleaseNotes { get; set; }
    }
}
