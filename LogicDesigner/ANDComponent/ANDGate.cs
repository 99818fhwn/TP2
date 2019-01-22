// -----------------------------------------------------------------------     
// <copyright file="ANDGate.cs" company="FHWN">    
// Copyright (c) FHWN. All rights reserved.    
// </copyright>    
// <summary>The ANDGateComponent for logic designers that implements IDisplayableNode</summary>    
// <author>Fabian Weisser</author>    
// -----------------------------------------------------------------------
namespace ANDComponent
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Shared;
    using SharedClasses;

    /// <summary>
    /// This class represent the ANDGate.
    /// </summary>
    /// <seealso cref="Shared.IDisplayableNode" />
    public class ANDGate : IDisplayableNode
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ANDGate"/> class.
        /// </summary>
        public ANDGate()
        {
            this.Inputs = new List<IPin>();
            this.Outputs = new List<IPin>();
            this.Label = "AND";
            this.Description = "If all inputs are true, the output is true";
            this.Picture = Properties.Resources.ANDGate;
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
        /// The output pins.
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
        /// The description of the component.
        /// </value>
        public string Description
        {
            get;
        }

        /// <summary>
        /// Gets the picture.
        /// </summary>
        /// <value>
        /// The picture that represents the component.
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
        /// Activates this instances features.
        /// </summary>
        public void Activate()
        {
            return;
        }

        /// <summary>
        /// Executes this instance.
        /// </summary>
        public void Execute()
        {
            if (!this.Inputs.Any(x => (bool)x.Value.Current == false))
            {
                foreach (var o in this.Outputs)
                {
                    o.Value.Current = false;
                }
            }
            else
            {
                foreach (var o in this.Outputs)
                {
                    o.Value.Current = true;
                }
            }
        }
    }
}
