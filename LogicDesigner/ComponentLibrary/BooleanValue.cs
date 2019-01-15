using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared;

namespace ComponentLibrary
{
    public class BooleanValue : IValueGeneric<bool>
    {
        public BooleanValue(bool value)
        {
            this.Value = value;
        }

        public bool Value
        {
            get;
            set;
        }

        object IValue.Value
        {
            get
            {
                return this.Value;
            }

            set
            {
                this.Value = (bool)value;
            }
        }
    }
}
