// -----------------------------------------------------------------------     
// <copyright file="Component.cs" company="FHWN">    
// Copyright (c) FHWN. All rights reserved.    
// </copyright>    
// <summary>The component.</summary>
// -----------------------------------------------------------------------
namespace LogicDesigner.Model
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Text;
    using System.Threading.Tasks;
    using Shared;

    /// <summary>
    /// The component class.
    /// </summary>
    /// <seealso cref="Shared.INode" />
    /// <seealso cref="System.Runtime.Serialization.ISerializable" />
    [Serializable]
    public class Component : INode, ISerializable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Component"/> class.
        /// </summary>
        /// <param name="info">The information.</param>
        /// <param name="context">The context.</param>
        internal Component(SerializationInfo info, StreamingContext context)
        {
            this.Label = info.GetString(nameof(this.Label));
            this.Description = info.GetString(nameof(this.Description));
            this.Type = (NodeType)info.GetValue(nameof(this.Type), typeof(NodeType));
            this.Inputs = (List<IPin>)info.GetValue(nameof(this.Inputs), typeof(List<IPin>));
            this.Outputs = (List<IPin>)info.GetValue(nameof(this.Outputs), typeof(List<IPin>));
        }

        /// <summary>
        /// Gets the inputs.
        /// </summary>
        /// <value>
        /// The inputs.
        /// </value>
        public ICollection<IPin> Inputs
        {
            get;
        }

        /// <summary>
        /// Gets the outputs.
        /// </summary>
        /// <value>
        /// The outputs.
        /// </value>
        public ICollection<IPin> Outputs
        {
            get;
        }

        /// <summary>
        /// Gets the label.
        /// </summary>
        /// <value>
        /// The label.
        /// </value>
        public string Label
        {
            get;
        }

        /// <summary>
        /// Gets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        public string Description
        {
            get;
        }

        /// <summary>
        /// Gets the type.
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        public NodeType Type
        {
            get;
        }

        /// <summary>
        /// Activates this instance.
        /// </summary>
        /// <exception cref="NotImplementedException">Throws exception.</exception>
        public void Activate()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Executes this instance.
        /// </summary>
        /// <exception cref="NotImplementedException">Throws exception.</exception>
        public void Execute()
        {
            throw new NotImplementedException();
        }
        
        /// <summary>
        /// Populates a <see cref="T:System.Runtime.Serialization.SerializationInfo"/> with the data needed to serialize the target object.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo"/> to populate with data.</param>
        /// <param name="context">The destination (see <see cref="T:System.Runtime.Serialization.StreamingContext"/>) for this serialization.</param>
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(this.Inputs), this.Inputs, this.Inputs.GetType());
            info.AddValue(nameof(this.Outputs), this.Outputs, this.Outputs.GetType());
            info.AddValue(nameof(this.Label), this.Label, this.Label.GetType());
            info.AddValue(nameof(this.Description), this.Description, this.Description.GetType());
            info.AddValue(nameof(this.Type), this.Type, this.Type.GetType());
        }
    }
}
