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
            this.Current = value;
        }

        public bool Current
        {
            get;
            set;
        }

        object IValue.Current
        {
            get
            {
                return this.Current;
            }

            set
            {
                this.Current = (bool)value;
            }
        }
    }
}
