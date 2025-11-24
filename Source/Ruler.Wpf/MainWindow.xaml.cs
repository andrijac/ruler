using Ruler.Wpf.Common;
using Ruler.Wpf.Models;
using Ruler.Wpf.Services;
using Ruler.Wpf.ViewModels;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Ruler.Wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        // --- Win32 Interop Definitions ---
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        // This is exactly where you declare the external function:
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);
        // --- Win32 Interop Definitions for MonitorFromRect ---
        [DllImport("user32.dll")]
        private static extern IntPtr MonitorFromRect(ref RECT lprc, uint dwFlags);

        private RulerViewModel _viewModel;
        private bool _isSizeChangingProgrammatically = false;
        private bool _isMoving = false;
        private bool _isResizing = false;
        private const double EDGE_TOLERANCE = 5.0;
        private const int WM_NCHITTEST = 0x0084;
        private const int HTCLIENT = 0x0001;
        private const int HTNOWHERE = 0x0000;
        // Resize HitTest codes (You MUST use these original codes for detection)
        private const int HTTOPLEFT = 13;
        private const int HTTOPRIGHT = 14;
        private const int HTBOTTOMLEFT = 16;
        private const int HTBOTTOMRIGHT = 17;
        private const int HTLEFT = 10;
        private const int HTRIGHT = 11;
        private const int HTTOP = 12;
        private const int HTBOTTOM = 15;
        private const int SYSTEM_WINDOW_BORDER_SIZE = 8;
        private const double WINDOW_NON_CLIENT_OFFSET_DIP = 8.0;
        private Point _mouseDownPosition;
        private Point _mouseUpPosition;

        // Win32 Message Constants
        private const int HTCAPTION = 0x0002;            // Hit Test Caption (allows dragging)
        private const int WM_WINDOWPOSCHANGED = 0x0047;  // Sent after the size/position is changed
        private const int WM_MOVE = 0x0003;
        private ILoggingService _loggingService;    
        // Points used for calculating delta movements.
        private Point _startPoint;
        private bool isMouseResizeCommand = false;
        private ResizeRegion resizeRegion = ResizeRegion.None;
        // Flag to track if a drag operation has started
        private bool _isDragging = false;

        public MainWindow(RulerViewModel viewModel, ILoggingService loggingService)
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
            this.SourceInitialized += MainWindow_SourceInitialized;
            this.DataContext = viewModel;
            _loggingService = loggingService;
            this.Loaded += MainWindow_Loaded;
            RECT rulerarea = GetWindowRectFromWpf();
            bool isVisible = IsWindowVisible(rulerarea);
            if (!isVisible)
            {
                _loggingService.LogInfo("Ruler is NOT visible on any monitor.");
                _viewModel.DisplayedLocation = new Point(0, 0);
                _viewModel.LocationX = 0;
                _viewModel.LocationY = 0;
            }
        }
        //private void ApplyDpiAwareMarginFix()
        //{
        //    PresentationSource source = PresentationSource.FromVisual(this);

        //    if (source?.CompositionTarget != null)
        //    {
        //        // Get the scale factor for diagnostic logging only
        //        Matrix matrix = source.CompositionTarget.TransformToDevice;
        //        double scaleFactor = matrix.M22;

        //        // CRITICAL FIX: The margin must be a constant -8 DIPs (WPF units), 
        //        // regardless of the DPI scale factor, because WPF handles the scaling internally.
        //        double dynamicMargin = -WINDOW_NON_CLIENT_OFFSET_DIP;

        //        // Apply the calculated margin to the ItemsControls               
        //        this.RightRuler.Margin = new Thickness(0, dynamicMargin, 0, 0);

        //        Console.WriteLine($"DPI Scale Factor: {scaleFactor}. Applied Top Margin Fix: {dynamicMargin} DIPs");
        //    }
        //}

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
     
                if (this.DataContext is RulerViewModel viewModel && !viewModel.IsInitialized)
                {
                    
                    this.LocationChanged -= Window_LocationChanged;
                  //  ApplyDpiAwareMarginFix();
                    // Set the initialization flag

                    _viewModel = this.DataContext as RulerViewModel;
                    _loggingService.LogInfo("MainWindow loaded. Initializing size and position.");
                    // Use the ViewModel's stored DIU values directly.
                    // WPF handles the DPI scaling automatically.
                    this.Width = viewModel.Width;
                    this.Height = viewModel.Height;
                    this.Left = viewModel.LocationX;
                    this.Top = viewModel.LocationY;
     
                    
                    viewModel.IsInitialized = true;
                    this.LocationChanged += Window_LocationChanged;
                    Console.WriteLine($"Canvas Actual Height: {RulerCanvas.ActualHeight}, Actual Width: {RulerCanvas.ActualWidth}");
                  
                }
            }
            catch (Exception ex)
            {
                var a = ex.Message;
            }
   
        }
        private const int WM_RBUTTONDOWN = 0x0204;
        private const int WM_CONTEXTMENU = 0x007B;
        private void MainWindow_SourceInitialized(object sender, EventArgs e)
        {
            // Get the window handle and set up the message loop override
            WindowInteropHelper helper = new WindowInteropHelper(this);
            HwndSource source = HwndSource.FromHwnd(helper.Handle);
           // 1. Hook up the window message handler for drag, lock, and move events
            source?.AddHook(HwndHook);          
            // 2. Subscribe to the DpiChanged event to fix the vertical ruler margin when the DPI changes
          //  source.DpiChanged += Source_DpiChanged;
        }
        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {          
            if (msg == WM_MOVE || msg == WM_WINDOWPOSCHANGED)
            {
                Window window = (Window)HwndSource.FromHwnd(hwnd).RootVisual;
                if (window != null)
                {
                    // This is the custom method that fixes the location reporting issue
                    GetDIPLocationFromWin32Rect(hwnd, window);
                }
            }

            return IntPtr.Zero;
        }
        private void GetDIPLocationFromWin32Rect(IntPtr hwnd, Window window)
        {
            // 1. Get the window's physical location in screen pixels using the native GetWindowRect
            if (GetWindowRect(hwnd, out RECT rect))
            {
                HwndSource source = PresentationSource.FromVisual(window) as HwndSource;

                if (source != null)
                {
                    // 2. Get the Transformation Matrix (Device Pixels -> DIPs)
                    if (source.CompositionTarget != null)
                    {
                        // TransformFromDevice maps screen pixels to WPF units (DIPs)
                        Matrix matrix = source.CompositionTarget.TransformFromDevice;

                        // 3. Apply the transformation matrix to the Left/Top pixel coordinates
                        Point locationInDIP = matrix.Transform(new Point(rect.Left, rect.Top));

                        double actualLeft = locationInDIP.X;
                        double actualTop = locationInDIP.Y;

                        // Debug logging of the corrected values
                        Console.WriteLine($"Win32 Location Changed: Left={actualLeft}, Top={actualTop}");

                        // 4. Manually write the corrected position back to the ViewModel properties.
                        _viewModel.UpdateLocation(actualLeft, actualTop);
                    }
                }
                // Fallback in case HwndSource is not available
                else
                {
                    Console.WriteLine($"HwndSource not found. Falling back to WPF: Left={window.Left}, Top={window.Top}");
                    _viewModel.UpdateLocation(window.Left, window.Top);
                }
            }
        }  

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_viewModel is null)
            {
                return;
            }                
           _viewModel.SetRulerDimensions(e.NewSize.Width, e.NewSize.Height);
        }

        private void Window_LocationChanged(object sender, EventArgs e)
        {
            if (_viewModel != null)
            {
                Window window = (Window)sender;
               

                IntPtr windowHandle = new WindowInteropHelper(window).Handle;

                // 1. Call the native Windows API to get the true screen position in pixels
                if (GetWindowRect(windowHandle, out RECT rect))
                {
                    // 2. Convert the Win32 pixel coordinates to WPF's Device Independent Pixels (DIPs)
                    PresentationSource source = PresentationSource.FromVisual(window);

                    if (source != null && source.CompositionTarget != null)
                    {
                        Matrix matrix = source.CompositionTarget.TransformFromDevice;

                        // Apply the transformation matrix to the Left/Top pixel coordinates
                        Point locationInDIP = matrix.Transform(new Point(rect.Left, rect.Top));

                        double actualLeft = locationInDIP.X;
                        double actualTop = locationInDIP.Y;                                             

                        // 3. Manually write the window's current screen position back to the ViewModel properties.
                        _viewModel.UpdateLocation(actualLeft, actualTop);
                        //_viewModel.LocationX = actualLeft;
                        //_viewModel.LocationY = actualTop;

                    }
                }
                // If the Win32 call fails, fall back to WPF properties (will likely still be 0/78)
                else
                {
                    Console.WriteLine($"Win32 API Failed. Falling back to WPF: Left={window.Left}, Top={window.Top}");
                    _viewModel.UpdateLocation(window.Left, window.Top);
                    //_viewModel.LocationX = window.Left;
                    //_viewModel.LocationY = window.Top;
                }
            }
        }
        private const int MONITOR_DEFAULTTONULL = 0x00000000;

        private static bool IsWindowVisible(RECT rect)
        {
            // MonitorFromRect returns a handle (IntPtr) to the display monitor
            // that intersects the rectangle. We use MONITOR_DEFAULTTONULL (0)
            // so it returns NULL if the rectangle does not intersect any display monitor.
            IntPtr monitorHandle = MonitorFromRect(ref rect, MONITOR_DEFAULTTONULL);

            // If the handle is not zero (NULL), a monitor was found, meaning the window is visible.
            return monitorHandle != IntPtr.Zero;
        }

        /// <summary>
        /// Helper to convert WPF position/size to native RECT.
        /// </summary>
        private RECT GetWindowRectFromWpf()
        {
            return new RECT
            {
                Left = (int)this.Left,
                Top = (int)this.Top,
                Right = (int)(this.Left + this.Width),
                Bottom = (int)(this.Top + this.Height)
            };
        }

        private void RulerCanvas_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && RulerCanvas.IsMouseCaptured)
            {
                Point currentPoint = e.GetPosition(RulerCanvas);
                if (!_isDragging && (Math.Abs(currentPoint.X - _startPoint.X) > SystemParameters.MinimumHorizontalDragDistance ||
                                     Math.Abs(currentPoint.Y - _startPoint.Y) > SystemParameters.MinimumVerticalDragDistance))
                {
                    _isDragging = true;
                    RulerCanvas.ReleaseMouseCapture();
                    this.DragMove();
                }

            }
        }
        private void RulerCanvas_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            RulerCanvas.ReleaseMouseCapture();
            if (!_isDragging)
            {
                Point clickPoint = e.GetPosition(RulerCanvas);
                double position;
                if (_viewModel.IsVertical)
                {
                    position = clickPoint.Y;
                }
                else
                {
                    position = clickPoint.X;
                }
                // Update the ViewModel's guide line property
                _viewModel.SetGuideLinePosition(position);
                // Also ensure the line is visible
                _viewModel.IsGuideLineVisible = true;
            }

        }
        private void RulerCanvas_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _startPoint = e.GetPosition(RulerCanvas);
            _isDragging = false;
            RulerCanvas.CaptureMouse();
        }
        private void RulerCanvas_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_viewModel != null && _viewModel.IsLocked)
            {
                // 1. Mark the event as handled to prevent it from propagating further up 
                //    to the Window/System level where it might be consumed.
                e.Handled = true;

                if (RulerCanvas.ContextMenu != null)
                {
                    // 2. Manually set the placement target to the canvas itself
                    RulerCanvas.ContextMenu.PlacementTarget = RulerCanvas;

                    // 3. Set the position of the menu to the current mouse click position
                    Point clickPoint = e.GetPosition(RulerCanvas);
                    RulerCanvas.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.AbsolutePoint;
                    RulerCanvas.ContextMenu.HorizontalOffset = clickPoint.X;
                    RulerCanvas.ContextMenu.VerticalOffset = clickPoint.Y;

                    // 4. Open the ContextMenu
                    RulerCanvas.ContextMenu.IsOpen = true;
                }
            }
        }
    }
}
