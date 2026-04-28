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
        public bool SuppressNotifications { get; set; }
        /// <summary>
        /// Updates a property on an EXTERNAL object (like RulerInfo) and notifies the local View.
        /// This avoids Reflection while keeping the ViewModel and Model in sync.
        /// </summary>
        protected bool SetProperty<T>(T currentValue, T newValue, Action<T> setter, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(currentValue, newValue)) return false;

            setter(newValue); // Execute the update on the model object
            OnPropertyChanged(propertyName);
            return true;
        }
        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string name = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;

            field = value;

            if (!SuppressNotifications)
            {
                OnPropertyChanged(name);
            }

            return true;
        }

        //The C#6 version of the common implementation
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        public void RefreshAll()
        {
            OnPropertyChanged(string.Empty);
        }

    }
}
