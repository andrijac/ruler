using Ruler.Forms;
using Ruler.Shared.Factories;
using Ruler.Shared.Models;
using Ruler.Shared.Services;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Ruler
{
    public static class PersistenceManager
    {
        private static readonly RulerInfoPreprocessor _preprocessor = new RulerInfoPreprocessor();
        private static List<RulerInfo> currentrulers;
        

        /// <summary>
        /// Gathers all open MainForm instances, cleans their data via the Preprocessor,
        /// and saves the resulting JSON string to the User Settings (exe.config).
        /// </summary>
        public static void SaveAll(IEnumerable<Form> savingrulers)
        {
            try
            {
                // 1. Collect RulerInfo from all open MainForms
                // We use .ToList() to snap the current state of open windows
                if (savingrulers == null) { return; }

                var saverulers = savingrulers
                    .OfType<MainForm>()
                    .Select(f => f.RulerData) 
                    .ToList();

                // 2. Run the data through your Preprocessor (stripping based on SaveType)
                IEnumerable<RulerInfo> processedData = _preprocessor.Preprocess(saverulers);

                // 3. Serialize to JSON using your Library's SettingsService (Newtonsoft)
                string json = SettingsService.SerializeRulers(processedData);
                // 4. Save to the actual exe.config
              Properties.Settings.Default.RulerCollection = json;
              Properties.Settings.Default.Save();
            }
            catch (Exception ex)
            {
                // Log the error or handle it gracefully so the app can still close
                Console.WriteLine($"Failed to save settings: {ex.Message}");
            }
        }

        /// <summary>
        /// Reads the JSON string from exe.config, converts it back to RulerInfo objects,
        /// and spawns the necessary MainForm instances.
        /// </summary>
        public static IEnumerable<RulerInfo> LoadAll()
        {
            try
            {
                // 1. Pull the raw string from the config file
                string json = Properties.Settings.Default.RulerCollection;

                // 2. Deserialize using the Library Service
                // Note: DeserializeRulers should return a default ruler if the string is empty
                currentrulers = SettingsService.DeserializeRulers(json);

                // 3. If no saved rulers exist, create one default ruler to start with
                if (currentrulers == null || !currentrulers.Any())
                {
                   currentrulers = new List<RulerInfo> { RulerFactory.CreateDefault() };
                   
                }
                return currentrulers;
            }
            catch (Exception ex)
            {
                // Fallback: If loading fails, open at least one default ruler
                Console.WriteLine($"Failed to load settings: {ex.Message}");
               return new List<RulerInfo> { RulerFactory.CreateDefault() };
            }
        }
    }
}
