using Ruler.Shared.Enums;
using Ruler.Shared.Models;

using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Ruler.Wpf.Controls
{
    /// <summary>
    /// Interaction logic for RulerInfoDisplay.xaml
    /// </summary>
    public partial class RulerInfoDisplay : UserControl
    {
        private double _targetWidth;
        private double _targetHeight;
        private double _effectiveDpi = 96.0;

        public static readonly DependencyProperty RulerDataProperty =
            DependencyProperty.Register(
                nameof(RulerData),
                typeof(RulerInfo),
                typeof(RulerInfoDisplay),
                new PropertyMetadata(null, OnRulerDataChanged));

        public RulerInfo RulerData
        {
            get => (RulerInfo)GetValue(RulerDataProperty);
            set => SetValue(RulerDataProperty, value);
        }

        public RulerInfoDisplay()
        {
            InitializeComponent();
            UpdateVisibility();
        }

        public void UpdateMetrics(double width, double height, double dpi)
        {
            _targetWidth = width;
            _targetHeight = height;
            _effectiveDpi = dpi;
            UpdateVisibility();
            InvalidateVisual();
            InvalidateMeasure();
        }

        private static void OnRulerDataChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is RulerInfoDisplay control)
            {
                if (e.OldValue is INotifyPropertyChanged oldModel)
                {
                    oldModel.PropertyChanged -= control.RulerData_PropertyChanged;
                }

                if (e.NewValue is INotifyPropertyChanged newModel)
                {
                    newModel.PropertyChanged += control.RulerData_PropertyChanged;
                }
                control.UpdateVisibility();
                control.InvalidateVisual();
                control.InvalidateMeasure();
            }
        }

        private void RulerData_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            UpdateVisibility();
            InvalidateVisual();
            InvalidateMeasure();
        }

        private string GetUnitDisplayString(MeasurementUnit unitConfig, double unitValue)
        {
            string text;

            // Check if the active unit natively supports fractions
            if (unitConfig.SupportsFractions && unitConfig.FormatFraction != null)
            {
                text = unitConfig.FormatFraction(unitValue);
            }
            else
            {
                text = unitConfig.FormatLabel != null
                    ? unitConfig.FormatLabel(unitValue, RulerData.IsVertical)
                    : $"{unitValue:0.##} {unitConfig.Abbreviation}";
            }

            if (RulerData.RulerUnits == RulerUnit.Pixels && !text.Contains("px"))
            {
                text += " px";
            }

            return text;
        }
        private void UpdateVisibility()
        {
            if (RulerData == null)
            {
                Visibility = Visibility.Collapsed;
                return;
            }

            // Exclude Centimeters, Millimeters, and Points
            switch (RulerData.RulerUnits)
            {
                case RulerUnit.Centimeters:
                case RulerUnit.Millimeters:
                case RulerUnit.Points:
                    Visibility = Visibility.Collapsed;
                    break;
                default:
                    Visibility = Visibility.Visible;
                    break;
            }
        }
        private double MeasureCustomTextWidth(Typeface typeface, double fontSize, string labelText, double scaleFactor)
        {
            string[] parts = labelText.Split(' ');
            double totalWidth = 0;
            double spacing = 2.0; // Tighter spacing between parts

            for (int i = 0; i < parts.Length; i++)
            {
                string part = parts[i];
                if (part.Contains("/"))
                {
                    string[] fracParts = part.Split('/');
                    if (fracParts.Length == 2)
                    {
                        double subFontSize = fontSize * 0.75;
                        var numText = new FormattedText(fracParts[0], CultureInfo.CurrentCulture, FlowDirection.LeftToRight, typeface, subFontSize, Brushes.Black, scaleFactor);
                        var denText = new FormattedText(fracParts[1], CultureInfo.CurrentCulture, FlowDirection.LeftToRight, typeface, subFontSize, Brushes.Black, scaleFactor);

                        double fracWidth = Math.Max(numText.Width, denText.Width) + 2.0;
                        totalWidth += fracWidth;

                        // Add spacing if not the last item
                        if (i < parts.Length - 1) totalWidth += spacing;
                        continue;
                    }
                }

                var formatted = new FormattedText(part, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, typeface, fontSize, Brushes.Black, scaleFactor);
                totalWidth += formatted.Width;

                // Add spacing if not the last item
                if (i < parts.Length - 1) totalWidth += spacing;
            }

            return totalWidth;
        }
        private void DrawFractionText(DrawingContext dc, Typeface typeface, double fontSize, Brush brush, string labelText, Point origin, double scaleFactor)
        {
            string[] parts = labelText.Split(' ');
            double currentX = origin.X;
            double spacing = 2.0; // Tighter spacing between parts

            for (int i = 0; i < parts.Length; i++)
            {
                string part = parts[i];
                if (part.Contains("/"))
                {
                    string[] fracParts = part.Split('/');
                    if (fracParts.Length == 2)
                    {
                        string num = fracParts[0];
                        string den = fracParts[1];

                        double subFontSize = fontSize * 0.75;
                        var numText = new FormattedText(num, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, typeface, subFontSize, brush, scaleFactor);
                        var denText = new FormattedText(den, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, typeface, subFontSize, brush, scaleFactor);

                        double fracWidth = Math.Max(numText.Width, denText.Width) + 2.0;

                        // Draw Numerator (Top)
                        dc.DrawText(numText, new Point(currentX + (fracWidth - numText.Width) / 2.0, origin.Y - 2));

                        // Draw Horizontal Fraction Bar
                        var linePen = new Pen(brush, 1.0);
                        double barY = origin.Y + (fontSize / 2.0);
                        dc.DrawLine(linePen, new Point(currentX, barY), new Point(currentX + fracWidth, barY));

                        // Draw Denominator (Bottom)
                        dc.DrawText(denText, new Point(currentX + (fracWidth - denText.Width) / 2.0, barY + 1));

                        currentX += fracWidth;
                        if (i < parts.Length - 1) currentX += spacing;
                        continue;
                    }
                }

                var formatted = new FormattedText(part, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, typeface, fontSize, brush, scaleFactor);
                dc.DrawText(formatted, new Point(currentX, origin.Y));

                currentX += formatted.Width;
                if (i < parts.Length - 1) currentX += spacing;
            }
        }
        protected override Size MeasureOverride(Size availableSize)
        {
            if (RulerData == null || _targetWidth <= 0 || _targetHeight <= 0)
                return new Size(150, 25);

            var unitConfig = MeasurementUnit.Get(RulerData.RulerUnits);

            double totalPixels = RulerData.IsVertical ? _targetHeight : _targetWidth;
            if (RulerData.RulerUnits == RulerUnit.Pixels && RulerData.RulerScale > 0)
            {
                totalPixels /= RulerData.RulerScale;
            }

            double totalUnitValue = unitConfig.PixelToUnit(totalPixels, _effectiveDpi, totalPixels);
            var typeface = new Typeface("Segoe UI");
            double textScaleFactor = _effectiveDpi / 96.0;
            double fontSize = 9;

            // 1. Measure Line 1 (Size)
            var sizeLabelText = new FormattedText("Size: ", CultureInfo.CurrentCulture, FlowDirection.LeftToRight, typeface, fontSize, Brushes.Black, textScaleFactor);
            string sizeStr = GetUnitDisplayString(unitConfig, totalUnitValue);
            double sizeValWidth = MeasureCustomTextWidth(typeface, fontSize, sizeStr, textScaleFactor);

            double line1Width = 6.0 + sizeLabelText.Width + sizeValWidth + 6.0;
            double maxWidth = line1Width;
            double totalHeight = Math.Max(22.0, sizeLabelText.Height + 8);

            // 2. Measure Line 2 (Guideline) if enabled
            bool hasGuideline = RulerData.Guideline != null && RulerData.Guideline.IsEnabled;
            if (hasGuideline)
            {
                string lockStatus = "Guideline: ";
                var guidePrefixText = new FormattedText(lockStatus, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, typeface, fontSize, Brushes.Black, textScaleFactor);

                double guidePixels = RulerData.Guideline.Position;
                if (RulerData.RulerUnits == RulerUnit.Pixels && RulerData.RulerScale > 0)
                {
                    guidePixels /= RulerData.RulerScale;
                }

                double guideUnitValue = unitConfig.PixelToUnit(guidePixels, _effectiveDpi, totalPixels);
                string guideStr = GetUnitDisplayString(unitConfig, guideUnitValue);
                double guideValWidth = MeasureCustomTextWidth(typeface, fontSize, guideStr, textScaleFactor);

                double line2Width = 6.0 + guidePrefixText.Width + guideValWidth + 6.0;
                maxWidth = Math.Max(line1Width, line2Width);
                totalHeight = (sizeLabelText.Height * 2) + 12; // Expand height for two stacked lines
            }

            return new Size(maxWidth, totalHeight);
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            if (RulerData == null || _targetWidth <= 0 || _targetHeight <= 0)
                return;

            var unitConfig = MeasurementUnit.Get(RulerData.RulerUnits);

            double totalPixels = RulerData.IsVertical ? _targetHeight : _targetWidth;
            if (RulerData.RulerUnits == RulerUnit.Pixels && RulerData.RulerScale > 0)
            {
                totalPixels /= RulerData.RulerScale;
            }

            double totalUnitValue = unitConfig.PixelToUnit(totalPixels, _effectiveDpi, totalPixels);

            var typeface = new Typeface("Segoe UI");
            var textBrush = Brushes.Black;
            var grayBrush = Brushes.Gray;
            double textScaleFactor = _effectiveDpi / 96.0;
            double fontSize = 9;

            bool hasGuideline = RulerData.Guideline != null && RulerData.Guideline.IsEnabled;

            var sizeLabelText = new FormattedText("Size: ", CultureInfo.CurrentCulture, FlowDirection.LeftToRight, typeface, fontSize, textBrush, textScaleFactor);
            string sizeStr = $" {GetUnitDisplayString(unitConfig, totalUnitValue)}";

            if (!hasGuideline)
            {
                // Single line layout (Guideline disabled)
                double centerY = ActualHeight > 0 ? ActualHeight / 2.0 : 11.0;
                double currentX = 6.0;

                drawingContext.DrawText(sizeLabelText, new Point(currentX, centerY - (sizeLabelText.Height / 2)));
                currentX += sizeLabelText.Width;
                DrawFractionText(drawingContext, typeface, fontSize, textBrush, sizeStr, new Point(currentX, centerY - (sizeLabelText.Height / 2)), textScaleFactor);
            }
            else
            {
                // Two-line layout (Guideline enabled)
                double lineHeight = sizeLabelText.Height;
                double spacing = 3.0;
                double totalBlockHeight = (lineHeight * 2) + spacing;
                double startY = Math.Max(2.0, (ActualHeight - totalBlockHeight) / 2.0);

                // Line 1: Size
                double currentX1 = 6.0;
                drawingContext.DrawText(sizeLabelText, new Point(currentX1, startY));
                currentX1 += sizeLabelText.Width;
                DrawFractionText(drawingContext, typeface, fontSize, textBrush, sizeStr, new Point(currentX1, startY), textScaleFactor);

                // Line 2: Guideline
                string lockStatus = "Guideline: ";
                var guidePrefixText = new FormattedText(lockStatus, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, typeface, fontSize, textBrush, textScaleFactor);

                double guidePixels = RulerData.Guideline.Position;
                if (RulerData.RulerUnits == RulerUnit.Pixels && RulerData.RulerScale > 0)
                {
                    guidePixels /= RulerData.RulerScale;
                }

                double guideUnitValue = unitConfig.PixelToUnit(guidePixels, _effectiveDpi, totalPixels);
                string guideStr = GetUnitDisplayString(unitConfig, guideUnitValue);

                double currentX2 = 6.0;
                double line2Y = startY + lineHeight + spacing;
                drawingContext.DrawText(guidePrefixText, new Point(currentX2, line2Y));
                currentX2 += guidePrefixText.Width;
                DrawFractionText(drawingContext, typeface, fontSize, textBrush, guideStr, new Point(currentX2, line2Y), textScaleFactor);
            }
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            InvalidateVisual();
        }
    }
}
