using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Ruler.Shared.Commands
{
    public class DelegateCommand : ICommand
    {
        private readonly Action<object> _execute;
        public DelegateCommand(Action<object> execute) => _execute = execute;
        public bool CanExecute(object parameter) => true;
        public event EventHandler CanExecuteChanged;
        public void Execute(object parameter) => _execute(parameter);
    }
    public class DelegateCommand<T> : ICommand
    {
        private readonly Action<T> _execute;
        public DelegateCommand(Action<T> execute) => _execute = execute;
        public bool CanExecute(object parameter) => true;
        public event EventHandler CanExecuteChanged;
        public void Execute(object parameter) => _execute((T)parameter);
    }
}
