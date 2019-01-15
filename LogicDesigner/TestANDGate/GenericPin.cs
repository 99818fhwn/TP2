using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestANDGate
{
    public class GenericPin<T> : IPinGeneric<T>
    {
        public GenericPin(IValueGeneric<T> value, string label)
        {
            this.Value = value;
            this.Label = label;
        }

        public IValueGeneric<T> Value
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
                this.Value.Value = (T)value;
            }
        }
    }
}
