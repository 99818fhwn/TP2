using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Shared;

namespace SharedClasses
{
    [Serializable()]
    public class GenericValue<T> : IValueGeneric<T>, ISerializable
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

        internal GenericValue(SerializationInfo info, StreamingContext context)
        {
            this.Current = (T)info.GetValue(nameof(Current), typeof(T));
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(Current), Current, typeof(T));
        }
    }
}
