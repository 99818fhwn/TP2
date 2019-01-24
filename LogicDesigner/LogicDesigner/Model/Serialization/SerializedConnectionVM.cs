// -----------------------------------------------------------------------     
// <copyright file="SerializedConnectionVM.cs" company="FHWN">    
// Copyright (c) FHWN. All rights reserved.    
// </copyright>    
// <summary>Serialization view model.</summary>
// -----------------------------------------------------------------------
namespace LogicDesigner.Model.Serialization
{
    /// <summary>
    /// Serialization connection class.
    /// </summary>
    public class SerializedConnectionVM
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SerializedConnectionVM"/> class.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="output">The output.</param>
        /// <param name="inputParentID">The input parent identifier.</param>
        /// <param name="outputParentID">The output parent identifier.</param>
        /// <param name="inputX">The input x.</param>
        /// <param name="inputY">The input y.</param>
        /// <param name="outputX">The output x.</param>
        /// <param name="outputY">The output y.</param>
        /// <param name="connectionID">The connection identifier.</param>
        public SerializedConnectionVM(
            string input,
            string output,
            string inputParentID,
            string outputParentID, 
            double inputX,
            double inputY,
            double outputX,
            double outputY,
            string connectionID)
        {
            this.InputPinID = input;
            this.OutputPinID = output;
            this.InputParentID = inputParentID;
            this.OutputParentID = outputParentID;

            this.ConnectionID = connectionID;

            this.InputX = inputX;
            this.InputY = inputY;

            this.OutputX = outputX;
            this.OutputY = outputY;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SerializedConnectionVM"/> class.
        /// </summary>
        public SerializedConnectionVM()
        {
        }

        /// <summary>
        /// Gets or sets the input pin identifier.
        /// </summary>
        /// <value>
        /// The input pin identifier.
        /// </value>
        public string InputPinID { get; set; }

        /// <summary>
        /// Gets or sets the output pin identifier.
        /// </summary>
        /// <value>
        /// The output pin identifier.
        /// </value>
        public string OutputPinID { get; set; }

        /// <summary>
        /// Gets or sets the input parent identifier.
        /// </summary>
        /// <value>
        /// The input parent identifier.
        /// </value>
        public string InputParentID { get; set; }

        /// <summary>
        /// Gets or sets the output parent identifier.
        /// </summary>
        /// <value>
        /// The output parent identifier.
        /// </value>
        public string OutputParentID { get; set; }

        /// <summary>
        /// Gets or sets the connection identifier.
        /// </summary>
        /// <value>
        /// The connection identifier.
        /// </value>
        public string ConnectionID { get; set; }

        /// <summary>
        /// Gets or sets the input x.
        /// </summary>
        /// <value>
        /// The input x.
        /// </value>
        public double InputX { get; set; }

        /// <summary>
        /// Gets or sets the input y.
        /// </summary>
        /// <value>
        /// The input y.
        /// </value>
        public double InputY { get; set; }

        /// <summary>
        /// Gets or sets the output x.
        /// </summary>
        /// <value>
        /// The output x.
        /// </value>
        public double OutputX { get; set; }

        /// <summary>
        /// Gets or sets the output y.
        /// </summary>
        /// <value>
        /// The output y.
        /// </value>
        public double OutputY { get; set; }
    }
}
