using Newtonsoft.Json;

using Ruler.Shared.Factories;
using Ruler.Shared.Models;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ruler.Shared.Services
{
    /// <summary>
    /// Logic for converting Ruler data to and from string formats for persistence.
    /// This keeps the UI project from needing to know the details of serialization.
    /// </summary>
    public static class SettingsService
    {
        // Formatting.None keeps the string as small as possible for exe.config
        private static readonly JsonSerializerSettings _settings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented
        };

        /// <summary>
        /// Serializes a collection of RulerInfo objects into a single JSON string.
        /// </summary>
        public static string SerializeRulers(IEnumerable<RulerInfo> rulers)
        {
            if (rulers == null) return string.Empty;

            try
            {
               
                return JsonConvert.SerializeObject(rulers, _settings);
            }
            catch (Exception ex)
            {
                // In a real app, log this error to a file or console
              
                return string.Empty;
            }
        }

        /// <summary>
        /// Deserializes a JSON string back into a list of RulerInfo objects.
        /// If the string is empty or invalid, it returns a list containing one default ruler.
        /// </summary>
        public static List<RulerInfo> DeserializeRulers(string json)
        {
            
            if (string.IsNullOrWhiteSpace(json))
            {
                return new List<RulerInfo> { RulerFactory.CreateDefault() };
            }

            try
            {
                var result = JsonConvert.DeserializeObject<List<RulerInfo>>(json, _settings);
                return result ?? new List<RulerInfo> { RulerFactory.CreateDefault() };
            }
            catch (JsonException ex)
            {
                // Return default state so the user doesn't open an empty app on corruption
                return new List<RulerInfo> { RulerFactory.CreateDefault() };
            }
        }
    }
}
