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
        public Size FormSize
        {
            get;
            set;
        }
        public Point FormLocation
        {
            get;
            set;
        }

        public string ConvertToParameters()
        {
            return string.Format("{0} {1} {2} {3} {4} {5} {6}", this.Width, this.Height, this.IsVertical, this.Opacity, this.ShowToolTip, this.IsLocked, this.TopMost);
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

            RulerInfo rulerInfo = new RulerInfo
            {
                Width = int.Parse(width),
                Height = int.Parse(height),
                IsVertical = bool.Parse(isVertical),
                Opacity = double.Parse(opacity),
                ShowToolTip = bool.Parse(showToolTip),
                IsLocked = bool.Parse(isLocked),
                TopMost = bool.Parse(topMost),
                FormLocation = new Point(100, 200)
            };
            rulerInfo.FormSize = new Size(rulerInfo.Width, rulerInfo.Height);
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
                FormSize = new Size(400, 75),
                FormLocation = new Point(0, 0)

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
            targetInstance.FormSize = source.FormSize;
            targetInstance.FormLocation = source.FormLocation;
        }
        public static RulerInfo CheckforSavedSettings()
        {
            RulerInfo ruler = new RulerInfo();
            ruler.Width = (Properties.Settings.Default["Width"] == null) ? 400 : (int)Properties.Settings.Default["Width"];
            ruler.Height = (Properties.Settings.Default["Height"] == null) ? 75 : (int)Properties.Settings.Default["Height"];
            ruler.FormLocation = (Properties.Settings.Default["Location"] == null) ? new Point(0, 0) : (Point)Properties.Settings.Default["Location"];
            ruler.FormSize = (Properties.Settings.Default["Size"] == null) ? new Size(400, 75) : (Size)Properties.Settings.Default["Size"];
            ruler.Opacity = (Properties.Settings.Default["Opacity"] == null) ? 0.60 : (double)Properties.Settings.Default["Opacity"];
            ruler.IsLocked = (Properties.Settings.Default["Locked"] == null) ? false : (bool)Properties.Settings.Default["Locked"];
            ruler.IsVertical = (Properties.Settings.Default["Vertical"] == null) ? false : (bool)Properties.Settings.Default["Vertical"];
            ruler.ShowToolTip = (Properties.Settings.Default["ToolTip"] == null) ? true : (bool)Properties.Settings.Default["ToolTip"];
            ruler.TopMost = (Properties.Settings.Default["TopMost"] == null) ? true : (bool)Properties.Settings.Default["TopMost"];
            return ruler;
        }
        public static void SetSavedSettings(MainForm form)
        {
            MainForm mf = form;

            Properties.Settings.Default["Location"] = new Point(mf.Bounds.Left, mf.Bounds.Top);
            Properties.Settings.Default["Size"] = (Size)mf.Size;
            Properties.Settings.Default["Width"] = (int)mf.Width;
            Properties.Settings.Default["Height"] = (int)mf.Height;
            Properties.Settings.Default["Opacity"] = (double)mf.Opacity;
            Properties.Settings.Default["Vertical"] = (bool)mf.IsVertical;
            Properties.Settings.Default["TopMost"] = (bool)mf.TopMost;
            Properties.Settings.Default["Locked"] = (bool)mf.IsLocked;
            Properties.Settings.Default["ToolTip"] = (bool)mf.ShowToolTip;
            Properties.Settings.Default.Save();
        }
    }
}