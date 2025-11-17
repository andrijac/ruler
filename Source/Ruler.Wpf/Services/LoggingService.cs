using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ruler.Wpf.Services
{
    public class LoggingService : ILoggingService
    {
        public void LogInfo(string message) => System.Diagnostics.Debug.WriteLine($"INFO: {message}");
        public void LogError(string message, Exception ex) => System.Diagnostics.Debug.WriteLine($"ERROR: {message}\nException: {ex.Message}");
        public void LogDebug(string message) => System.Diagnostics.Debug.WriteLine($"DEBUG: {message}");
        public void LogWarning(string message) => System.Diagnostics.Debug.WriteLine($"WARNING: {message}");
    }
}
