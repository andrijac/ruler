using Ruler.Shared.Interfaces;
using Ruler.Shared.Models;
using Ruler.Shared.Services;
using Ruler.Wpf.ViewModels;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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
    /// Interaction logic for RulerCalibration.xaml
    /// </summary>
    /// <summary>
    /// Interaction logic for CalibrationWindow.xaml
    /// </summary>
    public partial class CalibrationWindow : Window
    {
        private readonly CalibrationViewModel _viewModel;

        public double ResultMultiplier => _viewModel.ResultMultiplier;

        public CalibrationWindow(IMonitorRegistry monitorRegistry, string targetMonitorName, double initialMultiplier = 1.0)
        {
            InitializeComponent();

            _viewModel = new CalibrationViewModel(monitorRegistry, targetMonitorName, initialMultiplier);
            _viewModel.RequestClose = (dialogResult) =>
            {
                DialogResult = dialogResult;
                Close();
            };

            DataContext = _viewModel;
        }
    }
}
