using Shared;
using System;

namespace LogicDesigner.ViewModel
{
    public class PinsConnectedEventArgs :  EventArgs
    {
        public PinsConnectedEventArgs(IPin outputPin, IPin inputPin)
        {
            this.OutputPin = outputPin;
            this.InputPin = inputPin;
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