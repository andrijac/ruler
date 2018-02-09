using System;
using System.Collections.Generic;
using System.Text;

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

		public int MinWidth
		{
			get;
			set;
		}

		public int MinHeight
		{
			get;
			set;
		}

		public string ConvertToParameters()
		{
			return string.Format("{0} {1} {2} {3} {4} {5} {6} {7} {8}", this.Width, this.Height, this.IsVertical, this.Opacity, this.ShowToolTip, this.IsLocked, this.TopMost, this.MinWidth, this.MinHeight);
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
			string minWidth = args[7];
			string minHeight = args[8];

			RulerInfo rulerInfo = new RulerInfo
			{
				Width = int.Parse(width),
				Height = int.Parse(height),
				IsVertical = bool.Parse(isVertical),
				Opacity = double.Parse(opacity),
				ShowToolTip = bool.Parse(showToolTip),
				IsLocked = bool.Parse(isLocked),
				TopMost = bool.Parse(topMost),
				MinWidth = int.Parse(minWidth),
				MinHeight = int.Parse(minHeight)
			};

			return rulerInfo;
		}

		public static RulerInfo GetDefaultRulerInfo()
		{
			RulerInfo rulerInfo = new RulerInfo
			{
				Width = Properties.Settings.Default.Width,
				Height = Properties.Settings.Default.Height,
				Opacity = Properties.Settings.Default.Opacity,
				ShowToolTip = Properties.Settings.Default.ShowToolTip,
				IsLocked = Properties.Settings.Default.IsLocked,
				IsVertical = Properties.Settings.Default.IsVertical,
				TopMost = Properties.Settings.Default.TopMost,
				MinHeight = Properties.Settings.Default.MinHeight,
				MinWidth = Properties.Settings.Default.MinWidth
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
			targetInstance.MinWidth = source.MinWidth;
			targetInstance.MinHeight = source.MinHeight;
		}
	}	
}