using Ruler.Shared.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ruler.Shared.Interfaces
{
    public interface IMonitorRegistry
    {
        /// <summary>
        /// Retrieves an existing profile for the specified device name, or creates a new one if it doesn't exist.
        /// </summary>
        MonitorCalibrationProfile GetOrCreateProfile(string deviceName);

        /// <summary>
        /// Saves all active profiles using the underlying serializer.
        /// </summary>
        void Save();

        /// <summary>
        /// Loads profiles from storage.
        /// </summary>
        void Load();
        void UpdateProfile(string deviceName, double? newMultiplier = null, int? newWidth = null, int? newHeight = null);
        /// <summary>
        /// Gets a read-only view of all currently tracked monitor profiles.
        /// </summary>
        IReadOnlyDictionary<string, MonitorCalibrationProfile> GetAllProfiles();
    }
}
