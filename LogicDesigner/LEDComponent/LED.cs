// -----------------------------------------------------------------------     
// <copyright file="LED.cs" company="FHWN">    
// Copyright (c) FHWN. All rights reserved.    
// </copyright>    
// <summary>The ANDGateComponent for logic designers that implements IDisplayableNode</summary>    
// <author>Fabian Weisser</author>    
// -----------------------------------------------------------------------
namespace LEDComponent
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Shared;
    using SharedClasses;

    /// <summary>
    /// The class that represents a LED.
    /// </summary>
    /// <seealso cref="Shared.IDisplayableNode" />
    public class LED : IDisplayableNode
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LED"/> class.
        /// </summary>
        public LED()
        {
            this.Inputs = new List<IPin>();
            this.Outputs = new List<IPin>();
            this.IsOn = false;
            this.Label = "LED";
            this.Description = "If input is true, the LED is on";
            this.Picture = Properties.Resources.LEDOff;
            this.Type = NodeType.Display;
            this.Inputs.Add(new GenericPin<bool>(new GenericValue<bool>(false), "Pin1"));
        }

        /// <summary>
        /// Occurs when [picture changed].
        /// </summary>
        public event EventHandler PictureChanged;

        /// <summary>
        /// Gets or sets a value indicating whether the LED is on.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this LED is on; otherwise, <c>false</c>.
        /// </value>
        public bool IsOn
        {
            get;
            set;
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
        /// The description of the LED.
        /// </value>
        public string Description
        {
            get;
        }

        /// <summary>
        /// Gets the picture.
        /// </summary>
        /// <value>
        /// The picture of the LED.
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
        /// The type of the component.
        /// </value>
        public NodeType Type
        {
            get;
        }

        /// <summary>
        /// Activates this instance.
        /// </summary>
        public void Activate()
        {
        }

        /// <summary>
        /// Executes the logic of the LED and if one input is active the LED is on.
        /// </summary>
        public void Execute()
        {
            if (this.Inputs.Any(x => (bool)x.Value.Current == true))
            {
                if (!this.IsOn)
                {
                    this.IsOn = true;
                    this.Picture = Properties.Resources.LEDOn;
                    this.FireOnPictureChanged();
                }
            }
            else
            {
                if (this.IsOn)
                {
                    this.IsOn = false;
                    this.Picture = Properties.Resources.LEDOff;
                    this.FireOnPictureChanged();
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
