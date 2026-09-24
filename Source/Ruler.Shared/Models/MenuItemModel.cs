using Ruler.Shared.Commands;

using System;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Ruler.Shared.Models
{
    public class MenuItemModel:ModelBase
    {
        private string _header;
        private bool _isChecked;
        private string _toolTip;
        public string Header {
            get => _header;
                 set=> SetProperty(ref _header, value); }
        public object Value { get; set; } // Stores the opacity (e.g., 0.5)
        public string InputGestureText { get; set; } // used to display keyboard shortcut
        public ICommand Command { get; set; }
        public bool IsCheckable { get; set; } = false;
        public bool IsChecked { get=>_isChecked; set { SetProperty(ref _isChecked, value); } }
        public string ToolTip { 
            get=> _toolTip;
            set { SetProperty(ref _toolTip, value); } 
        }
        public ObservableCollection<MenuItemModel> Items { get; set; } = new ObservableCollection<MenuItemModel>();

        public MenuItemModel() { }
        public MenuItemModel(string header, object value, Action<MenuItemModel> executeAction, bool isCheckable = false)
        {
            Header = header;
            Value = value; // Store the opacity value
            IsCheckable = isCheckable;
            Command = new DelegateCommand(param => executeAction((MenuItemModel)param));
        }
    }
}
