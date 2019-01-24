// -----------------------------------------------------------------------     
// <copyright file="PinVMConnectionChangedEventArgs.cs" company="FHWN">    
// Copyright (c) FHWN. All rights reserved.    
// </copyright>    
// <summary>Pin view model connection class.</summary>
// -----------------------------------------------------------------------
namespace LogicDesigner.ViewModel
{
    using System;

    /// <summary>
    /// The pin connection view model class.
    /// </summary>
    /// <seealso cref="System.EventArgs" />
    public class PinVMConnectionChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PinVMConnectionChangedEventArgs"/> class.
        /// </summary>
        /// <param name="connection">The connection.</param>
        public PinVMConnectionChangedEventArgs(ConnectionVM connection)
        {
            this.Connection = connection;
        }

        /// <summary>
        /// Gets the connection.
        /// </summary>
        /// <value>
        /// The connection.
        /// </value>
        public ConnectionVM Connection
        {
            get;
            private set;
        }
    }
}
