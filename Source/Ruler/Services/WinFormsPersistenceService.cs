using Newtonsoft.Json;

using Ruler.Shared.Interfaces;
using Ruler.Shared.Models;
using Ruler.Shared.Services;

using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;

namespace Ruler.Services
{
    public class WinformsPersistanceService :IPersistanceService
    {
        private const string SettingKey = "RulerData";
        private readonly IRulerInfoPreprocessor _processor;
        private readonly IRulerSerializer _settingsService;
        private readonly IRulerRegistry _registry;
        public WinformsPersistanceService(IRulerInfoPreprocessor rulerInfoPreprocessor, IRulerSerializer settings,IRulerRegistry registry)
        {
            _processor = rulerInfoPreprocessor; 
            _settingsService = settings;
            _registry = registry;
        }
        public List<RulerInfo> LoadAll()
        {
            var json = ConfigurationManager.AppSettings[SettingKey];
           return _settingsService.DeserializeRulers(json);
        }

        public void SaveAll(IEnumerable<RulerInfo> rulers)
        {
            var rulersToSave = _processor.Preprocess(rulers);
            var json = _settingsService.SerializeRulers(rulersToSave);
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            if (config.AppSettings.Settings[SettingKey] == null)
                config.AppSettings.Settings.Add(SettingKey, json);
            else
                config.AppSettings.Settings[SettingKey].Value = json;

            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
        }

        public void Update(RulerInfo info)
        {
            if(_registry.RulerExists(info.ID))
            {
                _registry.UpdateRulerByID(info);
            }
            SaveAll((_registry.GetActiveRulers()).Select(r=>r.RulerData).ToList());
        }
        public void Reset()
        {
            // 1. Open the configuration file for the current exe
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            // 2. Clear the app settings
            config.AppSettings.Settings.Clear();

            // 3. Save the changes to the config file
            config.Save(ConfigurationSaveMode.Modified);

            // 4. IMPORTANT: Refresh the section so the application 
            // realizes the settings have changed without needing a restart
            ConfigurationManager.RefreshSection("appSettings");
        }
    }
}
