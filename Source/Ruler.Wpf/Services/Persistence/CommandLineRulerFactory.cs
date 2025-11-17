using Ruler.Wpf.Common;
using Ruler.Wpf.Models;

using System;
using System.Collections.Generic;
using System.Windows;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ruler.Wpf.Services.Persistence
{
    public static class CommandLineRulerFactory
    {       
        public static string ConvertToParameters(RulerInfo rulerInfo)
        {
            return string.Format("{0} {1} {2} {3} {4} {5} {6} {7} {8}", rulerInfo.Width, rulerInfo.Height, rulerInfo.IsVertical, rulerInfo.Opacity, rulerInfo.ShowToolTip, rulerInfo.IsLocked, rulerInfo.TopMost, rulerInfo.DisplayedLocation, rulerInfo.SaveType);
        }
        public static RulerInfo CovertToRulerInfo(string[] args)
        {
            string width = args[0];
            string height = args[1];
            string isVertical = args[2];
            string opacity = args[3];
            string showToolTip = args[4];
            string isLocked = args[5];
            string topMost = args[6];
            string location = (args.Length >= 8) ? args[7] : "0,0";
            string savetype = (args.Length >= 9) ? args[8] : "none";

            SaveTypes saveArgs;
            string[] startlocation = location.Split(',');
            Point pt = new Point(int.Parse(startlocation[0]), int.Parse(startlocation[1]));
            if (!Enum.TryParse<SaveTypes>(savetype, true, out saveArgs))
            {
                saveArgs = SaveTypes.none;
            }

            RulerInfo rulerInfo = new RulerInfo
            {
                Width = int.Parse(width),
                Height = int.Parse(height),
                IsVertical = bool.Parse(isVertical),
                Opacity = double.Parse(opacity),
                ShowToolTip = bool.Parse(showToolTip),
                IsLocked = bool.Parse(isLocked),
                TopMost = bool.Parse(topMost),
                DisplayedLocation = pt,
                SaveType = saveArgs
            };

            return rulerInfo;
        }
    }
}
