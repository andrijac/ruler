using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ruler.Shared.Models
{
    public class FileSignature
    {
        public string FileName { get; set; }     // e.g., "Ruler.exe"
        public string Version { get; set; }      // e.g., "1.2.0.0"
        public string Hash { get; set; }         // SHA256 fingerprint
        public string Signature { get; set; }    // RSA signature of this specific file
    }
}
