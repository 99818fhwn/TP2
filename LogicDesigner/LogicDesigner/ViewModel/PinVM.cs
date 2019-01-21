using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogicDesigner.ViewModel
{
    public class PinVM
    {
        private readonly IPin pin;
        private readonly bool isInputPin;

        public PinVM(IPin pin, bool isInputPin)
        {
            this.pin = pin;
            this.isInputPin = isInputPin;
        }

        public IPin Pin
        {
            get
            {
                return this.pin;
            }
        }

        public bool IsInputPin
        {
            get
            {
                return this.isInputPin;
            }
        }
    }
}
