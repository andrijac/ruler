using Ruler.Wpf.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Ruler.Wpf.Models
{
    public class RulerInfo:IRulerInfo
    {
        public double Width
        {
            get;
            set;
        }

        public double Height
        {
            get;
            set;
        }

        /// <summary>
        /// TODO
        /// </summary>
        public bool IsVertical
        {
            get;
            set;
        }

        public double Opacity
        {
            get;
            set;
        }

        /// <summary>
        /// TODO
        /// </summary>
        public bool ShowToolTip
        {
            get;
            set;
        }

        /// <summary>
        /// TODO
        /// </summary>
        public bool IsLocked
        {
            get;
            set;
        }

        public bool TopMost
        {
            get;
            set;
        }
        public double LocationX
        {
            get;
            set;
        }
        public double LocationY
        {
            get;
            set;
        }
        public Point DisplayedLocation
        {
            get
            {
                return new Point(LocationX, LocationY);
            }
            set
            {
                LocationX = value.X;
                LocationY = value.Y;
            }
        }   
        public SaveTypes SaveType
        {
            get;
            set;
        }      
        public bool IsGuideLineVisible 
        { 
            get;
            set; 
        }
        public double GuideLinePosition 
        { 
            get;
            set; 
        }

        public static RulerInfo GetDefaultRulerInfo()
        {
            RulerInfo rulerInfo = new RulerInfo
            {
                Width = 400,
                Height = 75,
                Opacity = 0.60,
                ShowToolTip = true,
                IsLocked = false,
                IsVertical = false,
                TopMost = true,
                LocationX = 0,
                LocationY = 0,
                SaveType = SaveTypes.none
            };

            return rulerInfo;
        }

        public static void CopyInto(IRulerInfo source, IRulerInfo targetInstance)
        {
            targetInstance.Width = source.Width;
            targetInstance.Height = source.Height;
            targetInstance.IsVertical = source.IsVertical;
            targetInstance.Opacity = source.Opacity;
            targetInstance.ShowToolTip = source.ShowToolTip;
            targetInstance.IsLocked = source.IsLocked;
            targetInstance.TopMost = source.TopMost;
            targetInstance.LocationX = source.LocationX;
            targetInstance.LocationY = source.LocationY;
            targetInstance.SaveType = source.SaveType;
        }
   
    }
}
