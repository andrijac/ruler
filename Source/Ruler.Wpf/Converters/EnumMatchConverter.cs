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
    /// Evaluates whether an active binding enum matches a targeted XAML string parameter.
    /// Supports two-way updates for UI components like checkable context menu items.
    /// </summary>
    public class EnumMatchConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return false;

            string bindingValueStr = value.ToString();
            string parameterValueStr = parameter.ToString();

            // Perform a case-insensitive comparison of the enum name strings
            return string.Equals(bindingValueStr, parameterValueStr, StringComparison.OrdinalIgnoreCase);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // If a checkable UI element is selected (true), parse the parameter back to the target Enum type
            if (value is bool isChecked && isChecked && parameter != null)
            {
                if (targetType.IsEnum)
                {
                    try
                    {
                        return Enum.Parse(targetType, parameter.ToString(), true);
                    }
                    catch
                    {
                        return Binding.DoNothing;
                    }
                }
            }
            return Binding.DoNothing;
        }
    }
}
