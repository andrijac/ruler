using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Ruler.Shared.Models
{
    public class ModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        // 1. Backing field for your suppression property
        public bool _suppressNotifications;
        // 2. Explicit methods to control the toggle switch easily
        public void SuspendEvents() => _suppressNotifications = true;
        public void ResumeEvents() => _suppressNotifications = false;

        /// <summary>
        /// Updates a property on an EXTERNAL object (like RulerInfo) and notifies the local View.
        /// This avoids Reflection while keeping the ViewModel and Model in sync.
        /// </summary>
        protected bool SetProperty<T>(T currentValue, T newValue, Action<T> setter, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(currentValue, newValue)) return false;

            setter(newValue); // Execute the update on the model object

            // FIXED: Honor the suppression flag here too!
            if (!_suppressNotifications)
            {
                OnPropertyChanged(propertyName);
            }
            return true;
        }

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string name = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;

            field = value;

            if (!_suppressNotifications)
            {
                OnPropertyChanged(name);
            }

            return true;
        }

        // Centralized event raiser that honors the suppression flag
        public void OnPropertyChanged([CallerMemberName] string name = null)
        {
            if (_suppressNotifications) return; // Guard clause to drop events early

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public void RefreshAll()
        {
            OnPropertyChanged(string.Empty);
        }
    }
}
