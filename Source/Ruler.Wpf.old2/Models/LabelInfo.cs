using System.Windows.Media;


namespace Ruler.Wpf.Models
{
    public class LabelInfo
    {
        public string Text { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double FontSize { get; set; } = 10;
        public Brush Foreground { get; set; } = new SolidColorBrush(Colors.Black);
    }
}
