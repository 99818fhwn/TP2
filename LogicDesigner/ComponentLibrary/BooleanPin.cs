using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared;

namespace ComponentLibrary
{
    public class BooleanPin : IPinGeneric<bool>
    {
        public BooleanPin(IValueGeneric<bool> value, string label)
        {
            this.Value = value;
            this.Label = label;
        }

        public IValueGeneric<bool> Value
        {
            get;
            set;
        }

        public string Label
        {
            get;
            set;
        }

        IValue IPin.Value
        {
            get
            {
                return this.Value;
            }

            set
            {
                this.Value.Value = (bool)value.Value;
            }
        }
    }
}
