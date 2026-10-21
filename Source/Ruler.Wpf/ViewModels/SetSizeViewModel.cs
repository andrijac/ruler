using Ruler.Shared.Commands;
using Ruler.Shared.Enums;
using Ruler.Shared.Models;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Ruler.Wpf.ViewModels
{
    public class SetSizeViewModel : ViewModelBase
    {
        private readonly RulerInfo _targetRulerInfo;
        private double _widthInPixels;
        private double _heightInPixels;
        private readonly double _baseDpi;

        private string _widthText;
        public string WidthText
        {
            get => _widthText;
            set => SetProperty(ref _widthText, value);
        }

        private string _heightText;
        public string HeightText
        {
            get => _heightText;
            set => SetProperty(ref _heightText, value);
        }

        private RulerUnit _selectedUnit;
        public RulerUnit SelectedUnit
        {
            get => _selectedUnit;
            set
            {
                if (_selectedUnit != value)
                {
                    CommitTextValuesToPixels();
                    _selectedUnit = value;
                    OnPropertyChanged(nameof(SelectedUnit));
                    UpdateDisplayValues();
                }
            }
        }

        public IEnumerable<RulerUnit> AvailableUnits { get; }

        private string _widthLabel = "Width (px)";
        public string WidthLabel
        {
            get => _widthLabel;
            set => SetProperty(ref _widthLabel, value);
        }

        private string _heightLabel = "Height (px)";
        public string HeightLabel
        {
            get => _heightLabel;
            set => SetProperty(ref _heightLabel, value);
        }

        public int UpdatedWidth => (int)Math.Round(_widthInPixels);
        public int UpdatedHeight => (int)Math.Round(_heightInPixels);
        public RulerUnit UpdatedUnit => SelectedUnit;

        // Callback action to close the window and set DialogResult
        public Action<bool?> RequestClose { get; set; }

        public ICommand OkCommand { get; private set; }
        public ICommand CancelCommand { get; private set; }

        public SetSizeViewModel(RulerInfo targetRuler, double baseDpi)
        {
            _targetRulerInfo = targetRuler ?? throw new ArgumentNullException(nameof(targetRuler));
            _baseDpi = baseDpi;

            _widthInPixels = targetRuler.Width;
            _heightInPixels = targetRuler.Height;

            AvailableUnits = Enum.GetValues(typeof(RulerUnit)).Cast<RulerUnit>();
            _selectedUnit = targetRuler.RulerUnits;

            UpdateDisplayValues();
            InitializeCommands();
        }

        private void InitializeCommands()
        {
            OkCommand = new DelegateCommand(_ => {
                CommitTextValuesToPixels();
                RequestClose?.Invoke(true);
            });

            CancelCommand = new DelegateCommand(_ => {
                RequestClose?.Invoke(false);
            });
        }

        private void CommitTextValuesToPixels()
        {
            var unitConfig = MeasurementUnit.Get(_selectedUnit);

            if (double.TryParse(WidthText, NumberStyles.Any, CultureInfo.CurrentCulture, out double wVal))
            {
                _widthInPixels = unitConfig.UnitToPixel(wVal, _baseDpi, _widthInPixels);
            }

            if (double.TryParse(HeightText, NumberStyles.Any, CultureInfo.CurrentCulture, out double hVal))
            {
                _heightInPixels = unitConfig.UnitToPixel(hVal, _baseDpi, _heightInPixels);
            }
        }

        private void UpdateDisplayValues()
        {
            var unitConfig = MeasurementUnit.Get(_selectedUnit);

            double displayWidth = unitConfig.PixelToUnit(_widthInPixels, _baseDpi, _widthInPixels);
            double displayHeight = unitConfig.PixelToUnit(_heightInPixels, _baseDpi, _baseDpi);

            WidthLabel = $"Width ({unitConfig.Abbreviation})";
            HeightLabel = $"Height ({unitConfig.Abbreviation})";

            WidthText = displayWidth.ToString("0.##", CultureInfo.CurrentCulture);
            HeightText = displayHeight.ToString("0.##", CultureInfo.CurrentCulture);
        }
    }
}
