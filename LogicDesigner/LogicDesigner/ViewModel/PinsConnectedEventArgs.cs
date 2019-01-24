// -----------------------------------------------------------------------     
// <copyright file="PinsConnectedEventArgs.cs" company="FHWN">    
// Copyright (c) FHWN. All rights reserved.    
// </copyright>    
// <summary>Contains class for pins connected event args.</summary>    
// -----------------------------------------------------------------------
namespace LogicDesigner.ViewModel
{
    using System;
    using Shared;

    /// <summary>
    /// Pins connect event arguments class.
    /// </summary>
    /// <seealso cref="System.EventArgs" />
    public class PinsConnectedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PinsConnectedEventArgs"/> class.
        /// </summary>
        /// <param name="outputPin">The output pin.</param>
        /// <param name="inputPin">The input pin.</param>
        public PinsConnectedEventArgs(IPin outputPin, IPin inputPin)
        {
            this.OutputPin = outputPin;
            this.InputPin = inputPin;
        }

        /// <summary>
        /// Gets the output pin.
        /// </summary>
        /// <value>
        /// The output pin.
        /// </value>
        public IPin OutputPin
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the input pin.
        /// </summary>
        /// <value>
        /// The input pin.
        /// </value>
        public IPin InputPin
        {
            get;
            private set;
        }
    }
}