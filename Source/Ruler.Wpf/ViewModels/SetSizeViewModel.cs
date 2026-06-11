using Ruler.Shared.Models;

using System.ComponentModel;

namespace Ruler.Wpf.ViewModels
{
    public class SetSizeViewModel : ModelBase, IDataErrorInfo
    {
        private double _width;
        private double _height;

        public SetSizeViewModel(double width, double height)
        {
            _width = width;
            _height = height;
        }

        public double Width
        {
            get => _width;
            set => SetProperty(ref _width, value);
        }

        public double Height
        {
            get => _height;
            set => SetProperty(ref _height, value);
        }

        // IDataErrorInfo Members
        public string Error => null;

        public string this[string columnName]
        {
            get
            {
                string error = null;

                switch (columnName)
                {
                    case nameof(Width):
                        if (Width < 10 || Width > 10000)
                        {
                            error = "Width must be between 10 and 10,000 pixels.";
                        }
                        break;

                    case nameof(Height):
                        if (Height < 10 || Height > 10000)
                        {
                            error = "Height must be between 10 and 10,000 pixels.";
                        }
                        break;
                }

                return error;
            }
        }
    }
}
