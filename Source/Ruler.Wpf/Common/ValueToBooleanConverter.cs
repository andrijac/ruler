using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Ruler.Wpf.Common
{
    /// <summary>
    /// A generic converter that compares a bound value to a CommandParameter
    /// and returns a boolean. Useful for "radio-button" like behavior.
    /// </summary>
    public class ValueToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Check if the bound value is equal to the parameter value.
            // We use .Equals() to handle various types correctly.
            if (value is double doubleValue && parameter is string paramString && double.TryParse(paramString, NumberStyles.Any, culture, out double paramDouble))
            {
                return doubleValue == paramDouble;
            }
            return value != null && value.Equals(parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // This method is not needed for this use case, as the MenuItem's
            // IsChecked property is being set by the binding.
            return Binding.DoNothing;
        }
    }
}
