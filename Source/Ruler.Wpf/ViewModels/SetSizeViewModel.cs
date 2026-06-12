using Ruler.Shared.Models;

using System.ComponentModel;

namespace Ruler.Wpf.ViewModels
{
    public class SetSizeViewModel : ModelBase
    {
        private string _widthString;
        private string _heightString;

        // Bind your textboxes to these string properties
        public string WidthString
        {
            get => _widthString;
            set { _widthString = value; OnPropertyChanged(); }
        }

        public string HeightString
        {
            get => _heightString;
            set { _heightString = value; OnPropertyChanged(); }
        }

        // Expose numeric properties for your RulerViewModel to read after ShowDialog returns
        public int Width => int.TryParse(WidthString, out int w) ? w : 0;
        public int Height => int.TryParse(HeightString, out int h) ? h : 0;

        public SetSizeViewModel(double currentWidth, double currentHeight)
        {
            // Initialize strings with the current ruler coordinates
            WidthString = ((int)currentWidth).ToString();
            HeightString = ((int)currentHeight).ToString();
        }
    }
}
