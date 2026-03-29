using Ruler.Wpf.Common;

using System.Security.AccessControl;
using System.Windows;


namespace Ruler.Wpf.Models
{
    public interface IRulerInfo
    {
        double Width
        {
            get;
            set;
        }

        double Height
        {
            get;
            set;
        }

        bool IsVertical
        {
            get;
            set;
        }

        double Opacity
        {
            get;
            set;
        }

        bool ShowToolTip
        {
            get;
            set;
        }

        bool IsLocked
        {
            get;
            set;
        }

        bool TopMost
        {
            get;
            set;
        }
       double LocationX
        {
            get;
            set;
        }
        double LocationY
        {
            get;
            set;
        }
        SaveTypes SaveType
        {
            get;
            set;
        }

       
        //bool IsHighContrast { get; set; }
        //double ZoomFactor { get; set; }
        //UnitType Unit { get; set; }
        bool IsGuideLineVisible { get; set; }
        double GuideLinePosition { get; set; }

    }
}