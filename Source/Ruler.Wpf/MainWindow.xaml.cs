using Ruler.Wpf.Common;
using Ruler.Wpf.ViewModels;

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

        // Points used for calculating delta movements.
        private Point _startPoint;

        public MainWindow()
        {
            InitializeComponent();

            // Create an instance of the ViewModel and set it as the DataContext.
            // This links the XAML's bindings to the ViewModel's properties and commands.
            IDialogService dialogService = new DialogService();

            // Create an instance of the ViewModel and pass the service to it.
            _viewModel = new RulerViewModel(dialogService);
            this.DataContext = _viewModel;
       //   this.SizeChanged += MainWindow_SizeChanged;
            this.Loaded += MainWindow_Loaded;
        }     
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _isSizeChangingProgrammatically = true;
            var source = PresentationSource.FromVisual(this);
            if (source != null)
            {
                Matrix m = source.CompositionTarget.TransformToDevice;
                double dpiScaleX = m.M11;
                double dpiScaleY = m.M22;

                // Set the window's size using the converted DIU values.
                this.Width = _viewModel.Width / dpiScaleX;
                this.Height = _viewModel.Height / dpiScaleY;
            }
            _isSizeChangingProgrammatically = false;
        }

        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_isSizeChangingProgrammatically) return;

            var source = PresentationSource.FromVisual(this);
            if (source != null)
            {
                Matrix m = source.CompositionTarget.TransformToDevice;
                double dpiScaleX = m.M11;
                double dpiScaleY = m.M22;

                // Update the ViewModel with the new pixel size.
                _viewModel.Width = (int)(this.Width * dpiScaleX);
                _viewModel.Height = (int)(this.Height * dpiScaleY);
            }
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
            // Do nothing if neither moving nor resizing is in progress.
            if (!_isMoving && !_isResizing) return;

            // Get the current mouse position.
            Point currentPosition = e.GetPosition(this);
            double deltaX = currentPosition.X - _startPoint.X;
            double deltaY = currentPosition.Y - _startPoint.Y;

            // Perform the move or resize operation based on the active flag.
            if (_isMoving)
            {
                // Update the ViewModel with the new location.
                // _viewModel.DisplayedLocation = new Point(this.Left + deltaX, this.Top + deltaY);
                this.DragMove();
            }
            else if (_isResizing)
            {
                // Update the ViewModel with the new size. The SizeChanged event
                // will automatically fire and update the ViewModel's properties.
                this.Width = ((this.Width + deltaX)>50)?this.Width+deltaX:50;
                this.Height = ((this.Height + deltaY)>50)?this.Height+deltaY:50;
            }
        }

        /// <summary>
        /// Handles the mouse left button up event.
        /// This ends the drag-to-move or drag-to-resize operation.
        /// </summary>
        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);

            // Release the mouse capture.
            this.ReleaseMouseCapture();

            // Call the ViewModel's method to end the interaction.
            _viewModel.MouseLeftButtonUp();
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
    }
}
