using Ruler.Wpf;
using Ruler.Wpf.Common;
using Ruler.Wpf.Models;

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Ruler.Wpf.ViewModels
{
    // The core ViewModel class for the Ruler application
    public class RulerViewModel : INotifyPropertyChanged
    {
        // The RulerInfo class would be our Model
        private RulerInfo _rulerInfo;
        private IDialogService _dialogService;
        // Collections for the ItemsControls to bind to
        public ObservableCollection<LineInfo> RulerLines { get; set; }
        public ObservableCollection<LabelInfo> RulerLabels { get; set; }
        public ObservableCollection<RulerShapes> RulerTicks { get; set; } = new ObservableCollection<RulerShapes>();
        private RulerOrientation _orientation;
      
        // State variables for mouse interaction
        private Point _startPoint;
        private Size _startSize;
        private bool _isResizing;
        private bool _isMoving;
        private double _length;

        public RulerViewModel(IDialogService dialogService)
        {
            // Initialize the model with a default or saved state.
            _rulerInfo = RulerInfo.GetSavedRulerInfo();
            _dialogService = dialogService ?? throw new ArgumentException(nameof(dialogService));

            // Initialize commands
            ToggleLockCommand = new RelayCommand(ToggleLock);
            ExitCommand = new RelayCommand(ExitApplication);
            ToggleVerticalCommand = new RelayCommand(ToggleVertical);
            ToggleTopMostCommand = new RelayCommand(ToggleTopMost);
            ToggleToolTipCommand = new RelayCommand(ToggleToolTip);
            SetOpacityCommand = new RelayCommand(SetOpacity);
            SetSaveTypeCommand = new RelayCommand(SetSaveType);
            ShowAboutCommand = new RelayCommand(NavigateAbout);
            // Initialize the collections
            RulerLines = new ObservableCollection<LineInfo>();
            RulerLabels = new ObservableCollection<LabelInfo>();
            Orientation = RulerOrientation.Horizontal;
            // Initial drawing of the ruler
            //  UpdateRulerDrawing();
            Length = 400;
            CalculateRulerItems();
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
                  //  UpdateRulerDrawing();
                    CalculateRulerItems();
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
                    CalculateRulerItems();
                   // UpdateRulerDrawing();
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
                Orientation = (value==true)?RulerOrientation.Vertical:RulerOrientation.Horizontal;
                OnPropertyChanged();
                CalculateRulerItems();
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

        public double Length
        {
            get => _length;
            set
            {
                if (_length != value)
                {
                    _length = value;
                    OnPropertyChanged(nameof(Length));
                    // Recalculate everything when the length changes
                    CalculateRulerItems();
                }
            }
        }
      

        public RulerOrientation Orientation
        {
            get => _orientation;
            set
            {
                if (_orientation != value)
                {
                    _orientation = value;
                    OnPropertyChanged(nameof(Orientation));
                    // Recalculate everything when the orientation changes
                    CalculateRulerItems();
                }
            }
        }

        // This method contains the core logic for generating ticks and labels
        public void CalculateRulerItems()
        {
            // Clear the main collection that holds both lines and labels
            RulerTicks.Clear();

            // Check the orientation and generate points accordingly
            if (Orientation == RulerOrientation.Horizontal)
            {
                // Generate horizontal lines and labels
                for (int i = 0; i <= Length; i += 1)
                {
                    // Major ticks (100mm)
                    if (i % 100 == 0)
                    {
                        // Add top major tick line
                        RulerTicks.Add(new LineInfo() { Y1 = 0, Y2 = 25, X1 = i, X2 = i, Thickness = 2 });
                        // Add top major tick label
                        RulerTicks.Add(new LabelInfo() { X = i, Y = 30, Text = i.ToString(), FontSize = 10 });
                        // Add bottom major tick line
                        RulerTicks.Add(new LineInfo() { Y1 = Height, Y2 = Height - 25, X1 = i, X2 = i, Thickness = 2 });
                        // Add bottom major tick label
                        RulerTicks.Add(new LabelInfo() { X = i, Y = Height - 30, Text = i.ToString(), FontSize = 10 });
                    }
                    // Half-inch ticks (50mm)
                    else if (i % 50 == 0)
                    {
                        // Add top half-inch tick line
                        RulerTicks.Add(new LineInfo() { Y1 = 0, Y2 = 18, X1 = i, X2 = i, Thickness = 1 });
                        // Add bottom half-inch tick line
                        RulerTicks.Add(new LineInfo() { Y1 = Height, Y2 = Height - 18, X1 = i, X2 = i, Thickness = 1 });
                    }
                    // Quarter-inch ticks (10mm)
                    else if (i % 10 == 0)
                    {
                        // Add top quarter-inch tick line
                        RulerTicks.Add(new LineInfo() { Y1 = 0, Y2 = 12, X1 = i, X2 = i, Thickness = 1 });
                        // Add bottom quarter-inch tick line
                        RulerTicks.Add(new LineInfo() { Y1 = Height, Y2 = Height - 12, X1 = i, X2 = i, Thickness = 1 });
                    }
                    // Smallest ticks
                    else
                    {
                        // Add top smallest tick line
                        RulerTicks.Add(new LineInfo() { Y1 = 0, Y2 = 6, X1 = i, X2 = i, Thickness = 1 });
                        // Add bottom smallest tick line
                        RulerTicks.Add(new LineInfo() { Y1 = Height, Y2 = Height - 6, X1 = i, X2 = i, Thickness = 1 });
                    }
                }
            }
            else // Vertical orientation
            {
                // Generate vertical lines and labels
                for (int i = 0; i <= Length; i += 1)
                {
                    if (i % 100 == 0) // Major ticks (100mm)
                    {
                        // Add left major tick line
                        RulerTicks.Add(new LineInfo() { X1 = 0, Y1 = i, X2 = 25, Y2 = i, Thickness = 2 });
                        // Add left major tick label
                        RulerTicks.Add(new LabelInfo() { X = 30, Y = i, Text = i.ToString(), FontSize = 10 });
                        // Add right major tick line
                        RulerTicks.Add(new LineInfo() { X1 = Width, Y1 = i, X2 = Width - 25, Y2 = i, Thickness = 2 });
                        // Add right major tick label
                        RulerTicks.Add(new LabelInfo() { X = Width - 30, Y = i, Text = i.ToString(), FontSize = 10 });
                    }
                    else if (i % 50 == 0) // Half-inch ticks (50mm)
                    {
                        RulerTicks.Add(new LineInfo() { X1 = 0, Y1 = i, X2 = 18, Y2 = i, Thickness = 1 });
                        RulerTicks.Add(new LineInfo() { X1 = Width, Y1 = i, X2 = Width - 18, Y2 = i, Thickness = 1 });
                    }
                    else if (i % 10 == 0) // Quarter-inch ticks (10mm)
                    {
                        RulerTicks.Add(new LineInfo() { X1 = 0, Y1 = i, X2 = 12, Y2 = i, Thickness = 1 });
                        RulerTicks.Add(new LineInfo() { X1 = Width, Y1 = i, X2 = Width - 12, Y2 = i, Thickness = 1 });
                    }
                    else // Smallest ticks
                    {
                        RulerTicks.Add(new LineInfo() { X1 = 0, Y1 = i, X2 = 6, Y2 = i, Thickness = 1 });
                        RulerTicks.Add(new LineInfo() { X1 = Width, Y1 = i, X2 = Width - 6, Y2 = i, Thickness = 1 });
                    }
                }
            }
            // Notify the UI that the collection has changed, allowing it to redraw
            OnPropertyChanged(nameof(RulerTicks));
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
            if (IsVertical)
            {
                // Position the size label near the middle of a vertical ruler.
                RulerLabels.Add(new LabelInfo
                {
                    Text = $"{Width} x {Height}",
                    FontSize = 12,
                    Foreground = Brushes.Black,
                    X = 20, // A fixed offset from the left side
                    Y = Height / 2.0 - 10, // Center it vertically
                });
            }
            else
            {
                // Position the size label near the middle of a horizontal ruler.
                RulerLabels.Add(new LabelInfo
                {
                    Text = $"{Width} x {Height}",
                    FontSize = 12,
                    Foreground = Brushes.Black,
                    X = Width / 2.0 - 30, // Center it horizontally
                    Y = 20, // A fixed offset from the top
                });
            }


            // Loop through the length to draw tick marks
            double length = IsVertical ? Height : Width;
            for (int i = 0; i <= length; i++)
            {
                double currentTickHeight = 0;
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

                // Draw the tick mark and label based on orientation
                if (IsVertical)
                {
                    RulerLines.Add(new LineInfo
                    {
                        X1 = 0,
                        Y1 = i,
                        X2 = currentTickHeight,
                        Y2 = i,
                        Stroke = Brushes.Black,
                        Thickness = lineThickness
                    });

                    if (labelText != null)
                    {
                        RulerLabels.Add(new LabelInfo
                        {
                            Text = labelText,
                            X = currentTickHeight + labelOffset,
                            Y = i,
                            FontSize = 10,
                            Foreground = Brushes.Black
                        });
                    }
                }
                else // Horizontal
                {
                    RulerLines.Add(new LineInfo
                    {
                        X1 = i,
                        Y1 = 0,
                        X2 = i,
                        Y2 = currentTickHeight,
                        Stroke = Brushes.Black,
                        Thickness = lineThickness
                    });

                    if (labelText != null)
                    {
                        RulerLabels.Add(new LabelInfo
                        {
                            Text = labelText,
                            X = i,
                            Y = currentTickHeight + labelOffset,
                            FontSize = 10,
                            Foreground = Brushes.Black
                        });
                    }
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
            (Height,Width) = (Width,Height);    
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


        // Boilerplate for INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}