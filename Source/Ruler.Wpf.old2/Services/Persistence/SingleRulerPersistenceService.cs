using Ruler.Wpf.Common;
using Ruler.Wpf.Models;
using Ruler.Wpf.Services.Persistence.Strategy;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ruler.Wpf.Services.Persistence
{
    public class SingleRulerPersistenceService
    {
        private readonly Dictionary<SaveTypes, IPersistenceStrategy> _strategies;
        private readonly ILoggingService _loggingService;

        public SingleRulerPersistenceService(ILoggingService loggingService )
        {
            _loggingService = loggingService;
            // Maps ALL SaveTypes to the single SettingsPersistenceStrategy instance.
            IPersistenceStrategy settingsStrategy = new SettingsPersistenceStrategy(_loggingService);

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
            if (_strategies.TryGetValue(SaveTypes.none, out IPersistenceStrategy strategy))
            {
                return strategy.Load();
            }
            // Fallback or throw an error if the required strategy isn't found
            throw new InvalidOperationException("Default persistence strategy not found.");
        }
    }
}
