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
            this.Value = value;
        }

        public T Value
        {
            get;
            set;
        }

        object IValue.Value
        {
            get;
            set;
        }
    }
}
