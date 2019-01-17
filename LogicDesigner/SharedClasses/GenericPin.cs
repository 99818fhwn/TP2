using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SharedClasses
{
    [Serializable()]
    public class GenericPin<T> : IPinGeneric<T>, ISerializable
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
                this.Value.Current = (T)value;
            }
        }

        #region Serialization
        /// <summary>
        /// Initializes a new instance of the <see cref="GenericPin{T}"/> class.
        /// </summary>
        /// <param name="info"> Serialization info userd for parametrization. </param>
        /// <param name="context"> StreamingContext of serialization stream. </param>
        internal GenericPin(SerializationInfo info, StreamingContext context)
        {
            this.Label = info.GetString(nameof(Label));
            this.Value = (IValueGeneric<T>)info.GetValue(nameof(Value), typeof(T));
        }

        /// <summary>
        /// Manages the serialization procedure for <see cref="GenericPin{T}"/>.
        /// </summary>
        /// <param name="info"> Serialization info userd for parametrization. </param>
        /// <param name="context"> StreamingContext of serialization stream. </param>
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(Label), Label, typeof(string));
            info.AddValue(nameof(Value), Value, typeof(IValueGeneric<T>));
        }
        #endregion 
    }
}
