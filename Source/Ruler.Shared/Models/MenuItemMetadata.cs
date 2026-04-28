using Ruler.Shared.Enums;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ruler.Shared.Models
{
    public class MenuItemMetadata
    {
        public MenuItemEnum Type { get; set; }
        public string Text { get; set; }
        public bool IsChecked { get; set; }
        public bool IsSeparator { get; set; }

        public MenuItemMetadata(MenuItemEnum type, string text, bool isChecked = false)
        {
            Type = type;
            Text = text;
            IsChecked = isChecked;
            IsSeparator = false;
        }

        // Static helper for quick separator creation
        public static MenuItemMetadata Separator => new MenuItemMetadata(MenuItemEnum.Separator, string.Empty)
        {
            IsSeparator = true
        };
    }
}
