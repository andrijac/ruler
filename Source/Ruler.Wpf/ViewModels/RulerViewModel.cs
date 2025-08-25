using Ruler.Wpf;
using Ruler.Wpf.Common;
using Ruler.Wpf.Models;

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Ruler.Wpf.ViewModels
{
    // The core ViewModel class for the Ruler application
    public class RulerViewModel : INotifyPropertyChanged
    {
        // The RulerInfo class would be our Model
        private RulerInfo _rulerInfo;

        // Collections for the ItemsControls to bind to
        public ObservableCollection<LineInfo> RulerLines { get; set; }
        public ObservableCollection<LabelInfo> RulerLabels { get; set; }

        // State variables for mouse interaction
        private Point _startPoint;
        private Size _startSize;
        private bool _isResizing;
        private bool _isMoving;

        public RulerViewModel()
        {
            // Initialize the model with a default or saved state.
            _rulerInfo = RulerInfo.GetSavedRulerInfo();

            // Initialize commands
            ToggleLockCommand = new RelayCommand(ToggleLock);
            ExitCommand = new RelayCommand(ExitApplication);
            ToggleVerticalCommand = new RelayCommand(ToggleVertical);
            ToggleTopMostCommand = new RelayCommand(ToggleTopMost);
            ToggleToolTipCommand = new RelayCommand(ToggleToolTip);
            SetOpacityCommand = new RelayCommand(SetOpacity);
            SetSaveTypeCommand = new RelayCommand(SetSaveType);

            // Initialize the collections
            RulerLines = new ObservableCollection<LineInfo>();
            RulerLabels = new ObservableCollection<LabelInfo>();

            // Initial drawing of the ruler
            UpdateRulerDrawing();
        }

        // Property for the ruler's width, with change notification
        public int Width
        {
            get => _rulerInfo.Width;
            set
            {
                if (_rulerInfo.Width != value)
                {
                    _rulerInfo.Width = value;
                    OnPropertyChanged();
                    UpdateRulerDrawing();
                }
            }
        }

        // Property for the ruler's height, with change notification
        public int Height
        {
            get => _rulerInfo.Height;
            set
            {
                if (_rulerInfo.Height != value)
                {
                    _rulerInfo.Height = value;
                    OnPropertyChanged();
                    UpdateRulerDrawing();
                }
            }
        }

        // Property for the ruler's location, with change notification
        public Point DisplayedLocation
        {
            get => _rulerInfo.DisplayedLocation;
            set
            {
                _rulerInfo.DisplayedLocation = value;
                OnPropertyChanged();
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
                    OnPropertyChanged();
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
                    OnPropertyChanged();
                }
            }
        }

        // Property for the TopMost state
        public bool TopMost
        {
            get => _rulerInfo.TopMost;
            set
            {
                _rulerInfo.TopMost = value;
                OnPropertyChanged();
            }
        }

        // Property for the vertical state
        public bool IsVertical
        {
            get => _rulerInfo.IsVertical;
            set
            {
                _rulerInfo.IsVertical = value;
                OnPropertyChanged();
            }
        }

        // Property for the tooltip state
        public bool ShowToolTip
        {
            get => _rulerInfo.ShowToolTip;
            set
            {
                _rulerInfo.ShowToolTip = value;
                OnPropertyChanged();
            }
        }

        // Property for the save type
        public SaveTypes SaveType
        {
            get => _rulerInfo.SaveType;
            set
            {
                _rulerInfo.SaveType = value;
                OnPropertyChanged();
            }
        }

        // Public property to expose the resizing state to the View
        public bool IsResizing
        {
            get => _isResizing;
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
                DisplayedLocation = new Point(DisplayedLocation.X + deltaX, DisplayedLocation.Y + deltaY);
                _startPoint = mousePosition; // Update start point for continuous dragging
            }
        }

        // Public method to handle mouse up from the View
        public void MouseLeftButtonUp()
        {
            _isResizing = false;
            _isMoving = false;
            OnPropertyChanged(nameof(IsResizing));
        }


        // This method contains all the logic for drawing the ruler based on its current size and orientation
        private void UpdateRulerDrawing()
        {
            // Clear existing lines and labels to redraw the ruler
            RulerLines.Clear();
            RulerLabels.Clear();

            // Constants for drawing
            const double tickHeight = 10;
            const double bigTickHeight = 20;
            const double lineThickness = 1;
            const double labelOffset = 5;

            // Add the ruler's size text, centered
            RulerLabels.Add(new LabelInfo
            {
                Text = $"{Width} x {Height}",
                FontSize = 12,
                Foreground = Brushes.Black,
                X = (IsVertical ? 0 : Width / 2 - 30),
                Y = (IsVertical ? Height / 2 - 10 : 0)
            });

            // Loop through the length to draw tick marks
            double length = IsVertical ? Height : Width;
            for (int i = 0; i <= length; i++)
            {
                double currentTickHeight;
                string labelText = null;

                if (i % 50 == 0) // Major tick mark every 50 pixels
                {
                    currentTickHeight = bigTickHeight;
                    labelText = i.ToString();
                }
                else if (i % 10 == 0) // Minor tick mark every 10 pixels
                {
                    currentTickHeight = tickHeight;
                }
                else if (i % 5 == 0) // Tiny tick mark every 5 pixels
                {
                    currentTickHeight = tickHeight / 2;
                }
                else
                {
                    continue; // Skip the rest of the loop for this iteration
                }

                // Add the line for the tick mark
                RulerLines.Add(new LineInfo
                {
                    X1 = IsVertical ? tickHeight : i,
                    Y1 = IsVertical ? i : 0,
                    X2 = IsVertical ? (tickHeight - currentTickHeight) : i,
                    Y2 = IsVertical ? i : currentTickHeight,
                    Stroke = Brushes.Black,
                    StrokeThickness = lineThickness
                });

                // If a label exists, add it as well
                if (labelText != null)
                {
                    RulerLabels.Add(new LabelInfo
                    {
                        Text = labelText,
                        X = IsVertical ? (tickHeight - currentTickHeight - labelOffset) : i,
                        Y = IsVertical ? i : (currentTickHeight + labelOffset),
                        FontSize = 10,
                        Foreground = Brushes.Black
                    });
                }
            }
        }


        // Command properties for UI actions
        public ICommand ToggleLockCommand { get; }
        public ICommand ExitCommand { get; }
        public ICommand ToggleVerticalCommand { get; }
        public ICommand ToggleTopMostCommand { get; }
        public ICommand ToggleToolTipCommand { get; }
        public ICommand SetOpacityCommand { get; }
        public ICommand SetSaveTypeCommand { get; }
        public ICommand ShowSetSizeFormCommand { get; }
        public ICommand ShowAboutCommand { get; }
        public ICommand DuplicateCommand { get; }

        // Logic for the ToggleLockCommand
        private void ToggleLock(object parameter)
        {
            IsLocked = !IsLocked;
        }

        // Logic for the ExitCommand
        private void ExitApplication(object parameter)
        {
            RulerInfo.SaveAll(_rulerInfo);
            Application.Current.Shutdown();
        }

        // Logic for the ToggleVerticalCommand
        private void ToggleVertical(object parameter)
        {
            IsVertical = !IsVertical;
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
            }
        }

        // Logic for the SetSaveTypeCommand
        private void SetSaveType(object parameter)
        {
            if (parameter is SaveTypes saveType)
            {
                SaveType = saveType;
            }
        }


        // Boilerplate for INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}