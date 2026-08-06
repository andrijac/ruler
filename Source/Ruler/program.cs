using Ruler.Forms;
using Ruler.Shared.Factories;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            
            RulerApplicationContext context = new RulerApplicationContext();

            // Pass the arguments to the Context's startup method
            context.Start(args);
        }
    }
}
