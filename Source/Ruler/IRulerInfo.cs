namespace Ruler
{
	public interface IRulerInfo
	{
		int Width
		{
			get;
			set;
		}

		int Height
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
	}
}