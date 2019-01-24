// -----------------------------------------------------------------------     
// <copyright file="FieldComponentEventArgs.cs" company="FHWN">    
// Copyright (c) FHWN. All rights reserved.    
// </copyright>    
// <summary>Contains class for field component event args.</summary>    
// -----------------------------------------------------------------------
namespace LogicDesigner.ViewModel
{
    using System;

    /// <summary>
    /// Field component event arguments class.
    /// </summary>
    /// <seealso cref="System.EventArgs" />
    public class FieldComponentEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FieldComponentEventArgs"/> class.
        /// </summary>
        /// <param name="component">The component.</param>
        public FieldComponentEventArgs(ComponentVM component)
        {
            this.Component = component;
        }

        /// <summary>
        /// Gets the component.
        /// </summary>
        /// <value>
        /// The component.
        /// </value>
        public ComponentVM Component
        {
            get;
            private set;
        }
    }
}