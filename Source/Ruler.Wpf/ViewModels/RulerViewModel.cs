using Ruler.Shared.Enums;
using Ruler.Shared.Factories;
using Ruler.Shared.Models;
using Ruler.Wpf.Commands;
using Ruler.Wpf.Services;
using Ruler.Wpf.Views;

using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Media3D;

namespace Ruler.Wpf.ViewModels
{
    public class RulerViewModel : ModelBase
    {
        private readonly WindowManager _windowManager;
        public RulerInfo Model { get; }
        private bool _isFlipping = false;

        // Interaction Threshold Buffers
        private const double MinimumDragDistance = 4.0;
        private Point _mouseDownScreenPos;

        // Two-Way Data-Bound Properties linked directly to the Window boundaries
        public int Width { get => Model.Width; 
            set { if (_isFlipping) return; Model.Width = value; OnPropertyChanged(); } }
        public int Height { get => Model.Height; 
            set { if (_isFlipping) return; Model.Height = value; OnPropertyChanged(); } }
        public double Top { get => Model.Top; set { Model.Top = (int)value; OnPropertyChanged(); } }
        public double Left { get => Model.Left; set { Model.Left = (int)value; OnPropertyChanged(); } }
        public bool IsVertical { get => Model.IsVertical; set { Model.IsVertical = value; OnPropertyChanged(); } }
        public bool TopMost { get => Model.TopMost; set { Model.TopMost = value; OnPropertyChanged(); } }
        public bool IsLocked { get => Model.IsLocked; set { Model.IsLocked = value; OnPropertyChanged(); } }
        public bool ShowToolTip { get => Model.ShowToolTip; set { Model.ShowToolTip = value; OnPropertyChanged(); } }
        public double Opacity { get => Model.Opacity; set { Model.Opacity = value; OnPropertyChanged(); } }
        public SaveTypes SaveType { get => Model.SaveType; set { Model.SaveType = value; OnPropertyChanged(); } }

        public RulerGuideline CurrentGuideline { get => Model.Guideline; set { Model.Guideline = value; OnPropertyChanged(); } }
        public ICommand ModifySettingCommand { get; private set; }

        // Context Menu / Direct UI Element Commands
        public ICommand ExitApplicationCommand { get; private set; }
        public ICommand ResetToDefaultCommand { get; private set; }
        public ICommand ShowAboutDialogCommand { get; private set; }
        public ICommand DuplicateRulerCommand { get; private set; }
        public ICommand ToggleOrientationCommand { get; private set; }
        public ICommand ToggleLockCommand { get; private set; }
        public ICommand ToggleToolTipCommand { get; private set; }
        public ICommand ToggleGuidelineCommand { get; private set; }
        public ICommand CloseRulerCommand { get; private set; }

        // Parameter-Driven Payload Commands
        public ICommand ChangeOpacityCommand { get; private set; }
        public ICommand SetSaveTypeCommand { get; private set; }
        public ICommand ResizeRulerCommand { get; private set; }


        public RulerViewModel(RulerInfo model, WindowManager windowManager)
        {
            Model = model ?? throw new ArgumentNullException(nameof(model));
            _windowManager = windowManager ?? throw new ArgumentNullException(nameof(windowManager));
            InitializeCommands();
        }
        public RulerViewModel(RulerInfo ruler)
        {
            Model = ruler;
            InitializeCommands();
        }

        /// <summary>
        /// Anchors the mouse cursor position when clicked.
        /// </summary>
        public void HandleMouseDown(Point screenPosition)
        {
            _mouseDownScreenPos = screenPosition;
        }

        /// <summary>
        /// Runs Pythagorean displacement validation logic on release to differentiate dragging from clicks.
        /// </summary>
        public void HandleMouseUp(Point screenPosition)
        {
            double deltaX = screenPosition.X - _mouseDownScreenPos.X;
            double deltaY = screenPosition.Y - _mouseDownScreenPos.Y;

            // Straight line calculation: a² + b² = c²
            double totalDistanceMoved = Math.Sqrt((deltaX * deltaX) + (deltaY * deltaY));

            if (totalDistanceMoved >= MinimumDragDistance)
            {
                // Process intentional drag completion workflow actions (e.g., auto-save new location settings)
                System.Diagnostics.Debug.WriteLine($"VM Drag Processed: Moved {totalDistanceMoved:F1}px.");
            }
            else
            {
                // Process static frame execution actions
                System.Diagnostics.Debug.WriteLine("VM Static Click Registered cleanly.");
            }
        }

        /// <summary>
        /// Handles mouse wheel scrolling coefficients for active magnifier instances.
        /// Returns true if input was captured and handled; otherwise false.
        /// </summary>
        public bool HandleMouseWheel(int delta)
        {
            return false;
        }


