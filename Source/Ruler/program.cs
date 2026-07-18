using System;
using System.Windows.Forms;

namespace Ruler
{
    internal static class Program
    {
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool SetProcessDPIAware();
        [STAThread]
        static void Main(string[] args)
        {
            SetProcessDPIAware();
            // OR, if you want it to behave like "old" apps:
            // Application.SetHighDpiMode(HighDpiMode.DpiUnaware);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
           

          
        }
    }
}
