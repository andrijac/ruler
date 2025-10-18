using Ruler.Wpf.Common;
using Ruler.Wpf.Models;
using Ruler.Wpf.Persistence.Strategy;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ruler.Wpf.Persistence
{
    public class SingleRulerPersistenceService
    {
        private readonly Dictionary<SaveTypes, IPersistenceStrategy> _strategies;

        public SingleRulerPersistenceService()
        {
            // Maps ALL SaveTypes to the single SettingsPersistenceStrategy instance.
            IPersistenceStrategy settingsStrategy = new SettingsPersistenceStrategy();

            _strategies = new Dictionary<SaveTypes, IPersistenceStrategy>
            {
                { SaveTypes.none, settingsStrategy },
                { SaveTypes.all, settingsStrategy },
                { SaveTypes.location, settingsStrategy },
                { SaveTypes.size, settingsStrategy }
            };
        }

        public void SaveRulerState(RulerInfo rulerInfo)
        {
            if (_strategies.TryGetValue(rulerInfo.SaveType, out IPersistenceStrategy strategy))
            {
                // Executes the selected strategy's Save method.
                strategy.Save(rulerInfo);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"Warning: No strategy found for SaveType: {rulerInfo.SaveType}");
            }
        }
        public RulerInfo LoadRulerState()
        {
            // For simplicity, just load using the SettingsStrategy, 
            // as it's the only one that defines loading current state.
            // NOTE: You must get an instance of the strategy here.
            return new SettingsPersistenceStrategy().Load();
        }
    }
}
