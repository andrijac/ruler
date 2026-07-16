using Ruler.Shared.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ruler.Shared.Interfaces
{
    public interface IRuler:IDisposable
    {
        // Data Access
        RulerInfo RulerData { get; }

        // Window Management
        void Show();
        void Close();
        void InvalidateView();
        void SetBounds(int left, int top, int width, int height);
        void SetTooltip(string text);

        // Visual Capture
        ImageData GetCurrentSnapshot();

        // User Commands
        void ShowAbout();
        void ShowShortcuts();
        Task CheckForUpdatesAsync();

        // Lifecycle Events
        event EventHandler Closed;
    }
}
