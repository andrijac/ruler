using Ruler.Wpf.Models;
using Ruler.Wpf.Persistence;
using Ruler.Wpf.ViewModels;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Ruler.Wpf.Common
{
    /// <summary>
    /// An implementation of IDialogService that shows the SetSizeWindow.
    /// </summary>
    public class DialogService : IDialogService
    {
        private readonly SingleRulerPersistenceService _persistenceService;
        public DialogService(SingleRulerPersistenceService persistenceService)
        {
            _persistenceService = persistenceService;
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

           
            // 1. Create a brand new instance of the Main Window
            MainWindow newWindow = new MainWindow();

            // 2. Create a new ViewModel for the new window, passing the initial state
            RulerViewModel newViewModel = new RulerViewModel(this, initialInfo, _persistenceService);

            // 3. Set the DataContext
            newWindow.DataContext = newViewModel;

            // 4. Add the reference to your list
            _openRulers.Add(newWindow);

            // 5. Clean up the list when the window is closed
            newWindow.Closed += (sender, e) => _openRulers.Remove(newWindow);

            newWindow.Show();
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
