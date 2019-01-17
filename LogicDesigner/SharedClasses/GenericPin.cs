// -----------------------------------------------------------------------     
// <copyright file="GenericPin.cs" company="FHWN">    
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
    /// This class represents the concrete implementation of the IPinGeneric interface.
    /// </summary>
    /// <typeparam name="T">The generic type parameter.</typeparam>
    /// <seealso cref="Shared.IPinGeneric{T}" />
    public class GenericPin<T> : IPinGeneric<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GenericPin{T}"/> class.
        /// </summary>
        /// <param name="value">The value of the pin.</param>
        /// <param name="label">The label of the pin.</param>
        public GenericPin(IValueGeneric<T> value, string label)
        {
            this.Value = value;
            this.Label = label;
        }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>
        /// The value of the pin.
        /// </value>
        public IValueGeneric<T> Value
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the label.
        /// </summary>
        /// <value>
        /// The label of the pin.
        /// </value>
        public string Label
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>
        /// The value of the pin interface.
        /// </value>
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
    }
}
