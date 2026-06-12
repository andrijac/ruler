using Ruler.Wpf.ViewModels;

using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace Ruler.Wpf.Views
{
    /// <summary>
    /// A high-performance visual rendering control that paints custom tick marks 
    /// and dual-sided pixel labels using WPF's low-level drawing context api.
    /// </summary>
    public partial class RulerControl : UserControl
    {
        private const int LabelPadding = 22; // Mirrors legacy WinForms structural padding layout precisely
        private const int Threshold = 90;    // Width/Height boundary below which dual-sided labels merge to center

        // Setup shared design resources once to optimize allocation footprints
        private readonly Typeface _fontTypeface = new Typeface("Segoe UI");
        private readonly Pen _majorTickPen = new Pen(new SolidColorBrush(Color.FromRgb(0x70, 0x70, 0x70)), 1.5);
        private readonly Pen _minorTickPen = new Pen(new SolidColorBrush(Color.FromRgb(0x99, 0x99, 0x99)), 1.0);
        private readonly Brush _textBrush = new SolidColorBrush(Color.FromRgb(0x1A, 0x1A, 0x1A));

        public RulerControl()
        {
            InitializeComponent();

            // Re-render whenever the dimensions alter
            SizeChanged += (s, e) => this.InvalidateVisual(); ;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            // 1. Try local DataContext first
            RulerViewModel viewModel = DataContext as RulerViewModel;

            // 2. Fallback: If local context is null mid-frame, grab it directly from the Window
            if (viewModel == null)
            {
                Window parentWindow = Window.GetWindow(this);
                if (parentWindow != null)
                {
                    viewModel = parentWindow.DataContext as RulerViewModel;
                }
            }

            // 3. Absolute safety check
            if (viewModel == null)
                return;

            // Grab the dimensions directly from the verified data model, NOT the layout engine
            bool isVertical = viewModel.IsVertical;
            double totalLength = isVertical ? this.RenderSize.Height : this.RenderSize.Width;
            double crossDimension = isVertical ? this.RenderSize.Width : this.RenderSize.Height;

            StreamGeometry geometry = new StreamGeometry();

            using (StreamGeometryContext ctx = geometry.Open())
            {
                for (int pos = 0; pos <= totalLength; pos += 2)
                {
                    double tickLength = 0;

                    if (pos % 50 == 0)      // Major
                        tickLength = 15;
                    else if (pos % 10 == 0) // Medium
                        tickLength = 10;
                    else if (pos % 2 == 0)  // Minor
                        tickLength = 5;

                    if (tickLength > 0)
                    {
                        if (isVertical)
                        {
                            // Left Edge
                            ctx.BeginFigure(new Point(0, pos), false, false);
                            ctx.LineTo(new Point(tickLength, pos), true, false);

                            // Right Edge
                            ctx.BeginFigure(new Point(crossDimension, pos), false, false);
                            ctx.LineTo(new Point(crossDimension - tickLength, pos), true, false);
                        }
                        else
                        {
                            // Top Edge
                            ctx.BeginFigure(new Point(pos, 0), false, false);
                            ctx.LineTo(new Point(pos, tickLength), true, false);

                            // Bottom Edge
                            ctx.BeginFigure(new Point(pos, crossDimension), false, false);
                            ctx.LineTo(new Point(pos, crossDimension - tickLength), true, false);
                        }
                    }
                }
            }

            geometry.Freeze();

            Pen tickPen = new Pen(Brushes.Black, 1.0);
            tickPen.Freeze();
            drawingContext.DrawGeometry(null, tickPen, geometry);

            // Draw text ticks using the verified model positions
            for (int pos = 50; pos < totalLength; pos += 50)
            {
                DrawRulerText(drawingContext, pos.ToString(), pos, isVertical, crossDimension);
            }
        }

        private void DrawTick(DrawingContext dc, double position, double tickLength, Pen pen, bool isVertical, double crossDimension)
        {
            if (isVertical)
            {
                // Left Side Ticks
                dc.DrawLine(pen, new Point(0, position), new Point(tickLength, position));
                // Right Side Ticks
                dc.DrawLine(pen, new Point(crossDimension - tickLength, position), new Point(crossDimension, position));
            }
            else
            {
                // Top Side Ticks
                dc.DrawLine(pen, new Point(position, 0), new Point(position, tickLength));
                // Bottom Side Ticks
                dc.DrawLine(pen, new Point(position, crossDimension - tickLength), new Point(position, crossDimension));
            }
        }

        private void DrawRulerText(DrawingContext dc, string text, double pos, bool isVertical, double crossDimension)
        {
            // Build the device independent formatted text visual layout sequence
#pragma warning disable CS0618 // Type or member is obsolete but required for standard framework back-compat layouts
            FormattedText formattedText = new FormattedText(
                text,
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                _fontTypeface,
                10.5,
                _textBrush);
#pragma warning restore CS0618

            if (!isVertical)
            {
                if (crossDimension < Threshold)
                {
                    // If the ruler is extremely thin, center the text layout vertically
                    double centerX = pos - (formattedText.Width / 2);
                    double centerY = (crossDimension - formattedText.Height) / 2;
                    dc.DrawText(formattedText, new Point(centerX, centerY));
                }
                else
                {
                    // Dual-Sided rendering mapping tracks top and bottom placement targets
                    double centerX = pos - (formattedText.Width / 2);
                    dc.DrawText(formattedText, new Point(centerX, LabelPadding));
                    dc.DrawText(formattedText, new Point(centerX, crossDimension - LabelPadding - formattedText.Height));
                }
            }
            else // Vertical Directional Rendering Paths
            {
                if (crossDimension < Threshold)
                {
                    // Center horizontal positioning when window bounds compress tight
                    double centerX = (crossDimension - formattedText.Width) / 2;
                    double centerY = pos - (formattedText.Height / 2);
                    dc.DrawText(formattedText, new Point(centerX, centerY));
                }
                else
                {
                    // Split symmetrically to the left and right border regions
                    double centerY = pos - (formattedText.Height / 2);
                    dc.DrawText(formattedText, new Point(LabelPadding, centerY));
                    dc.DrawText(formattedText, new Point(crossDimension - LabelPadding - formattedText.Width, centerY));
                }
            }
        }

        /// <summary>
        /// Forces an explicit manual drawing loop invalidation pass sequence 
        /// whenever data bindings push state updates down from the view model.
        /// </summary>
        public void InvalidatesOnRender()
        {
            InvalidateVisual();
        }

    }
}

