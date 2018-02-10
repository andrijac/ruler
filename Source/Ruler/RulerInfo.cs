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
		public int Rotation
		{
			get;
			set;
		}

		public string ConvertToParameters()
		{
			return string.Format("{0} {1} {2} {3} {4} {5} {6} {7}", this.Width, this.Height, this.IsVertical, this.Opacity, this.ShowToolTip, this.IsLocked, this.TopMost,this.Rotation);
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
			string rotation = args[7];

			RulerInfo rulerInfo = new RulerInfo
			{
				Width = int.Parse(width),
				Height = int.Parse(height),
				IsVertical = bool.Parse(isVertical),
				Opacity = double.Parse(opacity),
				ShowToolTip = bool.Parse(showToolTip),
				IsLocked = bool.Parse(isLocked),
				TopMost = bool.Parse(topMost),
				Rotation = int.Parse(rotation)
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
				ShowToolTip = false,
				IsLocked = false,
				IsVertical = false,
				TopMost = true,
				Rotation = 0
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
			targetInstance.Rotation = source.Rotation;
		}
	}	
}