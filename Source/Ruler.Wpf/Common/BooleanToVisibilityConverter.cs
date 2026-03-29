using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Ruler.Wpf.Common
{
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class BooleanToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// Converts a boolean value to a Visibility value.
        /// </summary>
        /// <param name="value">The boolean value to convert (true or false).</param>
        /// <param name="targetType">The type of the binding target property (Visibility).</param>
        /// <param name="parameter">An optional parameter to reverse the conversion.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>Visibility.Visible if value is true, Visibility.Collapsed if value is false.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool booleanValue)
            {
                // If a converter parameter is specified, invert the boolean value.
                if (parameter != null && parameter.ToString().ToLower() == "invert")
                {
                    booleanValue = !booleanValue;
                    Console.WriteLine("Bottom Ticks");
                }

                return booleanValue ? Visibility.Visible : Visibility.Collapsed;
            }

            return DependencyProperty.UnsetValue;
        }

        /// <summary>
        /// Converts a Visibility value to a boolean value. Not implemented.
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}
