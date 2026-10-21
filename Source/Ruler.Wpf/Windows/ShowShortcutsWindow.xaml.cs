using Ruler.Wpf.ViewModels;

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
    /// Interaction logic for ShowShortcutsWindow.xaml
    /// </summary>
    public partial class ShowShortcutsWindow : Window
    {
        public ShowShortcutsWindow()
        {
            InitializeComponent();
            DataContext = new ShowShortcutsViewModel();
        }
    }
}
