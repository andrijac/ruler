using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Ruler.Wpf.Converters
{
    /// <summary>
    /// Converts a boolean value to a WPF Visibility state (Visible vs Collapsed).
    /// Supports an optional string parameter "Invert" to flip the logic.
    /// </summary>
    public class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                // Check if the XAML passed Parameter="Invert" or Parameter="True" to flip the behavior
                bool invert = parameter != null &&
                             string.Equals(parameter.ToString(), "Invert", StringComparison.OrdinalIgnoreCase);

                if (invert)
                {
                    boolValue = !boolValue;
                }

                return boolValue ? Visibility.Visible : Visibility.Collapsed;
            }

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility visibility)
            {
                bool boolValue = visibility == Visibility.Visible;

                bool invert = parameter != null &&
                             string.Equals(parameter.ToString(), "Invert", StringComparison.OrdinalIgnoreCase);

                return invert ? !boolValue : boolValue;
            }

            return false;
        }
    }
}
