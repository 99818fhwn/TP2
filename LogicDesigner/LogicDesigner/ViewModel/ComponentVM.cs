// -----------------------------------------------------------------------     
// <copyright file="ComponentVM.cs" company="FHWN">    
// Copyright (c) FHWN. All rights reserved.    
// </copyright>    
// <summary>The component view model.</summary>
// -----------------------------------------------------------------------
namespace LogicDesigner.ViewModel
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Drawing;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Interop;
    using System.Windows.Media.Imaging;
    using LogicDesigner.Commands;
    using LogicDesigner.Model.Configuration;
    using Shared;

    /// <summary>
    /// The component view model class.
    /// </summary>
    /// <seealso cref="System.ComponentModel.INotifyPropertyChanged" />
    /// <seealso cref="System.Runtime.Serialization.ISerializable" />
    public class ComponentVM : INotifyPropertyChanged, ISerializable
    {
        /// <summary>
        /// The remove command.
        /// </summary>
        [NonSerialized]
        private readonly Command removeCommand;

        /// <summary>
        /// The unique name.
        /// </summary>
        private readonly string uniqueName;

        /// <summary>
        /// The configuration.
        /// </summary>
        private readonly ConfigurationLogic config;

        /// <summary>
        /// The node that is represented by this view model.
        /// </summary>
        [NonSerialized]
        private IDisplayableNode node;
        
        /// <summary>
        /// The x coordinate.
        /// </summary>
        private double xCoord;

        /// <summary>
        /// The y coordinate.
        /// </summary>
        private double yCoord;

        /// <summary>
        /// Initializes a new instance of the <see cref="ComponentVM" /> class.
        /// </summary>
        /// <param name="realComponent">The real component.</param>
        /// <param name="uniqueName">The unique name of the component.</param>
        /// <param name="pindIdAtion">The pin identifier action.</param>
        /// <param name="setPinCommand">The set pin command.</param>
        /// <param name="removeCommand">The remove command.</param>
        /// <param name="configurationLogic">The configuration logic.</param>
        public ComponentVM(
            IDisplayableNode realComponent,
            string uniqueName, 
            IEnumerator<int> pindIdAtion,
            Command setPinCommand,
            Command removeCommand,
            ConfigurationLogic configurationLogic)
        {
            this.node = realComponent;
            this.uniqueName = uniqueName;
            this.removeCommand = removeCommand;
            this.config = configurationLogic;

            this.node.PictureChanged += this.OnPictureChanged;

            this.OutputPinsVM = new ObservableCollection<PinVM>();
            this.InputPinsVM = new ObservableCollection<PinVM>();

            foreach (var pin in this.node.Outputs)
            {
                if (pin != null)
                {
                    pindIdAtion.MoveNext();
                    this.OutputPinsVM.Add(new PinVM(pin, pindIdAtion.Current, false, setPinCommand, this, this.config.PinActiveColor, this.config.PinPassiveColor));
                    
                    // this.OutputPinsVM.Add(new PinVM(pin, false, setPinCommand));
                }
            }
            
            foreach (var pin in this.node.Inputs)
            {
                if (pin != null)
                {
                    pindIdAtion.MoveNext();
                    this.InputPinsVM.Add(new PinVM(pin, pindIdAtion.Current, true, setPinCommand, this, this.config.PinActiveColor, this.config.PinPassiveColor));
                    
                    // this.InputPinsVM.Add(new PinVM(pin, true, setPinCommand));
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ComponentVM"/> class.
        /// </summary>
        public ComponentVM()
        {
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="ComponentVM"/> class.
        /// </summary>
        /// <param name="info">The information.</param>
        /// <param name="context">The context.</param>
        internal ComponentVM(SerializationInfo info, StreamingContext context)
        {
            this.node = (IDisplayableNode)info.GetValue(nameof(this.Node), typeof(IDisplayableNode));

            this.XCoord = (double)info.GetValue(nameof(this.XCoord), typeof(double));
            this.YCoord = (double)info.GetValue(nameof(this.YCoord), typeof(double));
            this.IsInField = (bool)info.GetValue(nameof(this.IsInField), typeof(bool));

            this.uniqueName = (string)info.GetValue(nameof(this.Name), typeof(string));

            this.removeCommand = (Command)info.GetValue(nameof(this.RemoveComponentCommand), typeof(Command));
        }

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Occurs when the component property changed.
        /// </summary>
        public event EventHandler<FieldComponentEventArgs> ComponentPropertyChanged;

        /// <summary>
        /// Gets the label.
        /// </summary>
        /// <value>
        /// The label.
        /// </value>
        public string Label
        {
            get { return this.node.Label; }
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>
        /// The unique name that is used in the view to identify the user interface element.
        /// </value>
        public string Name
        {
            get { return this.uniqueName; }
        }

        /// <summary>
        /// Gets the text value.
        /// </summary>
        /// <value>
        /// The text value.
        /// </value>
        public string TextValue
        {
            get { return this.node.Outputs.ElementAt(0).Value.ToString(); }
        }

        /// <summary>
        /// Gets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        public string Identifier
        {
            get => this.uniqueName;
        }

        /// <summary>
        /// Gets or sets the x coordinate.
        /// </summary>
        /// <value>
        /// The x coordinate.
        /// </value>
        public double XCoord
        {
            get
            {
                return this.xCoord;
            }

            set
            {
                this.xCoord = value;
                this.FireOnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the y coordinate.
        /// </summary>
        /// <value>
        /// The y coordinate.
        /// </value>
        public double YCoord
        {
            get
            {
                return this.yCoord;
            }

            set
            {
                this.yCoord = value;
                this.FireOnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets the image.
        /// </summary>
        /// <value>
        /// The image.
        /// </value>
        public BitmapSource Image
        {
            get
            {
                try
                {
                    return Imaging.CreateBitmapSourceFromHBitmap(
                        this.Picture.GetHbitmap(),
                        IntPtr.Zero, 
                        Int32Rect.Empty,
                        BitmapSizeOptions.FromEmptyOptions());
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Gets the picture.
        /// </summary>
        /// <value>
        /// The picture.
        /// </value>
        public Bitmap Picture
        {
            get { return this.node.Picture; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is in field.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is in field; otherwise, <c>false</c>.
        /// </value>
        public bool IsInField
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the node.
        /// </summary>
        /// <value>
        /// The node tat is represented by the component view model.
        /// </value>
        public IDisplayableNode Node
        {
            get
            {
                return this.node;
            }
        }

        /// <summary>
        /// Gets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        public string Description
        {
            get
            {
                return this.node.Description;
            }
        }

        /// <summary>
        /// Gets the remove component command.
        /// </summary>
        /// <value>
        /// The remove component command.
        /// </value>
        public Command RemoveComponentCommand
        {
            get
            {
                return this.removeCommand;
            }
        }

        /// <summary>
        /// Gets the output pins.
        /// </summary>
        /// <value>
        /// The output pins.
        /// </value>
        public ObservableCollection<PinVM> OutputPinsVM
        {
            get;
        }

        /// <summary>
        /// Gets the input pins.
        /// </summary>
        /// <value>
        /// The input pins.
        /// </value>
        public ObservableCollection<PinVM> InputPinsVM
        {
            get;
        }

        /// <summary>
        /// Activates this instance.
        /// </summary>
        public void Activate()
        {
            this.node.Activate();
        }

        /// <summary>
        /// Executes this instance.
        /// </summary>
        public void Execute()
        {
            this.node.Execute();
        }
        
        /// <summary>
        /// Populates a <see cref="T:System.Runtime.Serialization.SerializationInfo" /> with the data needed to serialize the target object.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> to populate with data.</param>
        /// <param name="context">The destination (see <see cref="T:System.Runtime.Serialization.StreamingContext" />) for this serialization.</param>
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(this.Node), this.Node, this.Node.GetType());

            info.AddValue(nameof(this.XCoord), this.XCoord, this.XCoord.GetType());
            info.AddValue(nameof(this.YCoord), this.YCoord, this.YCoord.GetType());
            info.AddValue(nameof(this.IsInField), this.IsInField, this.IsInField.GetType());

            info.AddValue(nameof(this.Name), this.Name, this.Name.GetType());

            info.AddValue(
                nameof(this.RemoveComponentCommand),
                this.RemoveComponentCommand, 
                this.RemoveComponentCommand.GetType());
        }

        /// <summary>
        /// Fires the on property changed.
        /// </summary>
        /// <param name="name">The name of the calling property.</param>
        protected void FireOnPropertyChanged([CallerMemberName]string name = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        /// <summary>
        /// Called when picture changed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void OnPictureChanged(object sender, EventArgs e)
        {
            this.FireOnPropertyChanged(nameof(this.Picture));
            this.FireOnComponentPropertyChanged(this);
        }

        /// <summary>
        /// Fires the on component property changed.
        /// </summary>
        /// <param name="componentVM">The component.</param>
        protected virtual void FireOnComponentPropertyChanged(ComponentVM componentVM)
        {
            this.ComponentPropertyChanged?.Invoke(this, new FieldComponentEventArgs(componentVM));
        }
    }
}