        private void InitializeCommands()
        {
            // 1. The Multi-Shortcut Switchboard Command
            ModifySettingCommand = new RelayCommand<object>(param => ExecuteModifySetting(param));

            // 2. Explicit Parameterless UI Mappings
            ExitApplicationCommand = new RelayCommand(() => ExecuteExitApplication());
            ResetToDefaultCommand = new RelayCommand(() => ExecuteResetToDefault());
            ShowAboutDialogCommand = new RelayCommand(() => ExecuteShowAboutDialog());
            DuplicateRulerCommand = new RelayCommand(() => ExecuteDuplicateRuler());
            ToggleOrientationCommand = new RelayCommand(() => ExecuteToggleOrientation());
            ToggleLockCommand = new RelayCommand(() => ExecuteToggleLock());
            ToggleToolTipCommand = new RelayCommand(() => ExecuteToggleToolTip());
            CloseRulerCommand = new RelayCommand(() => ExecuteClose());
            ToggleGuidelineCommand = new RelayCommand(() => ToggleGuideline());

            // 3. Explicit Parameterized UI Payload Mappings
            ChangeOpacityCommand = new RelayCommand<object>(param => ExecuteChangeOpacity(param));
            SetSaveTypeCommand = new RelayCommand<object>(param => ExecuteSetSaveType(param));
            ResizeRulerCommand = new RelayCommand(() => ExecuteResizeRuler());
        }

        public void UpdateGuideline()
        {
            OnPropertyChanged(nameof(CurrentGuideline));
        }

