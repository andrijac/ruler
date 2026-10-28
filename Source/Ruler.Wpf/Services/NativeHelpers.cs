using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Ruler.Wpf.Services
{
    public static class NativeHelpers
    {
        // Window Styles & Constants
        public const int GWL_EXSTYLE = -20;
        public const int WS_EX_TRANSPARENT = 0x00000020;
        public const int WS_EX_LAYERED = 0x00080000;
        public const int WS_CHILD = 0x40000000;
        public const int WS_VISIBLE = 0x10000000;

        // Window Message & Sizing Constants
        public const int WM_SYSCOMMAND = 0x0112;
        public const int SC_SIZE = 0xF000;
        public const int WM_SIZING = 0x0214;
        public const int WM_DISPLAYCHANGE = 0x007E;
        public const int WM_DPICHANGED = 0x02E0;

        // Monitor Tracking Constants
        public const uint MONITOR_DEFAULTTONEAREST = 0x00000002;
        private const int CCHDEVICENAME = 32;

        // Structs
        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MAGTRANSFORM
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 9)]
            public float[] v;
        }
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ClientToScreen(IntPtr hWnd, ref POINT lpPoint);
        [DllImport("gdi32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeleteObject(IntPtr hObject);
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct MONITORINFOEX
        {
            public int cbSize;
            public RECT rcMonitor;
            public RECT rcWork;
            public uint dwFlags;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCHDEVICENAME)]
            public string szDevice;
        }

        // P/Invoke Imports: Window Management & Styles
        [DllImport("user32.dll", SetLastError = true)]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetCursorPos(out POINT lpPoint);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern IntPtr CreateWindowEx(
            int exStyle, string className, string windowName, int style,
            int x, int y, int width, int height, IntPtr hwndParent,
            IntPtr menu, IntPtr hInstance, IntPtr param);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern int SetWindowRgn(IntPtr hWnd, IntPtr hRgn, bool bRedraw);

        // P/Invoke Imports: Magnification API
        [DllImport("magnification.dll", SetLastError = true)]
        public static extern bool MagSetWindowSource(IntPtr hwnd, RECT rc);

        [DllImport("magnification.dll", SetLastError = true)]
        public static extern bool MagSetWindowTransform(IntPtr hwnd, ref MAGTRANSFORM pTransform);

        [DllImport("magnification.dll", SetLastError = true)]
        public static extern bool MagInitialize();

        [DllImport("magnification.dll", SetLastError = true)]
        public static extern bool MagUninitialize();

        [DllImport("magnification.dll", SetLastError = true, EntryPoint = "MagSetLensUseBitmapSmoothing")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool MagSetLensUseBitmapSmoothing(IntPtr hwnd, [MarshalAs(UnmanagedType.Bool)] bool bUseSmoothing);

        // P/Invoke Imports: GDI
        [DllImport("gdi32.dll", SetLastError = true)]
        public static extern IntPtr CreateEllipticRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect);

        // P/Invoke Imports: Monitor Tracking
        [DllImport("user32.dll")]
        public static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint dwFlags);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFOEX lpmi);

        // --- Encapsulated Helper Methods ---

        /// <summary>
        /// Queries the Win32 API to retrieve the device name, width, and height of the monitor 
        /// where the specified window handle is currently located.
        /// </summary>
        public static (string DeviceName, int Width, int Height) GetMonitorInfoFromWindow(IntPtr hwnd)
        {
            if (hwnd == IntPtr.Zero)
            {
                return (@"\\.\DISPLAY1", (int)SystemParameters.PrimaryScreenWidth, (int)SystemParameters.PrimaryScreenHeight);
            }

            IntPtr hMonitor = MonitorFromWindow(hwnd, MONITOR_DEFAULTTONEAREST);
            var mi = new MONITORINFOEX();
            mi.cbSize = Marshal.SizeOf(typeof(MONITORINFOEX));

            if (GetMonitorInfo(hMonitor, ref mi))
            {
                string deviceName = mi.szDevice.Trim();
                int width = mi.rcMonitor.Right - mi.rcMonitor.Left;
                int height = mi.rcMonitor.Bottom - mi.rcMonitor.Top;
                return (deviceName, width, height);
            }

            return (@"\\.\DISPLAY1", (int)SystemParameters.PrimaryScreenWidth, (int)SystemParameters.PrimaryScreenHeight);
        }

        /// <summary>
        /// Determines if a window sizing/resizing command should be blocked due to lock constraints.
        /// </summary>
        public static bool ShouldBlockSizing(int msg, IntPtr wParam, bool isLocked)
        {
            if (!isLocked) return false;
            return msg == WM_SIZING || (msg == WM_SYSCOMMAND && (wParam.ToInt32() & 0xFFF0) == SC_SIZE);
        }

        /// <summary>
        /// Determines if a window message indicates a display resolution or DPI shift.
        /// </summary>
        public static bool IsDisplayOrDpiChange(int msg)
        {
            return msg == WM_DISPLAYCHANGE || msg == WM_DPICHANGED;
        }
    }
}
