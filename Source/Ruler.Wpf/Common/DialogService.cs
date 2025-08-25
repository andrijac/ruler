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
        /// <summary>
        /// Shows the SetSizeWindow dialog.
        /// </summary>
        /// <returns>A nullable Size object containing the new width and height if the user
        /// clicks OK; otherwise, returns null.</returns>
        public Size? ShowSetSizeDialog()
        {
            // Create a new instance of the SetSizeWindow.
            var setSizeWindow = new SetSizeWindow();

            // Show the dialog and capture the result.
            bool? result = setSizeWindow.ShowDialog();

            // Check if the DialogResult was true (the user clicked the OK button).
            if (result == true)
            {
                // If the dialog was successful, return the new size from the window's property.
                return setSizeWindow.NewSize;
            }

            // If the dialog was cancelled, return null.
            return null;
        }
    }
}
