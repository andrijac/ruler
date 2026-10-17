using Microsoft.Extensions.DependencyInjection;

using Newtonsoft.Json;

using Ruler.Shared.Factories;
using Ruler.Shared.Interfaces;
using Ruler.Shared.Models;
using Ruler.Shared.Services;
using Ruler.Wpf.Controllers;
using Ruler.Wpf.Factories;
using Ruler.Wpf.Properties;
using Ruler.Wpf.Services;
using Ruler.Wpf.Windows;

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Ruler.Wpf
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static IServiceProvider ServiceProvider { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            NativeHelpers.MagInitialize();
            var services = new ServiceCollection();
            ConfigureServices(services);
            ServiceProvider = services.BuildServiceProvider();

            // Resolve the AppController (which replaced RulerApplicationContext)
            var controller = ServiceProvider.GetRequiredService<AppController>();

            // Hand off the command line arguments and start the logic
            controller.Run(e.Args);
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            // 1. Shared Infrastructure
            services.AddSingleton<IRulerSerializer, RulerSerializer>();
            services.AddSingleton<IRulerInfoPreprocessor, RulerInfoPreprocessor>();
            services.AddSingleton<IRulerRegistry, RulerRegistry>();
            services.AddSingleton<MonitorSerializer>();

            // 2. WPF-specific Services (Converted from WinForms versions)
            services.AddSingleton<IRulerPersistanceService, WpfRulerPersistenceService>();
            services.AddSingleton<IMonitorPersistenceService, MonitorPersistenceService>();

            // 3. Factories (Converted for WPF)
            services.AddTransient<IRulerFactory, RulerFactory>();
            services.AddSingleton<IMainFormFactory, RulerWindowFactory>();

            // 3.5 . Monitor Registry
            services.AddSingleton<IMonitorRegistry, MonitorRegistry>();
            // 4. Controller & UI
            services.AddSingleton<AppController>();
            services.AddTransient<MainWindow>();
        }
        protected override void OnExit(ExitEventArgs e)
        {
            NativeHelpers.MagUninitialize();
            base.OnExit(e);
        }

    }
}
