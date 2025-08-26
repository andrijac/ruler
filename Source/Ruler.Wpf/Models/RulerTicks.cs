using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ruler.Wpf.Models
{
    public class RulerTick
    {
        public double X1 { get; set; }
        public double Y1 { get; set; }
        public double X2 { get; set; }
        public double Y2 { get; set; }
        public double Thickness { get; set; } = 1;
        public string LabelText { get; set; }
        public double LabelX { get; set; }
        public double LabelY { get; set; }
    }
}
