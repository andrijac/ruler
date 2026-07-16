using Ruler.Shared.Enums;
using Ruler.Shared.Interfaces;
using Ruler.Shared.Models;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Ruler.Shared.Factories
{
    public class RulerFactory : IRulerFactory
    {
        public  RulerInfo CreateDefault()
        {
            return new RulerInfo
            {
                Width = 400,
                Height = 75,
                Opacity = 0.60,
                ShowToolTip = false,
                IsVertical = false,
                TopMost = true,
                Top = 0,
                Left = 0,
                SaveType = SaveTypes.None
            };
        }
        /// <summary>
        /// Maps properties from a source RulerInfo to a destination RulerInfo.
        /// Useful for "stripping" data during save or cloning objects.
        /// </summary>
        public void CopyValues(RulerInfo source, RulerInfo target)
        {
            if (source == null || target == null) return;

            // Get all public instance properties
            var properties = typeof(RulerInfo).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in properties)
            {
                // 1. Ensure the property can be read and written
                if (prop.CanRead && prop.CanWrite)
                {
                    // 2. OPTIONAL: Skip properties you don't want copied
                    // Example: Don't overwrite the ID if we are cloning, or skip read-only UI logic
                    if (prop.Name == "Id") continue;

                    var value = prop.GetValue(source);
                    prop.SetValue(target, value);
                }
            }
        }
        public RulerInfo CreateFromArguments(string[] args)
        {
            RulerInfo info = CreateDefault();

            // The list has 9 main segments (Left,Top is usually one arg if space-delimited)
            if (args == null || args.Length < 8) return info;

            try
            {
                // 1 & 2: Width and Height
                info.Width = ParseInt(args[0], 300);
                info.Height = ParseInt(args[1], 50);

                // 3: IsVertical
                info.IsVertical = ParseBool(args[2], false);

                // 4: Opacity
                info.Opacity = ParseDouble(args[3], 1.0);

                // 5: ShowToolTip
                info.ShowToolTip = ParseBool(args[4], true);

                // 6: IsLocked
                info.IsLocked = ParseBool(args[5], false);

                // 7: TopMost
                info.TopMost = ParseBool(args[6], true);

                // 8: Location (Handle the "Left,Top" comma segment)
                string[] locationParts = args[7].Split(',');
                if (locationParts.Length == 2)
                {
                    info.Left = ParseInt(locationParts[0], 100);
                    info.Top = ParseInt(locationParts[1], 100);
                }

                // 9: SaveType
                // Assuming info.SaveType is an Enum
                if (args.Length >= 9 && Enum.TryParse(args[8], true, out SaveTypes result))
                {
                    info.SaveType = result;
                }
            }
            catch
            {
                return CreateDefault(); 
            }

            return info;
        }
        private int ParseInt(string value, int defaultValue)
    => int.TryParse(value, out int result) ? result : defaultValue;

        private bool ParseBool(string value, bool defaultValue)
            => bool.TryParse(value, out bool result) ? result : defaultValue;

        private double ParseDouble(string value, double defaultValue)
            => double.TryParse(value, System.Globalization.NumberStyles.Any,
               System.Globalization.CultureInfo.InvariantCulture, out double result) ? result : defaultValue;

        public string ToParameterString(RulerInfo info)
        {
            return $"{info.Width} {info.Height} {info.IsVertical} {info.Opacity} {info.ShowToolTip} {info.IsLocked} {info.TopMost} {info.Left},{info.Top} {info.SaveType}";
        }
    }
}
