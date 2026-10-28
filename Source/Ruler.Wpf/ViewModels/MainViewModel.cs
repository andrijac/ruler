using Ruler.Shared.Commands;
using Ruler.Shared.Enums;
using Ruler.Shared.Helpers;
using Ruler.Shared.Interfaces;
using Ruler.Shared.Models;
using Ruler.Shared.Services;
using Ruler.Wpf.Windows;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Ruler.Wpf.ViewModels
{
    public class RulerViewModel : ViewModelBase
    {
        private RulerInfo _rulerInfo;
       
        private readonly IMonitorRegistry _monitorRegistry;
        private readonly IRulerFactory _rulerFactory;

        public RulerInfo RulerData
        {
            get => _rulerInfo;
            set => SetProperty(ref _rulerInfo, value);
        }

        public ObservableCollection<MenuItemModel> MenuItems { get; set; } = new ObservableCollection<MenuItemModel>();

        private bool _showToolTips = true;
        public bool ShowToolTips
        {
            get => _showToolTips;
            set
            {
                if (SetProperty(ref _showToolTips, value))
                {
                    var menuItem = MenuItems.FirstOrDefault(i => i.Header == "Show Menu Tooltips");
                    if (menuItem != null)
                    {
                        menuItem.IsChecked = _showToolTips;
                    }
                }
            }
        }

        private string _updateMenuHeader = "Check for Updates";
        public string UpdateMenuHeader
        {
            get => _updateMenuHeader;
            set => SetProperty(ref _updateMenuHeader, value);
        }

        // --- Action Callbacks for View-Side Interop & Dialogs ---
        public Action RequestInvalidate { get; set; }
        public Action RequestOpenSetSize { get; set; }
        public Action RequestOpenCalibration { get; set; }
        public Action RequestOpenUpdate { get; set; }
        public Action RequestOpenShortcuts { get; set; }
        public Action RequestOpenAbout { get; set; }
        public Action RequestOpenMagnifier { get; set; }
        public Action RequestCloseWindow { get; set; }
        public Action RequestExit { get; set; }
        public Action<int, int> RequestResizeWindow { get; set; } // (width, height)
        public Action<int, int> RequestMoveWindow { get; set; }   // (deltaX, deltaY)
        public Action SyncTooltip { get; set; }

        public event EventHandler<RulerInfo> DuplicateRequested;

        // --- Command Properties ---
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
        public ICommand SetRulerScaleCommand { get; private set; }

#if DEBUG
        private readonly string _owner = "IsaacMorris1980";
#else
        private readonly string _owner = "andrijac";
#endif
        private readonly string _repo = "ruler";

        public RulerViewModel(RulerInfo info, IMonitorRegistry monitorRegistry, IRulerFactory rulerFactory)
        {
            _rulerInfo = info ?? throw new ArgumentNullException(nameof(info));
            _monitorRegistry = monitorRegistry ?? throw new ArgumentNullException(nameof(monitorRegistry));
            _rulerFactory = rulerFactory ?? throw new ArgumentNullException(nameof(rulerFactory));

            InitializeCommands();
            PopulateMenu();
        }

        private void InitializeCommands()
        {
            ToggleTopMostCommand = new DelegateCommand(_ => {
                RulerData.TopMost = !RulerData.TopMost;
                OnPropertyChanged(nameof(RulerData));
                RequestInvalidate?.Invoke();
            });

            ToggleLockResizingCommand = new DelegateCommand(_ => {
                RulerData.IsLocked = !RulerData.IsLocked;
                RequestInvalidate?.Invoke();
            });

            ToggleOrientationCommand = new DelegateCommand(_ => {
                RulerData.ToggleOrientation();
                RequestResizeWindow?.Invoke(RulerData.Width, RulerData.Height);
                RequestInvalidate?.Invoke();
            });

            ToggleToolTipCommand = new DelegateCommand(_ => {
                RulerData.ShowToolTip = !RulerData.ShowToolTip;
              SyncTooltip?.Invoke();
                var menuItem = MenuItems.FirstOrDefault(i => i.Header == "Show ToolTip");
                if (menuItem != null)
                {
                    menuItem.IsChecked = RulerData.ShowToolTip;
                }
                RequestInvalidate?.Invoke();
            });

            ToggleSetSizeCommand = new DelegateCommand(_ => {
                RequestOpenSetSize?.Invoke();
            });

            ToggleDuplicateCommand = new DelegateCommand(_ => {
                DuplicateRequested?.Invoke(this, RulerData);
            });

            ToggleShowGuidelineCommand = new DelegateCommand(_ => {
                RulerData.Guideline.IsEnabled = !RulerData.Guideline.IsEnabled;
                var menuItem = MenuItems.FirstOrDefault(i => i.Header == "Show Guideline");
                if (menuItem != null)
                {
                    menuItem.IsChecked = RulerData.Guideline.IsEnabled;
                }
                RequestInvalidate?.Invoke();
            });

            ToggleResetToDefaultCommand = new DelegateCommand(_ => {
                RulerInfo info = _rulerFactory.CreateDefault();
                _rulerFactory.CopyValues(info, RulerData);
                RequestResizeWindow?.Invoke(RulerData.Width, RulerData.Height);
                OnPropertyChanged(nameof(RulerData));
                RequestInvalidate?.Invoke();
            });

            ToggleAboutCommand = new DelegateCommand(_ => {
                RequestOpenAbout?.Invoke();
            });

            ToggleCloseCommand = new DelegateCommand(_ => {
                RequestCloseWindow?.Invoke();
            });

            ToggleExitCommand = new DelegateCommand(_ => {
                RequestExit?.Invoke();
            });

            ToggleShowShortcutsCommand = new DelegateCommand(_ => {
                RequestOpenShortcuts?.Invoke();
            });

            SetSaveTypeCommand = new DelegateCommand<object>(param => {
                if (param is SaveTypes selectedType)
                {
                    RulerData.SaveType = selectedType;
                }
                else if (param is MenuItemModel model && model.Value is SaveTypes modelType)
                {
                    RulerData.SaveType = modelType;
                }
                RequestInvalidate?.Invoke();
            });

            SetMagnficationScaleCommand = new DelegateCommand<object>(param => {
                double scale = 0;
                if (param is MenuItemModel model && model.Value is double dVal) scale = dVal;
                else if (param is double directVal) scale = directVal;

                if (scale > 0) RulerData.Magnifier.ZoomLevel = scale;
            });

            SetOpacityCommand = new DelegateCommand<object>(parameter => {
                double targetOpacity = RulerData.Opacity;
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

                if (isRelative)
                {
                    RulerData.Opacity = Math.Max(0.1, Math.Min(1.0, RulerData.Opacity + relativeDelta));
                }
                else
                {
                    RulerData.Opacity = Math.Max(0.1, Math.Min(1.0, targetOpacity));
                }
                RequestInvalidate?.Invoke();
            });

            MaxOpacityCommand = new DelegateCommand(_ => {
                RulerData.Opacity = 1.0;
                RequestInvalidate?.Invoke();
            });

            MinOpacityCommand = new DelegateCommand(_ => {
                RulerData.Opacity = 0.1;
                RequestInvalidate?.Invoke();
            });

            ToggleMagnifierCommand = new DelegateCommand(_ => {
                RulerData.Magnifier.IsActive = !RulerData.Magnifier.IsActive;
                RequestOpenMagnifier?.Invoke();
            });

            ToggleCalibrationWindow = new DelegateCommand(_ => {
                RequestOpenCalibration?.Invoke();
            });

            ToggleShowUpdateWindowCommand = new DelegateCommand(_ => {
                RequestOpenUpdate?.Invoke();
            });

            ToggleShowMenuTooltips = new DelegateCommand(_ => {
                ShowToolTips = !ShowToolTips;
                RequestInvalidate?.Invoke();
            });
            SetRulerScaleCommand = new DelegateCommand<object>(param => {
                double scale = 1.0;
                if (param is MenuItemModel model && model.Value is double dVal) scale = dVal;
                else if (param is double directVal) scale = directVal;

                if (RulerData.RulerUnits == RulerUnit.Pixels)
                {
                    RulerScale = Math.Max(0.25, Math.Min(5.0, scale));
                }
            });

            ToggleMeasurementUnit = new DelegateCommand<object>(val => {
                RulerUnit unit = default;
                bool success = false;

                if (val is RulerUnit directUnit)
                {
                    unit = directUnit;
                    success = true;
                }
                else if (val is MenuItemModel model && model.Value is RulerUnit modelUnit)
                {
                    unit = modelUnit;
                    success = true;
                }
                else if (val is string str && Enum.TryParse(str, out RulerUnit parsedUnit))
                {
                    unit = parsedUnit;
                    success = true;
                }

                if (success)
                {
                    RulerData.RulerUnits = unit;
                }
                RequestInvalidate?.Invoke();
            });
        }

        public void NudgeSize(int deltaWidth, int deltaHeight)
        {
            int newWidth = Math.Max(50, RulerData.Width + deltaWidth);
            int newHeight = Math.Max(50, RulerData.Height + deltaHeight);

            RulerData.Width = newWidth;
            RulerData.Height = newHeight;

            RequestResizeWindow?.Invoke(newWidth, newHeight);
            RequestInvalidate?.Invoke();
        }

        public void NudgeLocation(int deltaX, int deltaY)
        {
            RulerData.Left += deltaX;
            RulerData.Top += deltaY;

            RequestMoveWindow?.Invoke(deltaX, deltaY);
            RequestInvalidate?.Invoke();
        }

     
        public void PopulateMenu()
        {
            MenuItems.Clear();
            MenuItems.Add(new MenuItemModel { Header = "Stay On Top", IsCheckable = true, IsChecked = RulerData.TopMost, InputGestureText = "T", Command = ToggleTopMostCommand });
            MenuItems.Add(new MenuItemModel { Header = "Lock Resizing", IsCheckable = true, IsChecked = RulerData.IsLocked, InputGestureText = "Ctrl + L", Command = ToggleLockResizingCommand });
            MenuItems.Add(new MenuItemModel { Header = "Is Vertical?", IsCheckable = true, IsChecked = RulerData.IsVertical, InputGestureText = "O/Space", Command = ToggleOrientationCommand });
            MenuItems.Add(new MenuItemModel { Header = "Set Ruler Size", InputGestureText = "R", Command = ToggleSetSizeCommand });
            MenuItems.Add(new MenuItemModel { Header = "Show ToolTip", IsCheckable = true, IsChecked = RulerData.ShowToolTip, InputGestureText = "T", Command = ToggleToolTipCommand });
            MenuItems.Add(new MenuItemModel { Header = "Duplicate Ruler", InputGestureText = "D", Command = ToggleDuplicateCommand });
            MenuItems.Add(new MenuItemModel { Header = "Show Guideline", IsCheckable = true, IsChecked = RulerData.Guideline.IsEnabled, InputGestureText = "G", Command = ToggleShowGuidelineCommand });

            var magnificationMenu = MenuHelper.CreateRangeMenuItems(1.5, 5.0, 0.5, "{0:0.#}x Zoom", i => i, SetMagnficationScaleCommand);
            MenuItems.Add(new MenuItemModel { Header = "Show Magnifier", IsCheckable = true, IsChecked = RulerData.Magnifier.IsActive, InputGestureText = "M", Command = ToggleMagnifierCommand });
            MenuItems.Add(new MenuItemModel { Header = "Magnification Scale", Items = magnificationMenu });
            var scaleMenuItems = new ObservableCollection<MenuItemModel>();
            double[] chromeZooms = { 0.25, 0.33, 0.50, 0.67, 0.75, 0.80, 0.90, 1.00, 1.10, 1.25, 1.50, 1.75, 2.00, 2.50, 3.00, 4.00, 5.00 };
            foreach (var zoom in chromeZooms)
            {
                int percent = (int)Math.Round(zoom * 100);
                scaleMenuItems.Add(new MenuItemModel
                {
                    Header = $"{percent}%",
                    Value = zoom,
                    Command = SetRulerScaleCommand
                });
            }
            MenuItems.Add(new MenuItemModel { Header = "Ruler Scale", ToolTip = "Scale ruler size (Pixels mode only)", Items = scaleMenuItems });
            MenuItems.Add(new MenuItemModel { Header = "Reset to Default", InputGestureText = "Ctrl+R", Command = ToggleResetToDefaultCommand});
            MenuItems.Add(new MenuItemModel { Header = "Show About", InputGestureText = "A", Command = ToggleAboutCommand });
            MenuItems.Add(new MenuItemModel { Header = "Close Ruler", InputGestureText = "Esc", Command = ToggleCloseCommand });
            MenuItems.Add(new MenuItemModel { Header = "Exit Application", InputGestureText = "Ctrl + Esc", Command = ToggleExitCommand });
            MenuItems.Add(new MenuItemModel { Header = "Show keyboard Shortcuts", InputGestureText = "I", Command = ToggleShowShortcutsCommand });

            var opacitymenuitems = MenuHelper.CreateRangeMenuItems(10, 100, 5, "{0}%", i => (double)i / 100.0, SetOpacityCommand);
            MenuItems.Add(new MenuItemModel { Header = "Opacity", ToolTip = "Select and see different opacity commands", Items = opacitymenuitems });

            var saveMenuItems = RulerData.SaveType.ToMenuItems(SetSaveTypeCommand);
            MenuItems.Add(new MenuItemModel { Header = "Save Rule Data?", Items = saveMenuItems });

            MenuItems.Add(new MenuItemModel { Header = "Show Menu Tooltips", ToolTip = "Enable or disable tooltips", IsCheckable = true, IsChecked = ShowToolTips, Command = ToggleShowMenuTooltips });

            MenuItems.Add(new MenuItemModel { Header = UpdateMenuHeader, ToolTip = "Click to update", Command = ToggleShowUpdateWindowCommand });

            var measurementMenuItems = RulerData.RulerUnits.ToMenuItems(ToggleMeasurementUnit);
            MenuItems.Add(new MenuItemModel { Header = "Measurement Units", Items = measurementMenuItems });

            MenuItems.Add(new MenuItemModel { Header = "Calibrate Monitor", ToolTip = "Open the monitor calibration window", Command = ToggleCalibrationWindow });
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
            if (item.Header == "Stay On Top") { item.IsCheckable = true; item.IsChecked = RulerData.TopMost; }
            else if (item.Header == "Lock Resizing") { item.IsCheckable = true; item.IsChecked = RulerData.IsLocked; }
            else if (item.Header == "Is Vertical?") { item.IsCheckable = true; item.IsChecked = RulerData.IsVertical; }
            else if (item.Header == "Show ToolTip") { item.IsCheckable = true; item.IsChecked = RulerData.ShowToolTip; }
            else if (item.Header == "Show Guideline") { item.IsCheckable = true; item.IsChecked = RulerData.Guideline.IsEnabled; }
            else if (item.Header == "Show Magnifier") { item.IsCheckable = true; item.IsChecked = RulerData.Magnifier.IsActive; }
            else if (item.Header == "Show Menu Tooltips") { item.IsCheckable = true; item.IsChecked = ShowToolTips; }
            else if (parentHeader == "Magnification Scale")
            {
                item.IsCheckable = true;
                if (item.Value is double scaleVal) item.IsChecked = Math.Abs(RulerData.Magnifier.ZoomLevel - scaleVal) < 0.0001;
            }
            else if (parentHeader == "Opacity")
            {
                item.IsCheckable = true;
                if (item.Value is double opacityVal) item.IsChecked = Math.Abs(opacityVal - RulerData.Opacity) < 0.001;
                else if (item.Value is int opacityInt)
                {
                    double itemOpacity = opacityInt / 100.0;
                    item.IsChecked = Math.Abs(itemOpacity - RulerData.Opacity) < 0.001;
                }
            }
            else if (parentHeader == "Save Rule Data?")
            {
                item.IsCheckable = true;
                if (item.Value is SaveTypes saveTypeVal)
                {
                    item.IsChecked = (RulerData.SaveType == saveTypeVal);
                }
            }
            else if (parentHeader == "Units" || parentHeader == "Measurement Units")
            {
                item.IsCheckable = true;
                if (item.Value is RulerUnit unitVal)
                {
                    item.IsChecked = (RulerData.RulerUnits == unitVal);
                }
            }
            else if (parentHeader == "Ruler Scale")
            {
                item.IsCheckable = true;
                if (item.Value is double rulerScaleVal) item.IsChecked = Math.Abs(RulerScale - rulerScaleVal) < 0.0001;
                item.IsEnabled = (RulerData.RulerUnits == RulerUnit.Pixels);
            }
        }
        public void ApplySizeChange(int newWidth, int newHeight, RulerUnit newUnit)
        {
            RulerData.Width = newWidth;
            RulerData.Height = newHeight;
            RulerData.RulerUnits = newUnit;

            OnPropertyChanged(nameof(RulerData));
            RequestResizeWindow?.Invoke(newWidth, newHeight);
            RequestInvalidate?.Invoke();
        }
        //private void SyncTooltipWindowState()
        //{
        //    if (_rulerInfo.ShowToolTip)
        //    {
        //        if (_tooltipWindow == null)
        //        {
        //            _tooltipWindow = new RulerTooltipWindow(_rulerInfo)
        //            {
        //                Owner = this,
        //                Topmost = true
        //            };
        //            _tooltipWindow.Show();
        //        }

        //        double baseDpi = 96.0 * VisualTreeHelper.GetDpi(this).PixelsPerDip;
        //        double effectiveDpi = _currentProfile != null ? _currentProfile.GetEffectiveDpi(baseDpi) : baseDpi;
        //        _tooltipWindow.UpdateMetrics(ActualWidth, ActualHeight, effectiveDpi);
        //    }
        //    else
        //    {
        //        _tooltipWindow?.Close();
        //        _tooltipWindow = null;
        //    }
        //}
        public double RulerScale
        {
            get => RulerData.RulerScale;
            set
            {
                if (RulerData.RulerScale != value)
                {
                    RulerData.RulerScale = value; // RulerInfo's internal SetProperty handles the backing field & notification
                    OnPropertyChanged(nameof(RulerScale));
                    RequestInvalidate?.Invoke();
                }
            }
        }
    }
}

