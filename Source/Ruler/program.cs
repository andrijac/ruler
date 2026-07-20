using Microsoft.Extensions.DependencyInjection;
using Ruler.Factories;
using Ruler.Forms;
using Ruler.Services;
using Ruler.Shared.Factories;
using Ruler.Shared.Interfaces;
using Ruler.Shared.Services;
using System;
using System.Configuration;
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

            var services = new ServiceCollection();

            // Call your separate method here
            ConfigureServices(services);

            using (var serviceProvider = services.BuildServiceProvider())
            {
                var context = serviceProvider.GetRequiredService<RulerApplicationContext>();
                context.Initialize(args);
                Application.Run(context);
            }


        }
        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IRulerSerializer,RulerSerializer>();
            services.AddSingleton<IRulerInfoPreprocessor, RulerInfoPreprocessor>();
            services.AddSingleton<IRulerRegistry, RulerRegistry>();
            services.AddSingleton<IPersistanceService, WinformsPersistanceService>();
            services.AddTransient<IRulerFactory, RulerFactory>();
            services.AddTransient<IMainFormFactory, MainFormFactory>();
            services.AddSingleton<RulerApplicationContext>();
            services.AddTransient<MainForm>();
        }
    }
}
