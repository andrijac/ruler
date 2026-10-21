using System;
using System.Windows;
using System.Windows.Interop;

namespace Ruler.Wpf.Services
{
    public static class MonitorTrackerService
    {
        public static (string DeviceName, int Width, int Height) GetCurrentMonitorInfo(Window window)
        {
            var helper = new WindowInteropHelper(window);
            IntPtr hwnd = helper.Handle;

            if (hwnd == IntPtr.Zero)
            {  return (@"\\.\DISPLAY1", (int)SystemParameters.PrimaryScreenWidth, (int)SystemParameters.PrimaryScreenHeight);
              
            }

            // Calls the imports straight from NativeHelpers
            IntPtr hMonitor = NativeHelpers.MonitorFromWindow(hwnd, NativeHelpers.MONITOR_DEFAULTTONEAREST);
            var mi = new NativeHelpers.MONITORINFOEX();
            mi.cbSize = System.Runtime.InteropServices.Marshal.SizeOf(typeof(NativeHelpers.MONITORINFOEX));

            if (NativeHelpers.GetMonitorInfo(hMonitor, ref mi))
            {
                string deviceName = mi.szDevice.Trim();
                int width = mi.rcMonitor.Right - mi.rcMonitor.Left;
                int height = mi.rcMonitor.Bottom - mi.rcMonitor.Top;
                return (deviceName, width, height);
            }

            return (@"\\.\DISPLAY1", (int)SystemParameters.PrimaryScreenWidth, (int)SystemParameters.PrimaryScreenHeight);
        }
    }
}

