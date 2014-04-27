using System;
using System.Collections.Generic;
using System.Text;

namespace Ruler
{
	public class RulerInfo
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

		public string ConvertToParameters()
		{
			return string.Format("{0} {1} {2} {3}", this.Width, this.Height, this.IsVertical, this.Opacity);
		}

		public static RulerInfo CovertToRulerInfo(string[] args)
		{
			string width = args[0];
			string height = args[1];
			string isVertical = args[2];
			string opacity = args[3];

			RulerInfo rulerInfo = new RulerInfo();

			rulerInfo.Width = int.Parse(width);
			rulerInfo.Height = int.Parse(height);
			rulerInfo.IsVertical = bool.Parse(isVertical);
			rulerInfo.Opacity = double.Parse(opacity);

			return rulerInfo;
		}
	}
}