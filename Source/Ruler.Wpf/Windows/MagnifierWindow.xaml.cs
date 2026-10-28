using Ruler.Shared.Models;
using Ruler.Shared.Services;
using Ruler.Wpf.Controls;
using Ruler.Wpf.Services;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
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

using Brushes = System.Windows.Media.Brushes;
using Color = System.Drawing.Color;
using Pen = System.Windows.Media.Pen;

namespace Ruler.Wpf.Windows
{
    /// <summary>
    /// Interaction logic for MagnifierWindow.xaml
    /// </summary>
    public partial class MagnifierWindow : Window
    {
        private BitmapSource _capturedSnapshot;
        private double _rulerWidthPhysical;
        private double _rulerHeightPhysical;
        private System.Windows.Point _rulerScreenOriginPhysical;
        private double _zoomFactor = 2.0;
        private bool _isNearEdge = false;

        public MagnifierWindow()
        {
            InitializeComponent();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            // Make window click-through using NativeHelpers constants
            IntPtr hwnd = new WindowInteropHelper(this).Handle;
            int extendedStyle = NativeHelpers.GetWindowLong(hwnd, NativeHelpers.GWL_EXSTYLE);
            NativeHelpers.SetWindowLong(hwnd, NativeHelpers.GWL_EXSTYLE, extendedStyle | NativeHelpers.WS_EX_TRANSPARENT);
        }

