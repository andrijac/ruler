using Ruler.Shared.Interfaces;
using Ruler.Shared.Models;
using Ruler.Shared.Services;

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ruler.Wpf.Services
{
    public class MonitorPersistenceService : IMonitorPersistenceService
    {
        private const string SettingKey = "MonitorData";
     
        private readonly MonitorSerializer _settingsService;

        public MonitorPersistenceService(
            MonitorSerializer settings)
        {
            _settingsService = settings;
        }

        public Dictionary<string, MonitorCalibrationProfile> LoadProfiles()
        {
            var json = ConfigurationManager.AppSettings[SettingKey];
            return _settingsService.DeserializeProfiles(json);
        }

        public void SaveProfiles(Dictionary<string, MonitorCalibrationProfile> profiles)
        {
            var json = _settingsService.SerializeProfiles(profiles);
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            if (config.AppSettings.Settings[SettingKey] == null)
                config.AppSettings.Settings.Add(SettingKey, json);
            else
                config.AppSettings.Settings[SettingKey].Value = json;

            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
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
