using System;
using System.Drawing;
using System.Windows.Forms;
using Ruler.Shared.Enums;

namespace Ruler.Forms
{
	public partial class SetSizeForm : Form
	{
		private readonly int originalWidth;
		private readonly int originalHeight;

		public SetSizeForm(int initWidth, int initHeight)
		{
			this.InitializeComponent();

			this.originalWidth = initWidth;
			this.originalHeight = initHeight;
            

            this.txtWidth.Text = initWidth.ToString();
			this.txtHeight.Text = initHeight.ToString();

			this.txtHeight.GotFocus += this.HandleTextBoxFocus;
			this.txtWidth.GotFocus += this.HandleTextBoxFocus;
		}

		private void HandleTextBoxFocus(object sender, EventArgs e)
		{
			((TextBox)sender).SelectAll();
		}
        private void BtnCancelClick(object sender, MouseEventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
        private void BtnOkClick(object sender, MouseEventArgs e)
        {
            this.DialogResult = DialogResult.OK;
        }
        public Size GetNewSize() 
            => new Size
			{
				Width = int.TryParse(this.txtWidth.Text, out int width) ? width : this.originalWidth,
				Height = int.TryParse(this.txtHeight.Text, out int height) ? height : this.originalHeight
			};

        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.txtWidth = new System.Windows.Forms.TextBox();
            this.txtHeight = new System.Windows.Forms.TextBox();
            this.BtnOk = new System.Windows.Forms.Button();
            this.BtnCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(60, 52);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(55, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Width (px)";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(71, 105);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(58, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Height (px)";
            // 
            // txtWidth
            // 
            this.txtWidth.Location = new System.Drawing.Point(199, 49);
            this.txtWidth.Name = "txtWidth";
            this.txtWidth.Size = new System.Drawing.Size(100, 20);
            this.txtWidth.TabIndex = 2;
            // 
            // txtHeight
            // 
            this.txtHeight.Location = new System.Drawing.Point(199, 105);
            this.txtHeight.Name = "txtHeight";
            this.txtHeight.Size = new System.Drawing.Size(100, 20);
            this.txtHeight.TabIndex = 3;
            // 
            // BtnOk
            // 
            this.BtnOk.Location = new System.Drawing.Point(74, 171);
            this.BtnOk.Name = "BtnOk";
            this.BtnOk.Size = new System.Drawing.Size(75, 23);
            this.BtnOk.TabIndex = 4;
            this.BtnOk.Text = "Ok";
            this.BtnOk.UseVisualStyleBackColor = true;
            this.BtnOk.MouseClick += new System.Windows.Forms.MouseEventHandler(this.BtnOkClick);
            // 
            // BtnCancel
            // 
            this.BtnCancel.Location = new System.Drawing.Point(226, 171);
            this.BtnCancel.Name = "BtnCancel";
            this.BtnCancel.Size = new System.Drawing.Size(75, 23);
            this.BtnCancel.TabIndex = 5;
            this.BtnCancel.Text = "Cancel";
            this.BtnCancel.UseVisualStyleBackColor = true;
            this.BtnCancel.MouseClick += new System.Windows.Forms.MouseEventHandler(this.BtnCancelClick);
            // 
            // SetSizeForm
            // 
            this.ClientSize = new System.Drawing.Size(373, 236);
            this.Controls.Add(this.BtnCancel);
            this.Controls.Add(this.BtnOk);
            this.Controls.Add(this.txtHeight);
            this.Controls.Add(this.txtWidth);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Name = "SetSizeForm";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

       
    }
}
