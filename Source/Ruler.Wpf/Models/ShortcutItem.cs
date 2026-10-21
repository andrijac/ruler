using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ruler.Wpf.Models
{
    public class ShortcutItem
    {
        public string Category { get; set; } // Used for headers or grouping
        public string KeyCombination { get; set; }
        public string Description { get; set; }
        public string ColorHex { get; set; } = "#FFFFFF";
        public bool IsHeader { get; set; }
        public bool IsBold { get; set; }
    }
}
