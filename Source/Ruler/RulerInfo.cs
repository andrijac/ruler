using System;
using System.Drawing;

namespace Ruler
{
    public class RulerInfo : IRulerInfo
    {
        public int Width
        {
            get;
            set;
        }

        public int Height
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
        public Point DisplayedLocation
        {
            get;
            set;
        }
        public SaveTypes SaveType
        {
            get;
            set;
        }

        public string ConvertToParameters()
        {
            return string.Format("{0} {1} {2} {3} {4} {5} {6} {7} {8}", this.Width, this.Height, this.IsVertical, this.Opacity, this.ShowToolTip, this.IsLocked, this.TopMost, this.DisplayedLocation, this.SaveType);
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
                DisplayedLocation = new Point(0, 0),
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
            targetInstance.DisplayedLocation = source.DisplayedLocation;
            targetInstance.SaveType = source.SaveType;
        }
        public static void SaveLocaton(MainForm form)
        {
            Settings.Default.Reset();
            Settings.Default["location"] = form.Location;
            Settings.Default["savetype"] = form.SaveType.ToString();
            Settings.Default.Save();
            Settings.Default.Reload();

        }
        public static void SaveSize(MainForm form)
        {
            Settings.Default.Reset();
            Settings.Default["width"] = form.Width;
            Settings.Default["height"] = form.Height;
            Settings.Default["savetype"] = form.SaveType.ToString();
            Settings.Default.Save();
            Settings.Default.Reload();
        }
        public static void SaveAll(MainForm form)
        {
            Settings.Default.Reset();
            Settings.Default["width"] = form.Width;
            Settings.Default["height"] = form.Height;
            Settings.Default["opacity"] = form.Opacity;
            Settings.Default["location"] = form.Location;
            Settings.Default["vertical"] = form.IsVertical;
            Settings.Default["locked"] = form.IsLocked;
            Settings.Default["top"] = form.TopMost;
            Settings.Default["tip"] = form.ShowToolTip;
            Settings.Default["savetype"] = form.SaveType.ToString();
            Settings.Default.Save();
            Settings.Default.Reload();

        }
        public static SaveTypes CheckSavedState()
        {

            SaveTypes saveArgs;
            string saveType = (string)Settings.Default["savetype"];
            if (!Enum.TryParse<SaveTypes>(saveType, true, out saveArgs))
            {
                saveArgs = SaveTypes.none;
            }
            return saveArgs;
        }
        public static RulerInfo GetSavedLocation()
        {
            RulerInfo ri = RulerInfo.GetDefaultRulerInfo();
            ri.DisplayedLocation = (Settings.Default["location"] == null) ? new Point(0, 0) : (Point)Settings.Default["location"];
            SaveTypes saveTypes;
            string saved = Settings.Default["savetype"] == null ? "none" : (string)Settings.Default["savetype"];
            if (!Enum.TryParse<SaveTypes>(saved, true, out saveTypes))
            {
                saveTypes = SaveTypes.none;
            }
            ri.SaveType = saveTypes;
            return ri;
        }
        public static RulerInfo GetSavedSize()
        {
            RulerInfo ri = RulerInfo.GetDefaultRulerInfo();
            ri.Width = (Settings.Default["width"] == null) ? ri.Width : (int)Settings.Default["height"];
            ri.Height = (Settings.Default["height"] == null) ? ri.Height : (int)Settings.Default["height"];
            SaveTypes saveTypes;
            string saved = Settings.Default["savetype"] == null ? "none" : (string)Settings.Default["savetype"];
            if (!Enum.TryParse<SaveTypes>(saved, true, out saveTypes))
            {
                saveTypes = SaveTypes.none;
            }
            ri.SaveType = saveTypes;
            return ri;
        }
        public static RulerInfo GetSavedRulerInfor()
        {
            RulerInfo ri = RulerInfo.GetDefaultRulerInfo();
            ri.Width = (Settings.Default["width"] == null) ? ri.Width : (int)Settings.Default["width"];
            ri.Height = (Settings.Default["height"] == null) ? ri.Height : (int)Settings.Default["height"];
            ri.Opacity = (Settings.Default["opacity"] == null) ? ri.Opacity : (double)Settings.Default["opacity"];
            ri.DisplayedLocation = (Settings.Default["location"] == null) ? new Point(0, 0) : (Point)Settings.Default["location"];
            ri.IsVertical = (Settings.Default["vertical"] == null) ? false : (bool)Settings.Default["vertical"];
            ri.IsLocked = (Settings.Default["locked"] == null) ? false : (bool)Settings.Default["locked"];
            ri.TopMost = (Settings.Default["top"] == null) ? true : (bool)Settings.Default["top"];
            ri.ShowToolTip = (Settings.Default["tip"] == null) ? true : (bool)Settings.Default["tip"];
            SaveTypes saveTypes;
            string saved = Settings.Default["savetype"] == null ? "none" : (string)Settings.Default["savetype"];
            if (!Enum.TryParse<SaveTypes>(saved, true, out saveTypes))
            {
                saveTypes = SaveTypes.none;
            }
            ri.SaveType = saveTypes;
            return ri;

        }


    }
    public enum SaveTypes
    {
        none,
        all,
        location,
        size
    }
}