using System;
using System.Drawing;
using System.Windows.Forms;

namespace Ruler
{
	public partial class SetSizeForm : Form
	{
		private int originalWidth;
		private int originalHeight;

		public SetSizeForm(int initWidth, int initHeight)
		{
			this.InitializeComponent();

			this.originalWidth = initWidth;
			this.originalHeight = initHeight;

			this.txtWidth.Text = initWidth.ToString();
			this.txtHeight.Text = initHeight.ToString();
		}

		private void BtnCancelClick(object sender, EventArgs e)
		{
			this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.Close();
		}

		private void BtnOkClick(object sender, EventArgs e)
		{
			this.DialogResult = System.Windows.Forms.DialogResult.OK;
		}

		public Size GetNewSize() 
            => new Size
			{
				Width = int.TryParse(this.txtWidth.Text, out int width) ? width : this.originalWidth,
				Height = int.TryParse(this.txtHeight.Text, out int height) ? height : this.originalHeight
			};
		
	}
}
