using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Ruler.Shared.Models;
using Ruler.Wpf.Views;
using Ruler.Shared.Services;
using Ruler.Wpf.ViewModels;
using Ruler.Shared.Factories;

namespace Ruler.Wpf.Services
{
    public class WindowManager
    {
        private static readonly List<RulerWindow> ActiveWindows = new List<RulerWindow>();

        // Master session cache - guarantees closed windows are remembered for the final SaveType check
        private static readonly List<RulerInfo> MasterRulerCache = new List<RulerInfo>();

        private readonly RulerInfoPreprocessor _preprocessor = new RulerInfoPreprocessor();

        /// <summary>
        /// Populates the master session cache and creates UI frames for initial records.
        /// </summary>
        public void InitializeWorkspace(IEnumerable<RulerInfo> loadedRulers)
        {
            if (loadedRulers == null) return;

            foreach (var ruler in loadedRulers)
            {
                if (!MasterRulerCache.Contains(ruler))
                {
                    MasterRulerCache.Add(ruler);
                }
                CreateRulerWindow(ruler);
            }
        }

        public void CreateRulerWindow(RulerInfo model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));

            if (!MasterRulerCache.Contains(model))
            {
                MasterRulerCache.Add(model);
            }

           
            var viewModel = new RulerViewModel(model, this);
            var window = new RulerWindow(viewModel);
            window.DataContext = viewModel;

            ActiveWindows.Add(window);
            window.Closed += (sender, args) => HandleWindowClosed(window);

            window.Show();
        }

        public void DuplicateRuler(RulerInfo sourceModel)
        {
            if (sourceModel == null) return;

            RulerInfo duplicateModel = RulerFactory.CreateDefault();
            RulerFactory.CopyValues(sourceModel, duplicateModel);
            duplicateModel.Left += 20;
            duplicateModel.Top += 20;

            CreateRulerWindow(duplicateModel);
        }

        public void CloseRuler(RulerViewModel viewModel)
        {
            if (viewModel == null) return;
            var targetWindow = ActiveWindows.FirstOrDefault(w => w.DataContext == viewModel);
            targetWindow?.Close();
        }

        private void HandleWindowClosed(RulerWindow window)
        {
            ActiveWindows.Remove(window);

            // Notice: The model stays inside MasterRulerCache here so its settings are safely
            // preserved at shutdown, respecting whatever SaveType was assigned to it.

            if (ActiveWindows.Count == 0 && Application.Current != null)
            {
                SaveAllActiveRulers();
                Application.Current.Shutdown();
            }
        }

        public void ClearAndResetWorkspace()
        {
            var windowsToClose = ActiveWindows.ToList();

            MasterRulerCache.Clear();
            Ruler.Wpf.Properties.Settings.Default.RulerCollection = string.Empty;
            Ruler.Wpf.Properties.Settings.Default.Save();

            // Spawn clean fallback ruler state
            CreateRulerWindow(RulerFactory.CreateDefault());

            foreach (var oldWindow in windowsToClose)
            {
                oldWindow.Close();
            }
        }

        public void SaveAllActiveRulers()
        {
            // Serialize and process against our full runtime Master Cache list
            var processedModels = _preprocessor.Preprocess(MasterRulerCache);
            string jsonString = SettingsService.SerializeRulers(processedModels);

            Ruler.Wpf.Properties.Settings.Default.RulerCollection = jsonString;
            Ruler.Wpf.Properties.Settings.Default.Save();
        }
    }
}
