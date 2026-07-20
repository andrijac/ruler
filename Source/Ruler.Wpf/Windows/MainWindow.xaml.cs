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
        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            // Use drawingContext.DrawLine, drawingContext.DrawText
            // NOTE: You will need to rewrite your DrawHorizontalRuler/DrawVerticalRuler logic 
            // to use DrawingContext primitives instead of System.Drawing.Graphics
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
