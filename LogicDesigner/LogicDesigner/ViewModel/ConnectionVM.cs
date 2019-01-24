// -----------------------------------------------------------------------     
// <copyright file="ConnectionVM.cs" company="FHWN">    
// Copyright (c) FHWN. All rights reserved.    
// </copyright>    
// <summary>The component view model.</summary>
// -----------------------------------------------------------------------
namespace LogicDesigner.ViewModel
{
    using System.Drawing;

    /// <summary>
    /// Connection view model class.
    /// </summary>
    public class ConnectionVM
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionVM"/> class.
        /// </summary>
        /// <param name="output">The output.</param>
        /// <param name="input">The input.</param>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="lineColor">Color of the line.</param>
        public ConnectionVM(PinVM output, PinVM input, string connectionId, Color lineColor)
        {
            this.ConnectionId = connectionId;
            this.LineColor = lineColor;
            this.OutputPin = output;
            this.InputPin = input;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionVM"/> class.
        /// </summary>
        public ConnectionVM()
        {
        }

        /// <summary>
        /// Gets or sets the output pin.
        /// </summary>
        /// <value>
        /// The output pin.
        /// </value>
        public PinVM OutputPin
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the input pin.
        /// </summary>
        /// <value>
        /// The input pin.
        /// </value>
        public PinVM InputPin
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the connection identifier.
        /// </summary>
        /// <value>
        /// The connection identifier.
        /// </value>
        public string ConnectionId
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the color of the line.
        /// </summary>
        /// <value>
        /// The color of the line.
        /// </value>
        public Color LineColor
        {
            get;
            set;
        }
    }
}
