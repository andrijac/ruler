using Ruler.Shared.Commands;
using Ruler.Shared.Enums;
using Ruler.Shared.Helpers;
using Ruler.Shared.Interfaces;
using Ruler.Shared.Models;
using Ruler.Shared.Services;
using Ruler.Wpf.Services;
using Ruler.Wpf.ViewModels;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace Ruler.Wpf.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IRuler
    {
        private RulerInfo _rulerInfo => _viewModel?.RulerData;
        private HwndSource _hwndSource;
        private bool _isUpdatingFromWindow = false;

        public event EventHandler<RulerInfo> DuplicateRequested;
        public RulerInfo RulerData
        {
            get => _viewModel?.RulerData;
            set
            {
                if (_viewModel != null)
                {
                    _viewModel.RulerData = value;
                    InvalidateVisual();
                }
            }
        }
        private MagnifierWindow _magnifierWindow;
        private CalibrationWindow _calibrationWindow;

        // DI & ViewModel Dependencies
        private readonly IRulerFactory _rulerFactory;
        private readonly IRulerRegistry _rulerRegistry;
        private readonly IMonitorRegistry _monitorRegistry;
        private readonly RulerViewModel _viewModel;

        // Monitor Context Tracking Fields
        private MonitorCalibrationProfile _currentProfile;
        private string _currentDeviceName;
        private bool _isInitialized = false;

        private Point _mouseScreenPosition;

        public MainWindow(RulerInfo info, IRulerFactory factory, IRulerRegistry rulerRegistry, IMonitorRegistry monitorRegistry)
        {
            _rulerFactory = factory ?? throw new ArgumentNullException(nameof(factory));
            _rulerRegistry = rulerRegistry ?? throw new ArgumentNullException(nameof(rulerRegistry));
            _monitorRegistry = monitorRegistry ?? throw new ArgumentNullException(nameof(monitorRegistry));

            // Initialize ViewModel and bind action callbacks
            _viewModel = new RulerViewModel(info, _monitorRegistry, _rulerFactory);

            _viewModel.RequestInvalidate = InvalidateView;
            _viewModel.RequestOpenSetSize = OpenSetSizeWindow;
            _viewModel.RequestOpenCalibration = OpenCalibrationWindow;
            _viewModel.RequestOpenUpdate = OpenUpdateWindow;
            _viewModel.RequestOpenShortcuts = OpenShortcutsWindow;
            _viewModel.RequestOpenAbout = OpenAboutWindow;
            _viewModel.RequestOpenMagnifier = HandleMagnifierToggle;
            _viewModel.RequestCloseWindow = Close;
            _viewModel.RequestExit = () => Application.Current.Shutdown();
            _viewModel.RequestResizeWindow = (w, h) => { this.Width = w; this.Height = h; };
            _viewModel.RequestMoveWindow = (dx, dy) => { this.Left += dx; this.Top += dy; };
            _viewModel.DuplicateRequested += (s, data) => DuplicateRequested?.Invoke(s, data);

            InitializeComponent();
            DataContext = _viewModel;

            _isUpdatingFromWindow = true;
            this.Width = _rulerInfo.Width;
            this.Height = _rulerInfo.Height;

            // Setup Interop Hook and Initialize Monitor Context
            this.SourceInitialized += (s, e) =>
            {
                _hwndSource = PresentationSource.FromVisual(this) as HwndSource;
                _hwndSource?.AddHook(WndProc);

                _isInitialized = true;
                UpdateMonitorContext();
            };

            this.LocationChanged += OnWindowLocationChanged;
            this.SizeChanged += OnWindowSizeChanged;
            this.ContextMenuOpening += (s, e) => _viewModel.UpdateAllMenuStates();
            _isUpdatingFromWindow = false;
        }

        private void UpdateMonitorContext()
        {
            if (!_isInitialized) return;

            var (newDeviceName, monitorWidth, monitorHeight) = NativeHelpers.GetMonitorInfoFromWindow(new WindowInteropHelper(this).Handle);

            if (_currentDeviceName == newDeviceName && _currentProfile != null)
            {
                _currentProfile.SyncResolution(monitorWidth, monitorHeight);
                return;
            }

            if (_currentProfile != null)
            {
                _currentProfile.CalibrationChanged -= OnProfileCalibrationChanged;
            }

            _currentDeviceName = newDeviceName;
            _rulerInfo.LastDeviceName = newDeviceName;

            _currentProfile = _monitorRegistry.GetOrCreateProfile(newDeviceName);
            _currentProfile.CalibrationChanged += OnProfileCalibrationChanged;

            _currentProfile.SyncResolution(monitorWidth, monitorHeight);
            InvalidateVisual();
        }

        private void OnProfileCalibrationChanged(object sender, EventArgs e)
        {
            InvalidateVisual();
        }

        private void OnWindowLocationChanged(object sender, EventArgs e)
        {
            if (_rulerInfo == null || _isUpdatingFromWindow) return;

            _isUpdatingFromWindow = true;
            _rulerInfo.Left = (int)this.Left;
            _rulerInfo.Top = (int)this.Top;
            UpdateMonitorContext();
            _isUpdatingFromWindow = false;
        }

        private void OnWindowSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_rulerInfo == null || _isUpdatingFromWindow) return;

            _isUpdatingFromWindow = true;
            _rulerInfo.Width = (int)this.ActualWidth;
            _rulerInfo.Height = (int)this.ActualHeight;
            UpdateMonitorContext();
            _isUpdatingFromWindow = false;
        }

        // --- Dialog & Window Management Callbacks ---
        private void OpenSetSizeWindow()
        {
            var setSizeWindow = new SetSizeWindow(_rulerInfo)
            {
                Topmost = true,
                Owner = this
            };

            if (setSizeWindow.ShowDialog() == true)
            {
                _viewModel.ApplySizeChange(
                    setSizeWindow.UpdatedWidth,
                    setSizeWindow.UpdatedHeight,
                    setSizeWindow.UpdatedUnit
                );

                this.Width = setSizeWindow.UpdatedWidth;
                this.Height = setSizeWindow.UpdatedHeight;
            }
        }

        private void OpenCalibrationWindow()
        {
            if (_calibrationWindow == null)
            {
                _calibrationWindow = new CalibrationWindow(_monitorRegistry, _currentDeviceName) { Owner = this };
                _calibrationWindow.Closed += (s, args) => _calibrationWindow = null;
                _calibrationWindow.ShowDialog();
            }
            else
            {
                _calibrationWindow.Activate();
            }
        }

        private void OpenUpdateWindow()
        {
            var updateWindow = new UpdateWindow
            {
                Topmost = true,
                Owner = this
            };
            updateWindow.ShowDialog();
        }

        private void OpenShortcutsWindow()
        {
            var shortcut = new ShowShortcutsWindow
            {
                Topmost = true,
                Owner = this
            };
            shortcut.ShowDialog();
        }

        private void OpenAboutWindow()
        {
            string version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0";
            string message = string.Format(
                "Original Ruler implemented by Jeff Key\nwww.sliver.com\nruler.codeplex.com\nIcon by Kristen Magee @ www.kbecca.com.\nMaintained by Andrija Cacanovic\nHosted on \nhttps://github.com/andrijac/ruler\nVersion {0}",
                version);

            MessageBox.Show(message, "About Ruler", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void HandleMagnifierToggle()
        {
            if (!_rulerInfo.Magnifier.IsActive)
            {
                _magnifierWindow?.Close();
            }
            else
            {
                if (_magnifierWindow != null) _magnifierWindow.Show();
                else
                {
                    _magnifierWindow = new MagnifierWindow(_rulerInfo) { Owner = this };
                    _magnifierWindow.Closed += (s, args) => _magnifierWindow = null;
                    _magnifierWindow.Show();
                }
            }
        }

        // --- Interop Message Loop Hook ---
        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (NativeHelpers.ShouldBlockSizing(msg, wParam, _rulerInfo.IsLocked))
            {
                handled = true;
                return IntPtr.Zero;
            }

            if (NativeHelpers.IsDisplayOrDpiChange(msg))
            {
                UpdateMonitorContext();
                handled = true;
            }

            return IntPtr.Zero;
        }

        // --- MEASUREMENT & RULER RENDERING ENGINE ---
        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            if (ActualWidth <= 0 || ActualHeight <= 0) return;

            var tickPen = new Pen(Brushes.Black, 1);
            var textBrush = Brushes.Black;
            var fontTypeface = new Typeface("Segoe UI");
            double dpi = VisualTreeHelper.GetDpi(this).PixelsPerDip;

            var rulerRect = new Rect(0, 0, ActualWidth, ActualHeight);
            drawingContext.DrawRectangle(Brushes.LightSlateGray, tickPen, rulerRect);

            bool isVertical = _rulerInfo?.IsVertical ?? false;

            if (isVertical)
            {
                DrawVerticalRuler(drawingContext, tickPen, textBrush, fontTypeface, dpi);
            }
            else
            {
                DrawHorizontalRuler(drawingContext, tickPen, textBrush, fontTypeface, dpi);
            }

            if (_rulerInfo?.Guideline?.IsEnabled == true)
            {
                Brush guideBrush = Brushes.Red;
                if (!string.IsNullOrEmpty(_rulerInfo.Guideline.GuidelineColorHex))
                {
                    try { guideBrush = (Brush)new BrushConverter().ConvertFromString(_rulerInfo.Guideline.GuidelineColorHex); }
                    catch { guideBrush = Brushes.Red; }
                }

                var guidePen = new Pen(guideBrush, 1);
                if (isVertical)
                {
                    double y = _rulerInfo.Guideline.Position;
                    drawingContext.DrawLine(guidePen, new Point(0, y), new Point(ActualWidth, y));
                }
                else
                {
                    double x = _rulerInfo.Guideline.Position;
                    drawingContext.DrawLine(guidePen, new Point(x, 0), new Point(x, ActualHeight));
                }
            }
            if (_rulerInfo.RulerUnits == RulerUnit.Pixels)
            {
                DrawRulerInfoText(drawingContext, fontTypeface, textBrush, dpi);
            }
        }

        private void DrawHorizontalRuler(DrawingContext dc, Pen pen, Brush brush, Typeface typeface, double dpi)
        {
            bool dualSided = ActualHeight > 100;
            var unitConfig = MeasurementUnit.Get(_rulerInfo.RulerUnits);

            double baseDpi = 96.0 * VisualTreeHelper.GetDpi(this).PixelsPerDip;
            double effectiveDpi = _currentProfile != null ? _currentProfile.GetEffectiveDpi(baseDpi) : baseDpi;

            double majorInterval = unitConfig.MajorTickInterval;
            int subdivisions = unitConfig.MinorTickSubdivisions;
            double minorStep = majorInterval / subdivisions;

            double maxUnitValue = unitConfig.PixelToUnit(ActualWidth, effectiveDpi, ActualWidth);
            int totalSteps = (int)Math.Ceiling(maxUnitValue / minorStep);

            double lastLabelX = -999.0;

            for (int i = 0; i <= totalSteps; i++)
            {
                double u = i * minorStep;
                double x = unitConfig.UnitToPixel(u, effectiveDpi, ActualWidth);
                if (x > ActualWidth) break;

                bool isMajor = (i % subdivisions == 0) || Math.Abs(u % majorInterval) < 0.0001;
                int subIndex = i % subdivisions;

                double tickLength = unitConfig.MajorTickLength;
                if (!isMajor)
                {
                    tickLength = unitConfig.GetMinorTickLength?.Invoke(subIndex) ?? 4.0;
                }

                dc.DrawLine(pen, new Point(x, 0), new Point(x, tickLength));
                dc.DrawLine(pen, new Point(x, ActualHeight), new Point(x, ActualHeight - tickLength));

                bool shouldLabel = unitConfig.ShouldLabel != null ? unitConfig.ShouldLabel(u) : (isMajor && u > 0);
                if (_rulerInfo.RulerUnits == RulerUnit.Inches)
                {
                    shouldLabel = (Math.Abs(u - Math.Round(u)) < 0.0001) && (u > 0);
                }
                if (shouldLabel)
                {
                    if (unitConfig.CustomDraw != null)
                    {
                        if (x - lastLabelX > 16)
                        {
                            if (dualSided)
                            {
                                unitConfig.CustomDraw(dc, typeface, brush, pen, x, 22, u, effectiveDpi, lastLabelX, ActualWidth);
                                double bottomY = ActualHeight - tickLength - 14;
                                unitConfig.CustomDraw(dc, typeface, brush, pen, x, bottomY, u, effectiveDpi, lastLabelX, ActualWidth);
                            }
                            else
                            {
                                unitConfig.CustomDraw(dc, typeface, brush, pen, x, ActualHeight / 2, u, effectiveDpi, lastLabelX, ActualWidth);
                            }
                            lastLabelX = x;
                        }
                    }
                    else
                    {
                        string labelText = unitConfig.FormatLabel != null ? unitConfig.FormatLabel(u, false) : u.ToString();
                        var formattedText = new FormattedText(labelText, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, typeface, 12, brush, effectiveDpi);

                        if (x - lastLabelX > formattedText.Width + 8)
                        {
                            if (dualSided)
                            {
                                dc.DrawText(formattedText, new Point(x - (formattedText.Width / 2), 14));
                                double bottomY = ActualHeight - tickLength - formattedText.Height - 3;
                                dc.DrawText(formattedText, new Point(x - (formattedText.Width / 2), bottomY));
                            }
                            else
                            {
                                dc.DrawText(formattedText, new Point(x - (formattedText.Width / 2), (ActualHeight / 2) - (formattedText.Height / 2)));
                            }
                            lastLabelX = x;
                        }
                    }
                }
            }
        }

        private void DrawVerticalRuler(DrawingContext dc, Pen pen, Brush brush, Typeface typeface, double dpi)
        {
            bool dualSided = ActualWidth > 100;
            var unitConfig = MeasurementUnit.Get(_rulerInfo.RulerUnits);

            double baseDpi = 96.0 * VisualTreeHelper.GetDpi(this).PixelsPerDip;
            double effectiveDpi = _currentProfile != null ? _currentProfile.GetEffectiveDpi(baseDpi) : baseDpi;

            double majorInterval = unitConfig.MajorTickInterval;
            int subdivisions = unitConfig.MinorTickSubdivisions;
            double minorStep = majorInterval / subdivisions;

            double maxUnitValue = unitConfig.PixelToUnit(ActualHeight, effectiveDpi, ActualHeight);
            int totalSteps = (int)Math.Ceiling(maxUnitValue / minorStep);

            double lastLabelY = -999.0;

            for (int i = 0; i <= totalSteps; i++)
            {
                double u = i * minorStep;
                double y = unitConfig.UnitToPixel(u, effectiveDpi, ActualHeight);
                if (y > ActualHeight) break;

                bool isMajor = (i % subdivisions == 0) || Math.Abs(u % majorInterval) < 0.0001;
                int subIndex = i % subdivisions;

                double tickLength = unitConfig.MajorTickLength;
                if (!isMajor)
                {
                    tickLength = unitConfig.GetMinorTickLength?.Invoke(subIndex) ?? 4.0;
                }

                dc.DrawLine(pen, new Point(0, y), new Point(tickLength, y));
                dc.DrawLine(pen, new Point(ActualWidth, y), new Point(ActualWidth - tickLength, y));

                bool shouldLabel = unitConfig.ShouldLabel != null ? unitConfig.ShouldLabel(u) : (isMajor && u > 0);
                if (_rulerInfo.RulerUnits == RulerUnit.Inches)
                {
                    shouldLabel = (Math.Abs(u - Math.Round(u)) < 0.0001) && (u > 0);
                }
                if (shouldLabel)
                {
                    if (unitConfig.CustomDraw != null)
                    {
                        if (y - lastLabelY > 16)
                        {
                            if (dualSided)
                            {
                                unitConfig.CustomDraw(dc, typeface, brush, pen, 22, y, u, effectiveDpi, lastLabelY, ActualHeight);
                                double rightX = ActualWidth - 22;
                                unitConfig.CustomDraw(dc, typeface, brush, pen, rightX, y, u, effectiveDpi, lastLabelY, ActualHeight);
                            }
                            else
                            {
                                unitConfig.CustomDraw(dc, typeface, brush, pen, ActualWidth / 2, y, u, effectiveDpi, lastLabelY, ActualHeight);
                            }
                            lastLabelY = y;
                        }
                    }
                    else
                    {
                        string labelText = unitConfig.FormatLabel != null ? unitConfig.FormatLabel(u, true) : u.ToString();
                        var formattedText = new FormattedText(labelText, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, typeface, 12, brush, effectiveDpi);

                        if (y - lastLabelY > 16)
                        {
                            if (dualSided)
                            {
                                dc.DrawText(formattedText, new Point(16, y - (formattedText.Height / 2)));
                                double rightX = ActualWidth - tickLength - formattedText.Width - 4;
                                dc.DrawText(formattedText, new Point(rightX, y - (formattedText.Height / 2)));
                            }
                            else
                            {
                                dc.DrawText(formattedText, new Point((ActualWidth / 2) - (formattedText.Width / 2), y - (formattedText.Height / 2)));
                            }
                            lastLabelY = y;
                        }
                    }
                }
            }
        }

        private void DrawRulerInfoText(DrawingContext dc, Typeface typeface, Brush brush, double dpi)
        {
            string infoText = CreateRulerText();
            double effectiveDpi = _currentProfile != null ? _currentProfile.GetEffectiveDpi(dpi) : dpi;

            var formattedText = new FormattedText(
                infoText,
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                typeface,
                9,
                brush,
                effectiveDpi);

            Point textPos = _rulerInfo.IsVertical
                ? new Point((ActualWidth - formattedText.Width) / 2, 25)
                : new Point(35, (ActualHeight - formattedText.Height) / 2);

            dc.DrawText(formattedText, textPos);
        }

        private string CreateRulerText()
        {
            var unitConfig = MeasurementUnit.Get(_rulerInfo.RulerUnits);
            double baseDpi = 96.0 * VisualTreeHelper.GetDpi(this).PixelsPerDip;
            double effectiveDpi = _currentProfile != null ? _currentProfile.GetEffectiveDpi(baseDpi) : baseDpi;

            double totalPixels = _rulerInfo.IsVertical ? ActualHeight : ActualWidth;
            double totalUnitValue = unitConfig.PixelToUnit(totalPixels, effectiveDpi, totalPixels);

            string sizeStr = unitConfig.FormatLabel != null
                ? unitConfig.FormatLabel(totalUnitValue, _rulerInfo.IsVertical).Replace("\n", " ")
                : $"{totalUnitValue:0.##} {unitConfig.Abbreviation}";

            if (_rulerInfo.RulerUnits == RulerUnit.Pixels && !sizeStr.Contains("px"))
            {
                sizeStr += " px";
            }

            string text = $"Size: {sizeStr}";

            if (_rulerInfo.Guideline.IsEnabled)
            {
                double guidePixels = _rulerInfo.Guideline.Position;
                double guideUnitValue = unitConfig.PixelToUnit(guidePixels, effectiveDpi, totalPixels);

                string guideStr = unitConfig.FormatLabel != null
                    ? unitConfig.FormatLabel(guideUnitValue, _rulerInfo.IsVertical).Replace("\n", " ")
                    : $"{guideUnitValue:0.##} {unitConfig.Abbreviation}";

                if (_rulerInfo.RulerUnits == RulerUnit.Pixels && !guideStr.Contains("px"))
                {
                    guideStr += " px";
                }
                text += $"{Environment.NewLine}Guideline: {guideStr}";
            }

            return text;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            var position = e.GetPosition(this);

            if (_rulerInfo.ShowToolTip && InfoOverlay != null)
            {
                double overlayWidth = InfoOverlay.ActualWidth > 0 ? InfoOverlay.ActualWidth : 120;
                double overlayHeight = InfoOverlay.ActualHeight > 0 ? InfoOverlay.ActualHeight : 25;

                double left = position.X + 15;
                if (left + overlayWidth > ActualWidth - 5)
                {
                    left = position.X - overlayWidth - 15;
                }
                left = Math.Max(5, left);

                double top = position.Y + 15;
                if (top + overlayHeight > ActualHeight - 5)
                {
                    top = position.Y - overlayHeight - 15;
                }
                top = Math.Max(5, top);

                InfoOverlay.Margin = new Thickness(left, top, 0, 0);
            }

            if (_rulerInfo?.Guideline?.IsEnabled != true || _rulerInfo.Guideline.IsLocked) return;

            if (position.X <= 0 && position.Y <= 0) return;
            double newPosition = _rulerInfo.IsVertical ? position.Y : position.X;

            double maxLimit = _rulerInfo.IsVertical ? ActualHeight : ActualWidth;
            if (newPosition < 0 || newPosition > maxLimit) return;

            _rulerInfo.Guideline.Position = newPosition;
            InvalidateView();
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.ChangedButton == MouseButton.Left)
            {
                _mouseScreenPosition = this.PointToScreen(new Point(0, 0));
                this.DragMove();
            }
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);
            if (e.ChangedButton == MouseButton.Left && _rulerInfo?.Guideline?.IsEnabled == true)
            {
                var mouseUpScreenPosition = this.PointToScreen(new Point(0, 0));
                double deltaX = Math.Abs(mouseUpScreenPosition.X - _mouseScreenPosition.X);
                double deltaY = Math.Abs(mouseUpScreenPosition.Y - _mouseScreenPosition.Y);

                if (deltaX < SystemParameters.MinimumHorizontalDragDistance && deltaX < SystemParameters.MinimumVerticalDragDistance)
                {
                    _rulerInfo.Guideline.IsLocked = !_rulerInfo.Guideline.IsLocked;
                    InvalidateView();
                }
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            bool isCtrl = Keyboard.Modifiers.HasFlag(ModifierKeys.Control);
            bool isShift = Keyboard.Modifiers.HasFlag(ModifierKeys.Shift);
            int step = isCtrl ? 1 : 10;

            int dx = 0;
            int dy = 0;

            bool up = Keyboard.IsKeyDown(Key.Up);
            bool down = Keyboard.IsKeyDown(Key.Down);
            bool left = Keyboard.IsKeyDown(Key.Left);
            bool right = Keyboard.IsKeyDown(Key.Right);

            if (up && left) { dx = -1; dy = -1; }
            else if (up && right) { dx = 1; dy = -1; }
            else if (down && left) { dx = -1; dy = 1; }
            else if (down && right) { dx = 1; dy = 1; }
            else if (up) { dy = -1; }
            else if (down) { dy = 1; }
            else if (left) { dx = -1; }
            else if (right) { dx = 1; }
            else { return; }

            if (isShift)
            {
                _viewModel.NudgeSize(dx * step, dy * step);
                e.Handled = true;
            }
            else
            {
                _viewModel.NudgeLocation(dx * step, dy * step);
                e.Handled = true;
            }
        }

        private void Window_MouseDoubleClick(object sender, MouseButtonEventArgs e) => _viewModel.ToggleOrientationCommand.Execute(null);

        public void InvalidateView()
        {
            RefreshToolTipDisplay();
            InvalidateVisual();
        }

        private void RefreshToolTipDisplay()
        {
            if (_rulerInfo.ShowToolTip && InfoContentPanel != null)
            {
                InfoOverlay.Visibility = Visibility.Visible;
                InfoContentPanel.Children.Clear();

                var unitConfig = MeasurementUnit.Get(_rulerInfo.RulerUnits);
                double baseDpi = 96.0 * VisualTreeHelper.GetDpi(this).PixelsPerDip;
                double effectiveDpi = _currentProfile != null ? _currentProfile.GetEffectiveDpi(baseDpi) : baseDpi;

                InfoContentPanel.Children.Add(new TextBlock { Text = "Size: ", FontSize = 9, Foreground = Brushes.Black, VerticalAlignment = VerticalAlignment.Center });

                double totalPixels = _rulerInfo.IsVertical ? ActualHeight : ActualWidth;
                double totalUnitValue = unitConfig.PixelToUnit(totalPixels, effectiveDpi, totalPixels);

                InfoContentPanel.Children.Add(CreateMeasurementDisplayElement(totalUnitValue, unitConfig.Abbreviation, effectiveDpi));

                if (_rulerInfo.Guideline.IsEnabled)
                {
                    InfoContentPanel.Children.Add(new TextBlock { Text = " | Guide: ", FontSize = 9, Foreground = Brushes.Gray, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(4, 0, 0, 0) });

                    string lockStatus = _rulerInfo.Guideline.IsLocked ? "Locked" : "Current";
                    InfoContentPanel.Children.Add(new TextBlock { Text = $"({lockStatus}) ", FontSize = 9, Foreground = Brushes.Gray, VerticalAlignment = VerticalAlignment.Center });

                    double guidePixels = _rulerInfo.Guideline.Position;
                    double guideUnitValue = unitConfig.PixelToUnit(guidePixels, effectiveDpi, totalPixels);

                    InfoContentPanel.Children.Add(CreateMeasurementDisplayElement(guideUnitValue, unitConfig.Abbreviation, effectiveDpi));
                }

                this.ToolTip = null;
            }
            else if (InfoOverlay != null)
            {
                InfoOverlay.Visibility = Visibility.Collapsed;
                this.ToolTip = null;
            }
        }

        private UIElement CreateMeasurementDisplayElement(double unitValue, string unitAbbrev, double effectiveDpi)
        {
            var unitConfig = MeasurementUnit.Get(_rulerInfo.RulerUnits);

            if (unitConfig.CustomDraw != null && _rulerInfo.RulerUnits == RulerUnit.Inches)
            {
                var visual = new DrawingVisual();
                using (var dc = visual.RenderOpen())
                {
                    var typeface = new Typeface("Segoe UI");
                    var brush = Brushes.Black;
                    var pen = new Pen(Brushes.Black, 1);

                    unitConfig.CustomDraw(dc, typeface, brush, pen, 22.0, 10.0, unitValue, effectiveDpi, 0.0, 20.0);
                }

                var drawingImage = new DrawingImage(visual.Drawing);
                return new System.Windows.Controls.Image
                {
                    Source = drawingImage,
                    Width = 44,
                    Height = 20,
                    VerticalAlignment = VerticalAlignment.Center
                };
            }
            else
            {
                string text = $"{unitValue:0.##} {unitAbbrev}";
                return new TextBlock
                {
                    Text = text,
                    FontSize = 9,
                    FontWeight = FontWeights.Bold,
                    Foreground = Brushes.Black,
                    VerticalAlignment = VerticalAlignment.Center
                };
            }
        }

        // Interface stub implementations required by IRuler if applicable
        public void SetRulerInfo(RulerInfo ruler)
        {
            _viewModel.RulerData = ruler;
            this.InvalidateVisual();
        }

        public void SetBounds(int left, int top, int width, int height) => throw new NotImplementedException();
        public void SetTooltip(string text) => throw new NotImplementedException();
        public void ShowAbout() => throw new NotImplementedException();
        public void ShowShortcuts() => throw new NotImplementedException();
        public void Dispose() => throw new NotImplementedException();
    }
}