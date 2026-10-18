using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Ruler.Wpf.Windows
{
    public static class NativeHelpers
    {
        // Window Styles & Constants
        public const int GWL_EXSTYLE = -20;
        public const int WS_EX_TRANSPARENT = 0x00000020;
        public const int WS_EX_LAYERED = 0x00080000;
        public const int WS_CHILD = 0x40000000;
        public const int WS_VISIBLE = 0x10000000;

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

        // P/Invoke Imports
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

        [DllImport("magnification.dll", SetLastError = true)]
        public static extern bool MagSetWindowSource(IntPtr hwnd, RECT rc);

        [DllImport("magnification.dll", SetLastError = true)]
        public static extern bool MagSetWindowTransform(IntPtr hwnd, ref MAGTRANSFORM pTransform);
        [DllImport("magnification.dll", SetLastError = true)]
        public static extern bool MagInitialize();

        [DllImport("magnification.dll", SetLastError = true)]
        public static extern bool MagUninitialize();
        [DllImport("gdi32.dll", SetLastError = true)]
        public static extern IntPtr CreateEllipticRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern int SetWindowRgn(IntPtr hWnd, IntPtr hRgn, bool bRedraw);
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);
        [DllImport("magnification.dll", SetLastError = true, EntryPoint = "MagSetLensUseBitmapSmoothing")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool MagSetLensUseBitmapSmoothing(IntPtr hwnd, [MarshalAs(UnmanagedType.Bool)] bool bUseSmoothing);
        // Monitor Tracking Constants
        public const uint MONITOR_DEFAULTTONEAREST = 0x00000002;
        private const int CCHDEVICENAME = 32;

        // Monitor Tracking Structs (reusing your existing RECT struct)
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

        // Monitor Tracking P/Invoke Imports
        [DllImport("user32.dll")]
        public static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint dwFlags);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFOEX lpmi);
    }
}
