using Ruler.Shared.Enums;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Ruler
{
    public class MenuItemHolder
    {
        public MenuItemEnum MenuItemEnum { get; set; }
        public MenuItem MenuItem { get; set; }

        // Standard constructor for items
        public MenuItemHolder(MenuItemEnum type, string text, EventHandler handler, bool isChecked)
        {
            MenuItemEnum = type;
            MenuItem = new MenuItem(text, handler)
            {
                Checked = isChecked
            };
        }

        // Special constructor for separators
        private MenuItemHolder() { }

        public static MenuItemHolder Separator => new MenuItemHolder
        {
            MenuItemEnum = MenuItemEnum.Separator,
            MenuItem = new MenuItem("-")
        };
    }
}
