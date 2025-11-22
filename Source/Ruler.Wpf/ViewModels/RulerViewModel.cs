using Ruler.Wpf;
using Ruler.Wpf.Common;
using Ruler.Wpf.Models;
using Ruler.Wpf.Services;
using Ruler.Wpf.Services.Persistence;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.AccessControl;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Ruler.Wpf.ViewModels
{
    // The core ViewModel class for the Ruler application
    public class RulerViewModel : ViewModelBase
    {
        // The RulerInfo class would be our Model
        private RulerInfo _rulerInfo;
        private IDialogService _dialogService;
        private ILoggingService _loggingService;

        // Collections for the ItemsControls to bind to
        private ObservableCollection<RulerTick> _topRulerTicks = new ObservableCollection<RulerTick>();
        private ObservableCollection<RulerTick> _bottomRulerTicks = new ObservableCollection<RulerTick>();
        //Opacity boolean flags
        private bool _isOpacity5Percent;
        private bool _isOpacity10Percent;
        private bool _isOpacity15Percent;
        private bool _isOpacity20Percent;
        private bool _isOpacity25Percent;
        private bool _isOpacity30Percent;
        private bool _isOpacity35Percent;
        private bool _isOpacity40Percent;
        private bool _isOpacity45Percent;
        private bool _isOpacity50Percent;
        private bool _isOpacity55Percent;
        private bool _isOpacity60Percent;
        private bool _isOpacity65Percent;
        private bool _isOpacity70Percent;
        private bool _isOpacity75Percent;
        private bool _isOpacity80Percent;
        private bool _isOpacity85Percent;
        private bool _isOpacity90Percent;
        private bool _isOpacity95Percent;
        private bool _isOpacity100Percent;

        private bool _isInitialized = false;

        private Dictionary<int, Action<bool>> _opacitySetterMap;
        private Dictionary<SaveTypes, Action<bool>> _saveTypeSetterMap;

        //SaveType boolean flags
        private bool _isSaveTypeNone;
        private bool _isSaveTypeSize;
        private bool _isSaveTypeLocation;
        private bool _isSaveTypeAll;


        private ICommand _toggleLockCommand;
        private ICommand _exitCommand;
        private ICommand _toggleVerticalCommand;
        private ICommand _toggleTopMostCommand;
        private ICommand _toggleToolTipCommand;
        private ICommand _setOpacityCommand;
        private ICommand _setSaveTypeCommand;
        private ICommand _showSetSizeFormCommand;
        private ICommand _showAboutCommand;
        private ICommand _duplicateCommand;
        private ICommand _resetToDefaultCommand;

        private int _horizontalMinHeight = 85;
        private int _vericalMinWidth = 93;

        #region  SaveTypeBooleans
        public bool IsSaveTypeNone
        {
            get => _isSaveTypeNone;
            set
            {
                if (_isSaveTypeNone != value)
                {
                    _isSaveTypeNone = value;
                    OnPropertyChanged(nameof(IsSaveTypeNone));
                }
            }
        }
        public bool IsSaveTypeSize
        {
            get => _isSaveTypeSize;
            set
            {
                if (_isSaveTypeSize != value)
                {
                    _isSaveTypeSize = value;
                    OnPropertyChanged(nameof(IsSaveTypeSize));
                }
            }
        }
        public bool IsSaveTypeLocation
        {
            get => _isSaveTypeLocation;
            set
            {
                if (_isSaveTypeLocation != value)
                {
                    _isSaveTypeLocation = value;
                    OnPropertyChanged(nameof(IsSaveTypeLocation));
                }
            }
        }
        public bool IsSaveTypeAll
        {
            get => _isSaveTypeAll;
            set
            {
                if (_isSaveTypeAll != value)
                {
                    _isSaveTypeAll = value;
                    OnPropertyChanged(nameof(IsSaveTypeAll));
                }
            }
        }
        #endregion
        #region OpacityBooleans
        public bool IsOpacity5Percent
        {
            get => _isOpacity5Percent;
            set
            {
                if (_isOpacity5Percent != value)
                {
                    _isOpacity5Percent = value;
                    OnPropertyChanged(nameof(IsOpacity5Percent));
                }
            }
        }
        public bool IsOpacity10Percent
        {
            get => _isOpacity10Percent;
            set
            {
                if (_isOpacity10Percent != value)
                {
                    _isOpacity10Percent = value;
                    OnPropertyChanged(nameof(IsOpacity10Percent));
                }
            }
        }
        public bool IsOpacity15Percent
        {
            get => _isOpacity15Percent;
            set
            {
                if (_isOpacity15Percent != value)
                {
                    _isOpacity15Percent = value;
                    OnPropertyChanged(nameof(IsOpacity15Percent));
                }
            }
        }
        public bool IsOpacity20Percent
        {
            get => _isOpacity20Percent;
            set
            {
                if (_isOpacity20Percent != value)
                {
                    _isOpacity20Percent = value;
                    OnPropertyChanged(nameof(IsOpacity20Percent));
                }
            }
        }
        public bool IsOpacity25Percent
        {
            get => _isOpacity25Percent;
            set
            {
                if (_isOpacity25Percent != value)
                {
                    _isOpacity25Percent = value;
                    OnPropertyChanged(nameof(IsOpacity25Percent));
                }
            }
        }
        public bool IsOpacity30Percent
        {
            get => _isOpacity30Percent;
            set
            {
                if (_isOpacity30Percent != value)
                {
                    _isOpacity30Percent = value;
                    OnPropertyChanged(nameof(IsOpacity30Percent));
                }
            }
        }
        public bool IsOpacity35Percent
        {
            get => _isOpacity35Percent;
            set
            {
                if (_isOpacity35Percent != value)
                {
                    _isOpacity35Percent = value;
                    OnPropertyChanged(nameof(IsOpacity35Percent));
                }
            }
        }
        public bool IsOpacity40Percent
        {
            get => _isOpacity40Percent;
            set
            {
                if (_isOpacity40Percent != value)
                {
                    _isOpacity40Percent = value;
                    OnPropertyChanged(nameof(IsOpacity40Percent));
                }
            }
        }
        public bool IsOpacity45Percent
        {
            get => _isOpacity45Percent;
            set
            {
                if (_isOpacity45Percent != value)
                {
                    _isOpacity45Percent = value;
                    OnPropertyChanged(nameof(IsOpacity45Percent));
                }
            }
        }
        public bool IsOpacity50Percent
        {
            get => _isOpacity50Percent;
            set
            {
                if (_isOpacity50Percent != value)
                {
                    _isOpacity50Percent = value;
                    OnPropertyChanged(nameof(IsOpacity50Percent));
                }
            }
        }
        public bool IsOpacity55Percent
        {
            get => _isOpacity55Percent;
            set
            {
                if (_isOpacity55Percent != value)
                {
                    _isOpacity55Percent = value;
                    OnPropertyChanged(nameof(IsOpacity55Percent));
                }
            }
        }
        public bool IsOpacity60Percent
        {
            get => _isOpacity60Percent;
            set
            {
                if (_isOpacity60Percent != value)
                {
                    _isOpacity60Percent = value;
                    OnPropertyChanged(nameof(IsOpacity60Percent));
                }
            }
        }
        public bool IsOpacity65Percent
        {
            get => _isOpacity65Percent;
            set
            {
                if (_isOpacity65Percent != value)
                {
                    _isOpacity65Percent = value;
                    OnPropertyChanged(nameof(IsOpacity65Percent));
                }
            }
        }
        public bool IsOpacity70Percent
        {
            get => _isOpacity70Percent;
            set
            {
                if (_isOpacity70Percent != value)
                {
                    _isOpacity70Percent = value;
                    OnPropertyChanged(nameof(IsOpacity70Percent));
                }
            }
        }
        public bool IsOpacity75Percent
        {
            get => _isOpacity75Percent;
            set
            {
                if (_isOpacity75Percent != value)
                {
                    _isOpacity75Percent = value;
                    OnPropertyChanged(nameof(IsOpacity75Percent));
                }
            }
        }
        public bool IsOpacity80Percent
        {
            get => _isOpacity80Percent;
            set
            {
                if (_isOpacity80Percent != value)
                {
                    _isOpacity80Percent = value;
                    OnPropertyChanged(nameof(IsOpacity80Percent));
                }
            }
        }
        public bool IsOpacity85Percent
        {
            get => _isOpacity85Percent;
            set
            {
                if (_isOpacity85Percent != value)
                {
                    _isOpacity85Percent = value;
                    OnPropertyChanged(nameof(IsOpacity85Percent));
                }
            }
        }
        public bool IsOpacity90Percent
        {
            get => _isOpacity90Percent;
            set
            {
                if (_isOpacity90Percent != value)
                {
                    _isOpacity90Percent = value;
                    OnPropertyChanged(nameof(IsOpacity90Percent));
                }
            }
        }
        public bool IsOpacity95Percent
        {
            get => _isOpacity95Percent;
            set
            {
                if (_isOpacity95Percent != value)
                {
                    _isOpacity95Percent = value;
                    OnPropertyChanged(nameof(IsOpacity95Percent));
                }
            }
        }
        public bool IsOpacity100Percent
        {
            get => _isOpacity100Percent;
            set
            {
                if (_isOpacity100Percent != value)
                {
                    _isOpacity100Percent = value;
                    OnPropertyChanged(nameof(_isOpacity100Percent));
                }

            }
        }
        #endregion


        private RulerOrientation _orientation;

        // State variables for mouse interaction
        private Point _startPoint;
        private Size _startSize;
        private bool _isResizing;
        private bool _isMoving;
        private double _length;
        private bool _isLocked;
        private int leftMargin = 111;
        private double actualWidth = 203;
        private bool _isLoadingState;
        private double _middlewidth = 1;
        private readonly SingleRulerPersistenceService _persistenceService;
        private double _topRowHeight;
        private bool _isGuideLineVisible = false;
        private double _guideLinePosition;
        private double _minheight = 45;
        private double _minwidth = 40;

        public bool IsGuideLineVisible
        {
            get => _isGuideLineVisible;
            set {
                if (_isGuideLineVisible!=value)
                {
                    _isGuideLineVisible = value;
                    OnPropertyChanged();
                   
                }
            }
        }

        /// <summary>
        /// Represents the X coordinate (for horizontal ruler) 
        /// or the Y coordinate (for vertical ruler) of the guide line.
        /// </summary>
        public double GuideLinePosition
        {
            get => _guideLinePosition;
            set
            {
                if (_guideLinePosition!=value)
                {
                    _guideLinePosition = value;
                    OnPropertyChanged();
                }
            }
        }
        public Color GuideLineColor { get; set; } = Colors.Red;

        public Double ActualWidth
        {
            get => actualWidth;
            set
            {
                if (actualWidth != value)
                {
                    if (IsVertical)
                    {
                        if (value < 110)
                        {
                            actualWidth = 110;
                        }
                    }
                    else
                    { 
                    actualWidth = value;
                    }
                   
                }
            }
        }
        public double MiddleWidth
        {
            get
            {
                if (IsVertical)
                {
                    Console.WriteLine($"Middlewidth: {_minwidth + (Width - 75)}");
                    return _minwidth + (Width - 75);
                }
                Console.WriteLine($"MiddleWidth Horizontal: {_minheight + (Height- 75)}");
                return  _minheight+( Height - 75);

            }
            set
            {
                if (_middlewidth != value)
                {
                    _middlewidth = value;
                    OnPropertyChanged(nameof(MiddleWidth));
                }
            }
        }
        private bool _firstMiddleWidthUpdate = true;
        public void UpdateMiddleWidth(double newMiddleWidth)
        {
            //double newvalue;
        
               
            //    if (IsVertical)
            //    {
            //        newvalue = _minwidth ;
            //    }
            //    else
            //    {
            //        newvalue = _minheight + newMiddleWidth;
            //    }
            //    Console.WriteLine($"MiddleWidth value {newvalue}");
            //    MiddleWidth = newvalue;
            
            
            //    Console.WriteLine($"New Value: {MiddleWidth + newMiddleWidth}");
            //  MiddleWidth += newMiddleWidth;
                      
        }




        public RulerViewModel(IDialogService dialogService, RulerInfo initialInfo, SingleRulerPersistenceService persistenceService, ILoggingService loggingService)
        {
            _dialogService = dialogService ?? throw new ArgumentException(nameof(dialogService));
            _persistenceService = persistenceService ?? throw new ArgumentException(nameof(persistenceService));
            _loggingService = loggingService ?? throw new ArgumentException(nameof(loggingService));
            _rulerInfo = initialInfo ?? throw new ArgumentException(nameof(initialInfo));
            InitializeCommands();

            _opacitySetterMap = new Dictionary<int, Action<bool>>()
            {
                {5, (val) => { IsOpacity5Percent = val; } },
                {10, (val) => { IsOpacity10Percent = val; } },
                {15, (val) => { IsOpacity15Percent = val; } },
                {20, (val) => { IsOpacity20Percent = val; } },
                {25, (val) => { IsOpacity25Percent = val; } },
                {30, (val) => { IsOpacity30Percent = val; } },
                {35, (val) => { IsOpacity35Percent = val; } },
                {40, (val) => { IsOpacity40Percent = val; } },
                {45, (val) => { IsOpacity45Percent = val; } },
                {50, (val) => { IsOpacity50Percent = val; } },
                {55, (val) => { IsOpacity55Percent = val; } },
                {60, (val) => { IsOpacity60Percent = val; } },
                {65, (val) => { IsOpacity65Percent = val; } },
                {70, (val) => { IsOpacity70Percent = val; } },
                {75, (val) => { IsOpacity75Percent = val; } },
                {80, (val) => { IsOpacity80Percent = val; } },
                {85, (val) => { IsOpacity85Percent = val; } },
                {90, (val) => { IsOpacity90Percent = val; } },
                {95, (val) => { IsOpacity95Percent = val; } },
                {100, (val) => { IsOpacity100Percent = val; } }
            };
            _saveTypeSetterMap = new Dictionary<SaveTypes, Action<bool>>()
            {
                {SaveTypes.none, (val) => { IsSaveTypeNone = val; } },
                {SaveTypes.size, (val) => { IsSaveTypeSize = val; } },
                {SaveTypes.location, (val) => { IsSaveTypeLocation = val; } },
                {SaveTypes.all, (val) => { IsSaveTypeAll = val; } }
            };
            CheckSingleRuler();
            if (IsVertical)
            {

                GenerateVerticalTicks(Height);
            }
            else
            {

                GenerateHorizontalTicks(Width);
            }
            SetOpacityFlags(_rulerInfo.Opacity);
            SetSaveTypeFlags(_rulerInfo.SaveType);
        }

        public void CheckSingleRuler()
        {
            if (IsVertical)
            {
                _isOnlySingleRulerVisible = Width < _vericalMinWidth;
            }
            else
            {
                _isOnlySingleRulerVisible = Height < _horizontalMinHeight;
            }
        }
        public void InitializeCommands()
        {
            _showSetSizeFormCommand = new RelayCommand(GetNavigateSetSizeForm);
            _resetToDefaultCommand = new RelayCommand(ResetDefault);
            _toggleLockCommand = new RelayCommand(ToggleLock);
            _exitCommand = new RelayCommand(ExitApplication);
            _toggleVerticalCommand = new RelayCommand(ToggleVertical);
            _toggleTopMostCommand = new RelayCommand(ToggleTopMost);
            _toggleToolTipCommand = new RelayCommand(ToggleToolTip);
            _setOpacityCommand = new RelayCommand(SetOpacity);
            _setSaveTypeCommand = new RelayCommand(SetSaveType);
            _showAboutCommand = new RelayCommand(NavigateAbout);
            _duplicateCommand = new RelayCommand(DuplicateRuler);
        }

        public void SetOpacityFlags(double opacity)
        {
            int percentage = (int)Math.Round((opacity * 100));
            foreach (var sets in _opacitySetterMap.Values)
            {
                sets.Invoke(false);
            }
            if (_opacitySetterMap.TryGetValue(percentage, out Action<bool> setter))
            {
                setter.Invoke(true);
            }
        }
        public void SetSaveTypeFlags(SaveTypes saveType)
        {

            foreach (var sets in _saveTypeSetterMap.Values)
            {
                sets.Invoke(false);
            }
            if (_saveTypeSetterMap.TryGetValue(saveType, out Action<bool> setter))
            {
                setter.Invoke(true);
            }
        }

        private void GetNavigateSetSizeForm(object parameters)
        {
            var s = _dialogService.ShowSetSizeDialog(this.Width, this.Height);
            SetRulerDimensions(s.Width, s.Height);
        }

        private void DuplicateRuler(object parameters)
        {
            RulerInfo ri = new RulerInfo();

            RulerInfo.CopyInto(_rulerInfo, ri);
            _dialogService.ShowNewRuler(ri);
        }

        public ObservableCollection<RulerTick> TopRulerTicks
        {
            get => _topRulerTicks;
            set
            {
                if (_topRulerTicks != value)
                {
                    _topRulerTicks = value;
                    OnPropertyChanged(nameof(TopRulerTicks));
                }
            }
        }

        public ObservableCollection<RulerTick> BottomRulerTicks
        {
            get => _bottomRulerTicks;
            set
            {
                if (_bottomRulerTicks != value)
                {
                    _bottomRulerTicks = value;
                    OnPropertyChanged(nameof(BottomRulerTicks));
                }

            }
        }
        // Property for the ruler's width, with change notification
        public double Width
        {
            get => _rulerInfo.Width;
            set
            {
                if (_rulerInfo.Width != value)
                {
                    _rulerInfo.Width = value;
                    OnPropertyChanged("Width");
                    OnPropertyChanged(nameof(MiddleWidth));
                    CheckSingleRuler();
                    if (IsVertical)
                    {
                        GenerateVerticalTicks(Height);
                    }
                    else
                    {
                        GenerateHorizontalTicks(Width);
                    }
                }
            }
        }

        // Property for the ruler's height, with change notification
        public double Height
        {
            get => _rulerInfo.Height;
            set
            {
                if (_rulerInfo.Height != value)
                {
                    _rulerInfo.Height = value;
                    OnPropertyChanged("Height");
                    OnPropertyChanged(nameof(MiddleWidth));
                    CheckSingleRuler();
                    if (IsVertical)
                    {
                        GenerateVerticalTicks(Height);
                    }
                    else
                    {
                        if (value<120)
                        {

                        }
                        GenerateHorizontalTicks(Width);
                    }
                }
            }
        }
        public double LocationX
        {
            get => _rulerInfo.LocationX;
            set
            {
                if (_rulerInfo.LocationX != value)
                {
                    _rulerInfo.LocationX = value;
                    OnPropertyChanged(nameof(LocationX));
                }
            }
        }
        public double LocationY
        {
            get => _rulerInfo.LocationY;
            set
            {
                if (_rulerInfo.LocationY != value)
                {
                    _rulerInfo.LocationY = value;
                    OnPropertyChanged(nameof(LocationY));
                }
            }
        }

        // Property for the ruler's location, with change notification
        public Point DisplayedLocation
        {
            get => new Point(LocationX, LocationY);
            set
            {
                if (_rulerInfo.DisplayedLocation != value)
                {
                    _rulerInfo.DisplayedLocation = value;
                    OnPropertyChanged(nameof(DisplayedLocation));
                    _rulerInfo.DisplayedLocation = value;
                }
            }
        }

        // Property for the lock state, with change notification
        public bool IsLocked
        {
            get => _rulerInfo.IsLocked;
            set
            {
                if (_rulerInfo.IsLocked != value)
                {
                    _rulerInfo.IsLocked = value;
                    CommandManager.InvalidateRequerySuggested();
                    OnPropertyChanged(nameof(IsLocked));
                }
            }
        }

        // Property for opacity, with change notification
        public double Opacity
        {
            get => _rulerInfo.Opacity;
            set
            {
                if (_rulerInfo.Opacity != value)
                {
                    _rulerInfo.Opacity = value;
                    OnPropertyChanged(nameof(Opacity));
                }
            }
        }

        // Property for the TopMost state
        public bool TopMost
        {
            get => _rulerInfo.TopMost;
            set
            {
                if (_rulerInfo.TopMost != value)
                {
                    _rulerInfo.TopMost = value;
                    OnPropertyChanged(nameof(TopMost));
                }
            }
        }

        // Property for the vertical state
        public bool IsVertical
        {
            get => _rulerInfo.IsVertical;
            set
            {
                if (_rulerInfo.IsVertical != value)
                {
                    _firstMiddleWidthUpdate = true;
                    _rulerInfo.IsVertical = value;

                    // 2. FIX: Swap Width and Height in the model immediately.
                    // This is the critical step to ensure 400x50 becomes 50x400.
                    //double oldWidth = _rulerInfo.Width;
                    //_rulerInfo.Width = _rulerInfo.Height;
                    //_rulerInfo.Height = oldWidth;

                    // 3. Notify the UI for all relevant properties
                    OnPropertyChanged(); // Notifies IsVertical

                    //OnPropertyChanged(nameof(TopRowHeight)); // Updates dependent property


                }
            }
        }

        // Property for the tooltip state
        public bool ShowToolTip
        {
            get => _rulerInfo.ShowToolTip;
            set
            {
                if (_rulerInfo.ShowToolTip != value)
                {
                    _rulerInfo.ShowToolTip = value;
                    OnPropertyChanged(nameof(ShowToolTip));
                }
            }
        }

        // Property for the save type
        public SaveTypes SaveType
        {
            get => _rulerInfo.SaveType;
            set
            {
                if (_rulerInfo.SaveType != value)
                {
                    _rulerInfo.SaveType = value;
                }
            }
        }

        // Public property to expose the resizing state to the View
        public bool IsResizing
        {
            get => _isResizing;
            set
            {
                SetProperty(ref _isResizing, value);
            }
        }

        public double Length
        {
            get => _length;
            set
            {
                SetProperty(ref _length, value);
            }
        }
        // Public method to handle mouse down from the View
        public void MouseLeftButtonDown(Point mousePosition, FrameworkElement originalSource)
        {
            if (IsLocked) return;

            _startPoint = mousePosition;
            _startSize = new Size(Width, Height);

            // Check the original source name to determine if the user is trying to resize.
            if (originalSource.Name == "resizingArea")
            {
                _isResizing = true;
                OnPropertyChanged(nameof(IsResizing));
            }
            else
            {
                _isMoving = true;
            }
        }
        // Public method to handle mouse move from the View
        public void MouseMove(Point mousePosition)
        {
            if (IsLocked) return;

            // Calculate the change in mouse position
            double deltaX = mousePosition.X - _startPoint.X;
            double deltaY = mousePosition.Y - _startPoint.Y;

            if (_isResizing)
            {
                Width = (int)(_startSize.Width + deltaX);
                Height = (int)(_startSize.Height + deltaY);
            }
            else if (_isMoving)
            {
                LocationX += deltaX;
                LocationY += deltaY;
                _startPoint = mousePosition; // Update start point for continuous dragging
            }
        }
        // Determines the height of the top/bottom rows (0 when vertical, 25 when horizontal)
        public double TopRowHeight => _rulerInfo.IsVertical ? 0 : 25;
        public double BottomRowHeight => _rulerInfo.IsVertical ? 0 : 25;

        // Determines the width of the left/right columns (25 when vertical, 0 when horizontal)
        public double TopColumnWidth => _rulerInfo.IsVertical ? 25 : 0;
        public double BottomColumnWidth => _rulerInfo.IsVertical ? 25 : 0;

        // Public method to handle mouse up from the View
        public void MouseLeftButtonUp()
        {
            _isResizing = false;
            _isMoving = false;
            OnPropertyChanged(nameof(IsResizing));
        }
        public void SetRulerDimensions(double newWidth, double newHeight)
        {
            if (this.Width == newWidth && this.Height == newHeight)
            {
                return;
            }
            Width = newWidth;
            Height = newHeight;
            if (IsVertical)
            {
                this.actualWidth = Width;
                GenerateVerticalTicks(Height);
            }
            else
            {
                GenerateHorizontalTicks(Width);
            }
            OnPropertyChanged(nameof(TopRulerTicks));
            OnPropertyChanged(nameof(BottomRulerTicks));
        }
        public void UpdateLocation(double left, double top)
        {
            LocationX = left;
            LocationY = top;
        }
        public bool IsInitialized
        {
            get => _isInitialized;
            set
            {
                SetProperty(ref _isInitialized, value);
            }
        }

        // Command properties for UI actions
        public ICommand ToggleLockCommand => _toggleLockCommand;
        public ICommand ExitCommand => _exitCommand;
        public ICommand ToggleVerticalCommand => _toggleVerticalCommand;
        public ICommand ToggleTopMostCommand => _toggleTopMostCommand;
        public ICommand ToggleToolTipCommand => _toggleToolTipCommand;
        public ICommand SetOpacityCommand => _setOpacityCommand;
        public ICommand SetSaveTypeCommand => _setSaveTypeCommand;
        public ICommand ShowSetSizeFormCommand => _showSetSizeFormCommand;
        public ICommand ShowAboutCommand => _showAboutCommand;
        public ICommand DuplicateCommand => _duplicateCommand;
        public ICommand ResetToDefaultCommand => _resetToDefaultCommand;

        // Logic for the ToggleLockCommand
        private void ToggleLock(object parameter)
        {
            IsLocked = !IsLocked;
        }
        public void ResetDefault(object parameter)
        {
            RulerInfo defaultRuler = RulerInfo.GetDefaultRulerInfo();
            RulerInfo.CopyInto(defaultRuler, _rulerInfo);
            OnPropertyChanged(nameof(Width));
            OnPropertyChanged(nameof(Height));
            OnPropertyChanged(nameof(IsVertical));
            OnPropertyChanged(nameof(Opacity));
            OnPropertyChanged(nameof(ShowToolTip));
            OnPropertyChanged(nameof(IsLocked));
            OnPropertyChanged(nameof(TopMost));
            OnPropertyChanged(nameof(LocationX));
            OnPropertyChanged(nameof(LocationY));
            OnPropertyChanged(nameof(SaveType));
        }

        // Logic for the ExitCommand
        private void ExitApplication(object parameter)
        {
            if (parameter is Window windowToClose)
            {
                _persistenceService.SaveRulerState(_rulerInfo);
                bool isLastRuler = _dialogService.OpenRulers.Count() == 1;
                windowToClose.Close();
                if (isLastRuler)
                {
                    Application.Current.Shutdown();
                }
            }

        }

        // Logic for the ToggleVerticalCommand
        private void ToggleVertical(object parameter)
        {
            IsVertical = !IsVertical;
            double oldWidth = Width;
            double oldHeight = Height;
            SetRulerDimensions(oldHeight, oldWidth);
        }
        private bool _isOnlySingleRulerVisible = false;
        public bool IsOnlySingleRulerVisible
        {
            get => _isOnlySingleRulerVisible;
            set
            {
                _isOnlySingleRulerVisible = value;
                OnPropertyChanged();
            }
        }



        // Logic for the ToggleTopMostCommand
        private void ToggleTopMost(object parameter)
        {
            TopMost = !TopMost;
        }

        // Logic for the ToggleToolTipCommand
        private void ToggleToolTip(object parameter)
        {
            ShowToolTip = !ShowToolTip;
        }

        // Logic for the SetOpacityCommand
        private void SetOpacity(object parameter)
        {
            if (parameter is string opacityValue && double.TryParse(opacityValue, out double result))
            {
                Opacity = result;
                SetOpacityFlags(result);
            }
        }

        // Logic for the SetSaveTypeCommand
        private void SetSaveType(object parameter)
        {
            SaveTypes saveTypes;
            if (parameter is string s && Enum.TryParse(s, true, out saveTypes))
            {
                SaveType = saveTypes;
                SetSaveTypeFlags(saveTypes);
            }
        }
        private void GenerateHorizontalTicks(double length)
        {
            // Calculate ticks based on Width
            _topRulerTicks.Clear();
            _bottomRulerTicks.Clear();
            var ticks = GenerateRulerTicks(length); // Assuming 100 pixels per major mark

            foreach (var tick in ticks)
            {
                // Horizontal: Position is Canvas.Left. No coordinate inversion needed.
                // Both top and bottom sides use the same position data.
                _topRulerTicks.Add(tick);
                if (!IsVertical && _isOnlySingleRulerVisible)
                {
                    tick.IsLabelVisible = false;
                }
                _bottomRulerTicks.Add(tick);
            }
            OnPropertyChanged(nameof(BottomRulerTicks));
            OnPropertyChanged(nameof(TopRulerTicks));
        }
        private void GenerateVerticalTicks(double length)
        {
            // Calculate ticks based on the effective height of the content area
            _topRulerTicks.Clear();
            _bottomRulerTicks.Clear();
            var ticks = GenerateRulerTicks(length);

            foreach (var tick in ticks)
            {
                // The tick's original position is the distance FROM THE TOP (0 at top, length at bottom).
                // This is the correct value for the LEFT SIDE (Grid.Column="0").
                _topRulerTicks.Add(tick);
                if (IsVertical && _isOnlySingleRulerVisible)
                {
                    tick.IsLabelVisible = false;
                }


                _bottomRulerTicks.Add(tick);
            }
            OnPropertyChanged(nameof(BottomRulerTicks));
            OnPropertyChanged(nameof(TopRulerTicks));
        }
        public static IEnumerable<RulerTick> GenerateRulerTicks(double rulerLength)
        {
            var ticks = new List<RulerTick>();

            // We iterate through the entire length of the ruler to determine tick positions.
            for (int i = 0; i < (int)rulerLength; i++)
            {
                // Every 100 pixels, we create a major tick with a label.
                if (i % 100 == 0)
                {
                    ticks.Add(new RulerTick
                    {
                        Position = i,
                        Label = i.ToString(),
                        TickSize = 25,
                        IsLabelVisible = true
                    });
                }
                // Every 50 pixels, we create a major tick without a label.
                else if (i % 50 == 0)
                {
                    ticks.Add(new RulerTick
                    {
                        Position = i,
                        TickSize = 20

                    });
                }
                // Every 10 pixels, we create a minor tick.
                else if (i % 10 == 0)
                {
                    ticks.Add(new RulerTick
                    {
                        Position = i,
                        TickSize = 10
                    });
                }
                // Every 5 pixels, we create an even smaller minor tick.
                else if (i % 5 == 0)
                {
                    ticks.Add(new RulerTick
                    {
                        Position = i,
                        TickSize = 5
                    });
                }
                // Every 2 pixels, we create the smallest minor tick.
                else if (i % 2 == 0)
                {
                    ticks.Add(new RulerTick
                    {
                        Position = i,
                        TickSize = 2
                    });
                }
            }

            return ticks;
        }

        private void NavigateAbout(object parameter)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            Version version = assembly.GetName().Version;
            string message = string.Format(
                "Original Ruler implemented by Jeff Key\n" +
                "www.sliver.com\n" +
                "ruler.codeplex.com\n" +
                "Icon by Kristen Magee @ www.kbecca.com.\n" +
                "Maintained by Andrija Cacanovic\n" +
                "Hosted on \n" +
                "https://github.com/andrijac/ruler\n" +
                "Version {0}",
                $"{version.Major}.{version.Minor}.{version.Build}.{version.MajorRevision}");
            MessageBox.Show(message, "About Ruler", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        internal void SetInitialState(RulerInfo initialInfo)
        {
            _isLoadingState = true;

            // 1. Set all non-dimension/non-orientation properties directly on the model
            _rulerInfo.Opacity = initialInfo.Opacity;
            _rulerInfo.ShowToolTip = initialInfo.ShowToolTip;
            _rulerInfo.IsLocked = initialInfo.IsLocked;
            _rulerInfo.TopMost = initialInfo.TopMost;
            _rulerInfo.LocationY = initialInfo.LocationY;
            _rulerInfo.LocationX = initialInfo.LocationX;
            _rulerInfo.DisplayedLocation = initialInfo.DisplayedLocation;
            _rulerInfo.SaveType = initialInfo.SaveType;


            // 2. IMPORTANT: Set IsVertical first for reconciliation
            _rulerInfo.IsVertical = initialInfo.IsVertical;


            // 3. Reconciliation Logic for Width and Height based on IsVertical
            double w = initialInfo.Width;
            double h = initialInfo.Height;

            // Determine if the saved dimensions are 'horizontal' (width >= height)
            bool savedHorizontal = w >= h;

            // If the saved orientation clashes with the saved dimensions, swap them.
            if ((_rulerInfo.IsVertical && savedHorizontal) || (!_rulerInfo.IsVertical && !savedHorizontal))
            {
                // Swap dimensions in the model to match the orientation
                _rulerInfo.Width = h;
                _rulerInfo.Height = w;
            }
            else
            {
                // Dimensions are already correctly oriented
                _rulerInfo.Width = w;
                _rulerInfo.Height = h;
            }

            // 4. Set flags for the UI
            SetOpacityFlags(_rulerInfo.Opacity);
            SetSaveTypeFlags(_rulerInfo.SaveType);


            // 5. Trigger UI updates for all properties
            // We use OnPropertyChanged for all properties to ensure the UI updates correctly from the reconciled model state.
            OnPropertyChanged(nameof(Width));
            OnPropertyChanged(nameof(Height));
            OnPropertyChanged(nameof(IsVertical));
            OnPropertyChanged(nameof(Opacity));
            OnPropertyChanged(nameof(ShowToolTip));
            OnPropertyChanged(nameof(IsLocked));
            OnPropertyChanged(nameof(TopMost));
            OnPropertyChanged(nameof(LocationY));
            OnPropertyChanged(nameof(LocationX));
            OnPropertyChanged(nameof(DisplayedLocation));
            OnPropertyChanged(nameof(SaveType));
            OnPropertyChanged(nameof(TopRowHeight));
            // 6. Reset flag after loading is complete
            _isLoadingState = false;

        }

        internal void SetGuideLinePosition(double position)
        {
            GuideLinePosition = position;
        }

        internal void UpdateRulerContentDimensions(double canvasContentHeight, double canvasContentWidth, double topRulerContentHeight, double leftRulerContentWidth, double rightRulerContentWidth, double bottomRulerContentHeight)
        {
            if (topRulerContentHeight > 0 && bottomRulerContentHeight > 0)
            {
                MiddleWidth = canvasContentHeight - topRulerContentHeight - bottomRulerContentHeight;
            }
            else if (leftRulerContentWidth > 0 && rightRulerContentWidth > 0)
            {
                MiddleWidth = canvasContentWidth - leftRulerContentWidth - rightRulerContentWidth;
            }
        }
    }
}
