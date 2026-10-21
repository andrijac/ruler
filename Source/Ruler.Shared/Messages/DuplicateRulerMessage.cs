using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ruler.Shared.Models;

namespace Ruler.Shared.Messages
{
    public class DuplicateRulerMessage
    {
        public RulerInfo InfotoDuplicate { get; set; }
    }
}
