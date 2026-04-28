using Ruler.Shared.Enums;
using Ruler.Shared.Models;

using System.Drawing;
using System.Windows.Forms;

namespace Ruler.Shared.Interfaces
{
    public interface IRulerInfo
    {
        int Width { get; set; }
        int Height { get; set; }
        bool IsVertical { get; set; }
        double Opacity { get; set; }
        bool ShowToolTip { get; set; }
        bool IsLocked { get; set; }
        bool TopMost { get; set; }
        int Top { get; set; }
        int Left { get; set; }
        SaveTypes SaveType { get; set; }
        Point DisplayLocation { get;} 
        string DisplayLocationString { get; set; }
        Orientation Orientation { get;}
        bool IsGuideline { get; set; }
        double GuidelineLocation { get; set; }
    }
}