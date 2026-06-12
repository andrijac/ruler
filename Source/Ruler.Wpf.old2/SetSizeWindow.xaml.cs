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

namespace Ruler.Wpf
{
    /// <summary>
    /// Interaction logic for SetSizeWindow.xaml
    /// </summary>
    public partial class SetSizeWindow : Window
    {
        public Size NewSize { get; private set; }

        public SetSizeWindow()
        {
            InitializeComponent();
            btnOk.Click += OnOkClick;
            btnCancel.Click += OnCancelClick;
        }
        public SetSizeWindow(double width, double height) : this()
        {
            txtWidth.Text = width.ToString();
            txtHeight.Text = height.ToString();
        }   

        private void OnOkClick(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(txtWidth.Text, out int width) && int.TryParse(txtHeight.Text, out int height))
            {
                NewSize = new Size(width, height);
                this.DialogResult = true;
            }
            else
            {
                MessageBox.Show("Please enter valid numbers for width and height.");
            }
        }

        private void OnCancelClick(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}
