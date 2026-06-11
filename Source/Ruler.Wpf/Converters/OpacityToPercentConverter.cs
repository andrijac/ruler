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
    /// Converts a fractional opacity double (e.g., 0.6) into a percentage string (e.g., "60%"),
    /// and converts percentage strings back into raw doubles.
    /// </summary>
    [ValueConversion(typeof(double), typeof(string))]
    public class OpacityToPercentConverter : IValueConverter
    {
        /// <summary>
        /// Converts the Model's raw double fraction into a UI-friendly percentage string.
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double opacity)
            {
                // Multiply by 100 and round to eliminate floating-point precision artifacts
                int percent = (int)Math.Round(opacity * 100);
                return $"{percent}%";
            }

            return "100%"; // Fallback default value if data structure is unexpected
        }

        /// <summary>
        /// Converts a UI percentage string or numerical entry back into a raw double fraction.
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string input && !string.IsNullOrWhiteSpace(input))
            {
                // Strip out any percentage symbol the user or UI appended
                string cleanInput = input.Replace("%", "").Trim();

                if (double.TryParse(cleanInput, NumberStyles.Any, culture, out double result))
                {
                    // If they entered a whole number like "60", convert it to "0.6"
                    if (result > 1.0)
                    {
                        result /= 100.0;
                    }

                    // Keep opacity constraints within stable boundaries (min 10% transparency, max 100%)
                    return Math.Min(Math.Max(result, 0.1), 1.0);
                }
            }

            return 1.0; // Fallback to full opacity if the format could not be parsed
        }
    }
}
