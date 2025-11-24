using Microsoft.Extensions.DependencyInjection;

using Ruler.Wpf.Models;
using Ruler.Wpf.Services.Persistence;
using Ruler.Wpf.ViewModels;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Ruler.Wpf.Services
{
    /// <summary>
    /// An implementation of IDialogService that shows the SetSizeWindow.
    /// </summary>
    public class DialogService : IDialogService
    {
        private readonly SingleRulerPersistenceService _persistenceService;
        private readonly ILoggingService _loggingService;
        private readonly IServiceProvider _serviceProvider;
        public DialogService(SingleRulerPersistenceService persistenceService, ILoggingService loggingService, IServiceProvider serviceProvider)
        {
            _persistenceService = persistenceService;
            _loggingService = loggingService;
            _serviceProvider = serviceProvider;
        }
        /// <summary>
        /// Shows the SetSizeWindow dialog.
        /// </summary>
        /// <returns>A nullable Size object containing the new width and height if the user
        /// clicks OK; otherwise, returns null.</returns>
        public Size ShowSetSizeDialog(double width,double height)
        {
            // Create a new instance of the SetSizeWindow.
            var setSizeWindow = new SetSizeWindow(width,height);

            // Show the dialog and capture the result.
            bool? result = setSizeWindow.ShowDialog();

            // Check if the DialogResult was true (the user clicked the OK button).
            if (result == true)
            {
                // If the dialog was successful, return the new size from the window's property.
                return setSizeWindow.NewSize;
            }

            // If the dialog was cancelled, return null.
            return new Size(400,200);
        }
        public void AddRuler(Window rulerWindow)
        {
            _openRulers.Add(rulerWindow);
            // Clean up the list when the window is closed
            rulerWindow.Closed += (sender, e) => _openRulers.Remove(rulerWindow);
        }
        private List<Window> _openRulers = new List<Window>();
        public IReadOnlyList<Window> OpenRulers => _openRulers;

        public void ShowNewRuler(RulerInfo initialInfo)
        {
            try
            {

                MainWindow newWindow = _serviceProvider.GetRequiredService<MainWindow>();

                // 2. Since MainWindow's constructor takes RulerViewModel, the ViewModel is created automatically.
                RulerViewModel newViewModel = (RulerViewModel)newWindow.DataContext;

                // 3. Set the specific data that the container couldn't know (the initialInfo)
                newViewModel.SetInitialState(initialInfo);
                _openRulers.Add(newWindow);

                // 5. Clean up the list when the window is closed
                newWindow.Closed += (sender, e) => _openRulers.Remove(newWindow);

                // ... rest of the logic ...
                newWindow.Show();       
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
