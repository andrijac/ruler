using Ruler.Shared.Enums;
using Ruler.Shared.Models;

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
    /// Interaction logic for RulerTooltipWindow.xaml
    /// </summary>
    public partial class RulerTooltipWindow : Window
    {
        private readonly RulerInfo _rulerInfo;
        public RulerTooltipWindow(RulerInfo rulerInfo)
        {
            InitializeComponent();
            _rulerInfo = rulerInfo;
        }
        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
        }
        public void UpdateMetrics(double width, double height, double dpi)
        {
            // Get the active measurement unit instance
            var unit = MeasurementUnit.Get(_rulerInfo.RulerUnits);

            // PixelToUnit signature: (pixelValue, dpi, totalWindowLength)
            double convertedWidth = unit.PixelToUnit(width, dpi, width);
            double convertedHeight = unit.PixelToUnit(height, dpi, height);
            double convertedGuide = unit.PixelToUnit(_rulerInfo.Guideline.Position, dpi, width);

            // Apply RulerScale if the unit is set to Pixels
            if (_rulerInfo.RulerUnits == RulerUnit.Pixels)
            {
                convertedWidth *= _rulerInfo.RulerScale;
                convertedHeight *= _rulerInfo.RulerScale;
                convertedGuide *= _rulerInfo.RulerScale;
            }

            string abbrev = unit.Abbreviation;

            SizeText.Text = $"{convertedWidth:0.0} x {convertedHeight:0.0} {abbrev}";
            if (_rulerInfo.Guideline.IsEnabled)
            {
                GuidelinePanel.Visibility = Visibility.Visible;
                GuidelineText.Text = $"{convertedGuide:0.0} {abbrev}";
            }
            else
            {
                GuidelinePanel.Visibility = Visibility.Collapsed;
            }
        }
    }
}
