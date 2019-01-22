using Shared;
using System;

namespace LogicDesigner.ViewModel
{
    public class PinsConnectedEventArgs :  EventArgs
    {
        public PinsConnectedEventArgs(IPin outputPinVM, IPin inputPinVM)
        {
            this.OutputPin = outputPinVM;
            this.InputPin = inputPinVM;
        }

        public IPin OutputPin
        {
            get;
            private set;
        }

        public IPin InputPin
        {
            get;
            private set;
        }
    }
}