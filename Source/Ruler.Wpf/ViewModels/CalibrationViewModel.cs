using Ruler.Shared.Commands;
using Ruler.Shared.Interfaces;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Ruler.Wpf.ViewModels
{
    public class CalibrationViewModel : ViewModelBase
    {
        private readonly IMonitorRegistry _monitorRegistry;
        private readonly string _targetMonitorName;

        // Standard credit card physical dimensions in millimeters
        private const double StandardCardWidthMm = 85.6;
        private const double StandardCardHeightMm = 54.0;
        private const double MmPerInch = 25.4;

        private double _base96DpiWidth;
        private double _base96DpiHeight;
        private double _currentMultiplier = 1.0;

        public double CardPixelWidth => _base96DpiWidth * _currentMultiplier;
        public double CardPixelHeight => _base96DpiHeight * _currentMultiplier;

        public double CurrentMultiplier
        {
            get => _currentMultiplier;
            set
            {
                if (SetProperty(ref _currentMultiplier, value))
                {
                    OnPropertyChanged(nameof(CardPixelWidth));
                    OnPropertyChanged(nameof(CardPixelHeight));

                    // Live update the registry profile as the slider moves
                    UpdateRegistryMultiplier(value);
                }
            }
        }

        public double ResultMultiplier => _currentMultiplier;

        // Action callback to handle closing the window and setting DialogResult from the view
        public Action<bool?> RequestClose { get; set; }

        public ICommand SaveCommand { get; private set; }
        public ICommand CancelCommand { get; private set; }

        public CalibrationViewModel(IMonitorRegistry monitorRegistry, string targetMonitorName, double initialMultiplier = 1.0)
        {
            _monitorRegistry = monitorRegistry ?? throw new ArgumentNullException(nameof(monitorRegistry));
            _targetMonitorName = targetMonitorName;

            // Calculate base size at standard 96 DPI
            _base96DpiWidth = (StandardCardWidthMm / MmPerInch) * 96.0;
            _base96DpiHeight = (StandardCardHeightMm / MmPerInch) * 96.0;

            _currentMultiplier = initialMultiplier;

            InitializeCommands();
        }

        private void InitializeCommands()
        {
            SaveCommand = new DelegateCommand(_ => {
                UpdateRegistryMultiplier(_currentMultiplier);
                RequestClose?.Invoke(true);
            });

            CancelCommand = new DelegateCommand(_ => {
                RequestClose?.Invoke(false);
            });
        }

        private void UpdateRegistryMultiplier(double multiplier)
        {
            if (string.IsNullOrEmpty(_targetMonitorName)) return;

            var profile = _monitorRegistry.GetOrCreateProfile(_targetMonitorName);
            if (profile != null)
            {
                profile.UserCalibrationMultiplier = multiplier;
            }
        }
    }
}
