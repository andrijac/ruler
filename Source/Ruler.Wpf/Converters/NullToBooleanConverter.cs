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
    /// Converts an object reference to a boolean. Returns true if the object is not null; otherwise false.
    /// </summary>
    public class NullToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // If the object is NOT null, return true (feature is enabled/instantiated)
            // If the object IS null, return false (feature is disabled)
            bool isNotNull = value != null;

            // Optional: Support inversion if Parameter="Invert" is passed
            if (parameter != null && string.Equals(parameter.ToString(), "Invert", StringComparison.OrdinalIgnoreCase))
            {
                return !isNotNull;
            }

            return isNotNull;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // One-Way bindings only require ConvertBack to return Binding.DoNothing 
            // or throw a NotImplementedException, since we don't convert booleans back to rich objects.
            return System.Windows.Data.Binding.DoNothing;
        }
    }
}
