using Ruler.Shared.Enums;
using Ruler.Shared.Factories;
using Ruler.Shared.Models;

using System.Collections.Generic;

namespace Ruler.Shared.Services
{
    public class RulerInfoPreprocessor
    {
        public IEnumerable<RulerInfo> Preprocess(IEnumerable<RulerInfo> items)
        {
            var outputList = new List<RulerInfo>();

            if (items == null) return outputList;

            foreach (var ruler in items)
            {
                // 1. If the user explicitly chose not to save this ruler, skip it entirely
                if (ruler.SaveType == SaveTypes.None) continue;

                // 2. Process a clean, isolated copy of the ruler based on its SaveType rules
                RulerInfo processedRuler = PrepareForSave(ruler);

                if (processedRuler != null)
                {
                    outputList.Add(processedRuler);
                }
            }

            return outputList;
        }

        private RulerInfo PrepareForSave(RulerInfo item)
        {
            if (item == null) return null;

            // Creates a fresh instance with its own default property allocations
            RulerInfo strippedCopy = RulerFactory.CreateDefault();
            strippedCopy.SaveType = item.SaveType;

            // Explicitly copy over historical identity so it updates correctly on reload
          //  strippedCopy.Guid = item.Guid;

            switch (item.SaveType)
            {
                case SaveTypes.All:
                    // Full clone of everything (including visual guidelines and tools)
                    RulerFactory.CopyValues(item, strippedCopy);
                    break;

                case SaveTypes.Location:
                    // Only preserve placement vectors; coordinates default to clean states
                    strippedCopy.Left = item.Left;
                    strippedCopy.Top = item.Top;
                    strippedCopy.IsVertical = item.IsVertical;
                    break;

                case SaveTypes.Size:
                    // Only preserve scale dimensions; coordinates default to clean states
                    strippedCopy.Width = item.Width;
                    strippedCopy.Height = item.Height;
                    strippedCopy.IsVertical = item.IsVertical;
                    break;
            }

            return strippedCopy;
        }
    }
}
