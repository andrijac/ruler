using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Ruler.Shared.Interfaces;
using Ruler.Shared.Models;

namespace Ruler.Shared.Services
{
    public class MonitorRegistry : IMonitorRegistry
    {
        private readonly Dictionary<string, MonitorCalibrationProfile> _profiles = new Dictionary<string, MonitorCalibrationProfile>();
        private readonly IMonitorPersistenceService _persistenceService;
        private readonly object _lock = new object();

        public MonitorRegistry(IMonitorPersistenceService persistenceService)
        {
            _persistenceService = persistenceService;
            Load();
        }

        public void Load()
        {
            lock (_lock)
            {
                var loaded = _persistenceService.LoadProfiles();
                _profiles.Clear();

                if (loaded != null)
                {
                    foreach (var kvp in loaded)
                    {
                        _profiles[kvp.Key] = kvp.Value;
                    }
                }
            }
        }
        public void UpdateProfile(string deviceName, double? newMultiplier = null, int? newWidth = null, int? newHeight = null)
        {
            lock (_lock)
            {
                var profile = GetOrCreateProfile(deviceName);
                profile.Update(newMultiplier, newWidth, newHeight); // This will also trigger Save() automatically via the profile
            }
        }
        public void Save()
        {
            lock (_lock)
            {
                _persistenceService.SaveProfiles(_profiles);
            }
        }

        public MonitorCalibrationProfile GetOrCreateProfile(string deviceName)
        {
            if (string.IsNullOrEmpty(deviceName))
                deviceName = "DEFAULT_DISPLAY";

            lock (_lock)
            {
                if (!_profiles.TryGetValue(deviceName, out var profile))
                {
                    // If this is a brand new monitor, create a default profile
                    profile = new MonitorCalibrationProfile { DeviceName = deviceName };
                    _profiles[deviceName] = profile;

                    // Immediately persist the new profile
                    Save();
                }

                return profile;
            }
        }

        public IReadOnlyDictionary<string, MonitorCalibrationProfile> GetAllProfiles()
        {
            lock (_lock)
            {
                // Return a thread-safe snapshot copy
                return new Dictionary<string, MonitorCalibrationProfile>(_profiles);
            }
        }
    }
}
