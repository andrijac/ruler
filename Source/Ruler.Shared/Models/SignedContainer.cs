using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ruler.Shared.Models
{
    public class SignedContainer
    {
        public string Payload { get; set; }
        public string Signature
        {
            get; set;
        }
    }
}
