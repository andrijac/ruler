using System.Windows.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.Windows;

namespace Ruler.Wpf.Converters
{
    public class BooleanToVisibilityConverter : IValueConverter
    {
        // Converts boolean to visibility (true -> Visible, false -> Collapsed)
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool booleanValue)
            {
                bool invert = false;

                // Check for the 'ConverterParameter' to invert the logic
                if (parameter != null && parameter.ToString().Equals("Invert", StringComparison.OrdinalIgnoreCase))
                {
                    invert = true;
                }

                // Apply the logic:
                // If invert is true, visible when booleanValue is false.
                // If invert is false, visible when booleanValue is true.
                Console.WriteLine(booleanValue);
                Console.WriteLine(invert);
                if (booleanValue != invert)
                {
                    Console.WriteLine("Visible");
                    return Visibility.Visible;
                }
            }
            Console.WriteLine("Collapsed");
            return Visibility.Collapsed;
        }

        // Converts visibility to boolean (not typically needed for UI binding)
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}
