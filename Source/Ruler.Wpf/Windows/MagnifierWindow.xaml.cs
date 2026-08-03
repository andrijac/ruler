using Ruler.Shared.Models;
using Ruler.Shared.Services;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
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
using System.Windows.Threading;

namespace Ruler.Wpf.Windows
{
    /// <summary>
    /// Interaction logic for MagnifierWindow.xaml
    /// </summary>
    public partial class MagnifierWindow : Window
    {
        private readonly RulerInfo _rulerData;
        private readonly MagnifierState _state;
        private MagnifierHost _magnifierHost;

        private readonly double _windowWidth = 150.0;
        private readonly double _windowHeight;

        private int _lastCursorX = -1;
        private int _lastCursorY = -1;

        public MagnifierWindow(RulerInfo rulerData)
        {
            InitializeComponent();

            _rulerData = rulerData ?? throw new ArgumentNullException(nameof(rulerData));
            _state = _rulerData.Magnifier;

            _windowHeight = _rulerData.Height / 2.0;

            this.Width = _windowWidth;
            this.Height = _windowHeight;

            var dpi = VisualTreeHelper.GetDpi(this);
            int physicalWidth = (int)(_windowWidth * dpi.DpiScaleX);
            int physicalHeight = (int)(_windowHeight * dpi.DpiScaleY);

            _magnifierHost = new MagnifierHost(physicalWidth, physicalHeight);
            HostContainer.Children.Add(_magnifierHost);

            float initialZoom = (float)(_state.ZoomLevel > 0 ? _state.ZoomLevel : 2.0f);
            SetZoom(initialZoom);

            if (_state is INotifyPropertyChanged inpc)
            {
                inpc.PropertyChanged += OnMagnifierStatePropertyChanged;
            }

            CompositionTarget.Rendering += OnRendering;
            this.Closed += (s, e) =>
            {
                CompositionTarget.Rendering -= OnRendering;
                if (_state is INotifyPropertyChanged notify)
                {
                    notify.PropertyChanged -= OnMagnifierStatePropertyChanged;
                }
            };
        }

        private void OnMagnifierStatePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MagnifierState.ZoomLevel))
            {
                // Force an immediate refresh when zoom changes, even if the mouse is stationary
                UpdateMagnifierPositionAndContent(forceUpdate: true);
            }
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            IntPtr hwnd = new WindowInteropHelper(this).Handle;

            int extendedStyle = NativeHelpers.GetWindowLong(hwnd, NativeHelpers.GWL_EXSTYLE);
            NativeHelpers.SetWindowLong(hwnd, NativeHelpers.GWL_EXSTYLE, extendedStyle | NativeHelpers.WS_EX_TRANSPARENT | NativeHelpers.WS_EX_LAYERED);

            NativeHelpers.GetClientRect(hwnd, out NativeHelpers.RECT clientRect);

            IntPtr hRgn = NativeHelpers.CreateEllipticRgn(0, 0, clientRect.Right, clientRect.Bottom);
            NativeHelpers.SetWindowRgn(hwnd, hRgn, true);
        }

        private void OnRendering(object sender, EventArgs e)
        {
            UpdateMagnifierPositionAndContent(forceUpdate: false);
        }

        private void UpdateMagnifierPositionAndContent(bool forceUpdate)
        {
            if (NativeHelpers.GetCursorPos(out NativeHelpers.POINT cursorPoint))
            {
                var dpi = VisualTreeHelper.GetDpi(this);
                double dpiX = dpi.DpiScaleX;
                double dpiY = dpi.DpiScaleY;

                // Check bounds against owner window using physical pixels + buffer
                if (Owner is Window ownerWindow)
                {
                    var ownerDpi = VisualTreeHelper.GetDpi(ownerWindow);
                    double ownerLeftPx = ownerWindow.Left * ownerDpi.DpiScaleX;
                    double ownerTopPx = ownerWindow.Top * ownerDpi.DpiScaleY;
                    double ownerWidthPx = ownerWindow.ActualWidth * ownerDpi.DpiScaleX;
                    double ownerHeightPx = ownerWindow.ActualHeight * ownerDpi.DpiScaleY;

                    double bufferPx = 30.0;
                    bool isNearRuler = cursorPoint.X >= (ownerLeftPx - bufferPx) &&
                                       cursorPoint.X <= (ownerLeftPx + ownerWidthPx + bufferPx) &&
                                       cursorPoint.Y >= (ownerTopPx - bufferPx) &&
                                       cursorPoint.Y <= (ownerTopPx + ownerHeightPx + bufferPx);

                    if (!isNearRuler)
                    {
                        if (this.Visibility != Visibility.Hidden)
                        {
                            this.Visibility = Visibility.Hidden;
                            _lastCursorX = -1;
                            _lastCursorY = -1;
                        }
                        return;
                    }
                }

                // Skip if mouse hasn't moved and we aren't forcing an update (prevents jitter)
                if (!forceUpdate && cursorPoint.X == _lastCursorX && cursorPoint.Y == _lastCursorY)
                {
                    return;
                }

                _lastCursorX = cursorPoint.X;
                _lastCursorY = cursorPoint.Y;

                this.Visibility = Visibility.Visible;

                // Center WPF window on mouse cursor
                this.Left = (cursorPoint.X / dpiX) - (_windowWidth / 2);
                this.Top = (cursorPoint.Y / dpiY) - (_windowHeight / 2);

                // Update zoom transform and source rectangle
                if (_magnifierHost?.MagnifierHandle != IntPtr.Zero)
                {
                    float currentZoom = (float)(_state.ZoomLevel > 0 ? _state.ZoomLevel : 2.0f);
                    Debug.WriteLine($"Current Zoom: {currentZoom}");
                    var matrix = new NativeHelpers.MAGTRANSFORM { v = new float[9] };
                    matrix.v[0] = currentZoom;
                    matrix.v[4] = currentZoom;
                    matrix.v[8] = 1.0f;
                    NativeHelpers.MagSetWindowTransform(_magnifierHost.MagnifierHandle, ref matrix);

                    int physicalWidth = (int)(_windowWidth * dpiX);
                    int physicalHeight = (int)(_windowHeight * dpiY);

                    int sourceWidth = (int)(physicalWidth / currentZoom);
                    int sourceHeight = (int)(physicalHeight / currentZoom);

                    int contentOffsetX = 0;
                    int captureCenterX = cursorPoint.X + contentOffsetX;
                    int captureCenterY = cursorPoint.Y;

                    var srcRect = new NativeHelpers.RECT
                    {
                        Left = captureCenterX - (sourceWidth / 2),
                        Top = captureCenterY - (sourceHeight / 2),
                        Right = captureCenterX + (sourceWidth / 2),
                        Bottom = captureCenterY + (sourceHeight / 2)
                    };

                    NativeHelpers.MagSetWindowSource(_magnifierHost.MagnifierHandle, srcRect);
                }
            }
        }

        public void SetZoom(float zoomFactor)
        {
            if (_magnifierHost?.MagnifierHandle != IntPtr.Zero)
            {
                var matrix = new NativeHelpers.MAGTRANSFORM { v = new float[9] };
                matrix.v[0] = zoomFactor;
                matrix.v[4] = zoomFactor;
                matrix.v[8] = 1.0f;

                NativeHelpers.MagSetWindowTransform(_magnifierHost.MagnifierHandle, ref matrix);
            }
        }
    }



}
