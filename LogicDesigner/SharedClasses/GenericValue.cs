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

        #region Serialization
        /// <summary>
        /// Initializes a new instance of the <see cref="GenericValue{T}"/> class.
        /// </summary>
        /// <param name="info"> Serialization info userd for parametrization. </param>
        /// <param name="context"> StreamingContext of serialization stream. </param>
        internal GenericValue(SerializationInfo info, StreamingContext context)
        {
            this.Current = (T)info.GetValue(nameof(Current), typeof(T));
        }

        /// <summary>
        /// Manages the serialization procedure for <see cref="GenericValue{T}"/>.
        /// </summary>
        /// <param name="info"> Serialization info userd for parametrization. </param>
        /// <param name="context"> StreamingContext of serialization stream. </param>
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(Current), Current, typeof(T));
        }
        #endregion
    }
}
