using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Ruler.Wpf.Common
{
    internal static class NativeMethods
    {
        // Constants for the Window Message (WM_SYSCOMMAND)
        public const int WM_SYSCOMMAND = 0x0112;
        public const int SC_SIZE = 0xF000;

        // Constants for the native HitTest codes (LPARAM)
        public const int HTLEFT = 10;
        public const int HTRIGHT = 11;
        public const int HTTOP = 12;
        public const int HTTOPLEFT = 13;
        public const int HTTOPRIGHT = 14;
        public const int HTBOTTOM = 15;
        public const int HTBOTTOMLEFT = 16;
        public const int HTBOTTOMRIGHT = 17;

        // P/Invoke Declaration for sending messages to a window
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);
    }
}
