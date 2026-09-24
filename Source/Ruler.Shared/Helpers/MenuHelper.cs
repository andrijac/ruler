using Ruler.Shared.Models;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Ruler.Shared.Helpers
{
    public static class MenuHelper
    {
        public static ObservableCollection<MenuItemModel> CreateRangeMenuItems(
            int start,
            int end,
            int step,
            string formatString,
            Func<int, object> valueSelector,
            ICommand command)
        {
            var items = new ObservableCollection<MenuItemModel>();

            for (int i = start; i <= end; i += step)
            {
                items.Add(new MenuItemModel()
                {
                    Header = string.Format(formatString, i),
                    Value = valueSelector(i),
                    Command = command,
                    IsCheckable = true
                });
            }

            return items;
        }
        public static ObservableCollection<MenuItemModel> CreateRangeMenuItems(
        double start,
        double end,
        double step,
        string formatString,
        Func<double, object> valueSelector,
        ICommand command)
        {
            var items = new ObservableCollection<MenuItemModel>();

            // Using a tiny epsilon (0.0001) prevents floating-point rounding bugs in loops
            for (double i = start; i <= end + 0.0001; i += step)
            {
                items.Add(new MenuItemModel()
                {
                    Header = string.Format(formatString, i),
                    Value = valueSelector(i),
                    Command = command,
                    IsCheckable = true
                });
            }

            return items;
        }
    
    }
}
