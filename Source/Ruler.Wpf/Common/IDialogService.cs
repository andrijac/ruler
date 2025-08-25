using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Ruler.Wpf.Common
{
    /// <summary>
    /// Defines a contract for services that show dialogs.
    /// This allows the ViewModel to request a dialog without knowing
    /// about the specific UI implementation.
    /// </summary>
    public interface IDialogService
    {
        Size? ShowSetSizeDialog();
    }
}
