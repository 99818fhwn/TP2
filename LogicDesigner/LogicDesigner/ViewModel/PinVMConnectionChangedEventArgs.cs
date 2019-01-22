using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogicDesigner.ViewModel
{
    public class PinVMConnectionChangedEventArgs : EventArgs
    {
        public PinVMConnectionChangedEventArgs(ConnectionVM connection)
        {
            this.Connection = connection;
        }

        public ConnectionVM Connection
        {
            get;
            private set;
        }
    }
}
