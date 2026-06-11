using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Ruler.Wpf.Common
{
    public class CenterConverter : IValueConverter
    {
        /// <summary>
        /// Calculates the position for a centered element.
        /// </summary>
        /// <param name="value">The actual size (width or height) of the parent Canvas.</param>
        /// <param name="targetType">The type of the binding target property (Canvas.Left or Canvas.Top).</param>
        /// <param name="parameter">An optional parameter, not used in this implementation.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>The calculated position to center an element.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Check if the value is a double, which is the type of ActualWidth and ActualHeight.
            if (value is double actualSize)
            {
                // The magic number '50' is an approximation for half the width/height
                // of the TextBlock itself. You may need to adjust this value to
                // get a perfect center, depending on the font and size.
                return (actualSize / 2) - 50;
            }

            // Return a default value if the conversion fails.
            return 0;
        }

        /// <summary>
        /// Not implemented for this use case, as the binding is one-way.
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}