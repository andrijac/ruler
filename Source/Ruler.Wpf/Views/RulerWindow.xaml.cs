using Ruler.Wpf.ViewModels;

using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace Ruler.Wpf.Views
{
    /// <summary>
    /// Interaction logic for RulerWindow.xaml. It captures raw OS input signals
    /// and safely passes them straight through to the ViewModel layer.
    /// </summary>
    public partial class RulerWindow : Window
    {
        public RulerViewModel viewModel { get; }
        private bool _isSyncing = false;
        private bool _isDragging = false;
        private Point _dragStartPoint;
        private bool _skipNextMouseMove = false;
        public RulerWindow(RulerViewModel viewModel)
        {
            InitializeComponent();
            this.DataContext = viewModel;
            // Intercept mouse wheel events globally before child elements swallow them
            PreviewMouseWheel += OnWindowPreviewMouseWheel;
           this.Loaded += OnWindowLoaded;
            //    viewModel.PropertyChanged += ViewModel_PropertyChanged;
            viewModel.PropertyChanged += (sender, e) =>
            {
                // 1. Only act when the orientation flag specifically shifts
                if (e.PropertyName == "IsVertical")
                {
                    // 1. If we are already mid-sync, bail out immediately to break the loop
                    if (_isSyncing) return;

                    // 2. Lock the gate
                    _isSyncing = true;

                    try
                    {
                        if (viewModel.IsVertical)
                        {
                            // Going Vertical: Expand Height first, then shrink Width
                            this.Height = viewModel.Height;
                            this.Width = viewModel.Width;
                        }
                        else
                        {
                            // Going Horizontal: Expand Width first, then shrink Height
                            this.Width = viewModel.Width;
                            this.Height = viewModel.Height;
                        }

                        this.UpdateLayout();
                    }
                    finally
                    {
                        // 3. Always release the lock in a finally block so the gate re-opens safely
                        _isSyncing = false;
                    }
                }
            };

        }

        private void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            if (!(DataContext is RulerViewModel viewModel)) return;

            // Force the window frame to match the true model orientation on launch
            _isSyncing = true;
            try
            {
                // Enforce the strict sizing sequence based on startup orientation
                if (viewModel.IsVertical)
                {
                    this.Height = viewModel.Height;
                    this.Width = viewModel.Width;
                }
                else
                {
                    this.Width = viewModel.Width;
                    this.Height = viewModel.Height;
                }

                this.UpdateLayout();
            }
            finally
            {
                _isSyncing = false;
            }
        }

        private void OnWindowMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!(DataContext is RulerViewModel viewModel)) return;

            // Handle window movement interactions if clicking the primary canvas body
            if (e.ChangedButton == MouseButton.Left && e.LeftButton == MouseButtonState.Pressed)
            {
                // Capture the absolute start point to let the VM process the click-distance threshold
                Point relativePos = e.GetPosition(this);
                viewModel.HandleMouseDown(PointToScreen(relativePos));

                // Optional: If you aren't clicking an explicit resize border edge zone, 
                // invoke native window drag-moving mechanics safely.
                if (e.OriginalSource == MainCanvas)
                {
                    DragMove();
                }
            }
        }

        private void OnWindowMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && DataContext is RulerViewModel viewModel)
            {
                Point relativePos = e.GetPosition(this);
                viewModel.HandleMouseUp(PointToScreen(relativePos));
            }
        }

        private void OnWindowPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (DataContext is RulerViewModel viewModel && viewModel.HandleMouseWheel(e.Delta))
            {
                // Mark the routed event context as handled so parent/child layouts drop duplicate cycles
                e.Handled = true;
            }
        }

        private void OnWindowMouseMove(object sender, MouseEventArgs e)
        {
          
        }

        private void ThisRulerWindow_MouseEnter(object sender, MouseEventArgs e)
        {

        }

        private void ThisRulerWindow_MouseLeave(object sender, MouseEventArgs e)
        {

        }

        private void MainCanvas_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _dragStartPoint = e.GetPosition(this);
            _isDragging = false;
        }

        private void MainCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && !_isDragging)
            {
                System.Windows.Point currentPosition = e.GetPosition(this);

                // 3. Calculate how far the mouse has traveled since the initial click
                double deltaX = Math.Abs(currentPosition.X - _dragStartPoint.X);
                double deltaY = Math.Abs(currentPosition.Y - _dragStartPoint.Y);

                // 4. Only switch to Window Drag mode if the cursor crosses the OS drag threshold
                if (deltaX > SystemParameters.MinimumHorizontalDragDistance ||
                    deltaY > SystemParameters.MinimumVerticalDragDistance)
                {
                    _isDragging = true;
                    this.DragMove(); // Safely moves the borderless window
                    return;
                }
            }

            // 5. Normal Hover Tracking: Guideline follows the mouse ONLY if NOT dragging the window
            if (!_isDragging &&
                DataContext is RulerViewModel viewModel &&
                viewModel.CurrentGuideline != null &&
                !viewModel.CurrentGuideline.IsLocked)
            {
                System.Windows.Point canvasMousePos = e.GetPosition(MainCanvas);
                viewModel.CurrentGuideline.Position = canvasMousePos.X;
                viewModel.OnPropertyChanged(nameof(viewModel.CurrentGuideline));

                Debug.WriteLine($"islocked: {viewModel.CurrentGuideline?.IsLocked}, isenabled: {viewModel.CurrentGuideline?.IsEnabled}");
            }
        }
        //        if (DataContext is RulerViewModel viewModel &&
        //viewModel.CurrentGuideline != null &&
        //viewModel.CurrentGuideline.IsEnabled &&
        //!viewModel.CurrentGuideline.IsLocked)
        //        {
        //            // WPF's LayoutTransform coordinate space magic shifts the axis automatically!
        //            // The tracking offset along the ruler face is always relative to the canvas's X plane.
        //            System.Windows.Point canvasMousePos = e.GetPosition(MainCanvas);

        //            viewModel.CurrentGuideline.Position = canvasMousePos.X;
        //            viewModel.UpdateGuideline();
        //            viewModel.OnPropertyChanged(nameof(viewModel.CurrentGuideline));
        //        }
        

        private void MainCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_isDragging)
            {
                _isDragging = false;
            }
            if (DataContext is RulerViewModel viewModel && viewModel.ModifySettingCommand != null)
            {
                // 1. Grab the precise X/Y point relative to the drawing canvas
                System.Windows.Point clickPoint = e.GetPosition(MainCanvas);

                // 2. Pack BOTH the string case key AND the point data together
                object[] package = new object[] { "MOUSE_LEFT_CLICK", clickPoint };

                // 3. Fire the command manually, passing the complete package
                if (viewModel.ModifySettingCommand.CanExecute(package))
                {
                    viewModel.ModifySettingCommand.Execute(package);
                    e.Handled = true; // Mark handled so it stops event routing
                }
            }

            //if (DataContext is RulerViewModel viewModel &&
            //    viewModel.CurrentGuideline != null &&
            //    viewModel.CurrentGuideline.IsEnabled)
            //{
            //    System.Windows.Point canvasReleasePos = e.GetPosition(MainCanvas);

            //    // 1. Snapshot the exact lock state right now before modifying anything
            //    bool wasLockedBeforeClick = viewModel.CurrentGuideline.IsLocked;

            //    if (wasLockedBeforeClick)
            //    {
            //        // UNLOCK: If it was locked, unlock it and match the current cursor coordinate
            //        viewModel.CurrentGuideline.IsLocked = false;
            //        viewModel.CurrentGuideline.Position = canvasReleasePos.X;
            //        System.Diagnostics.Debug.WriteLine("--- PROCESSING UNLOCK ---");
            //    }
            //    else
            //    {
            //        // LOCK: If it was tracking the mouse, freeze it where it is
            //        viewModel.CurrentGuideline.IsLocked = true;
            //        System.Diagnostics.Debug.WriteLine("--- PROCESSING LOCK ---");
            //    }

            //    // 2. Alert WPF to redraw the visual layers
            //    viewModel.OnPropertyChanged(nameof(viewModel.CurrentGuideline));

            //    // 3. Stop event routing and block the next trailing micro-movement frame
            //    e.Handled = true;
            //    _skipNextMouseMove = true;

            //    System.Diagnostics.Debug.WriteLine($"Verification Check - IsLocked is: {viewModel.CurrentGuideline.IsLocked}");
        }
        

        private void MainCanvas_MouseEnter(object sender, MouseEventArgs e)
        {
            if (DataContext is RulerViewModel viewModel &&
        viewModel.CurrentGuideline != null &&
        viewModel.CurrentGuideline.IsEnabled &&
        !viewModel.CurrentGuideline.IsLocked)
            {
                // Snap the line instantly to the cursor position as it comes back into the window
                System.Windows.Point canvasMousePos = e.GetPosition(MainCanvas);
                viewModel.CurrentGuideline.Position = canvasMousePos.X;
                viewModel.UpdateGuideline();
            }
        }

        private void MainCanvas_MouseLeave(object sender, MouseEventArgs e)
        {
            if (DataContext is RulerViewModel viewModel &&
        viewModel.CurrentGuideline != null &&
        viewModel.CurrentGuideline.IsEnabled &&
        !viewModel.CurrentGuideline.IsLocked)
            {
                // Throw the line completely off-screen (-10) so it hides cleanly 
                // while the cursor is outside the window bounds
                viewModel.CurrentGuideline.Position = -10;
                viewModel.UpdateGuideline();
            }
        }
      }
}



