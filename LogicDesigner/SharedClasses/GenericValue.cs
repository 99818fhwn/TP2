// -----------------------------------------------------------------------     
// <copyright file="GenericValue.cs" company="FHWN">    
// Copyright (c) FHWN. All rights reserved.    
// </copyright>    
// <summary>The SharedClass library holds the concrete implementation of Shared interfaces</summary>    
// <author>Fabian Weisser</author>    
// -----------------------------------------------------------------------
namespace SharedClasses
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Text;
    using System.Threading.Tasks;
    using Shared;

    /// <summary>
    /// This class represents the concrete implementation of the IGenericValue interface.
    /// </summary>
    /// <typeparam name="T">The generic type.</typeparam>
    /// <seealso cref="Shared.IValueGeneric{T}" />
    [Serializable]
    public class GenericValue<T> : IValueGeneric<T>, ISerializable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GenericValue{T}"/> class.
        /// </summary>
        /// <param name="value">The value that is stored.</param>
        public GenericValue(T value)
        {
            this.Current = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericValue{T}"/> class.
        /// </summary>
        /// <param name="info"> Serialization info. </param>
        /// <param name="context"> StreamingContext of serialization stream. </param>
        internal GenericValue(SerializationInfo info, StreamingContext context)
        {
            this.Current = (T)info.GetValue(nameof(this.Current), typeof(T));
        }

        /// <summary>
        /// Gets or sets the current.
        /// </summary>
        /// <value>
        /// The current value stored.
        /// </value>
        public T Current
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the current.
        /// </summary>
        /// <value>
        /// The current value stored in the interface.
        /// </value>
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

        /// <summary>
        /// Manages the serialization procedure for <see cref="GenericValue{T}"/>.
        /// </summary>
        /// <param name="info"> Serialization info. </param>
        /// <param name="context"> StreamingContext of serialization stream. </param>
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(this.Current), this.Current, typeof(T));
        }
    }
}
