using Shared;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogicDesigner.ViewModel
{
    public class ConnectionVM
    {
        public ConnectionVM(PinVM output, PinVM input, string connectionId)
        {
            this.ConnectionId = connectionId;
            this.LineColor = Color.Black;
            this.OutputPin = output;
            this.InputPin = input;
        }

        public ConnectionVM()
        {

        }

        public PinVM OutputPin
        {
            get;
            set;
        }

        public PinVM InputPin
        {
            get;
            set;
        }

        public string ConnectionId
        {
            get;
            set;
        }

        public Color LineColor
        {
            get;
            set;
        }
    }
}
