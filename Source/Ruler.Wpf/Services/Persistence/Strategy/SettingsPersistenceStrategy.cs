using Ruler.Wpf.Common;
using Ruler.Wpf.Models;

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Windows;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;


namespace Ruler.Wpf.Services.Persistence.Strategy
{
    public class SettingsPersistenceStrategy : IPersistenceStrategy
    {
        private readonly ILoggingService _loggingService;
        public SettingsPersistenceStrategy(ILoggingService loggingService)
        {
            _loggingService = loggingService;
        }
        // --- PUBLIC METHOD (Required by Interface) ---
        public void Save(RulerInfo ri)
        {
            ResetAllUserScopeSettingsToDefault();

            Settings.Default["savetype"] = ri.SaveType.ToString();

            if (ri.SaveType == SaveTypes.all)
            {
                SaveLocation(ri);
                SaveSize(ri);
                SaveGlobalFlags(ri);
            }
            else if (ri.SaveType == SaveTypes.location)
            {
                SaveLocation(ri);
            }
            else if (ri.SaveType == SaveTypes.size)
            {
                SaveSize(ri);
            }

            Settings.Default.Save();
        }

        // --- HELPER METHODS ---
        private void SaveLocation(RulerInfo ri)
        {
            Settings.Default["location"] = ri.DisplayedLocation;
            Settings.Default["locationx"] = ri.LocationX;
            Settings.Default["locationy"] = ri.LocationY;
            Settings.Default["vertical"] = ri.IsVertical;
        }

        private void SaveSize(RulerInfo ri)
        {
            Settings.Default["width"] = ri.Width;
            Settings.Default["height"] = ri.Height;
        }

        private void SaveGlobalFlags(RulerInfo ri)
        {
            Settings.Default["opacity"] = ri.Opacity;
            Settings.Default["locked"] = ri.IsLocked;
            Settings.Default["top"] = ri.TopMost;
            Settings.Default["tip"] = ri.ShowToolTip;
        }

        private void ResetAllUserScopeSettingsToDefault()
        {
            foreach (System.Configuration.SettingsProperty setting in Settings.Default.Properties)
            {
                var userScoped = setting.Attributes[typeof(System.Configuration.UserScopedSettingAttribute)]
                                        as System.Configuration.UserScopedSettingAttribute;

                if (userScoped != null)
                {
                    object defaultValue = setting.DefaultValue;
                    Type targetType = setting.PropertyType;

                    try
                    {
                        object convertedValue;

                        // 🛑 NEW CHECK FOR COMPLEX WPF TYPES (Size, Point, Thickness, etc.)
                        if (targetType == typeof(System.Windows.Size) ||
                            targetType == typeof(System.Windows.Point))
                        {
                            // If the default value is a string (which it is for complex types from Settings.Designer.cs)
                            if (defaultValue is string defaultString)
                            {
                                // Use the TypeDescriptor to find and use the correct converter (e.g., SizeConverter)
                                TypeConverter converter = TypeDescriptor.GetConverter(targetType);

                                // Convert the string value to the actual struct type (System.Windows.Size)
                                convertedValue = converter.ConvertFromInvariantString(defaultString);
                            }
                            else
                            {
                                // Fallback if the default is somehow already the correct type
                                convertedValue = defaultValue;
                            }
                        }
                        else
                        {
                            // Original logic for primitive types (double, bool, int, string)
                            convertedValue = Convert.ChangeType(defaultValue, targetType);
                        }

                        // Assign the type-safe value
                        Settings.Default[setting.Name] = convertedValue;
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Failed to reset setting '{setting.Name}'. Error: {ex.Message}");
                        // Handle or log the error
                    }
                }
            }
        }

        public RulerInfo Load()
        {
            RulerInfo ri = new RulerInfo();
            ri.Width = (Settings.Default["width"] == null) ? 400 : (double)Settings.Default["width"];
            ri.Height = (Settings.Default["height"] == null) ? 75 : (double)Settings.Default["height"];
            ri.Opacity = (Settings.Default["opacity"] == null) ? 0.75 : (double)Settings.Default["opacity"];
            ri.DisplayedLocation = (Settings.Default["location"] == null) ? new Point(0, 0) : (Point)Settings.Default["location"];
            ri.IsVertical = (Settings.Default["vertical"] == null) ? false : (bool)Settings.Default["vertical"];
            ri.IsLocked = (Settings.Default["locked"] == null) ? false : (bool)Settings.Default["locked"];
            ri.TopMost = (Settings.Default["top"] == null) ? true : (bool)Settings.Default["top"];
            ri.ShowToolTip = (Settings.Default["tip"] == null) ? true : (bool)Settings.Default["tip"];
            ri.LocationX = ( Settings.Default["locationx"] == null) ? 0 : ((double)Settings.Default["locationx"]);
            ri.LocationY = ( Settings.Default["locationy"] == null) ? 0 : ((double)Settings.Default["locationy"]);
            string savedTypeString = Settings.Default["savetype"] == null ? "none" : (string)Settings.Default["savetype"];

            if (Enum.TryParse<SaveTypes>(savedTypeString, true, out SaveTypes loadedSaveType))
            {
                ri.SaveType = loadedSaveType;
            }
            else
            {
                ri.SaveType = SaveTypes.none;
            }
            Console.WriteLine($"LocationY: {ri.LocationY}");
            return ri;
        }
    }
}
