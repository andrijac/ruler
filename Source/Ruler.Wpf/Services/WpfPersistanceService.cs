using Newtonsoft.Json;

using Ruler.Shared.Interfaces;
using Ruler.Shared.Models;

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ruler.Wpf.Services
{
    public class WpfPersistenceService : IPersistanceService
    {
        private const string SettingKey = "RulerData";
        private readonly IRulerInfoPreprocessor _processor;
        private readonly IRulerSerializer _settingsService;
        private readonly IRulerRegistry _registry;

        public WpfPersistenceService(
            IRulerInfoPreprocessor rulerInfoPreprocessor,
            IRulerSerializer settings,
            IRulerRegistry registry)
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
            if (_registry.RulerExists(info.ID))
            {
                _registry.UpdateRulerByID(info);
            }
            SaveAll(_registry.GetActiveRulers().Select(r => r.RulerData).ToList());
        }

        public void Reset()
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings.Clear();
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
        }
    }
}
