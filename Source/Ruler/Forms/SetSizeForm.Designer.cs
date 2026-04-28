using System;
using System.Windows.Forms;

namespace Ruler.Forms
{
    public partial class SetSizeForm : Form
    {
        // Properties to hold the values the MainForm will read
        public int NewWidth { get; private set; }
        public int NewHeight { get; private set; }

        //public SetSizeForm(int currentWidth, int currentHeight)
        //{
        //    InitializeComponent();

        //    Set the initial values in the textboxes/ numericupdowns
        //    this.txtWidth.Text = currentWidth.ToString();
        //    this.txtHeight.Text = currentHeight.ToString();

        //    this.AcceptButton = BtnOk;   // Enter key clicks OK
        //    this.CancelButton = BtnCancel; // Esc key clicks Cancel
        //}

        private void btnOk_Click(object sender, EventArgs e)
        {
            // 1. Validate Input
            if (int.TryParse(txtWidth.Text, out int w) && int.TryParse(txtHeight.Text, out int h))
            {
                // 2. Ensure dimensions aren't zero or negative
                if (w > 10 && h > 10)
                {
                    this.NewWidth = w;
                    this.NewHeight = h;
                    this.DialogResult = DialogResult.OK; // Closes the form and returns OK
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Ruler must be at least 10x10 pixels.", "Invalid Size");
                }
            }
            else
            {
                MessageBox.Show("Please enter valid whole numbers.", "Input Error");
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private Label label1;
        private Label label2;
        private TextBox txtWidth;
        private TextBox txtHeight;
        private Button BtnOk;
        private Button BtnCancel;
    }
}