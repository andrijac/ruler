﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Ruler
{
	public partial class SetSizeForm : Form
	{
		private int originalWidth;
		private int originalHeight;

		public SetSizeForm(string title, int initWidth, int initHeight)
		{
			this.InitializeComponent();

            this.Text = title;

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
		{
			int width;
			int height;

			Size size = new Size();

			size.Width = int.TryParse(this.txtWidth.Text, out width) ? width : originalWidth;
			size.Height = int.TryParse(this.txtHeight.Text, out height) ? height : originalHeight;

			return size;
		}
	}
}