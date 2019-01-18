// -----------------------------------------------------------------------     
// <copyright file="BinaryConverter.cs" company="FHWN">    
// Copyright (c) FHWN. All rights reserved.    
// </copyright>    
// <summary>The BinaryConverterComponent for logic designers that implements IDisplayableNode</summary>    
// <author>Fabian Weisser</author>    
// -----------------------------------------------------------------------
namespace BinaryConverterComponent
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
    public class BinaryConverter : IDisplayableNode
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BinaryConverter"/> class.
        /// </summary>
        public BinaryConverter()
        {
            this.Inputs = new List<IPin>();
            this.Outputs = new List<IPin>();
            this.Label = "BinaryConverter";
            this.Description = "Convert the incoming signal (bit) by converting from binrary to decimal (integer)";
            this.Picture = Properties.Resources.BinrayConverter;
            this.Type = NodeType.Logic;
            this.Pin1 = new GenericPin<bool>(new GenericValue<bool>(false), "Pin1");
            this.Inputs.Add(this.Pin1);
            this.Pin2 = new GenericPin<bool>(new GenericValue<bool>(false), "Pin2");
            this.Inputs.Add(this.Pin2);
            this.Pin3 = new GenericPin<bool>(new GenericValue<bool>(false), "Pin3");
            this.Inputs.Add(this.Pin3);
            this.Pin4 = new GenericPin<bool>(new GenericValue<bool>(false), "Pin4");
            this.Inputs.Add(this.Pin4);
            this.Outputs.Add(new GenericPin<int>(new GenericValue<int>(0), "Pin5"));
        }

        /// <summary>
        /// Occurs when [picture changed].
        /// </summary>
        public event EventHandler PictureChanged;

        /// <summary>
        /// Gets or sets the pin1.
        /// </summary>
        /// <value>
        /// The the signal value.
        /// </value>
        public IPin Pin1
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the pin2.
        /// </summary>
        /// <value>
        /// The the signal value.
        /// </value>
        public IPin Pin2
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the pin3.
        /// </summary>
        /// <value>
        /// The the signal value.
        /// </value>
        public IPin Pin3
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the pin3.
        /// </summary>
        /// <value>
        /// The the signal value.
        /// </value>
        public IPin Pin4
        {
            get;
            set;
        }

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
        /// Converts the signals of the input pins to bytes and then to an integer.
        /// </summary>
        public void Execute()
        {
            var boolArr = new bool[] { (bool)this.Pin1.Value.Current, (bool)this.Pin2.Value.Current, (bool)this.Pin3.Value.Current, (bool)this.Pin4.Value.Current };
            string bitRepres = string.Empty;

            foreach (var v in boolArr)
            {
                if (v)
                {
                    bitRepres += 1;
                }
                else
                {
                    bitRepres += 0;
                }
            }

            int resultNumber = Convert.ToInt32(bitRepres, 2);

            foreach (var o in this.Outputs)
            {
                o.Value.Current = resultNumber;
            }
        }
    }
}
