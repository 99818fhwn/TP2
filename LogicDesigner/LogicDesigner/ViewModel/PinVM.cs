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
        private double xposition;
        private double yposition;
        private ComponentVM parent;

        public PinVM(IPin pin, bool isInputPin, Command setPinCommand, ComponentVM parent)
        {
            this.parent = parent;
            this.pin = pin;
            this.isInputPin = isInputPin;
            this.setPinCommand = setPinCommand;
            this.xposition = 0;
            this.yposition = 0;
        }

        public PinVM(IPin pin, bool isInputPin, Command setPinCommand)
        {
            this.pin = pin;
            this.isInputPin = isInputPin;
            this.setPinCommand = setPinCommand;
            this.xposition = 0;
            this.yposition = 0;
        }

        public IPin Pin
        {
            get
            {
                return this.pin;
            }
        }

        public Type PinValueType
        {
            get
            {
                return this.pin.Value.Current?.GetType();
            }
        }

        public bool IsInputPin
        {
            get
            {
                return this.isInputPin;
            }
        }

        public Command SetPinCommand
        {
            get
            {
                return this.setPinCommand;
            }
        }

        public double XPosition
        {
            get
            {
                return this.xposition + this.parent.XCoord;
            }
            set
            {
                this.xposition = value;
            }
        }

        public double YPosition
        {
            get
            {
                return this.yposition + this.parent.YCoord;
            }
            set
            {
                this.yposition = value;
            }
        }

        public ComponentVM Parent
        {
            get
            {
                return this.parent;
            }
        }
    }
}
