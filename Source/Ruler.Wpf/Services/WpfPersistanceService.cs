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
    public class WpfPersistanceService : IPersistanceService
    {
        private const string SettingKey = "RulerData";

        public List<RulerInfo> LoadAll()
        {
            var json = ConfigurationManager.AppSettings[SettingKey];
            if (string.IsNullOrWhiteSpace(json))
            {
                // Note: If you need RulerFactory here, you can inject it via constructor
                return new List<RulerInfo>();
            }
            try
            {
                var rulers = JsonConvert.DeserializeObject<List<RulerInfo>>(json);
                return rulers ?? new List<RulerInfo>();
            }
            catch (JsonException ex)
            {
                Debug.WriteLine(ex);
                return new List<RulerInfo>();
            }
        }

        public void SaveAll(IEnumerable<RulerInfo> rulers)
        {
            var json = JsonConvert.SerializeObject(rulers);
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
            var currentRulers = LoadAll();
            var index = currentRulers.FindIndex(r => r.ID == info.ID);

            if (index != -1) currentRulers[index] = info;
            else currentRulers.Add(info);

            SaveAll(currentRulers);
        }
    }
}
