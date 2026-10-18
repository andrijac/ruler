using Ruler.Shared.Commands;
using Ruler.Shared.Enums;
using Ruler.Shared.Helpers;
using Ruler.Shared.Interfaces;
using Ruler.Shared.Models;
using Ruler.Shared.Services;
using Ruler.Wpf.Services;

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
    public partial class MainWindow : Window, INotifyPropertyChanged, IRuler
    {
        public ObservableCollection<MenuItemModel> MenuItems { get; set; } = new ObservableCollection<MenuItemModel>();

        // Interop Constants
        private const int WM_SYSCOMMAND = 0x0112;
        private const int SC_SIZE = 0xF000;
        private const int WM_SIZING = 0x0214;

        private RulerInfo _rulerInfo;
        private HwndSource _hwndSource;
        private bool _isUpdatingFromWindow = false;
        public RulerInfo RulerData => _rulerInfo;
        public event EventHandler<RulerInfo> DuplicateRequested;
        private bool _showToolTips = true;
        private UpdatePackageInfo _cachedUpdatePackage;
        private MenuItemModel _updateMenuItem;
        private readonly Dictionary<MenuItemModel, string> _originalToolTips = new Dictionary<MenuItemModel, string>();
        private bool _isUpdateAvailable = false;

        // Monitor Profile & Context Tracking Dependencies & Fields
        private readonly IMonitorRegistry _monitorRegistry;
        private MonitorCalibrationProfile _currentProfile;
        private string _currentDeviceName;
        private bool _isInitialized = false;

#if DEBUG
        private string _owner = "IsaacMorris1980";
#else
        private string _owner = "andrijac";
#endif
        private string _repo = "ruler";

        public bool ShowToolTips
        {
            get => _showToolTips;
            set
            {
                if (SetProperty(ref _showToolTips, value))
                {
                    foreach (var kvp in _originalToolTips)
                    {
                        kvp.Key.ToolTip = _showToolTips ? kvp.Value : null;
                    }

                    var menuItem = MenuItems.FirstOrDefault(i => i.Header == "Show Menu Tooltips");
                    if (menuItem != null)
                    {
                        menuItem.IsChecked = _showToolTips;
                    }
                }
            }
        }

        private MagnifierWindow _magnifierWindow;
        private CalibrationWindow _calibrationWindow;

        public ICommand ToggleTopMostCommand { get; private set; }
        public ICommand ToggleLockResizingCommand { get; private set; }
        public ICommand ToggleOrientationCommand { get; private set; }
        public ICommand ToggleToolTipCommand { get; private set; }
        public ICommand ToggleSetSizeCommand { get; private set; }
        public ICommand ToggleDuplicateCommand { get; private set; }
        public ICommand ToggleShowGuidelineCommand { get; private set; }
        public ICommand ToggleResetToDefaultCommand { get; private set; }
        public ICommand ToggleAboutCommand { get; private set; }
        public ICommand ToggleCloseCommand { get; private set; }
        public ICommand ToggleExitCommand { get; private set; }
        public ICommand ToggleShowShortcutsCommand { get; private set; }
        public ICommand SetOpacityCommand { get; private set; }
        public ICommand MaxOpacityCommand { get; private set; }
        public ICommand MinOpacityCommand { get; private set; }
        public ICommand ToggleMagnifierCommand { get; private set; }
        public ICommand SetSaveTypeCommand { get; private set; }
        public ICommand SetMagnficationScaleCommand { get; private set; }
        public ICommand ToggleShowUpdateWindowCommand { get; private set; }
        public ICommand ToggleShowMenuTooltips { get; private set; }
        public ICommand ToggleCalibrationWindow { get; private set; }
        public ICommand ToggleMeasurementUnit { get; private set; }

        // DI Dependencies
        private readonly IRulerFactory _rulerFactory;
        private readonly IRulerRegistry _rulerRegistry;

        public MainWindow(RulerInfo info, IRulerFactory factory, IRulerRegistry rulerRegistry, IMonitorRegistry monitorRegistry)
        {
            _rulerInfo = info;
            _rulerFactory = factory;
            _rulerRegistry = rulerRegistry;
            _monitorRegistry = monitorRegistry ?? throw new ArgumentNullException(nameof(monitorRegistry));

            InitializeComponent();
            DataContext = this;
            _isUpdatingFromWindow = true;

            this.Width = _rulerInfo.Width;
            this.Height = _rulerInfo.Height;

            // Setup Interop and Initialize Monitor Context once handle is ready
            this.SourceInitialized += (s, e) =>
            {
                _hwndSource = PresentationSource.FromVisual(this) as HwndSource;
                _hwndSource?.AddHook(WndProc);

                _isInitialized = true;
                UpdateMonitorContext();
            };

            PoplateCommands();
            PopulateMenu();

            this.LocationChanged += OnWindowLocationChanged;
            this.SizeChanged += OnWindowSizeChanged;
            Loaded += async (s, e) => await CheckForUpdatesAsync();
            this.ContextMenuOpening += (s, e) => UpdateAllMenuStates();
            _isUpdatingFromWindow = false;
        }
        private void ApplyRulerInfoToWindow()
        {
            if (_rulerInfo == null) return;

            _isUpdatingFromWindow = true;

            // 1. Dimensions and position
            this.Width = _rulerInfo.Width;
            this.Height = _rulerInfo.Height;
            this.WindowStartupLocation = WindowStartupLocation.Manual;
            this.Left = _rulerInfo.Left;
            this.Top = _rulerInfo.Top;

            // 2. Window flags and visual settings
            this.Topmost = _rulerInfo.TopMost;
            this.Opacity = _rulerInfo.Opacity > 0 ? _rulerInfo.Opacity : 1.0;

            // 3. Tooltip overlay state
            if (_rulerInfo.ShowToolTip)
            {
                RefreshToolTipDisplay();
            }

            // 4. Magnifier state restoration
            if (_rulerInfo.Magnifier != null && _rulerInfo.Magnifier.IsActive)
            {
                if (_magnifierWindow == null)
                {
                    _magnifierWindow = new MagnifierWindow(RulerData) { Owner = this };
                    _magnifierWindow.Closed += (s, args) => _magnifierWindow = null;
                    _magnifierWindow.Show();
                }
            }

            _isUpdatingFromWindow = false;
        }
        private void UpdateMonitorContext()
        {
            Console.WriteLine($"Current Monitor {_currentProfile}");
            if (!_isInitialized) return;

            // Use MonitorTrackerService cleanly for low-level screen interop
            var (newDeviceName, monitorWidth, monitorHeight) = MonitorTrackerService.GetCurrentMonitorInfo(this);

            // If we are still on the same monitor, ensure resolution is synced
            if (_currentDeviceName == newDeviceName && _currentProfile != null)
            {
                _currentProfile.SyncResolution(monitorWidth, monitorHeight);
                Console.WriteLine($"Current Monitor {_currentProfile}");
                return;
            }

            // Unsubscribe from old profile to prevent memory leaks
            if (_currentProfile != null)
            {
                _currentProfile.CalibrationChanged -= OnProfileCalibrationChanged;
            }

            _currentDeviceName = newDeviceName;
            _rulerInfo.LastDeviceName = newDeviceName;

            // Fetch or create profile for this monitor from the thread-safe registry
            _currentProfile = _monitorRegistry.GetOrCreateProfile(newDeviceName);
            _currentProfile.CalibrationChanged += OnProfileCalibrationChanged;

            // Sync resolution using monitor's physical dimensions
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

        public void PoplateCommands()
        {
            ToggleTopMostCommand = new DelegateCommand(_ => {
                this.Topmost = !_rulerInfo.TopMost;
                _rulerInfo.TopMost = this.Topmost;
                InvalidateView();
            });
            ToggleShowUpdateWindowCommand = new DelegateCommand(_ => {
                UpdateWindow updateWindow = new UpdateWindow()
                {
                    Topmost = true,
                    Owner = this
                };
                updateWindow.ShowDialog();
            });
            ToggleLockResizingCommand = new DelegateCommand(_ =>
            {
                _rulerInfo.IsLocked = !_rulerInfo.IsLocked;
                InvalidateView();
            });
            ToggleOrientationCommand = new DelegateCommand(_ =>
            {
                _isUpdatingFromWindow = true;
                _rulerInfo.ToggleOrientation();
                this.Width = _rulerInfo.Width;
                this.Height = _rulerInfo.Height;
                InvalidateView();
                _isUpdatingFromWindow = false;
            });
            ToggleToolTipCommand = new DelegateCommand(_ =>
            {
                // Explicitly toggle only when command is invoked
                _rulerInfo.ShowToolTip = !_rulerInfo.ShowToolTip;
                RefreshToolTipDisplay();

                var menuItem = MenuItems.FirstOrDefault(i => i.Header == "Show ToolTip");
                if (menuItem != null)
                {
                    menuItem.IsChecked = _rulerInfo.ShowToolTip; // Fixed: now checks ShowToolTip instead of IsVertical
                }
                InvalidateView();
            });
            ToggleSetSizeCommand = new DelegateCommand(_ =>
            {
                UpdateSize();
                InvalidateView();
            });
            ToggleDuplicateCommand = new DelegateCommand(_ =>
            {
                DuplicateRequested?.Invoke(this, this.RulerData);
            });
            ToggleShowGuidelineCommand = new DelegateCommand(_ =>
            {
                _rulerInfo.Guideline.IsEnabled = !_rulerInfo.Guideline.IsEnabled;
                if (_rulerInfo.Guideline.IsEnabled && _rulerInfo.Guideline.Position == 0)
                {
                    var mousePos = Mouse.GetPosition(this);
                    bool isMouseInside = mousePos.X >= 0 && mousePos.X <= ActualWidth &&
                                         mousePos.Y >= 0 && mousePos.Y <= ActualHeight;

                    if (isMouseInside)
                    {
                        _rulerInfo.Guideline.Position = _rulerInfo.IsVertical ? mousePos.Y : mousePos.X;
                    }
                    else
                    {
                        _rulerInfo.Guideline.Position = _rulerInfo.IsVertical ? ActualHeight / 2 : ActualWidth / 2;
                    }
                }
                var menuItem = MenuItems.FirstOrDefault(i => i.Header == "Show Guideline");
                if (menuItem != null)
                {
                    menuItem.IsChecked = _rulerInfo.Guideline.IsEnabled;
                }
                this.InvalidateView();
            });
            ToggleResetToDefaultCommand = new DelegateCommand(_ =>
            {
                RulerInfo info = _rulerFactory.CreateDefault();
                _rulerFactory.CopyValues(info, this.RulerData);
                InvalidateView();
            });
            ToggleAboutCommand = new DelegateCommand(_ =>
            {
                string version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0";
                string message = string.Format(
                    "Original Ruler implemented by Jeff Key\nwww.sliver.com\nruler.codeplex.com\nIcon by Kristen Magee @ www.kbecca.com.\nMaintained by Andrija Cacanovic\nHosted on \nhttps://github.com/andrijac/ruler\nVersion {0}",
                    version);

                MessageBox.Show(message, "About Ruler", MessageBoxButton.OK, MessageBoxImage.Information);
            });
            ToggleCloseCommand = new DelegateCommand(_ => { this.Close(); });
            ToggleExitCommand = new DelegateCommand(_ => { Application.Current.Shutdown(); });
            ToggleShowShortcutsCommand = new DelegateCommand(_ =>
            {
                ShowShortcutsWindow shortcut = new ShowShortcutsWindow();
                shortcut.Topmost = true;
                shortcut.Owner = this;
                shortcut.ShowDialog();
            });
            SetSaveTypeCommand = new DelegateCommand<object>(param =>
            {
                // Depending on what your CommandParameter passes, check for the enum directly 
                // or through your MenuItemModel wrapper:
                if (param is SaveTypes selectedType)
                {
                    _rulerInfo.SaveType = selectedType;
                }
                else if (param is MenuItemModel model && model.Value is SaveTypes modelType)
                {
                    _rulerInfo.SaveType = modelType;
                }
                InvalidateView();
            });
            ToggleShowMenuTooltips = new DelegateCommand(_ => { ShowToolTips = !ShowToolTips; InvalidateView(); });
            SetMagnficationScaleCommand = new DelegateCommand<object>(param =>
            {
                double scale = 0;
                if (param is MenuItemModel model && model.Value is double dVal) scale = dVal;
                else if (param is double directVal) scale = directVal;

                if (scale > 0) _rulerInfo.Magnifier.ZoomLevel = scale;
            });
            SetOpacityCommand = new DelegateCommand<object>(parameter =>
            {
                double targetOpacity = this.Opacity;
                bool isRelative = false;
                double relativeDelta = 0;

                if (parameter is MenuItemModel model && model.Value is double modelVal) targetOpacity = modelVal;
                else if (parameter is string strVal)
                {
                    strVal = strVal.Trim();
                    if (strVal.StartsWith("+") || strVal.StartsWith("-"))
                    {
                        if (double.TryParse(strVal, out double delta))
                        {
                            isRelative = true;
                            relativeDelta = delta / 100.0;
                        }
                    }
                    else
                    {
                        string cleanStr = strVal.Replace("%", "").Trim();
                        if (double.TryParse(cleanStr, out double parsedVal))
                        {
                            targetOpacity = parsedVal > 1.0 ? parsedVal / 100.0 : parsedVal;
                        }
                    }
                }
                else if (parameter is int intVal) targetOpacity = intVal > 1.0 ? intVal / 100.0 : intVal;
                else if (parameter is double doubleVal) targetOpacity = doubleVal > 1.0 ? doubleVal / 100.0 : doubleVal;

                if (isRelative) AdjustOpacityStep(relativeDelta);
                else
                {
                    double finalOpacity = Math.Max(0.1, Math.Min(1.0, targetOpacity));
                    this.Opacity = finalOpacity;
                    _rulerInfo.Opacity = finalOpacity;
                }

                InvalidateView();
            });
            MaxOpacityCommand = new DelegateCommand(_ =>
            {
                this.Opacity = 1.0;
                _rulerInfo.Opacity = 1.0;
                this.InvalidateView();
            });
            MinOpacityCommand = new DelegateCommand(_ =>
            {
                this.Opacity = 0.1;
                _rulerInfo.Opacity = 0.1;
                this.InvalidateView();
            });
            ToggleMagnifierCommand = new DelegateCommand(_ =>
            {
                _rulerInfo.Magnifier.IsActive = !_rulerInfo.Magnifier.IsActive;
                if (!_rulerInfo.Magnifier.IsActive)
                {
                    _magnifierWindow?.Close();
                }
                else
                {
                    if (_magnifierWindow != null) _magnifierWindow.Show();
                    else
                    {
                        _magnifierWindow = new MagnifierWindow(RulerData) { Owner = this };
                        _magnifierWindow.Closed += (s, args) => _magnifierWindow = null;
                        _magnifierWindow.Show();
                    }
                }
            });
            ToggleCalibrationWindow = new DelegateCommand(_ =>
            {
                if (_calibrationWindow == null)
                {
                    _calibrationWindow = new CalibrationWindow(_monitorRegistry, _currentDeviceName) { Owner = this };
                    _calibrationWindow.Closed += (s, args) => _calibrationWindow = null;
                    _calibrationWindow.Show();
                }
                else
                {
                    _calibrationWindow.Activate();
                }
            });
            ToggleMeasurementUnit = new DelegateCommand<object>(val =>
            {
                RulerUnit unit = default;
                bool success = false;

                // 1. If it's passed directly as the enum
                if (val is RulerUnit directUnit)
                {
                    unit = directUnit;
                    success = true;
                }
                // 2. If it's the MenuItemModel wrapper, check its Value property
                else if (val is MenuItemModel model && model.Value is RulerUnit modelUnit)
                {
                    unit = modelUnit;
                    success = true;
                }
                // 3. If it's passed as a string
                else if (val is string str && Enum.TryParse(str, out RulerUnit parsedUnit))
                {
                    unit = parsedUnit;
                    success = true;
                }

                if (success)
                {
                    _rulerInfo.RulerUnits = unit;
                    
                }
                InvalidateView();
            });
        }

        public void PopulateMenu()
        {
            MenuItems.Clear();
            MenuItems.Add(new MenuItemModel { Header = "Stay On Top", IsCheckable = true, IsChecked = _rulerInfo.TopMost, InputGestureText = "T", Command = ToggleTopMostCommand });
            MenuItems.Add(new MenuItemModel { Header = "Lock Resizing", IsCheckable = true, IsChecked = _rulerInfo.IsLocked, InputGestureText = "Ctrl + L", Command = ToggleLockResizingCommand });
            MenuItems.Add(new MenuItemModel { Header = "Is Vertical?", IsCheckable = true, IsChecked = _rulerInfo.IsVertical, InputGestureText = "O/Space", Command = ToggleOrientationCommand });
            MenuItems.Add(new MenuItemModel { Header = "Set Ruler Size", InputGestureText = "R", Command = ToggleSetSizeCommand });
            MenuItems.Add(new MenuItemModel { Header = "Show ToolTip", IsCheckable = true, IsChecked = _rulerInfo.ShowToolTip, InputGestureText = "T", Command = ToggleToolTipCommand });
            MenuItems.Add(new MenuItemModel { Header = "Duplicate Ruler", InputGestureText = "D", Command = ToggleDuplicateCommand });
            MenuItems.Add(new MenuItemModel { Header = "Show Guideline", IsCheckable = true, IsChecked = _rulerInfo.Guideline.IsEnabled, InputGestureText = "G", Command = ToggleShowGuidelineCommand });

            var magnificationMenu = MenuHelper.CreateRangeMenuItems(1.5, 5.0, 0.5, "{0:0.#}x Zoom", i => i, SetMagnficationScaleCommand);
            MenuItems.Add(new MenuItemModel { Header = "Show Magnifier", IsCheckable = true, IsChecked = _rulerInfo.Magnifier.IsActive, InputGestureText = "M", Command = ToggleMagnifierCommand });
            MenuItems.Add(new MenuItemModel { Header = "Magnification Scale", Items = magnificationMenu });
            MenuItems.Add(new MenuItemModel { Header = "Reset to Default", InputGestureText = "Ctrl+R", Command = ToggleResetToDefaultCommand });
            MenuItems.Add(new MenuItemModel { Header = "Show About", InputGestureText = "A", Command = ToggleAboutCommand });
            MenuItems.Add(new MenuItemModel { Header = "Close Ruler", InputGestureText = "Esc", Command = ToggleCloseCommand });
            MenuItems.Add(new MenuItemModel { Header = "Exit Application", InputGestureText = "Ctrl + Esc", Command = ToggleExitCommand });
            MenuItems.Add(new MenuItemModel { Header = "Show keyboard Shortcuts", InputGestureText = "I", Command = ToggleShowShortcutsCommand });

            var opacitymenuitems = MenuHelper.CreateRangeMenuItems(10, 100, 5, "{0}%", i => (double)i / 100.0, SetOpacityCommand);
            MenuItems.Add(new MenuItemModel { Header = "Opacity", ToolTip = "Select i and see different opacity commands", Items = opacitymenuitems });

            var saveMenuItems = _rulerInfo.SaveType.ToMenuItems(SetSaveTypeCommand);
            MenuItems.Add(new MenuItemModel { Header = "Save Rule Data?", Items = saveMenuItems });

            MenuItems.Add(new MenuItemModel { Header = "Show Menu Tooltips", ToolTip = "Enable or disable tooltips", IsCheckable = true, IsChecked = ShowToolTips, Command = ToggleShowMenuTooltips });

            _updateMenuItem = new MenuItemModel { Header = "Update Available", ToolTip = "Click to update", Command = ToggleShowUpdateWindowCommand };
            MenuItems.Add(_updateMenuItem);
            var measurementMenuItems = _rulerInfo.RulerUnits.ToMenuItems(ToggleMeasurementUnit);
            MenuItems.Add(new MenuItemModel { Header = "Measurement Units", Items = measurementMenuItems });
            MenuItems.Add(new MenuItemModel { Header = "Calibrate Monitor", ToolTip = "Open the monitor calibration window", Command = ToggleCalibrationWindow });
            _originalToolTips.Clear();
            CacheToolTipsRecursively(MenuItems);
            UpdateAllMenuStates();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            var position = e.GetPosition(this);

            // 1. InfoOverlay mouse-following with edge-flipping logic
            if (_rulerInfo.ShowToolTip && InfoOverlay != null)
            {
                // Fallback dimensions if not yet measured
                double overlayWidth = InfoOverlay.ActualWidth > 0 ? InfoOverlay.ActualWidth : 120;
                double overlayHeight = InfoOverlay.ActualHeight > 0 ? InfoOverlay.ActualHeight : 25;

                // --- X Axis Flipping ---
                // Try placing it to the right of the cursor by default (+15px)
                double left = position.X + 15;
                // If it overflows the right edge, flip it to the left side of the cursor (-15px)
                if (left + overlayWidth > ActualWidth - 5)
                {
                    left = position.X - overlayWidth - 15;
                }
                // Final safety clamp for the left edge
                left = Math.Max(5, left);

                // --- Y Axis Flipping ---
                // Try placing it below the cursor by default (+15px)
                double top = position.Y + 15;
                // If it overflows the bottom edge, flip it above the cursor (-15px)
                if (top + overlayHeight > ActualHeight - 5)
                {
                    top = position.Y - overlayHeight - 15;
                }
                // Final safety clamp for the top edge
                top = Math.Max(5, top);

                InfoOverlay.Margin = new Thickness(left, top, 0, 0);
            }

            // 2. Existing guideline movement logic
            if (_rulerInfo?.Guideline?.IsEnabled != true || _rulerInfo.Guideline.IsLocked) return;

            if (position.X <= 0 && position.Y <= 0) return;
            double newPosition = _rulerInfo.IsVertical ? position.Y : position.X;

            double maxLimit = _rulerInfo.IsVertical ? ActualHeight : ActualWidth;
            if (newPosition < 0 || newPosition > maxLimit) return;

            _rulerInfo.Guideline.Position = newPosition;
            InvalidateView();
        }
        private string CreateRulerText()
        {
            var unitConfig = MeasurementUnit.Get(_rulerInfo.RulerUnits);
            double baseDpi = 96.0 * VisualTreeHelper.GetDpi(this).PixelsPerDip;
            double effectiveDpi = _currentProfile != null ? _currentProfile.GetEffectiveDpi(baseDpi) : baseDpi;

            // 1. Calculate total ruler length in current units
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

            // 2. Append guideline info if enabled
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
        private UIElement CreateMeasurementDisplayElement(double unitValue, string unitAbbrev, double effectiveDpi)
        {
            var unitConfig = MeasurementUnit.Get(_rulerInfo.RulerUnits);

            // If unit supports custom drawing (like Inches), render the stacked fraction via DrawingVisual
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
                // Fallback text format for Pixels, Centimeters, etc.
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
        private void UpdateToolTip()
        {
           // _rulerInfo.ShowToolTip = !_rulerInfo.ShowToolTip;

            if (_rulerInfo.ShowToolTip && InfoContentPanel != null)
            {
                InfoOverlay.Visibility = Visibility.Visible;
                InfoContentPanel.Children.Clear();

                var unitConfig = MeasurementUnit.Get(_rulerInfo.RulerUnits);
                double baseDpi = 96.0 * VisualTreeHelper.GetDpi(this).PixelsPerDip;
                double effectiveDpi = _currentProfile != null ? _currentProfile.GetEffectiveDpi(baseDpi) : baseDpi;

                // 1. Add Ruler Size label and custom drawn value
                InfoContentPanel.Children.Add(new TextBlock { Text = "Size: ", FontSize = 9, Foreground = Brushes.Black, VerticalAlignment = VerticalAlignment.Center });

                double totalPixels = _rulerInfo.IsVertical ? ActualHeight : ActualWidth;
                double totalUnitValue = unitConfig.PixelToUnit(totalPixels, effectiveDpi, totalPixels);

                InfoContentPanel.Children.Add(CreateMeasurementDisplayElement(totalUnitValue, unitConfig.Abbreviation, effectiveDpi));

                // 2. Add Guideline info if enabled
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

        private void UpdateSize()
        {
            var setSizeWindow = new SetSizeWindow(_rulerInfo)
            {
                Topmost = true,
                Owner = this
            };

            if (setSizeWindow.ShowDialog() == true)
            {
                // Update model state first
                _rulerInfo.Width = setSizeWindow.UpdatedWidth;
                _rulerInfo.Height = setSizeWindow.UpdatedHeight;
                _rulerInfo.RulerUnits = setSizeWindow.UpdatedUnit;

                // Sync window dimensions from the model
                this.Width = _rulerInfo.Width;
                this.Height = _rulerInfo.Height;

                InvalidateView();
            }
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (_rulerInfo.IsLocked)
            {
                if (msg == WM_SIZING || (msg == WM_SYSCOMMAND && (wParam.ToInt32() & 0xFFF0) == SC_SIZE))
                {
                    handled = true;
                    return IntPtr.Zero;
                }
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
                Console.WriteLine("Drawing vertical ruler");
                DrawVerticalRuler(drawingContext, tickPen, textBrush, fontTypeface, dpi);
            }
            else
            {
                Console.WriteLine("Drawing horizontal ruler");
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
            string infoText = CreateRulerText();
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
                    // Pass the integer subdivision index directly
                    tickLength = unitConfig.GetMinorTickLength?.Invoke(subIndex) ?? 4.0;
                }

                // Draw top and bottom tick marks
                dc.DrawLine(pen, new Point(x, 0), new Point(x, tickLength));
                dc.DrawLine(pen, new Point(x, ActualHeight), new Point(x, ActualHeight - tickLength));

                bool shouldLabel = unitConfig.ShouldLabel != null ? unitConfig.ShouldLabel(u) : (isMajor && u > 0);

                if (shouldLabel)
                {
                    if (unitConfig.CustomDraw != null)
                    {
                        if (x - lastLabelX > 16)
                        {
                            if (dualSided)
                            {
                                unitConfig.CustomDraw(dc, typeface, brush, pen, x, 22, u, effectiveDpi, lastLabelX, ActualHeight);
                                double bottomY = ActualHeight - tickLength - 14;
                                unitConfig.CustomDraw(dc, typeface, brush, pen, x, bottomY, u, effectiveDpi, lastLabelX, ActualHeight);
                            }
                            else
                            {
                                unitConfig.CustomDraw(dc, typeface, brush, pen, x, ActualHeight / 2, u, effectiveDpi, lastLabelX, ActualHeight);
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

                // Draw left and right tick marks
                dc.DrawLine(pen, new Point(0, y), new Point(tickLength, y));
                dc.DrawLine(pen, new Point(ActualWidth, y), new Point(ActualWidth - tickLength, y));

                bool shouldLabel = unitConfig.ShouldLabel != null ? unitConfig.ShouldLabel(u) : (isMajor && u > 0);

                if (shouldLabel)
                {
                    if (unitConfig.CustomDraw != null)
                    {
                        if (y - lastLabelY > 16)
                        {
                            if (dualSided)
                            {
                                // Left side: X = 22, Y = y
                                unitConfig.CustomDraw(dc, typeface, brush, pen, 22, y, u, effectiveDpi, lastLabelY, ActualHeight);
                                // Right side: X = ActualWidth - 22, Y = y
                                double rightX = ActualWidth - 22;
                                unitConfig.CustomDraw(dc, typeface, brush, pen, rightX, y, u, effectiveDpi, lastLabelY, ActualHeight);
                            }
                            else
                            {
                                // Single-sided: centered horizontally, Y = y
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
        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.ChangedButton == MouseButton.Left)
            {
                _mouseScreenPosition = this.PointToScreen(new Point(0, 0));
                this.DragMove();
            }
        }

        public void SetRulerInfo(RulerInfo ruler)
        {
            _rulerInfo = ruler;
            this.InvalidateVisual();
        }

        private void AdjustOpacityStep(double step)
        {
            double newOpacity = Math.Max(0.1, Math.Min(1.0, this.Opacity + step));
            this.Opacity = newOpacity;
            _rulerInfo.Opacity = newOpacity;
        }

        public void InvalidateView()
        {
            RefreshToolTipDisplay();
            InvalidateVisual();
        }

        public void SetBounds(int left, int top, int width, int height) => throw new NotImplementedException();
        public void SetTooltip(string text) => throw new NotImplementedException();
        public void ShowAbout() => throw new NotImplementedException();
        public void ShowShortcuts() => throw new NotImplementedException();

        private Point _mouseScreenPosition;
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
        public async Task CheckForUpdatesAsync()
        {
            string entryVersion = Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? "1.0.0.0";
            var udateinfo = await UpdateService.GetLatestGitHubAssetUrlsAsync(_owner, _repo);
            var latestVersion = await UpdateService.CheckForUpdateAsync(udateinfo.ManifestUrl, entryVersion);
            if (latestVersion.UpdateAvailable)
            {
                _updateMenuItem.Header = $"Update Available: {latestVersion.LatestVersion}";
            }
            else
            {
                _updateMenuItem.Header = $"No Updates Available (Current Version: {entryVersion})";
            }
        }

        public void Dispose() => throw new NotImplementedException();

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
                NudgeSize(dx * step, dy * step);
                e.Handled = true;
            }
            else
            {
                NudgeLocation(dx * step, dy * step);
                e.Handled = true;
            }
        }

        private void NudgeSize(int deltaWidth, int deltaHeight)
        {
            int newWidth = Math.Max(50, (int)this.Width + deltaWidth);
            int newHeight = Math.Max(50, (int)this.Height + deltaHeight);

            this.Width = newWidth;
            this.Height = newHeight;

            _rulerInfo.Width = newWidth;
            _rulerInfo.Height = newHeight;

            this.InvalidateView();
        }

        private void NudgeLocation(int deltaX, int deltaY)
        {
            this.Left += deltaX;
            this.Top += deltaY;

            _rulerInfo.Left = (int)this.Left;
            _rulerInfo.Top = (int)this.Top;

            this.InvalidateView();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected bool SetProperty<T>(T currentValue, T newValue, Action<T> setter, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(currentValue, newValue)) return false;
            setter(newValue);
            OnPropertyChanged(propertyName);
            return true;
        }

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string name = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(name);
            return true;
        }

        public void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public void UpdateAllMenuStates() => RecursiveUpdateStates(MenuItems, null);

        private void RecursiveUpdateStates(IEnumerable<MenuItemModel> items, string parentHeader)
        {
            if (items == null) return;
            foreach (var item in items)
            {
                EvaluateItemState(item, parentHeader);
                if (item.Items != null && item.Items.Count > 0)
                {
                    RecursiveUpdateStates(item.Items, item.Header);
                }
            }
        }

        private void EvaluateItemState(MenuItemModel item, string parentHeader)
        {
            if (item.Header == "Stay On Top") { item.IsCheckable = true; item.IsChecked = _rulerInfo.TopMost; }
            else if (item.Header == "Lock Resizing") { item.IsCheckable = true; item.IsChecked = _rulerInfo.IsLocked; }
            else if (item.Header == "Is Vertical?") { item.IsCheckable = true; item.IsChecked = _rulerInfo.IsVertical; }
            else if (item.Header == "Show ToolTip") { item.IsCheckable = true; item.IsChecked = _rulerInfo.ShowToolTip; }
            else if (item.Header == "Show Guideline") { item.IsCheckable = true; item.IsChecked = _rulerInfo.Guideline.IsEnabled; }
            else if (item.Header == "Show Magnifier") { item.IsCheckable = true; item.IsChecked = _rulerInfo.Magnifier.IsActive; }
            else if (item.Header == "Show Menu Tooltips") { item.IsCheckable = true; item.IsChecked = ShowToolTips; }
            else if (parentHeader == "Magnification Scale")
            {
                item.IsCheckable = true;
                if (item.Value is double scaleVal) item.IsChecked = Math.Abs(_rulerInfo.Magnifier.ZoomLevel - scaleVal) < 0.0001;
            }
            else if (parentHeader == "Opacity")
            {
                item.IsCheckable = true;
                if (item.Value is double opacityVal) item.IsChecked = Math.Abs(opacityVal - _rulerInfo.Opacity) < 0.001;
                else if (item.Value is int opacityInt)
                {
                    double itemOpacity = opacityInt / 100.0;
                    item.IsChecked = Math.Abs(itemOpacity - _rulerInfo.Opacity) < 0.001;
                }
            }
            else if (parentHeader == "Save Rule Data?")
            {
                item.IsCheckable = true;
                if (item.Value is SaveTypes saveTypeVal)
                {
                    item.IsChecked = (_rulerInfo.SaveType == saveTypeVal);
                }
            }
            // Added: Check for Measurement Units submenu items
            else if (parentHeader == "Units" || parentHeader == "Measurement Units")
            {
                item.IsCheckable = true;
                if (item.Value is RulerUnit unitVal)
                {
                    item.IsChecked = (_rulerInfo.RulerUnits == unitVal);
                }
            }
        }

        private void CacheToolTipsRecursively(IEnumerable<MenuItemModel> items)
        {
            if (items == null) return;
            foreach (var item in items)
            {
                if (!string.IsNullOrEmpty(item.ToolTip)) _originalToolTips[item] = item.ToolTip.ToString();
                if (item.Items != null && item.Items.Count > 0) CacheToolTipsRecursively(item.Items);
            }
        }

        private void Window_MouseDoubleClick(object sender, MouseButtonEventArgs e) => ToggleOrientationCommand.Execute(null);

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
        private void RefreshToolTipDisplay()
        {
            if (_rulerInfo.ShowToolTip && InfoContentPanel != null)
            {
                InfoOverlay.Visibility = Visibility.Visible;
                InfoContentPanel.Children.Clear();

                var unitConfig = MeasurementUnit.Get(_rulerInfo.RulerUnits);
                double baseDpi = 96.0 * VisualTreeHelper.GetDpi(this).PixelsPerDip;
                double effectiveDpi = _currentProfile != null ? _currentProfile.GetEffectiveDpi(baseDpi) : baseDpi;

                // 1. Add Ruler Size label and custom drawn value
                InfoContentPanel.Children.Add(new TextBlock { Text = "Size: ", FontSize = 9, Foreground = Brushes.Black, VerticalAlignment = VerticalAlignment.Center });

                double totalPixels = _rulerInfo.IsVertical ? ActualHeight : ActualWidth;
                double totalUnitValue = unitConfig.PixelToUnit(totalPixels, effectiveDpi, totalPixels);

                InfoContentPanel.Children.Add(CreateMeasurementDisplayElement(totalUnitValue, unitConfig.Abbreviation, effectiveDpi));

                // 2. Add Guideline info if enabled
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
    }
}