using Ruler.Wpf.Common;
using Ruler.Wpf.Models;
using Ruler.Wpf.ViewModels;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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
        private RulerViewModel _viewModel;
        private bool _isSizeChangingProgrammatically = false;
        private bool _isMoving = false;
        private bool _isResizing = false;
        private const double EDGE_TOLERANCE = 5.0;

        // Points used for calculating delta movements.
        private Point _startPoint;
        private bool isMouseResizeCommand = false;
        private ResizeRegion resizeRegion = ResizeRegion.None;

        public MainWindow()
        {
            InitializeComponent();           
            this.Loaded += MainWindow_Loaded;
        }
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
              if (this.DataContext is RulerViewModel viewModel && !viewModel.IsInitialized)
                {

                    // Set the initialization flag
                  
                    _viewModel= this.DataContext as RulerViewModel;
                    // Use the ViewModel's stored DIU values directly.
                    // WPF handles the DPI scaling automatically.

                    this.Width = viewModel.Width;
                    this.Height = viewModel.Height;
                    this.Left = viewModel.DisplayedLocation.X;
                    this.Top = viewModel.DisplayedLocation.Y;
                    viewModel.IsInitialized = true;
                }
            }
            catch (Exception ex)
            {
                var a = ex.Message;
            }
        }
        public ResizeRegion GetResizeRegion(Point clientCursorPos, double controlWidth, double controlHeight)
        {
            ResizeRegion region = ResizeRegion.None;

            // Check corners first (TopLeft, TopRight, etc.)
            bool nearLeft = clientCursorPos.X <= EDGE_TOLERANCE;
            bool nearRight = clientCursorPos.X >= controlWidth - EDGE_TOLERANCE;
            bool nearTop = clientCursorPos.Y <= EDGE_TOLERANCE;
            bool nearBottom = clientCursorPos.Y >= controlHeight - EDGE_TOLERANCE;

            // --- Corner Checks ---
            if (nearTop && nearLeft)
            {
                region = ResizeRegion.TopLeft;
            }
            else if (nearTop && nearRight)
            {
                region = ResizeRegion.TopRight;
            }
            else if (nearBottom && nearLeft)
            {
                region = ResizeRegion.BottomLeft;
            }
            else if (nearBottom && nearRight)
            {
                region = ResizeRegion.BottomRight;
            }
            // --- Edge Checks (Only if not a corner) ---
            else if (nearLeft)
            {
                region = ResizeRegion.Left;
            }
            else if (nearRight)
            {
                region = ResizeRegion.Right;
            }
            else if (nearTop)
            {
                region = ResizeRegion.Top;
            }
            else if (nearBottom)
            {
                region = ResizeRegion.Bottom;
            }

            return region;
        }

        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_isSizeChangingProgrammatically) return;

            if (_viewModel==null)
            {
                return;
            }
            _viewModel.SetRulerDimensions(e.NewSize.Width, e.NewSize.Height);
            _viewModel.UpdateLocation(this.Left, this.Top);
        }

        /// <summary>
        /// Handles the mouse left button down event.
        /// This is the start of a drag-to-move or drag-to-resize operation.
        /// </summary>
        //protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        //{
        //    // If the ruler is locked, we don't allow any operations.
        //    if (_viewModel.IsLocked) return;

        //    // Get the position of the mouse relative to the window.
        //    _startPoint = e.GetPosition(this);

        //    // Check if the user clicked on the resizing area.
        //    if (e.OriginalSource is FrameworkElement element && element.Name == "resizingArea")
        //    {
        //        _isResizing = true;
        //        this.CaptureMouse();
        //    }
        //    // Check if the user clicked on the main ruler area to start moving.
        //    else if (e.OriginalSource is FrameworkElement && ((FrameworkElement)e.OriginalSource).Name == "rulerBorder")
        //    {
        //        _isMoving = true;
        //        this.CaptureMouse();
        //    }
        //}

        /// <summary>
        /// Handles the mouse move event.
        /// This is the main logic for moving and resizing the window.
        /// </summary>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            FrameworkElement surface = (FrameworkElement)Bordered;

            // If the mouse button is pressed and we've started a resize, the OS is handling it. Exit.
            if (e.LeftButton == MouseButtonState.Pressed && this.isMouseResizeCommand)
            {
                return;
            }

            // 1. Get the current region for hover feedback
            Point clientCursorPos = e.GetPosition(surface);
            ResizeRegion region = GetResizeRegion(
                clientCursorPos,
                surface.ActualWidth,
                surface.ActualHeight
            );

            // 2. Set the appropriate cursor
            if (region != ResizeRegion.None)
            {
                SetCursorForResizeRegion(region);
            }
            else
            {
                // Only reset to the default if the left button isn't pressed (i.e., not dragging)
                if (e.LeftButton == MouseButtonState.Released)
                {
                    this.Cursor = Cursors.Arrow;
                }
            }

            // Update the temporary hover state
            this.resizeRegion = region;
        }
        private void HandleResize()
        {
            // 1. Get the native window handle (HWND)
            IntPtr windowHandle = new WindowInteropHelper(this).Handle;

            // 2. Translate the custom enum to the native hit-test code
            int hitTestCode = GetHitTest(this.resizeRegion);

            if (hitTestCode != -1)
            {
                // 3. Send the message to the OS to start resizing
                // The message is WM_SYSCOMMAND, wParam is SC_SIZE, and lParam is the hit test code.
                NativeMethods.SendMessage(
                    windowHandle,
                    NativeMethods.WM_SYSCOMMAND,
                    (IntPtr)(NativeMethods.SC_SIZE | hitTestCode),
                    IntPtr.Zero
                );
            }
        }
        private int GetHitTest(ResizeRegion region)
        {
            switch (region)
            {
                case ResizeRegion.Left: return NativeMethods.HTLEFT;
                case ResizeRegion.Right: return NativeMethods.HTRIGHT;
                case ResizeRegion.Top: return NativeMethods.HTTOP;
                case ResizeRegion.Bottom: return NativeMethods.HTBOTTOM;
                case ResizeRegion.TopLeft: return NativeMethods.HTTOPLEFT;
                case ResizeRegion.TopRight: return NativeMethods.HTTOPRIGHT;
                case ResizeRegion.BottomLeft: return NativeMethods.HTBOTTOMLEFT;
                case ResizeRegion.BottomRight: return NativeMethods.HTBOTTOMRIGHT;
                default: return -1; // Indicate no resizing
            }
        }

        private void SetCursorForResizeRegion(ResizeRegion region)
        {
            Cursor newCursor = Cursors.Arrow;
            switch (region)
            {
                case ResizeRegion.None:
                   newCursor = Cursors.Arrow;
                    break;
                case ResizeRegion.Left:
                    newCursor = Cursors.SizeWE;
                    break;
                case ResizeRegion.TopLeft:
                    newCursor = Cursors.SizeNWSE;
                    break;
                case ResizeRegion.Top:
                    newCursor = Cursors.SizeNS;
                    break;
                case ResizeRegion.TopRight:
                    newCursor = Cursors.SizeNESW;
                    break;
                case ResizeRegion.Right:
                    newCursor = Cursors.SizeWE;
                    break;
                case ResizeRegion.BottomRight:
                    newCursor = Cursors.SizeNWSE;
                    break;
                case ResizeRegion.Bottom:
                    newCursor = Cursors.SizeNS;
                    break;
                case ResizeRegion.BottomLeft:
                    newCursor = Cursors.SizeNESW;
                    break;
                default:
                    newCursor = Cursors.Arrow;
                    break;
            }
            this.Cursor = newCursor;
        }
        private void MainWindow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                // If the user clicks anywhere, start the move operation.
                // The OS should automatically start resizing if the click occurred in the 
                // 10-pixel ResizeBorderThickness defined in WindowChrome.
                this.DragMove();
            }
        }
        private void MainWindow_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            // Stops the active resize command and releases mouse capture.
            this.isMouseResizeCommand = false;
            this.resizeRegion = ResizeRegion.None;
            this.Cursor = Cursors.Arrow;
            this.Bordered.ReleaseMouseCapture();

            // If you used this.CaptureMouse() during the down event, call this.ReleaseMouseCapture()
            // DragMove() automatically handles mouse capture release.
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
                _viewModel.IsVertical = !_viewModel.IsVertical;
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
         this.Cursor=   Cursors.SizeNESW;
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
    }
}
