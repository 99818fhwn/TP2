// -----------------------------------------------------------------------     
// <copyright file="PinVM.cs" company="FHWN">    
// Copyright (c) FHWN. All rights reserved.    
// </copyright>    
// <summary>Contains class for a pin view model.</summary>    
// -----------------------------------------------------------------------
namespace LogicDesigner.ViewModel
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using LogicDesigner.Commands;
    using Shared;
    
    /// <summary>
    /// The pin view model class.
    /// </summary>
    public class PinVM
    {
        /// <summary>
        /// The pin that is represented by this view model.
        /// </summary>
        private readonly IPin pin;

        /// <summary>
        /// The is input pin.
        /// </summary>
        private readonly bool isInputPin;

        /// <summary>
        /// The set pin command.
        /// </summary>
        private readonly Command setPinCommand;

        /// <summary>
        /// The x position.
        /// </summary>
        private double xposition;

        /// <summary>
        /// The y position.
        /// </summary>
        private double yposition;

        /// <summary>
        /// The parent.
        /// </summary>
        private ComponentVM parent;

        /// <summary>
        /// The active color.
        /// </summary>
        private Color activeColor;

        /// <summary>
        /// The passive color.
        /// </summary>
        private Color passiveColor;

        /// <summary>
        /// The is active boolean.
        /// </summary>
        private bool isActive;

        /// <summary>
        /// The unique number which is used to identify every <see cref="PinVM"/>.
        /// </summary>
        private int uniqueNumber;

        /// <summary>
        /// Initializes a new instance of the <see cref="PinVM" /> class.
        /// </summary>
        /// <param name="pin">The pin that is represented.</param>
        /// <param name="idNumber">The identifier number of the pin.</param>
        /// <param name="isInputPin">If set to <c>true</c> [is input pin].</param>
        /// <param name="setPinCommand">The set pin command.</param>
        /// <param name="parent">The parent of pin.</param>
        /// <param name="activeColor">Color of the active.</param>
        /// <param name="passiveColor">Color of the passive.</param>
        public PinVM(IPin pin, int idNumber, bool isInputPin, Command setPinCommand, ComponentVM parent, Color activeColor, Color passiveColor)
        {
            this.parent = parent;
            this.pin = pin;
            this.isInputPin = isInputPin;
            this.setPinCommand = setPinCommand;
            this.xposition = 0;
            this.yposition = 0;
            this.activeColor = activeColor;
            this.passiveColor = passiveColor;
            this.isActive = false;
            this.uniqueNumber = idNumber;
            this.InitialValue = pin.Value.Current == null ? null : Activator.CreateInstance(pin.Value.Current.GetType());
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PinVM" /> class.
        /// </summary>
        /// <param name="pin">The pin that is represented.</param>
        /// <param name="idNumber">The identifier number of the pin.</param>
        /// <param name="isInputPin">If set to <c>true</c> [is input pin].</param>
        /// <param name="setPinCommand">The set pin command.</param>
        public PinVM(IPin pin, int idNumber, bool isInputPin, Command setPinCommand)
        {
            this.pin = pin;
            this.isInputPin = isInputPin;
            this.setPinCommand = setPinCommand;
            this.xposition = 0;
            this.yposition = 0;
            this.uniqueNumber = idNumber;
            this.InitialValue = pin.Value.Current == null ? null : Activator.CreateInstance(pin.Value.Current.GetType());
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PinVM"/> class.
        /// </summary>
        public PinVM()
        {
        }

        /// <summary>
        /// Gets the pin.
        /// </summary>
        /// <value>
        /// The pin that is represented by the pin view model.
        /// </value>
        public IPin Pin
        {
            get
            {
                return this.pin;
            }
        }

        /// <summary>
        /// Gets the type of the pin value.
        /// </summary>
        /// <value>
        /// The type of the pin value.
        /// </value>
        public Type PinValueType
        {
            get
            {
                return this.pin.Value.Current?.GetType();
            }
        }

        /// <summary>
        /// Gets the identifier number.
        /// </summary>
        /// <value>
        /// The identifier number of the pin.
        /// </value>
        public int IDNumber
        {
            get
            {
                return this.uniqueNumber;
            }
        }

        /// <summary>
        /// Gets the initial value.
        /// </summary>
        /// <value>
        /// The initial value of the pin.
        /// </value>
        public object InitialValue
        {
            get;
        }

        /// <summary>
        /// Gets a value indicating whether this instance is input pin.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is input pin; otherwise, <c>false</c>.
        /// </value>
        public bool IsInputPin
        {
            get
            {
                return this.isInputPin;
            }
        }

        /// <summary>
        /// Gets the set pin command.
        /// </summary>
        /// <value>
        /// The set pin command.
        /// </value>
        public Command SetPinCommand
        {
            get
            {
                return this.setPinCommand;
            }
        }

        /// <summary>
        /// Gets or sets the x position.
        /// </summary>
        /// <value>
        /// The x position.
        /// </value>
        public double XPosition
        {
            get
            {
                return this.xposition + this.parent.XCoord;
            }

            set
            {
                this.xposition = value;
            }
        }

        /// <summary>
        /// Gets or sets the y position.
        /// </summary>
        /// <value>
        /// The y position.
        /// </value>
        public double YPosition
        {
            get
            {
                return this.yposition + this.parent.YCoord;
            }

            set
            {
                this.yposition = value;
            }
        }

        /// <summary>
        /// Gets the parent.
        /// </summary>
        /// <value>
        /// The parent.
        /// </value>
        public ComponentVM Parent
        {
            get
            {
                return this.parent;
            }
        }

        /// <summary>
        /// Gets or sets the color when pin is active.
        /// </summary>
        /// <value>
        /// The color of the active.
        /// </value>
        public Color ActiveColor
        {
            get
            {
                return this.activeColor;
            }

            set
            {
                this.activeColor = value;
            }
        }

        /// <summary>
        /// Gets or sets the color when pin is passive.
        /// </summary>
        /// <value>
        /// The color of the passive.
        /// </value>
        public Color PassiveColor
        {
            get
            {
                return this.passiveColor;
            }

            set
            {
                this.passiveColor = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="PinVM"/> is active.
        /// </summary>
        /// <value>
        ///   <c>true</c> if active; otherwise, <c>false</c>.
        /// </value>
        public bool Active
        {
            get
            {
                return this.isActive;
            }

            set
            {
                this.isActive = value;
            }
        }
    }
}
