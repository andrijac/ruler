using Ruler.Shared.Models;
using Ruler.Shared.Services;
using Ruler.Wpf.ViewModels;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
    /// Interaction logic for UpdateWindow.xaml
    /// </summary>
    public partial class UpdateWindow : Window
    {
        private readonly UpdateViewModel _viewModel;

        public UpdateWindow()
        {
            InitializeComponent();

            _viewModel = new UpdateViewModel();
            _viewModel.RequestClose = () => this.Close();

            DataContext = _viewModel;

         //   Loaded += async (s, e) => await _viewModel.CheckForUpdatesAsync();
        }
    }
}
