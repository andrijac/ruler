using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Interop;

namespace Ruler.Wpf.Windows
{
    public class MagnifierHost : HwndHost
    {
        private IntPtr _hwndMagnifier;
        private readonly int _width;
        private readonly int _height;

        public MagnifierHost(int width, int height)
        {
            _width = width;
            _height = height;
        }

        public IntPtr MagnifierHandle => _hwndMagnifier;

        protected override HandleRef BuildWindowCore(HandleRef hwndParent)
        {
            _hwndMagnifier = NativeHelpers.CreateWindowEx(
                0,
                "Magnifier",
                "",
                NativeHelpers.WS_CHILD | NativeHelpers.WS_VISIBLE,
                0, 0, _width, _height,
                hwndParent.Handle,
                IntPtr.Zero,
                IntPtr.Zero,
                IntPtr.Zero
            );

            return new HandleRef(this, _hwndMagnifier);
        }

        protected override void DestroyWindowCore(HandleRef hwnd) { }
    }
}
