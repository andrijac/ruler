using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

namespace Ruler.Wpf.Controls
{
    public class PassthroughBorder : Border
    {
        protected override HitTestResult HitTestCore(PointHitTestParameters hitTestParameters)
        {
            // Always return null.
            // Returning null tells WPF: "I am not here for input. Pass this event 
            // to the element underneath me (which is the Window's invisible resize handle)."
            return null;
        }
    }
}
