using Newtonsoft.Json;

using Ruler.Shared.Interfaces;
using Ruler.Shared.Models;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ruler.Shared.Services
{
    // <summary>
    /// Logic for converting monitor calibration profiles to and from JSON strings.
    /// Keeps persistence and UI layers decoupled from raw JSON mechanics.
    /// </summary>
    public class MonitorSerializer : IMonitorSerializer
    {
        public string SerializeProfiles(IDictionary<string, MonitorCalibrationProfile> profiles)
        {
            return JsonConvert.SerializeObject(profiles, Formatting.Indented);
        }

        public Dictionary<string, MonitorCalibrationProfile> DeserializeProfiles(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return new Dictionary<string, MonitorCalibrationProfile>();
            }
            try
            {
                var profiles = JsonConvert.DeserializeObject<Dictionary<string, MonitorCalibrationProfile>>(json);
                return profiles ?? new Dictionary<string, MonitorCalibrationProfile>();
            }
            catch (JsonException ex)
            {
                Debug.WriteLine($"Error deserializing monitor profiles: {ex.Message}");
                return new Dictionary<string, MonitorCalibrationProfile>();
            }
        }
        public void ClearAllMonitorProfiles()
        {
            Dictionary<string, MonitorCalibrationProfile> profiles = new Dictionary<string, MonitorCalibrationProfile>();
            SerializeProfiles(profiles);
        }
    }
}