        #region Command Execution Stubs
        private void ExecuteToggleOrientation()
        {
            try
            {
                Model.ToggleOrientation();
            }
            finally
            {
                OnPropertyChanged(nameof(IsVertical));
            }
            Console.WriteLine(Model.ToString());
            // OnPropertyChanged(nameof(CurrentGuideline));
        }
        private void ExecuteClose()
        {
            _windowManager.CloseRuler(this);
        }
        private void ExecuteDuplicateRuler()
        {
            RulerInfo newRuler = new RulerInfo();
            RulerFactory.CopyValues(Model, newRuler);
            _windowManager.CreateRulerWindow(newRuler);
        }
        private void ExecuteToggleLock()
        {
            IsLocked = !IsLocked;
        }
        private void ExecuteToggleToolTip()
        {
            ShowToolTip = !ShowToolTip;
        }
        private void ExecuteChangeOpacity(object param)
        {
            // 1. Check if it's already a raw double
            if (param is double newOpacity)
            {
                Opacity = newOpacity;
            }
            // 2. Check if it's a string, and safely parse it inline
            else if (param is string strParam && double.TryParse(strParam, out double parsedOpacity))
            {
                Opacity = parsedOpacity;
            }
        }
        private void ExecuteSetSaveType(object param)
        {
            // 1. Check if it's already a save type enum
            if (param is SaveTypes newSaveType)
            {
                SaveType = newSaveType;
            }
            // 2. Check if it's a string, and safely parse it inline
            else if (param is string strParam && Enum.TryParse(strParam, out SaveTypes parsedSaveType))
            {
                SaveType = parsedSaveType;
            }
        }
        private void ToggleGuideline()
        {
            if (Model.Guideline == null)
            {
                Model.Guideline = new RulerGuideline()
                {
                    IsEnabled = true,
                    IsLocked = false,
                    Position = 0
                };
                OnPropertyChanged(nameof(CurrentGuideline));
            }
            else
            {
                CurrentGuideline = null;
                OnPropertyChanged(nameof(CurrentGuideline));
            }
        }
        private void ExecuteResetToDefault()
        {
            Width = 400;
            Height = 150;
            Opacity = 1.0;
            IsLocked = false;
            ShowToolTip = true;
        }
        private void ExecuteExitApplication()
        {
            _windowManager.SaveAllActiveRulers();
            Application.Current.Shutdown();
        }  
        private void ExecuteResizeRuler()
        {
            SetSizeViewModel resizeVM = new SetSizeViewModel(Width, Height);
            SetSizeWindow resizeWindow = new SetSizeWindow
            {
                DataContext = resizeVM,
                Owner = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive)
            };
            if (resizeWindow.ShowDialog() == true)
            {
                // Only write changes back if validation passed and user clicked OK
                this.Width = (int)resizeVM.Width;
                this.Height = (int)resizeVM.Height;
            }
        }
        private void ExecuteShowAboutDialog()
        {
            MessageBox.Show("Ruler Utility\nVersion 2.0", "About", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        private void ExecuteToggleOnTop()
        {
            TopMost = !TopMost;
        }
        private void SetLeft(double newLeft)
        {
            Left = newLeft;
        }
        private void SetTop(double newTop)
        {   
            Top = newTop;
        }
        private void ExecuteMouseClick()
        {
            if (CurrentGuideline != null && CurrentGuideline.IsEnabled)
            {
                // 1. Toggle the lock state atomically
                bool newLockState = !CurrentGuideline.IsLocked;

                this.IsLocked = newLockState;
                CurrentGuideline.IsLocked = newLockState;

                // 2. Broadcast the change to WPF to redraw the line state
                OnPropertyChanged(nameof(CurrentGuideline));

                System.Diagnostics.Debug.WriteLine($"Guideline Lock Toggled Via Mouse! IsLocked: {CurrentGuideline.IsLocked}");
            }
        }
        private void ExecuteModifySetting(object parameter)
        {
            string commandKey = string.Empty;
            System.Windows.Point? clickPoint = null;

            // 1. Check if the incoming data is our packed mouse array
            if (parameter is object[] dataPackage && dataPackage.Length == 2)
            {
                commandKey = dataPackage[0] as string;
                clickPoint = dataPackage[1] as System.Windows.Point?;
            }
            else
            {
                // Otherwise, treat it as a standard keyboard shortcut string key
                commandKey = parameter as string;
            }
            if (string.IsNullOrEmpty(commandKey)) return;

            switch (commandKey)
            {
                // --- MOUSE & LOCK ACTIONS ---
                case "MOUSE_LEFT_CLICK":
                    if (CurrentGuideline != null && CurrentGuideline.IsEnabled)
                    {
                        // Toggle the lock state
                        bool newLockState = !CurrentGuideline.IsLocked;
                        this.IsLocked = newLockState;
                        CurrentGuideline.IsLocked = newLockState;

                        // If we just unlocked it, snap its position to the exact click coordinate!
                        if (!newLockState && clickPoint.HasValue)
                        {
                            CurrentGuideline.Position = clickPoint.Value.X;
                        }

                        OnPropertyChanged(nameof(CurrentGuideline));
                        OnPropertyChanged(nameof(IsLocked));

                        System.Diagnostics.Debug.WriteLine($"Guideline Toggled at X: {CurrentGuideline.Position}");
                    }
                    break;

                case "LOCKED":
                  ExecuteToggleLock();
                    break;

                case "SHOW_GUIDELINE":
                   ToggleGuideline();
                    break;

                // --- RULER LAYOUT & APPEARANCE ---
                case "FLIP_ORIENTATION":
                    ExecuteToggleOrientation();
                    break;

                case "RESIZE":
                    ExecuteResizeRuler();   
                    break;

                case "RESET_RULER_DEFAULT":
                    ExecuteResetToDefault();
                    break;

                case "TOGGLE_ONTOP":
                    ExecuteToggleOnTop();
                        break;

                case "TOGGLE_TOOLTIP":
                    ExecuteToggleToolTip();
                    break;

                // --- PERSISTENCE & SAVE TYPES ---
                case "SAVETYPE_ALL":
                    ExecuteSetSaveType(SaveTypes.All);
                    break;

                case "SAVETYPE_SIZE":
                    ExecuteSetSaveType(SaveTypes.Size);
                    break;

                case "SAVETYPE_LOCATION":
                    ExecuteSetSaveType(SaveTypes.Location);
                    break;

                case "SAVETYPE_NONE":
                    ExecuteSetSaveType(SaveTypes.None);
                    break;

                // --- OPACITY CONFIGURATIONS ---
                case "OPACITY_STEP_UP_1":
                    var newOpacity = Math.Min(Opacity + 0.1, 1.0);
                    ExecuteChangeOpacity(newOpacity);
                    break;

                case "OPACITY_STEP_UP_10":
                    ExecuteChangeOpacity(1.0);
                    break;

                case "OPACITY_STEP_DOWN_1":
                    var lowerOpacity = Math.Max(Opacity - 0.1, 0.0);
                    ExecuteChangeOpacity(lowerOpacity);
                    break;

                case "OPACITY_STEP_DOWN_10":
                    ExecuteChangeOpacity(0.10);
                    break;

                // --- PRECISION NUDGE NAVIGATION (10 PX) ---
                case "NAVIGATE_LEFT_10":
                    var newLeft = Left - 10;
                    SetLeft(newLeft);
                    break;

                case "NAVIGATE_RIGHT_10":
                    var  moveRight = Left + 10;
                    SetLeft(moveRight);
                    break;

                case "NAVIGATE_UP_10":
                    var moveUp = Top - 10;
                    SetTop(moveUp);
                    break;

                case "NAVIGATE_DOWN_10":
                    var moveDown = Top + 10;
                    SetTop(moveDown);
                    break;

                // --- FINE-GRAINED NUDGE NAVIGATION (1 PX) ---
                case "NAVIGATE_LEFT_1":
                    var smallLeft = Left - 1;   
                    SetLeft(smallLeft);
                    break;

                case "NAVIGATE_RIGHT_1":
                    var smallRight = Left + 1;
                    SetLeft(smallRight);
                    break;

                case "NAVIGATE_UP_1":
                    var smallUp = Top - 1;
                    SetTop(smallUp);
                    break;

                case "NAVIGATE_DOWN_1":
                    var smallDown = Top + 1;
                    SetTop(smallDown);
                    break;

                // --- APP LIFECYCLE & UTILITY ---
                case "ABOUT":
                    ExecuteShowAboutDialog();
                    break;

                case "CLOSE":
                    ExecuteClose();
                    break;

                case "EXIT_ALL":
                    ExecuteExitApplication();
                    break;
                case "DUPLICATE":
                    ExecuteDuplicateRuler();
                    break;


                default:
                    System.Diagnostics.Debug.WriteLine($"Unassigned command parameter received: {commandKey}");
                    break;
            }
        }
        #endregion
    }
}
