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

namespace Ruler.Wpf.Views
{
    /// <summary>
    /// Interaction logic for ShortcutWindow.xaml
    /// </summary>
    public partial class ShortcutWindow : Window
    {
        public ShortcutWindow()
        {
            InitializeComponent();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            // Close effortlessly if escape is pressed while focused
            if (e.Key == Key.Escape)
            {
                this.Close();
            }
        }
    }
}
