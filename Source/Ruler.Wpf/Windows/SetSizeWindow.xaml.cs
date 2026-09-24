using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Ruler.Wpf.Windows
{
    /// <summary>
    /// Interaction logic for SetSizeWindow.xaml
    /// </summary>
    public partial class SetSizeWindow : Window
    {
        private readonly int originalWidth;
        private readonly int originalHeight;

        public SetSizeWindow(int initWidth, int initHeight)
        {
            InitializeComponent();
            this.originalWidth = initWidth;
            this.originalHeight = initHeight;

            txtWidth.Text = initWidth.ToString();
            txtHeight.Text = initHeight.ToString();
        }

        private void HandleTextBoxFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (sender is System.Windows.Controls.TextBox textBox)
            {
                textBox.SelectAll();
            }
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true; // Signals Success
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false; // Signals Cancel
        }

        public Size GetNewSize() => new Size
        {
            Width = int.TryParse(txtWidth.Text, out int w) ? w : originalWidth,
            Height = int.TryParse(txtHeight.Text, out int h) ? h : originalHeight
        };
    }
}
