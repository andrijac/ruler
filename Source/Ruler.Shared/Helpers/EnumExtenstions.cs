using Ruler.Shared.Enums;
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
    public static class EnumExtensions
    {
        public static string GetDisplayName(this Enum enumValue)
        {
            // Handle special custom cases globally
            if (enumValue is SaveTypes saveType && saveType == SaveTypes.All)
            {
                return "All";
            }

            // Optional: Support [Description] attributes for other enums if desired
            var field = enumValue.GetType().GetField(enumValue.ToString());
            if (field != null)
            {
                var attribute = Attribute.GetCustomAttribute(field, typeof(System.ComponentModel.DescriptionAttribute)) as System.ComponentModel.DescriptionAttribute;
                if (attribute != null)
                {
                    return attribute.Description;
                }
            }

            // Default fallback
            return enumValue.ToString();
        }
        public static ObservableCollection<MenuItemModel> ToMenuItems<T>(this T enumInstance, ICommand command) where T : Enum
        {
            var items = new ObservableCollection<MenuItemModel>();

            foreach (T enumValue in Enum.GetValues(typeof(T)))
            {
                items.Add(new MenuItemModel()
                {
                    Header = enumValue.GetDisplayName(), // Calls your previous extension method
                    Value = enumValue,
                    Command = command,
                    IsCheckable = true
                });
            }

            return items;
        }
    }
}
