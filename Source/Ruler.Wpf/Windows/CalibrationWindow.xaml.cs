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

using Ruler.Shared.Interfaces;
using Ruler.Shared.Models;
using Ruler.Shared.Services;

namespace Ruler.Wpf.Windows
{
    /// <summary>
    /// Interaction logic for RulerCalibration.xaml
    /// </summary>
    public partial class CalibrationWindow : Window, INotifyPropertyChanged
    {
        private IMonitorRegistry _monitorregistry;
        private readonly string _targetMonitorName;
        private bool isLoaded = false;
        // Standard credit card physical dimensions in millimeters
        private const double StandardCardWidthMm = 85.6;
        private const double StandardCardHeightMm = 54.0;
        private const double MmPerInch = 25.4;

        private double _base96DpiWidth;
        private double _base96DpiHeight;
        private double _currentMultiplier = 1.0;

        /// <summary>
        /// The final calculated user multiplier to save into the registry.
        /// </summary>
        public double ResultMultiplier => _currentMultiplier;

        public double CardPixelWidth => _base96DpiWidth * _currentMultiplier;
        public double CardPixelHeight => _base96DpiHeight * _currentMultiplier;

        public double CurrentMultiplier
        {
            get => _currentMultiplier;
            set
            {
                if (_currentMultiplier != value)
                {
                    _currentMultiplier = value;
                    OnPropertyChanged(nameof(CurrentMultiplier));
                    OnPropertyChanged(nameof(CardPixelWidth));
                    OnPropertyChanged(nameof(CardPixelHeight));
                }
            }
        }

        public CalibrationWindow(IMonitorRegistry monitorRegistry, string targetMonitorName, double initialMultiplier = 1.0)
        {
            InitializeComponent();
            DataContext = this;
            _monitorregistry = monitorRegistry;
            _targetMonitorName = targetMonitorName;

            // Calculate base size at standard 96 DPI
            _base96DpiWidth = (StandardCardWidthMm / MmPerInch) * 96.0;
            _base96DpiHeight = (StandardCardHeightMm / MmPerInch) * 96.0;

            // Initialize with any previously saved multiplier for this monitor
            _currentMultiplier = initialMultiplier;
            ScaleSlider.Value = initialMultiplier;
        }

        private void ScaleSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!IsLoaded) return;

            double newMultiplier = e.NewValue;

            // 1. Update the profile in the MonitorRegistry for this monitor
            var profile = _monitorregistry.GetOrCreateProfile(_targetMonitorName);
            if (profile != null)
            {
                profile.UserCalibrationMultiplier = newMultiplier;
            }

            // 2. Broadcast that calibration changed so all open rulers redraw instantly
           
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            RulerCalibration.UserMultiplier = CurrentMultiplier;
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
