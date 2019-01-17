using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace LogicDesigner.Command
{
    public class Command : ICommand
    {
        private Action<object> action;
        public event EventHandler CanExecuteChanged;

        public Command(Action<object> action)
        {
            this.action = action;
        }

        public bool CanExecute(object parameter)
        {
            // if item implements IClickable interface
            return true;
        }

        public void Execute(object parameter)
        {
            this.action(parameter);
        }
    }
}
