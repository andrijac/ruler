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
    public class BooleanToResizeModeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isLocked)
            {
                // If locked, disable resizing completely.
                if (isLocked)
                {
                    return ResizeMode.NoResize;
                }
                // If unlocked, enable resizing from all edges and corners (CanResize is usually better for borderless windows).
                else
                {
                    return ResizeMode.CanResize;
                }
            }
            return ResizeMode.CanResize; // Default to allowing resize
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Not used for this unidirectional binding
            throw new NotImplementedException();
        }
    }
}
