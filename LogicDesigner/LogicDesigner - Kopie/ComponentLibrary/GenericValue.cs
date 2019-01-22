using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared;

namespace ComponentLibrary
{
    public class GenericValue<T> : IValueGeneric<T>
    {
        public GenericValue(T value)
        {
            this.Current = value;
        }

        public T Current
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
                this.Current = (T)value;
            }
        }
    }
}
