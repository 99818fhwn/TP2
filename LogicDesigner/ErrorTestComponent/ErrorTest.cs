// -----------------------------------------------------------------------     
// <copyright file="ErrorTest.cs" company="FHWN">    
// Copyright (c) FHWN. All rights reserved.    
// </copyright>    
// <summary>The ErrorComponent for logic designers that implements IDisplayableNode</summary>    
// <author>Fabian Weisser</author>    
// -----------------------------------------------------------------------
namespace ErrorTestComponent
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Shared;
    using SharedClasses;

    /// <summary>
    /// This class represent the Error.
    /// </summary>
    /// <seealso cref="Shared.IDisplayableNode" />
    public class ErrorTest : IDisplayableNode
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorTest"/> class.
        /// </summary>
        public ErrorTest()
        {
            this.Inputs = new List<IPin>();
            this.Outputs = new List<IPin>();
            this.Label = "Error";
            this.Description = "Produces errors and unwanted pin values";
            this.Picture = Properties.Resources.ErrorProducer;
            this.Type = NodeType.Logic;
            this.Inputs.Add(new GenericPin<string>(new GenericValue<string>(null), "Pin1"));
            this.Outputs.Add(new GenericPin<string>(new GenericValue<string>(null), "Pin2"));
        }

        /// <summary>
        /// Occurs when [picture changed].
        /// </summary>
        public event EventHandler PictureChanged;

        /// <summary>
        /// Gets or sets the inputs.
        /// </summary>
        /// <value>
        /// The input pins.
        /// </value>
        public ICollection<IPin> Inputs
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the outputs.
        /// </summary>
        /// <value>
        /// The output pins.
        /// </value>
        public ICollection<IPin> Outputs
        {
            get;
            set;
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
            throw new Exception();
        }

        /// <summary>
        /// When executing all gets set to null and an exception gets thrown.
        /// </summary>
        public void Execute()
        {
            foreach (var i in this.Inputs)
            {
                i.Value.Current = null;
            }

            foreach (var o in this.Outputs)
            {
                o.Value.Current = null;
            }

            this.Inputs = null;
            this.Outputs = null;

            throw new Exception();
        }
    }
}
