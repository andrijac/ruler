using Ruler.Shared.Interfaces;
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
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Ruler.Wpf.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IRuler
    {
        // Interop Constants
        private const int WM_SYSCOMMAND = 0x0112;
        private const int SC_SIZE = 0xF000;
        private const int WM_SIZING = 0x0214;

        private RulerInfo _rulerInfo;
        private HwndSource _hwndSource;

        public RulerInfo RulerData => _rulerInfo;
        public event EventHandler<RulerInfo> DuplicateRequested;

        // DI Dependencies
        private readonly IRulerFactory _rulerFactory;
        private readonly IMainFormFactory _mainFormFactory;
        private readonly IRulerRegistry _rulerRegistry;

        public MainWindow(RulerInfo info, IRulerFactory factory, IMainFormFactory mainFormFactory, IRulerRegistry rulerRegistry)
        {
            _rulerInfo = info;
            _rulerFactory = factory;
            _mainFormFactory = mainFormFactory;
            _rulerRegistry = rulerRegistry;

            InitializeComponent();

            // Set size based on info
            this.Width = _rulerInfo.Width;
            this.Height = _rulerInfo.Height;

            // Setup Interop for Locking/Resize logic
            this.SourceInitialized += (s, e) =>
            {
                _hwndSource = PresentationSource.FromVisual(this) as HwndSource;
                _hwndSource?.AddHook(WndProc);
            };
            this.InvalidateVisual();
        }

        // Equivalent to WinForms WndProc
        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (_rulerInfo.IsLocked)
            {
                if (msg == WM_SIZING)
                {
                    handled = true; // Block resize
                    return IntPtr.Zero;
                }
                if (msg == WM_SYSCOMMAND && (wParam.ToInt32() & 0xFFF0) == SC_SIZE)
                {
                    handled = true; // Block system menu resize
                    return IntPtr.Zero;
                }
            }
            return IntPtr.Zero;
        }

        // Equivalent to OnPaint
        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);
            
            dc.DrawRectangle(SystemColors.ControlBrush, null, new Rect(0, 0, this.ActualWidth, this.ActualHeight));
            // Force a high-contrast pen for testing
            var testPen = new Pen(Brushes.Red, 2.0);

            // Draw a single line to see if ANYTHING shows up
            dc.DrawLine(testPen, new Point(0, 0), new Point(400, 75));

            var testpen2 = new Pen(Brushes.Green, 2.0);
            dc.DrawLine(testpen2, new Point(0, this.Height), new Point(400, 0));
        
        // 1. Setup Pens and Resources once
        // Reusing the pen is more efficient than creating it in the loop
        var tickPen = new Pen(Brushes.Black, 1.0);
            var typeface = new Typeface("Arial");
            var dpi = VisualTreeHelper.GetDpi(this).PixelsPerDip;

            // 2. Choose orientation
            if (!_rulerInfo.IsVertical)
            {
                DrawHorizontalRuler(dc, tickPen, typeface, dpi);
            }
            else
            {
                DrawVerticalRuler(dc, tickPen, typeface, dpi);
            }
        }

        private void DrawHorizontalRuler(DrawingContext dc, Pen pen, Typeface tf, double dpi)
        {
            double width = this.Width;
            double height = this.Height;
            // Debug: Check if the loop is even running
            System.Diagnostics.Debug.WriteLine($"Ruler Size: {width} x {height}");
            for (int i = 0; i <= this.Width; i += 2)
            {
                // Logic from your original MainForm[cite: 4]
                int tickHeight = (i % 100 == 0) ? 15 : ((i % 10 == 0) ? 10 : 5);

                // Draw Ticks
                dc.DrawLine(pen, new Point(i, 0), new Point(i, tickHeight));
                dc.DrawLine(pen, new Point(i, this.Height), new Point(i, this.Height - tickHeight));

                // Draw Labels
                if (i % 100 == 0 || i == 85)
                {
                    DrawLabel(dc, i.ToString(), i, tf, dpi, true);
                }
            }
        }
        private void DrawVerticalRuler(DrawingContext dc, Pen pen, Typeface tf, double dpi)
        {
            for (int i = 0; i <= this.Height; i += 2)
            {
                // Logic from your original MainForm[cite: 4]
                int tickHeight = (i % 100 == 0) ? 15 : ((i % 10 == 0) ? 10 : 5);

                // Draw Ticks
                dc.DrawLine(pen, new Point(0, i), new Point(tickHeight, i));
                dc.DrawLine(pen, new Point(this.Width, i), new Point(this.Width - tickHeight,i));

                // Draw Labels
                if (i % 100 == 0 || i == 85)
                {
                    DrawLabel(dc, i.ToString(), i, tf, dpi, true);
                }
            }
        }
        private void DrawLabel(DrawingContext dc, string text, int pos, Typeface tf, double dpi, bool isHorizontal)
        {
            var formattedText = new FormattedText(
                text,
                System.Globalization.CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight,
                tf,
                12,
                Brushes.Black,
                dpi);

            // Calculate position (logic mirrored from original MainForm[cite: 4])
            double x = isHorizontal ? (pos - formattedText.Width / 2) : 22;
            double y = isHorizontal ? 22 : (pos - formattedText.Height / 2);

            dc.DrawText(formattedText, new Point(x, y));
        }

        // Mouse Events
        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove(); // WPF native helper for moving windows
        }

        // Implementation of IRuler
        public void SetRulerInfo(RulerInfo ruler)
        {
            _rulerInfo = ruler;
            this.InvalidateVisual(); // WPF equivalent of Invalidate()
        }

      

        public void InvalidateView()
        {
            throw new NotImplementedException();
        }

        public void SetBounds(int left, int top, int width, int height)
        {
            throw new NotImplementedException();
        }

        public void SetTooltip(string text)
        {
            throw new NotImplementedException();
        }

        public void ShowAbout()
        {
            throw new NotImplementedException();
        }

        public void ShowShortcuts()
        {
            throw new NotImplementedException();
        }

        public Task CheckForUpdatesAsync()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
