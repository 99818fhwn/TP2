// -----------------------------------------------------------------------     
// <copyright file="ORGate.cs" company="FHWN">    
// Copyright (c) FHWN. All rights reserved.    
// </copyright>    
// <summary>The ORGateComponent for logic designers that implements IDisplayableNode</summary>    
// <author>Fabian Weisser</author>    
// -----------------------------------------------------------------------
namespace ORGateComponent
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Shared;
    using SharedClasses;

    /// <summary>
    /// The class that represents logic of an ORGate.
    /// </summary>
    /// <seealso cref="Shared.IDisplayableNode" />
    public class ORGate : IDisplayableNode
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ORGate"/> class.
        /// </summary>
        public ORGate()
        {
            this.Inputs = new List<IPin>();
            this.Outputs = new List<IPin>();
            this.Label = "OR";
            this.Description = "If one or more inputs are true, the output is true";
            this.Picture = Properties.Resources.ORGate;
            this.Type = NodeType.Logic;
            this.Inputs.Add(new GenericPin<bool>(new GenericValue<bool>(false), "Pin1"));
            this.Inputs.Add(new GenericPin<bool>(new GenericValue<bool>(false), "Pin2"));
            this.Outputs.Add(new GenericPin<bool>(new GenericValue<bool>(false), "Pin3"));
        }

        /// <summary>
        /// Occurs when [picture changed].
        /// </summary>
        public event EventHandler PictureChanged;

        /// <summary>
        /// Gets the inputs.
        /// </summary>
        /// <value>
        /// The input pins.
        /// </value>
        public ICollection<IPin> Inputs
        {
            get;
        }

        /// <summary>
        /// Gets the outputs.
        /// </summary>
        /// <value>
        /// The output pin.
        /// </value>
        public ICollection<IPin> Outputs
        {
            get;
        }

        /// <summary>
        /// Gets the label.
        /// </summary>
        /// <value>
        /// The label of the component.
        /// </value>
        public string Label
        {
            get;
        }

        /// <summary>
        /// Gets the description.
        /// </summary>
        /// <value>
        /// The description of the ORGate component.
        /// </value>
        public string Description
        {
            get;
        }

        /// <summary>
        /// Gets the picture.
        /// </summary>
        /// <value>
        /// The picture that represents the ORGate visually.
        /// </value>
        public System.Drawing.Bitmap Picture
        {
            get;
        }

        /// <summary>
        /// Gets the type.
        /// </summary>
        /// <value>
        /// The component type.
        /// </value>
        public NodeType Type
        {
            get;
        }

        /// <summary>
        /// Activates can change the state of the component when used.
        /// </summary>
        public void Activate()
        {
            return;
        }

        /// <summary>
        /// Executes changes the output depending on the inputs, if one or more inputs are active the outputs are true.
        /// </summary>
        public void Execute()
        {
            if (this.Inputs.Any(x => (bool)x.Value.Current == true))
            {
                foreach (var o in this.Outputs)
                {
                    o.Value.Current = true;
                }
            }
        }
    }
}
