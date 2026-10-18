using Ruler.Shared.Enums;
using Ruler.Shared.Models;

using System;
using System.Collections.Generic;
using System.Globalization;
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
        private readonly RulerInfo _targetRulerInfo;
        private double _widthInPixels;
        private double _heightInPixels;
        private bool _isInitializing = true;

        // Expose properties for the MainWindow
        public int UpdatedWidth => (int)Math.Round(_widthInPixels);
        public int UpdatedHeight => (int)Math.Round(_heightInPixels);
        public RulerUnit UpdatedUnit { get; private set; }

        public SetSizeWindow(RulerInfo targetRuler)
        {
            InitializeComponent();
            _targetRulerInfo = targetRuler ?? throw new ArgumentNullException(nameof(targetRuler));

            // Cache dimensions in pixels internally
            _widthInPixels = targetRuler.Width;
            _heightInPixels = targetRuler.Height;

            cmbUnits.ItemsSource = Enum.GetValues(typeof(RulerUnit)).Cast<RulerUnit>();
            cmbUnits.SelectedItem = targetRuler.RulerUnits;
            UpdatedUnit = targetRuler.RulerUnits;

            _isInitializing = false;
            UpdateDisplayValues();
        }

        private void CmbUnits_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isInitializing || cmbUnits.SelectedItem == null) return;
            if (!(cmbUnits.SelectedItem is RulerUnit selectedUnit)) return;

            // Commit any current text input back to pixels before switching units
            CommitTextBoxesToPixels();

            UpdatedUnit = selectedUnit;
            UpdateDisplayValues();
        }

        private void CommitTextBoxesToPixels()
        {
            if (cmbUnits.SelectedItem == null || !(cmbUnits.SelectedItem is RulerUnit currentUnit)) return;

            var unitConfig = MeasurementUnit.Get(currentUnit);
            double baseDpi = 96.0 * VisualTreeHelper.GetDpi(this).PixelsPerDip;

            if (double.TryParse(txtWidth.Text, NumberStyles.Any, CultureInfo.CurrentCulture, out double wVal))
            {
                _widthInPixels = unitConfig.UnitToPixel(wVal, baseDpi, _widthInPixels);
            }

            if (double.TryParse(txtHeight.Text, NumberStyles.Any, CultureInfo.CurrentCulture, out double hVal))
            {
                _heightInPixels = unitConfig.UnitToPixel(hVal, baseDpi, _heightInPixels);
            }
        }

        private void UpdateDisplayValues()
        {
            if (cmbUnits.SelectedItem == null || !(cmbUnits.SelectedItem is RulerUnit currentUnit)) return;

            var unitConfig = MeasurementUnit.Get(currentUnit);
            double baseDpi = 96.0 * VisualTreeHelper.GetDpi(this).PixelsPerDip;

            // Convert internal pixel values to the newly selected unit
            double displayWidth = unitConfig.PixelToUnit(_widthInPixels, baseDpi, _widthInPixels);
            double displayHeight = unitConfig.PixelToUnit(_heightInPixels, baseDpi, _heightInPixels);

            // Dynamically update labels with the unit abbreviation (e.g., "Width (in)")
            lblWidth.Content = $"Width ({unitConfig.Abbreviation})";
            lblHeight.Content = $"Height ({unitConfig.Abbreviation})";

            // Update text boxes with formatted values
            txtWidth.Text = displayWidth.ToString("0.##", CultureInfo.CurrentCulture);
            txtHeight.Text = displayHeight.ToString("0.##", CultureInfo.CurrentCulture);
        }

        private void HandleTextBoxFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                textBox.SelectAll();
            }
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            CommitTextBoxesToPixels();
            UpdatedUnit = cmbUnits.SelectedItem is RulerUnit unit ? unit : _targetRulerInfo.RulerUnits;

            this.DialogResult = true; // Signals Success
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false; // Signals Cancel
        }
    }
}
