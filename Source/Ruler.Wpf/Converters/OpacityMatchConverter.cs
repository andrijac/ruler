using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Ruler.Wpf.Converters
{
    /// <summary>
    /// Checks if the current ruler opacity matches a specific menu option percentage parameter.
    /// Returns true if they match, allowing checkmarks to update dynamically in the context menu.
    /// </summary>
    public class OpacityMatchConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double activeOpacity && parameter != null)
            {
                // Parse the target opacity constant passed via ConverterParameter in XAML
                if (double.TryParse(parameter.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out double targetVal))
                {
                    // Use a small epsilon threshold (0.01) to prevent floating-point precision mismatch issues
                    return Math.Abs(activeOpacity - targetVal) < 0.001;
                }
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // We only need a one-way conversion to check the menu item state.
            // When clicked, the ViewModel command handles changing the value directly.
            return Binding.DoNothing;
        }
    }
}
