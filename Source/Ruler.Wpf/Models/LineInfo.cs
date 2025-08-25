using System.Windows.Media;

namespace Ruler.Wpf.Models
{
    /// <summary>
    /// A simple data class to represent a line to be drawn on the Canvas.
    /// This allows the ViewModel to generate drawing data without knowing
    /// about the specific UI elements.
    /// </summary>
    public class LineInfo
    {
        public double X1 { get; set; }
        public double Y1 { get; set; }
        public double X2 { get; set; }
        public double Y2 { get; set; }
        public Brush Stroke { get; set; }
        public double StrokeThickness { get; set; }
    }
}
