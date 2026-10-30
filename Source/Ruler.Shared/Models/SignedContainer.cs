using Newtonsoft.Json;

using Ruler.Shared.Converters;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ruler.Shared.Models
{
    public class SignedContainer
    {
        [JsonConverter(typeof(RawJsonConverter))]
        public string Payload { get; set; }
        public string Signature
        {
            get; set;
        }
    }
}
