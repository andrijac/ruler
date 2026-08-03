using Ruler.Shared.Commands;
using Ruler.Shared.Enums;
using Ruler.Shared.Helpers;
using Ruler.Shared.Interfaces;
using Ruler.Shared.Models;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
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
    public partial class MainWindow : Window,INotifyPropertyChanged, IRuler
    {
        // Inside RulerWindow.xaml.cs
        public ObservableCollection<MenuItemModel> MenuItems { get; set; } = new ObservableCollection<MenuItemModel>();
        // Interop Constants
        private const int WM_SYSCOMMAND = 0x0112;
        private const int SC_SIZE = 0xF000;
        private const int WM_SIZING = 0x0214;

        private RulerInfo _rulerInfo;
        private HwndSource _hwndSource;

        public RulerInfo RulerData => _rulerInfo;
        public event EventHandler<RulerInfo> DuplicateRequested;
        private bool _showToolTips = true;
        public bool ShowToolTips
        {
            get => _showToolTips;
            set
            {
                SetProperty(ref _showToolTips,value);
            }
        }
        private MagnifierWindow _magnifierWindow;
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
            DataContext = this;
            // Set size based on info
            this.Width = _rulerInfo.Width;
            this.Height = _rulerInfo.Height;

            // Setup Interop for Locking/Resize logic
            this.SourceInitialized += (s, e) =>
            {
                _hwndSource = PresentationSource.FromVisual(this) as HwndSource;
                _hwndSource?.AddHook(WndProc);
            };
            PoplateCommands();
            PopulateMenu();
        }
        public void PoplateCommands()
        {
            ToggleTopMostCommand = new DelegateCommand(_ => {
                // Toggle both the WPF window and your model state
                this.Topmost = !_rulerInfo.TopMost;
                _rulerInfo.TopMost = this.Topmost;

                // Find the menu item and update its checkmark (handles keyboard triggers)
                var menuItem = MenuItems.FirstOrDefault(i => i.Header == "Stay On Top");
                if (menuItem != null)
                {
                    menuItem.IsChecked = this.Topmost;
                }
                InvalidateView();
            });
            ToggleLockResizingCommand = new DelegateCommand(_ =>
            {
                _rulerInfo.IsLocked = !_rulerInfo.IsLocked;
                var menuItem = MenuItems.FirstOrDefault(i => i.Header == "Lock Resizing");
                if (menuItem != null)
                {
                    menuItem.IsChecked = _rulerInfo.IsLocked;
                }
                InvalidateView();
        });
            ToggleOrientationCommand = new DelegateCommand(_ =>
            {
            _rulerInfo.ToggleOrientation();
            var menuItem = MenuItems.FirstOrDefault(i => i.Header == "Is Vertical?");
                if (menuItem != null)
                {
                    menuItem.IsChecked = _rulerInfo.IsVertical;
                }
                InvalidateView();
            });
            ToggleToolTipCommand = new DelegateCommand(_ =>
            {
                UpdateToolTip();
                var menuItem = MenuItems.FirstOrDefault(i => i.Header == "Show ToolTip");
                if (menuItem != null)
                {
                    menuItem.IsChecked = _rulerInfo.IsVertical;
                }
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
                // If enabling the guideline and position hasn't been set yet
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
                    "Original Ruler implemented by Jeff Key\n" +
                    "www.sliver.com\n" +
                    "ruler.codeplex.com\n" +
                    "Icon by Kristen Magee @ www.kbecca.com.\n" +
                    "Maintained by Andrija Cacanovic\n" +
                    "Hosted on \n" +
                    "https://github.com/andrijac/ruler\n" +
                    "Version {0}",
                    version);

                MessageBox.Show(
                    message,
                    "About Ruler",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            });
            ToggleCloseCommand = new DelegateCommand(_ =>
            {
                this.Close();
            });
            ToggleExitCommand = new DelegateCommand(_ =>
            {
                Application.Current.Shutdown();
            });
            ToggleShowShortcutsCommand = new DelegateCommand(_ =>
            {
                ShortcutWindow shortcut = new ShortcutWindow();
                shortcut.Topmost = true;
                shortcut.Owner = this;
                shortcut.ShowDialog();
            });
            SetSaveTypeCommand = new DelegateCommand<object>(param =>
            {
                if (param is MenuItemModel model && model.Value is SaveTypes selectedType)
                {
                    if (selectedType == SaveTypes.None)
                    {
                        _rulerInfo.SaveType = SaveTypes.None;
                    }
                    else if (selectedType == SaveTypes.All)
                    {
                        _rulerInfo.SaveType = SaveTypes.All;
                    }
                    else
                    {
                        // Toggle individual flag
                        if (_rulerInfo.SaveType.HasFlag(selectedType))
                        {
                            _rulerInfo.SaveType &= ~selectedType;
                        }
                        else
                        {
                            _rulerInfo.SaveType |= selectedType;
                        }
                    }

                    UpdateSaveTypeMenuStates();
                }
            });

            SetMagnficationScaleCommand = new DelegateCommand<object>(param =>
            {
                double scale = 0;

                // Handles whether the command parameter passes the MenuItemModel or the direct double value
                if (param is MenuItemModel model && model.Value is double dVal)
                {
                    scale = dVal;
                }
                else if (param is double directVal)
                {
                    scale = directVal;
                }

                if (scale > 0)
                {
                    _rulerInfo.Magnifier.ZoomLevel = scale;

                    // Optional: Update check states for magnification menu items if you have a state-updater method
                    UpdateMagnificationMenuStates();
                }
            });
            SetOpacityCommand = new DelegateCommand<object>(parameter =>
            {
                double targetOpacity = this.Opacity;
                bool isRelative = false;
                double relativeDelta = 0;

                if (parameter is MenuItemModel model && model.Value is double modelVal)
                {
                    // 1. Absolute value from the Context Menu item
                    targetOpacity = modelVal;
                }
                else if (parameter is string strVal)
                {
                    strVal = strVal.Trim();

                    // Check if it's a relative change starting with '+' or '-' (e.g., "-5" or "+5")
                    if (strVal.StartsWith("+") || strVal.StartsWith("-"))
                    {
                        if (double.TryParse(strVal, out double delta))
                        {
                            isRelative = true;
                            relativeDelta = delta / 100.0; // Converts -5 to -0.05
                        }
                    }
                    else
                    {
                        // Absolute string value (e.g., "50%" or "0.5")
                        string cleanStr = strVal.Replace("%", "").Trim();
                        if (double.TryParse(cleanStr, out double parsedVal))
                        {
                            targetOpacity = parsedVal > 1.0 ? parsedVal / 100.0 : parsedVal;
                        }
                    }
                }
                else if (parameter is int intVal)
                {
                    targetOpacity = intVal > 1.0 ? intVal / 100.0 : intVal;
                }
                else if (parameter is double doubleVal)
                {
                    targetOpacity = doubleVal > 1.0 ? doubleVal / 100.0 : doubleVal;
                }

                // Apply the change
                if (isRelative)
                {
                    AdjustOpacityStep(relativeDelta); // Uses your existing step logic
                }
                else
                {
                    double finalOpacity = Math.Max(0.1, Math.Min(1.0, targetOpacity));
                    this.Opacity = finalOpacity;
                    _rulerInfo.Opacity = finalOpacity;
                }

                UpdateOpacityMenuStates();
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
                }
                );
            ToggleMagnifierCommand = new DelegateCommand(_ =>
            {
                if (_magnifierWindow != null)
                {
                    // If it's already open, close it
                    _magnifierWindow.Close();
                }
                else
                {
                    // If it's closed, create, track, and show it
                    _magnifierWindow = new MagnifierWindow(RulerData)
                    {
                        Owner = this
                    };

                    // Clear the reference automatically when the window closes
                    _magnifierWindow.Closed += (s, args) => _magnifierWindow = null;

                    _magnifierWindow.Show();
                }
            });
        }

        public void PopulateMenu()
        {
            MenuItems.Clear();
            MenuItems.Add(new MenuItemModel()
            {
                Header = "Stay On Top",
                IsCheckable = true,
                IsChecked = _rulerInfo.TopMost,
                InputGestureText = "T",
                Value = null,
                Command = ToggleTopMostCommand
            });
            MenuItems.Add(new MenuItemModel()
            {
                Header = "Lock Resizing",
                IsCheckable = true,
                IsChecked = _rulerInfo.IsLocked,
                InputGestureText = "Ctrl + L",
                Value = null,
                Command = ToggleLockResizingCommand
            });
            MenuItems.Add(new MenuItemModel()
            {
                Header = "Is Vertical?",
                IsCheckable = true,
                IsChecked = _rulerInfo.IsVertical,
                InputGestureText = "O/Space",
                Value = null,
                Command = ToggleOrientationCommand
            });
            MenuItems.Add(new MenuItemModel()
            {
                Header = "Set Ruler Size",
                IsCheckable = false,
                IsChecked = false,
                InputGestureText = "R",
                Value = null,
                Command = ToggleSetSizeCommand
            });
            MenuItems.Add(new MenuItemModel()
            {
                Header = "Show ToolTip",
                IsCheckable = true,
                IsChecked = _rulerInfo.ShowToolTip,
                InputGestureText = "T",
                Value = null,
                Command = ToggleToolTipCommand
            });
            MenuItems.Add(new MenuItemModel()
            {
                Header = "Duplicate Ruler",
                IsCheckable = false,
                InputGestureText = "D",
                Command = ToggleDuplicateCommand
                
            });
            MenuItems.Add(new MenuItemModel()
            {
                Header = "Show Guideline",
                IsCheckable = true,
                IsChecked = _rulerInfo.Guideline.IsEnabled,
                InputGestureText = "G",
                Command = ToggleShowGuidelineCommand
            });
             var magnificationMenu = MenuHelper.CreateRangeMenuItems(
    start: 1.5,
    end: 10.0,
    step: 0.5,
    formatString: "{0:0.#}x Zoom", // {0:0.#} ensures clean formatting like "1.5x Zoom" or "2x Zoom"
    valueSelector: i => i,          // Value is already a double
    command: SetMagnficationScaleCommand
);
            MenuItems.Add(new MenuItemModel()
            {
                Header = "Show Magnifier",
                IsCheckable = true,
                IsChecked = _rulerInfo.Magnifier.IsActive,
                InputGestureText = "M",
                Command = ToggleMagnifierCommand
            });
            MenuItems.Add(new MenuItemModel()
            {
                Header = "Magnification Scale",
                IsCheckable = false,
                IsChecked = false,
                Items = magnificationMenu
            });
            MenuItems.Add(new MenuItemModel()
            {
                Header = "Reset to Default",
                IsCheckable = false,
                InputGestureText= "Ctrl+R",
                Command = ToggleResetToDefaultCommand
            });
            MenuItems.Add(new MenuItemModel()
            {
                Header = "Show About",
                IsCheckable = false,
               InputGestureText = "A",
                Command = ToggleAboutCommand
            });
            MenuItems.Add(new MenuItemModel()
            {
                Header = "Close Ruler",
                IsCheckable = false,
                InputGestureText = "Esc",
                Command = ToggleCloseCommand
            });
            MenuItems.Add( new MenuItemModel()
            {
                Header = "Exit Application",
                IsCheckable = false,
                InputGestureText = "Ctrl + Esc",
                Command = ToggleExitCommand
            });
            MenuItems.Add( new MenuItemModel()
            {
                Header = "Show keyboard Shortcuts",
                IsCheckable = false,
                InputGestureText = "I",
                Command = ToggleShowShortcutsCommand
            });
            var opacitymenuitems = MenuHelper.CreateRangeMenuItems(
    start: 10,
    end: 100,
    step: 5,
    formatString: "{0}%",
    valueSelector: i => (double)i / 100.0, // Converts 50 to 0.5
    command: SetOpacityCommand
);
            MenuItemModel opacity = new MenuItemModel()
            {
                Header = "Opacity",
                ToolTip = "Select i and see the different opacity commands on the short cuts page",
                Items = opacitymenuitems
               
            };
            var saveMenuItems = SaveTypes.None.ToMenuItems(SetSaveTypeCommand);
            MenuItemModel saveTypes = new MenuItemModel()
            {
                Header = "Save Rule Data?",
                Items = saveMenuItems
                
            };

            MenuItems.Add(new MenuItemModel
            {
                Header = "Show Menu Tooltips",
                ToolTip = "Enable or disable tooltips across the context menu",
                IsCheckable = true,
                IsChecked = ShowToolTips,
                Command = new DelegateCommand(_ => ShowToolTips = !ShowToolTips)
            });

        }
        private void UpdateOpacityMenuStates()
        {
            double currentOpacity = _rulerInfo.Opacity;

            foreach (var item in MenuItems)
            {
                if (!item.IsCheckable || item.Value == null) continue;

                double itemOpacity = 0.0;
                bool isValid = false;

                // Pattern matching handles int, double, or string automatically
                if (item.Value is int intVal)
                {
                    itemOpacity = intVal / 100.0;
                    isValid = true;
                }
                else if (item.Value is double doubleVal)
                {
                    itemOpacity = doubleVal > 1.0 ? doubleVal / 100.0 : doubleVal;
                    isValid = true;
                }
                else if (item.Value is string strVal && double.TryParse(strVal.Replace("%", "").Trim(), out double parsedVal))
                {
                    itemOpacity = parsedVal > 1.0 ? parsedVal / 100.0 : parsedVal;
                    isValid = true;
                }

                if (isValid)
                {
                    // Use a small epsilon tolerance for floating-point comparison
                    item.IsChecked = Math.Abs(itemOpacity - currentOpacity) < 0.001;
                }
            }
        }
        private void UpdateSaveTypeMenuStates()
        {
            var saveTypeParent = MenuItems.FirstOrDefault(i => i.Header == "Save Settings");
            if (saveTypeParent?.Items == null) return;

            foreach (var item in saveTypeParent.Items)
            {
                if (item.Value is SaveTypes enumValue)
                {
                    if (enumValue == SaveTypes.None)
                    {
                        item.IsChecked = (_rulerInfo.SaveType == SaveTypes.None);
                    }
                    else if (enumValue == SaveTypes.All)
                    {
                        item.IsChecked = (_rulerInfo.SaveType == SaveTypes.All);
                    }
                    else
                    {
                        item.IsChecked = _rulerInfo.SaveType.HasFlag(enumValue);
                    }
                }
            }
        }
        private void UpdateMagnificationMenuStates()
        {
            var magnificationParent = MenuItems.FirstOrDefault(i => i.Header == "Magnification Scale");
            if (magnificationParent?.Items == null) return;

            foreach (var item in magnificationParent.Items)
            {
                if (item.Value is double scaleValue)
                {
                    item.IsChecked = Math.Abs(_rulerInfo.Magnifier.ZoomLevel - scaleValue) < 0.0001;
                }
            }
        }
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (!_rulerInfo.Guideline.IsLocked)
            {
                var position = e.GetPosition(this);

                // If vertical ruler, track Y coordinate; otherwise track X coordinate
                _rulerInfo.Guideline.Position = _rulerInfo.IsVertical ? position.Y : position.X;
            }
        }

        private void UpdateToolTip()
        {
            _rulerInfo.ShowToolTip = !_rulerInfo.ShowToolTip;
            string toolTipString;
            if (_rulerInfo.ShowToolTip)
            {
                toolTipString = _rulerInfo.IsVertical
    ? $"{(int)this.Height} pixels"
    : $"{(int)this.Width} pixels";
                if (_rulerInfo.Guideline.IsEnabled && _rulerInfo.Guideline.IsLocked)
                {

                    toolTipString += $"{Environment.NewLine}Guideline: {(int)_rulerInfo.Guideline.Position}";
                }
                this.ToolTip = toolTipString;
            }
            else
            {
                this.ToolTip = null;
            }
        }
        private void UpdateSize()
        {
            var setSizeWindow = new SetSizeWindow((int)this.Width, (int)this.Height);
            setSizeWindow.Topmost = true;
            setSizeWindow.Owner = this;
            if (setSizeWindow.ShowDialog() == true)
            {
                var size = setSizeWindow.GetNewSize();
                this.Width=size.Width;
                this.Height = size.Height;
                _rulerInfo.Width =(int) this.Width;
                _rulerInfo.Height =(int) this.Height;
            }
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
        }

        private void DrawHorizontalRuler(DrawingContext dc, Pen pen, Brush brush, Typeface typeface, double dpi)
        {
            bool dualSided = ActualHeight > 100;

            for (double x = 0; x <= ActualWidth; x += 10)
            {
                bool isMajor = (x % 50 == 0);
                bool isMedium = (x % 25 == 0);
                double tickLength = isMajor ? 12 : (isMedium ? 8 : 4);

                dc.DrawLine(pen, new Point(x, 0), new Point(x, tickLength));
                dc.DrawLine(pen, new Point(x, ActualHeight), new Point(x, ActualHeight - tickLength));

                if (isMajor && x > 0)
                {
                    var formattedText = new FormattedText(
                        x.ToString(),
                        System.Globalization.CultureInfo.CurrentCulture,
                        FlowDirection.LeftToRight,
                        typeface,
                        10,
                        brush,
                        dpi);

                    if (dualSided)
                    {
                        dc.DrawText(formattedText, new Point(x - (formattedText.Width / 2), 15));
                        dc.DrawText(formattedText, new Point(x - (formattedText.Width / 2), ActualHeight - 25));
                    }
                    else
                    {
                        dc.DrawText(formattedText, new Point(x - (formattedText.Width / 2), (ActualHeight / 2) - (formattedText.Height / 2)));
                    }
                }
            }
        }

        private void DrawVerticalRuler(DrawingContext dc, Pen pen, Brush brush, Typeface typeface, double dpi)
        {
            bool dualSided = ActualWidth > 100;

            for (double y = 0; y <= ActualHeight; y += 10)
            {
                bool isMajor = (y % 50 == 0);
                bool isMedium = (y % 25 == 0);
                double tickLength = isMajor ? 12 : (isMedium ? 8 : 4);

                dc.DrawLine(pen, new Point(0, y), new Point(tickLength, y));
                dc.DrawLine(pen, new Point(ActualWidth, y), new Point(ActualWidth - tickLength, y));

                if (isMajor && y > 0)
                {
                    var formattedText = new FormattedText(
                        y.ToString(),
                        System.Globalization.CultureInfo.CurrentCulture,
                        FlowDirection.LeftToRight,
                        typeface,
                        10,
                        brush,
                        dpi);

                    if (dualSided)
                    {
                        dc.DrawText(formattedText, new Point(22, y - (formattedText.Height / 2)));
                        dc.DrawText(formattedText, new Point(ActualWidth - 28, y - (formattedText.Height / 2)));
                    }
                    else
                    {
                        dc.DrawText(formattedText, new Point((ActualWidth / 2) - (formattedText.Width / 2), y - (formattedText.Height / 2)));
                    }
                }
            }
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

        private void AdjustOpacityStep(double step)
        {
            // Clamp between 0.1 (10%) and 1.0 (100%) using Max and Min
            double newOpacity = Math.Max(0.1, Math.Min(1.0, this.Opacity + step));

            // Apply to the WPF Window
            this.Opacity = newOpacity;

            // Sync back to your model state
            _rulerInfo.Opacity = newOpacity;
        }

        public void InvalidateView()
        {

            UpdateToolTip();
            InvalidateVisual();
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
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            bool isCtrl = Keyboard.Modifiers.HasFlag(ModifierKeys.Control);
            bool isShift = Keyboard.Modifiers.HasFlag(ModifierKeys.Shift);
            int step = isCtrl ? 1 : 10; // Ctrl = 1px fine step, Normal = 10px step

            // 1. Determine direction first (Checking diagonals and cardinals)
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
            else { return; } // Exit if no direction keys are pressed

            // 2. Check modifiers to do the correct action
            if (isShift)
            {
                // Shift is held: Resize the ruler using the calculated direction & step
                NudgeSize(dx * step, dy * step);
                e.Handled = true;
            }
            else
            {
                // No Shift (Normal or Control): Move/Nudge the ruler's location
                NudgeLocation(dx * step, dy * step);
                e.Handled = true;
            }
        }
        private void NudgeSize(int deltaWidth, int deltaHeight)
        {
            // Keep a minimum size limit of 50 logical pixels
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

            setter(newValue); // Execute the update on the model object

            
                OnPropertyChanged(propertyName);
           
            return true;
        }

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string name = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;

            field = value;

         
                OnPropertyChanged(name);
            

            return true;
        }

        // Centralized event raiser that honors the suppression flag
        public void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public void RefreshAll()
        {
            OnPropertyChanged(string.Empty);
        }
    }
}
