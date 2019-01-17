// -----------------------------------------------------------------------     
// <copyright file="StringDisplay.cs" company="FHWN">    
// Copyright (c) FHWN. All rights reserved.    
// </copyright>    
// <summary>The ANDGateComponent for logic designers that implements IDisplayableNode</summary>    
// <author>Fabian Weisser</author>    
// -----------------------------------------------------------------------
namespace StringDisplayComponent
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Configuration;
    using Shared;
    using SharedClasses;

    /// <summary>
    /// The class that represents a string display.
    /// </summary>
    /// <seealso cref="Shared.IDisplayableNode" />
    public class StringDisplay : IDisplayableNode
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StringDisplay"/> class.
        /// </summary>
        public StringDisplay()
        {
            this.SetValuesByConfig();
            this.Inputs = new List<IPin>();
            this.Outputs = new List<IPin>();
            this.Label = "StringDisplay";
            const string V = "If int input has a value it will beconverted to string and " +
                             "writen in the output, the integer will be prefered in case of int and string input are active";
            this.Description = V;
            this.Picture = Properties.Resources.StringDisplay;
            this.Type = NodeType.Logic;
            this.Pin1 = new GenericPin<string>(new GenericValue<string>(string.Empty), "Pin1");
            this.Inputs.Add(this.Pin1);
            this.Pin2 = new GenericPin<int>(new GenericValue<int>(-1), "Pin2");
            this.Inputs.Add(this.Pin2);
            this.Outputs.Add(new GenericPin<string>(new GenericValue<string>(string.Empty), "Pin3"));
        }

        /// <summary>
        /// Occurs when [picture changed].
        /// </summary>
        public event EventHandler PictureChanged;

        /// <summary>
        /// Gets or sets the offset.
        /// </summary>
        /// <value>
        /// The offset of the ASCII conversion.
        /// </value>
        public int Offset
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets the pin1.
        /// </summary>
        /// <value>
        /// The pin1 that allows string input to be displayed and redirected to the output 
        /// this input will be ignored if pin2 has a value different than -1.
        /// </value>
        public IPin Pin1
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets the pin2.
        /// </summary>
        /// <value>
        /// The pin2 that allows the component to convert an integer to ASCII + an defined offset, this pin
        /// overrides the input of pin2.
        /// </value>
        public IPin Pin2
        {
            get; set;
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
        /// The picture that represents the string display.
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
        /// Sets the values by the configuration file.
        /// </summary>
        public void SetValuesByConfig()
        {
            int offsetValue = 0;

            if (File.Exists("stringdisplay_config.json"))
            {
                var conf = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("stringdisplay_config.json")
                .Build();
                var valid = int.TryParse(conf.GetSection("Offset")["Number"], out offsetValue);
            }

            this.Offset = offsetValue;
        }

        /// <summary>
        /// Activates this instance.
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
            if ((int)this.Pin2.Value.Current != -1)
            {
                foreach (var p in this.Outputs)
                {
                    p.Value.Current = ((char)(int)this.Pin2.Value.Current + this.Offset).ToString();
                }
            }
            else
            {
                foreach (var p in this.Outputs)
                {
                    p.Value.Current = (string)this.Pin1.Value.Current;
                }
            }
        }
    }
}
