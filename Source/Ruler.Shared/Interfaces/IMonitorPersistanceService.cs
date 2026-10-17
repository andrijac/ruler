using Ruler.Shared.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ruler.Shared.Interfaces
{
    public interface IMonitorPersistenceService
    {
        Dictionary<string, MonitorCalibrationProfile> LoadProfiles();
        void SaveProfiles(Dictionary<string, MonitorCalibrationProfile> profiles);
        void Reset();
    }
}
