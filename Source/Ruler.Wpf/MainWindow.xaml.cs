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
    
                public MainWindow(RulerViewModel viewModel,ILoggingService loggingService)
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
            this.SourceInitialized += MainWindow_SourceInitialized;
            this.DataContext = viewModel;
            _loggingService = loggingService;
            this.Loaded += MainWindow_Loaded;
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
                Console.WriteLine($"Actual Width: {this.ActualWidth}");
                Console.WriteLine($"Left Margin compared Width: {this.Width/111}");
                
                Console.WriteLine($"this.top:  {this.Top}");
                if (this.DataContext is RulerViewModel viewModel && !viewModel.IsInitialized)
                {
                    
                    this.LocationChanged -= Window_LocationChanged;
                  //  ApplyDpiAwareMarginFix();
                    // Set the initialization flag

                    _viewModel = this.DataContext as RulerViewModel;
                    _loggingService.LogInfo("MainWindow loaded. Initializing size and position.");
                    // Use the ViewModel's stored DIU values directly.
                    // WPF handles the DPI scaling automatically.
                    Console.WriteLine($"viewmodel.LocationY:  {_viewModel.LocationY}");
                    this.Width = viewModel.Width;
                    this.Height = viewModel.Height;
                    this.Left = viewModel.LocationX;
                    this.Top = viewModel.LocationY;
                    Console.WriteLine($"displayed location.y {viewModel.DisplayedLocation.Y}");
                    Console.WriteLine($"this.top after set:  {this.Top}");
                    viewModel.ActualWidth = this.ActualWidth;
                    viewModel.IsInitialized = true;
                    this.LocationChanged += Window_LocationChanged;
                    if (_viewModel.IsVertical)
                    {
                        Console.WriteLine($"Canvas Height: {RulerCanvas.ActualWidth}");
                        Console.WriteLine($"Left Ruler widht: {LeftRuler.ActualWidth}");
                        Console.WriteLine($"Right Ruler widht: {RightRuler.ActualWidth}");

                    }
                    else
                    {
                        Console.WriteLine($"Canvas Height: {RulerCanvas.ActualHeight}");
                        Console.WriteLine($"Top Ruler widht: {TopRuler.ActualHeight}");
                        Console.WriteLine($"Bottom Ruler widht: {BottomRuler.ActualHeight}");
                    }
                }
            }
            catch (Exception ex)
            {
                var a = ex.Message;
            }
            Console.WriteLine($"Final Actual Width: {this.ActualWidth}");
            Console.WriteLine($"Ratio: {this.ActualWidth / 111}");
            Console.WriteLine($"Final Left Margin compared Width: {_viewModel.LeftMargin}");
        }

      

        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_isSizeChangingProgrammatically) return;

            if (_viewModel == null)
            {
                return;
            }
            if (_viewModel.IsVertical)
            {
                _viewModel.MiddleWidth = (RulerCanvas.ActualWidth - (LeftRuler.ActualWidth + RightRuler.ActualWidth));
            }
            else
            {
                _viewModel.MiddleWidth = (RulerCanvas.ActualHeight - (TopRuler.ActualHeight + BottomRuler.ActualHeight));
            }
            _viewModel.SetRulerDimensions(e.NewSize.Width, e.NewSize.Height);
            _viewModel.UpdateLocation(this.Left, this.Top);
            _loggingService.LogInfo($"Window size changed to {e.NewSize.Width}x{e.NewSize.Height}");
        }      
        private void MainWindow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _mouseDownPosition = e.GetPosition(this);
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                // If the user clicks anywhere, start the move operation.
                // The OS should automatically start resizing if the click occurred in the 
                // 10-pixel ResizeBorderThickness defined in WindowChrome.
                _loggingService.LogInfo("Mouse left button down - starting DragMove.");
                this.DragMove();
            }
        }
        private void Source_DpiChanged(object sender, DpiChangedEventArgs e)
        {
            Console.WriteLine($"DPI Changed from {e.OldDpi.DpiScaleY} to {e.NewDpi.DpiScaleY}. Reapplying margin fix.");
            // Recalculate and apply the margin correction
         //   ApplyDpiAwareMarginFix();
        }
        private void MainWindow_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            // Stops the active resize command and releases mouse capture.
            this.isMouseResizeCommand = false;
            this.resizeRegion = ResizeRegion.None;
            this.Cursor = Cursors.Arrow;
          //  this.Bordered.ReleaseMouseCapture();
            _loggingService.LogInfo("Mouse left button up - ending DragMove or Resize.");

            // If you used this.CaptureMouse() during the down event, call this.ReleaseMouseCapture()
            // DragMove() automatically handles mouse capture release.
        }
        private void MainWindow_SourceInitialized(object sender, EventArgs e)
        {
            // Get the window handle and set up the message loop override
            WindowInteropHelper helper = new WindowInteropHelper(this);
            HwndSource source = HwndSource.FromHwnd(helper.Handle);
           // 1. Hook up the window message handler for drag, lock, and move events
            source.AddHook(HwndHook);

            // 2. Subscribe to the DpiChanged event to fix the vertical ruler margin when the DPI changes
          //  source.DpiChanged += Source_DpiChanged;
        }

        //private void Source_DpiChanged(object sender, HwndDpiChangedEventArgs e)
        //{
        //    Console.WriteLine($"DPI Changed from {e.OldDpi.DpiScaleY} to {e.NewDpi.DpiScaleY}. Reapplying margin fix.");
        //    // Recalculate and apply the margin correction
        //   // ApplyDpiAwareMarginFix();
        //}

        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            // 1. --- WINDOW LOCKING/DRAGGING LOGIC (WM_NCHITTEST) ---
            if (msg == WM_NCHITTEST)
            {
                // If locked, we return HTCAPTION (Hit Test Caption Area).
                // This allows the user to drag the window by any client area, 
                // but since we are not returning HT* border values, resizing is disabled.
                if (_viewModel != null && _viewModel.IsLocked)
                {
                    handled = true;
                    return new IntPtr(HTCAPTION);
                }
            }

            // 2. --- LOCATION FIX LOGIC (WM_MOVE / WM_WINDOWPOSCHANGED) ---
            // These messages trigger when the window moves, requiring a position update.
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
        //private void Source_DpiChanged(object sender, DpiChangedEventArgs e)
        //{
        //    Console.WriteLine($"DPI Changed from {e.OldDpi.DpiScaleY} to {e.NewDpi.DpiScaleY}. Reapplying margin fix.");
        //    Recalculate and apply the margin correction
        //    ApplyDpiAwareMarginFix();
        //}
        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            // 1. Check for the critical message: Non-Client Hit Test
            if (msg == WM_NCHITTEST)
            {


                // 🛑 CRITICAL LOGIC: BLOCK RESIZE ONLY WHEN LOCKED 🛑
                if (_viewModel.IsLocked)
                {
                    // If locked, we tell Windows that the entire area should be treated
                    // as a non-interactive client area. 
                    // HTCLIENT blocks OS-level resizing and dragging.
                    handled = true;
                    return new IntPtr(HTCLIENT);
                }
                // Unpack the mouse coordinates (packed x, y screen coordinates)
                int x = (int)lParam & 0xFFFF;
                    int y = (int)lParam >> 16;

                    // Convert screen coordinates to WPF device-independent units (client area)
                    System.Windows.Point screenPoint = new System.Windows.Point(x, y);
                    System.Windows.Point clientPoint = this.PointFromScreen(screenPoint);

                    // Check if the cursor is within the defined resize border thickness
                    bool isNearHorizontalEdge =
                        (clientPoint.X <= EDGE_TOLERANCE) ||
                        (clientPoint.X >= this.ActualWidth - EDGE_TOLERANCE);

                    bool isNearVerticalEdge =
                        (clientPoint.Y <= EDGE_TOLERANCE) ||
                        (clientPoint.Y >= this.ActualHeight - EDGE_TOLERANCE);

                    bool isNearResizeHandle = isNearHorizontalEdge || isNearVerticalEdge;

                    //if (isNearResizeHandle)
                    //{
                    //    // If locked AND near a border, override the OS hit-test.
                    //    // Return HTCAPTION (Draggable Title Bar) to block resize but allow dragging.
                    //    handled = true;
                    //    return new IntPtr(HTCAPTION);
                    //}
                
                // --------------------------------------------------------
                // If the code reaches here, one of two things is true:
                // 1. The window is NOT locked (isLocked == false).
                // 2. The window IS locked, but the cursor is NOT near a border.
                // In both cases, we must allow the default WindowChrome logic to execute.
                // Therefore, we DO NOT set handled = true and proceed to the default return.
            }

            // Default return: Pass the message to the next handler, which is usually 
            // the default WindowProc, allowing WindowChrome to process resize, move, 
            // minimize, and maximize commands.
            return IntPtr.Zero;
        }

        /// <summary>
        /// Handles the mouse left button up event.
        /// This ends the drag-to-move or drag-to-resize operation.
        /// </summary>
        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            this.isMouseResizeCommand = false;
            this.resizeRegion = ResizeRegion.None;
            this.Cursor = Cursors.Arrow;
        }

        /// <summary>
        /// Handles a mouse double-click event.
        /// This can be used as a shortcut to toggle the ruler lock.
        /// </summary>
        protected override void OnMouseDoubleClick(MouseButtonEventArgs e)
        {
            base.OnMouseDoubleClick(e);

            if (e.LeftButton == MouseButtonState.Pressed)
            {
             _viewModel.ToggleVerticalCommand.Execute(null);
            }
        }
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _viewModel.MouseLeftButtonDown(e.GetPosition(this), e.OriginalSource as FrameworkElement);

            // Now, check the ViewModel's IsMoving state to see if we should start dragging.
            if (!_viewModel.IsLocked && !_viewModel.IsResizing)
            {
                this.DragMove();
            }
        }

        private void Border_MouseEnter(object sender, MouseEventArgs e)
        {
            this.Cursor = Cursors.SizeNESW;
        }

        private void Border_MouseLeave(object sender, MouseEventArgs e)
        {

        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void Border_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_viewModel is null)
            {
                return;
            }
            _viewModel.ActualWidth = e.NewSize.Width;
            Console.WriteLine(this.ActualWidth);
            Console.WriteLine($"left side margin {_viewModel.LeftMargin}");
            Console.WriteLine($"Width: {e.NewSize.Width} Height: {e.NewSize.Height}");
          //  _viewModel.TopRowHeight = this.TopRowDefinition.ActualHeight;
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

                        // Debug logging of the corrected values
                        Console.WriteLine($"Win32 Location Changed: Left={actualLeft}, Top={actualTop}");

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

        private void Window_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

        }

        private void Bordered_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _startPoint = e.GetPosition(Bordered);
            _isDragging = false;
            Bordered.CaptureMouse();
        }

        private void Bordered_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton ==  MouseButtonState.Pressed && Bordered.IsMouseCaptured)
            {
                Point currentPoint = e.GetPosition(Bordered);
                if (!_isDragging && (Math.Abs(currentPoint.X - _startPoint.X) > SystemParameters.MinimumHorizontalDragDistance ||
                                     Math.Abs(currentPoint.Y - _startPoint.Y) > SystemParameters.MinimumVerticalDragDistance))
                {
                    _isDragging = true;
                    Bordered.ReleaseMouseCapture();
                    this.DragMove();
                }

            }
        }

        private void Bordered_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Bordered.ReleaseMouseCapture();
            if (!_isDragging)
            {
                Point clickPoint = e.GetPosition(Bordered);
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
    }
}
