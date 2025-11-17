using Microsoft.Extensions.DependencyInjection;

using Ruler.Wpf.Common;
using Ruler.Wpf.Models;
using Ruler.Wpf.Services;
using Ruler.Wpf.Services.Persistence;
using Ruler.Wpf.Services.Persistence.Strategy;
using Ruler.Wpf.ViewModels;

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
        // In App.xaml.cs
        public IServiceProvider ServiceProvider { get; private set; }
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            var log = new LoggingService();
            var settingsStraegy = new SettingsPersistenceStrategy(log);
            RulerInfo initialInfo;
            
            if (e.Args.Length>0)
            {
                initialInfo = CommandLineRulerFactory.CovertToRulerInfo(e.Args);
            }
            else
            {
                initialInfo = settingsStraegy.Load();
            }



            ServiceCollection serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton(initialInfo);
            ConfigureServices(serviceCollection);
           ServiceProvider = serviceCollection.BuildServiceProvider();

           
            ExecuteStartupLogic();
        }
        private void ConfigureServices(IServiceCollection services)
        {
            
            services.AddSingleton<ILoggingService, LoggingService>();
            services.AddSingleton<SettingsPersistenceStrategy>();
            services.AddSingleton<SingleRulerPersistenceService>();
            services.AddSingleton<IDialogService, DialogService>();           
            services.AddTransient<RulerViewModel>();
            services.AddTransient<MainWindow>();
        }
        private void ExecuteStartupLogic()
        {
            var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
            var dialogService = ServiceProvider.GetRequiredService<IDialogService>();
            var persistenceService = ServiceProvider.GetRequiredService<SingleRulerPersistenceService>();
            RulerInfo initialInfo = persistenceService.LoadRulerState();
            dialogService.AddRuler(mainWindow);            
            mainWindow.Show();
        }

    }
}

