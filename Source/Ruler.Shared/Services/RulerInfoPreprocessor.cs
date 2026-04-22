using Ruler.Shared.Enums;
using Ruler.Shared.Factories;
using Ruler.Shared.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ruler.Shared.Services
{
    public class RulerInfoPreprocessor 
    {
        public IEnumerable<RulerInfo> Preprocess(IEnumerable<RulerInfo> items)
        {
            return items
                .Where(x => x.SaveType != SaveTypes.none)
                .Select(PrepareForSave);
        }
        private RulerInfo PrepareForSave(RulerInfo item)
        {
            if (item == null) return null;
            RulerInfo strippedCopy = RulerFactory.CreateDefault();
            strippedCopy.SaveType = item.SaveType;
            switch (item.SaveType)
            {
                case SaveTypes.all:
                    RulerFactory.CopyValues(item, strippedCopy);
                    break;
                case SaveTypes.location:
                    strippedCopy.Left = item.Left;
                    strippedCopy.Top = item.Top;
                    strippedCopy.IsVertical = item.IsVertical;
                    break;
                case SaveTypes.size:
                    strippedCopy.Width = item.Width;
                    strippedCopy.Height = item.Height;
                    strippedCopy.IsVertical = item.IsVertical;
                    break;
                case SaveTypes.appearance:
                    strippedCopy.Opacity = item.Opacity;
                    strippedCopy.ShowToolTip = item.ShowToolTip;
                    strippedCopy.IsLocked = item.IsLocked;
                    strippedCopy.TopMost = item.TopMost;
                    break;
            }
            return strippedCopy;
        }
    }
}
