using System;

namespace LogicDesigner.ViewModel
{
    public class PinsConnectedEventArgs :  EventArgs
    {
        public PinsConnectedEventArgs(PinVM outputPinVM, PinVM inputPinVM)
        {
            this.OutputPinVM = outputPinVM;
            this.InputPinVM = inputPinVM;
        }

        public PinVM OutputPinVM
        {
            get;
            private set;
        }

        public PinVM InputPinVM
        {
            get;
            private set;
        }
    }
}