        public void UpdateMagnifier(System.Windows.Point mouseScreenPosPhysical, double rulerWidthPhysical, double rulerHeightPhysical, System.Windows.Point rulerScreenOriginPhysical, double zoomFactor = 2.0)
        {
            if (rulerWidthPhysical <= 0 || rulerHeightPhysical <= 0) return;

            _rulerWidthPhysical = rulerWidthPhysical;
            _rulerHeightPhysical = rulerHeightPhysical;
            _rulerScreenOriginPhysical = rulerScreenOriginPhysical;
            _zoomFactor = zoomFactor;

            var dpi = VisualTreeHelper.GetDpi(this);

            // Offset the window from the cursor so it doesn't overlap the capture zone
            Left = (mouseScreenPosPhysical.X / dpi.DpiScaleX) + 20;
            Top = (mouseScreenPosPhysical.Y / dpi.DpiScaleY) - Height - 20;

            _capturedSnapshot = CaptureRegionWithEdgeOverflow(mouseScreenPosPhysical, (int)Width, 50, _zoomFactor);
            InvalidateVisual();
        }
        private BitmapSource CaptureRegionWithEdgeOverflow(System.Windows.Point mousePosPhysical, int windowSize, int overflow, double zoomFactor)
        {
            double sourceCaptureSize = windowSize / zoomFactor;
            double halfSourceSize = sourceCaptureSize / 2.0;

            double localX = mousePosPhysical.X - _rulerScreenOriginPhysical.X;
            double localY = mousePosPhysical.Y - _rulerScreenOriginPhysical.Y;

            double desiredLeft = localX - halfSourceSize;
            double desiredTop = localY - halfSourceSize;
            double desiredRight = desiredLeft + sourceCaptureSize;
            double desiredBottom = desiredTop + sourceCaptureSize;

            bool isNearOrOverRuler = !(desiredRight < -overflow || desiredLeft > _rulerWidthPhysical + overflow ||
                                       desiredBottom < -overflow || desiredTop > _rulerHeightPhysical + overflow);

            int actualWidth, actualHeight;
            int srcX, srcY;
            int destX = 0, destY = 0;

            if (isNearOrOverRuler)
            {
                _isNearEdge = (desiredLeft < 0 || desiredTop < 0 || desiredRight > _rulerWidthPhysical || desiredBottom > _rulerHeightPhysical);

                double targetLeft = Math.Max(-overflow, desiredLeft);
                double targetTop = Math.Max(-overflow, desiredTop);
                double targetRight = Math.Min(_rulerWidthPhysical + overflow, desiredRight);
                double targetBottom = Math.Min(_rulerHeightPhysical + overflow, desiredBottom);

                actualWidth = Math.Max(0, (int)Math.Round(targetRight - targetLeft));
                actualHeight = Math.Max(0, (int)Math.Round(targetBottom - targetTop));

                srcX = (int)Math.Round(_rulerScreenOriginPhysical.X + targetLeft);
                srcY = (int)Math.Round(_rulerScreenOriginPhysical.Y + targetTop);

                destX = (int)Math.Round((targetLeft - desiredLeft) * zoomFactor);
                destY = (int)Math.Round((targetTop - desiredTop) * zoomFactor);
            }
            else
            {
                _isNearEdge = false;
                actualWidth = (int)Math.Round(sourceCaptureSize);
                actualHeight = (int)Math.Round(sourceCaptureSize);

                srcX = (int)Math.Round(mousePosPhysical.X - halfSourceSize);
                srcY = (int)Math.Round(mousePosPhysical.Y - halfSourceSize);
                destX = 0;
                destY = 0;
            }

            using (var finalBitmap = new Bitmap(windowSize, windowSize, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
            {
                using (var g = Graphics.FromImage(finalBitmap))
                {
                    g.Clear(Color.FromArgb(240, 240, 240));

                    if (actualWidth > 0 && actualHeight > 0)
                    {
                        using (var sourceBitmap = new Bitmap(actualWidth, actualHeight, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
                        {
                            using (var sg = Graphics.FromImage(sourceBitmap))
                            {
                                sg.CopyFromScreen(srcX, srcY, 0, 0, new System.Drawing.Size(actualWidth, actualHeight), CopyPixelOperation.SourceCopy);
                            }

                            int destWidth = (int)Math.Round(actualWidth * zoomFactor);
                            int destHeight = (int)Math.Round(actualHeight * zoomFactor);

                            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                            g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;

                            g.DrawImage(sourceBitmap,
                                new System.Drawing.Rectangle(destX, destY, destWidth, destHeight),
                                new System.Drawing.Rectangle(0, 0, actualWidth, actualHeight),
                                GraphicsUnit.Pixel);
                        }
                    }
                }

                // Convert HBitmap to WPF BitmapSource and safely release the GDI handle
                IntPtr hBitmap = finalBitmap.GetHbitmap();
                try
                {
                    var bitmapSource = Imaging.CreateBitmapSourceFromHBitmap(
                        hBitmap,
                        IntPtr.Zero,
                        Int32Rect.Empty,
                        BitmapSizeOptions.FromEmptyOptions());
                    bitmapSource.Freeze();
                    return bitmapSource;
                }
                finally
                {
                    NativeHelpers.DeleteObject(hBitmap);
                }
            }
        }
        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);

            if (_capturedSnapshot != null)
            {
                RenderOptions.SetBitmapScalingMode(this, BitmapScalingMode.NearestNeighbor);
                dc.DrawImage(_capturedSnapshot, new Rect(0, 0, ActualWidth, ActualHeight));
            }

            DrawMagnifierOverlay(dc);
        }

        private void DrawMagnifierOverlay(DrawingContext dc)
        {
            double centerX = ActualWidth / 2;
            double centerY = ActualHeight / 2;

            var crosshairPen = new Pen(Brushes.Red, 1);
            dc.DrawLine(crosshairPen, new System.Windows.Point(centerX - 12, centerY), new System.Windows.Point(centerX + 12, centerY));
            dc.DrawLine(crosshairPen, new System.Windows.Point(centerX, centerY - 12), new System.Windows.Point(centerX, centerY + 12));

            //if (_isNearEdge)
            //{
            //    var edgePen = new Pen(Brushes.OrangeRed, 2);
            //    edgePen.DashStyle = DashStyles.Dash;
            //    dc.DrawRectangle(null, edgePen, new Rect(6, 6, ActualWidth - 12, ActualHeight - 12));
            //}
        }
    }



}
