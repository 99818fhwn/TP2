// -----------------------------------------------------------------------     
// <copyright file="Switch.cs" company="FHWN">    
// Copyright (c) FHWN. All rights reserved.    
// </copyright>    
// <summary>The SwitchComponent for logic designers that implements IDisplayableNode</summary>    
// <author>Fabian Weisser</author>    
// -----------------------------------------------------------------------
namespace SwitchComponent
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Shared;
    using SharedClasses;

    /// <summary>
    /// The class that represents the functionality of a switch.
    /// </summary>
    /// <seealso cref="Shared.IDisplayableNode" />
    public class Switch : IDisplayableNode
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Switch"/> class.
        /// </summary>
        public Switch()
        {
            this.Inputs = new List<IPin>();
            this.Outputs = new List<IPin>();
            this.Label = "Switch";
            this.Description = "If activaed enable the input to be in the output";
            this.Picture = Properties.Resources.SwitchOpen;
            this.Type = NodeType.Switch;
            this.IsClosed = false;
            this.Inputs.Add(new GenericPin<bool>(new GenericValue<bool>(false), "Pin1"));
            this.Outputs.Add(new GenericPin<bool>(new GenericValue<bool>(false), "Pin2"));
        }

        /// <summary>
        /// Occurs when [picture changed].
        /// </summary>
        public event EventHandler PictureChanged;

        /// <summary>
        /// Gets or sets a value indicating whether the switch is closed.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is closed; otherwise, <c>false</c>.
        /// </value>
        public bool IsClosed
        {
            get; set;
        }

        /// <summary>
        /// Gets the inputs.
        /// </summary>
        /// <value>
        /// The input pin.
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
        /// The component label.
        /// </value>
        public string Label
        {
            get;
        }

        /// <summary>
        /// Gets the description.
        /// </summary>
        /// <value>
        /// The component description.
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
            private set;
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
        /// When activated the switch lets through or stops the input from going through.
        /// </summary>
        public void Activate()
        {
            if (this.IsClosed)
            {
                this.IsClosed = false;
                this.Picture = Properties.Resources.SwitchOpen;
                this.FireOnPictureChanged();
            }
            else
            {
                this.IsClosed = true;
                this.Picture = Properties.Resources.SwitchClosed;
                this.FireOnPictureChanged();
            }
        }

        /// <summary>
        /// Executes changed the output pin weather the input is active and the switch is closed.
        /// </summary>
        public void Execute()
        {
            if (this.IsClosed)
            {
                if (this.Inputs.Any(x => (bool)x.Value.Current == true))
                {
                    foreach (var o in this.Outputs)
                    {
                        o.Value.Current = true;
                    }
                }
            }
            else
            {
                foreach (var o in this.Outputs)
                {
                    o.Value.Current = false;
                }
            }
        }

        /// <summary>
        /// Fires the on picture changed.
        /// </summary>
        protected virtual void FireOnPictureChanged()
        {
            this.PictureChanged?.Invoke(this, new EventArgs());
        }
    }
}
