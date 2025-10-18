using Ruler.Wpf.Common;
using Ruler.Wpf.Models;
using Ruler.Wpf.Persistence;
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

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            SingleRulerPersistenceService persistenceService = new SingleRulerPersistenceService();
            DialogService dialogService = new DialogService(persistenceService);
            RulerInfo initialInfo = persistenceService.LoadRulerState();
          MainWindow mainWindow = new MainWindow();
            RulerViewModel viewModel;
            viewModel = new RulerViewModel(dialogService,initialInfo,persistenceService);
            dialogService.ShowNewRuler(initialInfo);
        }
    }
}
