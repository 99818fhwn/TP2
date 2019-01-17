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
    using System.Text;
    using System.Threading.Tasks;
    using Shared;

    /// <summary>
    /// This class represents the concrete implementation of the IGenericValue interface.
    /// </summary>
    /// <typeparam name="T">The generic type.</typeparam>
    /// <seealso cref="Shared.IValueGeneric{T}" />
    public class GenericValue<T> : IValueGeneric<T>
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
    }
}
