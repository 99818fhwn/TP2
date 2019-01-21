using LogicDesigner.Commands;
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
        private readonly Command setPinCommand;

        public PinVM(IPin pin, bool isInputPin, Command setPinCommand)
        {
            this.pin = pin;
            this.isInputPin = isInputPin;
            this.setPinCommand = setPinCommand;
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
