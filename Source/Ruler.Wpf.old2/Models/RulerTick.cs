using Ruler.Wpf.Common;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ruler.Wpf.Models
{
    public class RulerTick
    {
        public string Label { get; set; }
        public double Position { get; set; }
        public double TickSize { get; set; }
        public bool HasLabel => !string.IsNullOrEmpty(Label);
        public bool IsLabelVisible { get; set; }
    }
}
