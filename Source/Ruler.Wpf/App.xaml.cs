using Newtonsoft.Json;

using Ruler.Shared.Factories;
using Ruler.Shared.Models;
using Ruler.Wpf.Properties;
using Ruler.Wpf.Services;
using Ruler.Wpf.ViewModels;
using Ruler.Wpf.Views;

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
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            List<RulerInfo> rulerInfos = null;

            // 1. Pull raw JSON string from settings
            string jsonSettings = Settings.Default.RulerCollection;

            // 2. Deserialize into data contracts if string exists
            if (!string.IsNullOrWhiteSpace(jsonSettings))
            {
                try
                {
                    rulerInfos = JsonConvert.DeserializeObject<List<RulerInfo>>(jsonSettings);
                }
                catch (JsonException)
                {
                    rulerInfos = null; // Safety fallback on corruption
                }
            }

            // 3. Fallback to factory if list is null or empty
            if (rulerInfos == null || rulerInfos.Count == 0)
            {
                // Assuming factory can generate a default info contract or view model
                RulerInfo defaultInfo = RulerFactory.CreateDefault();
                rulerInfos = new List<RulerInfo> { defaultInfo };
            }

            // //Current.Dispatcher.InvokeAsync(() =>
            // //{
            // //    var windowManager = new WindowManager();
            // //    windowManager.InitializeWorkspace(rulerInfos);
            // //}, System.Windows.Threading.DispatcherPriority.ContextIdle);
            var windowManager = new WindowManager();

            //  5.Hand off the active ViewModels to the Window Manager

            windowManager.InitializeWorkspace(rulerInfos);
            //RulerInfo info = RulerFactory.CreateDefault();
            //var mainViewModel = new RulerViewModel(info);
            //RulerWindow window = new RulerWindow
            //{
            //    DataContext = mainViewModel
            //};
            //window.Show();
        }
    }
}
