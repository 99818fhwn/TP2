// -----------------------------------------------------------------------     
// <copyright file="PowerSource.cs" company="FHWN">    
// Copyright (c) FHWN. All rights reserved.    
// </copyright>    
// <summary>The PowerSourceComponent for logic designers that implements IDisplayableNode</summary>    
// <author>Fabian Weisser</author>    
// -----------------------------------------------------------------------
namespace SourceComponent
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Shared;
    using SharedClasses;

    /// <summary>
    /// This class represent the PowerSource.
    /// </summary>
    /// <seealso cref="Shared.IDisplayableNode" />
    public class PowerSource : IDisplayableNode
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PowerSource"/> class.
        /// </summary>
        public PowerSource()
        {
            this.Inputs = new List<IPin>();
            this.Outputs = new List<IPin>();
            this.Label = "+5V";
            this.Description = "Creates a +5V current on the output pin";
            this.Picture = Properties.Resources.CurrentSource;
            this.Type = NodeType.Source;
            this.Outputs.Add(new GenericPin<bool>(new GenericValue<bool>(false), "Pin1"));
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
        /// When executing the output will always be on.
        /// </summary>
        public void Execute()
        {
            foreach (var o in this.Outputs)
            {
                o.Value.Current = true;
            }
        }
    }
}