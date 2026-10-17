using Ruler.Shared.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ruler.Shared.Interfaces
{
    internal interface IMonitorSerializer
    {
      
            string SerializeProfiles(IDictionary<string, MonitorCalibrationProfile> profiles);
            Dictionary<string, MonitorCalibrationProfile> DeserializeProfiles(string json);
        }
    
}
