using Ruler.Shared.Commands;

using System;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Ruler.Shared.Models
{
    public class MenuItemModel:ModelBase
    {
        public string Header { get; set; }
        public object Value { get; set; } // Stores the opacity (e.g., 0.5)
        public string InputGestureText { get; set; } // used to display keyboard shortcut
        public ICommand Command { get; set; }
        public bool IsCheckable { get; set; }
        public bool IsChecked { get; set; }
       public string ToolTip { get; set;}
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
