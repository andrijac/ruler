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
        public void CopyValuesWithGUID(RulerInfo source, RulerInfo target)
        {
            if (source == null || target == null) return;

            // Get all public instance properties
            var properties = typeof(RulerInfo).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in properties)
            {
                // 1. Ensure the property can be read and written
                if (prop.CanRead && prop.CanWrite)
                {
                    var value = prop.GetValue(source);
                    prop.SetValue(target, value);
                }
            }
        }
        public RulerInfo CreateFromArguments(string[] args)
        {
            RulerInfo info = CreateDefault();
            if (args == null || args.Length == 0) return info;

            int positionalIndex = 0;

            for (int i = 0; i < args.Length; i++)
            {
                string arg = args[i];

                // 1. Handle Flags (e.g., --width 500)
                if (arg.StartsWith("--"))
                {
                    string key = arg.ToLower();
                    string value = (i + 1 < args.Length) ? args[i + 1] : "";

                    if (ApplyFlag(info, key, value))
                    {
                        i++; // Skip the value because we consumed it
                    }
                }
                // 2. Handle Positional (e.g., 500 500 true 1.0 ...)
                else
                {
                    ApplyPositional(info, positionalIndex++, arg);
                }
            }

            return info;
        }

        private bool ApplyFlag(RulerInfo info, string key, string value)
        {
            switch (key)
            {
                case "--width": info.Width = ParseInt(value, info.Width); return true;
                case "--height": info.Height = ParseInt(value, info.Height); return true;
                case "--isvertical": info.IsVertical = ParseBool(value, info.IsVertical); return true;
                case "--opacity": info.Opacity = ParseDouble(value, info.Opacity); return true;
                case "--showtooltip": info.ShowToolTip = ParseBool(value, info.ShowToolTip); return true;
                case "--islocked": info.IsLocked = ParseBool(value, info.IsLocked); return true;
                case "--topmost": info.TopMost = ParseBool(value, info.TopMost); return true;
                default: return false;
            }
        }

        private void ApplyPositional(RulerInfo info, int index, string value)
        {
            switch (index)
            {
                case 0: info.Width = ParseInt(value, info.Width); break;
                case 1: info.Height = ParseInt(value, info.Height); break;
                case 2: info.IsVertical = ParseBool(value, info.IsVertical); break;
                case 3: info.Opacity = ParseDouble(value, info.Opacity); break;
                case 4: info.ShowToolTip = ParseBool(value, info.ShowToolTip); break;
                case 5: info.IsLocked = ParseBool(value, info.IsLocked); break;
                case 6: info.TopMost = ParseBool(value, info.TopMost); break;
                case 7:
                    var loc = ParseLocation(value, info.Left, info.Top);
                    info.Left = loc.X;
                    info.Top = loc.Y;
                    break;
                case 8:
                    if (Enum.TryParse(value, true, out SaveTypes result)) info.SaveType = result;
                    break;
            }
        }

        private int ParseInt(string value, int defaultValue)
    => int.TryParse(value, out int result) ? result : defaultValue;

        private bool ParseBool(string value, bool defaultValue)
            => bool.TryParse(value, out bool result) ? result : defaultValue;

        private double ParseDouble(string value, double defaultValue)
            => double.TryParse(value, System.Globalization.NumberStyles.Any,
               System.Globalization.CultureInfo.InvariantCulture, out double result) ? result : defaultValue;

        private SaveTypes ParseSaveType(string saveType, SaveTypes defaultValue)
            => Enum.TryParse(saveType, out SaveTypes types) ? types : defaultValue;

        private (int X, int Y) ParseLocation(string value, int defaultX, int defaultY)
        {
            if (string.IsNullOrWhiteSpace(value))
                return (defaultX, defaultY);

            string[] parts = value.Split(',');

            if (parts.Length != 2)
                return (defaultX, defaultY);

            // Using your existing ParseInt helper
            return (ParseInt(parts[0], defaultX), ParseInt(parts[1], defaultY));
        }

        public string ToParameterString(RulerInfo info)
        {
            return $"{info.Width} {info.Height} {info.IsVertical} {info.Opacity} {info.ShowToolTip} {info.IsLocked} {info.TopMost} {info.Left},{info.Top} {info.SaveType}";
        }
    }
}
