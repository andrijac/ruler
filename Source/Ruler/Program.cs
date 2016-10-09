using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Ruler
{
    static class Program
    {
        static Mutex mutex = new Mutex(true, "{169e70a6-fc5c-4724-acee-8f1cf38f07ba}");

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            if (mutex.WaitOne(TimeSpan.Zero, true))
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                Application.Run(new RulerApplicationContext());
                mutex.ReleaseMutex();
            }
            else
            {
                MessageBox.Show("Ruler is already running", "Error");
            }
        }
    }
}
