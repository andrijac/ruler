using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Ruler.Wpf.Models;

namespace Ruler.Wpf.Services
{
    /// <summary>
    /// Defines a contract for services that show dialogs.
    /// This allows the ViewModel to request a dialog without knowing
    /// about the specific UI implementation.
    /// </summary>
    public interface IDialogService
    {
        Size ShowSetSizeDialog(double width,double height);
        void AddRuler(Window rulerWindow);
        IReadOnlyList<Window> OpenRulers { get; }
        void ShowNewRuler(RulerInfo initialInfo);
    }
}
