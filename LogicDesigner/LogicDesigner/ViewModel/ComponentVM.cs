using LogicDesigner.Commands;
using LogicDesigner.Model.Configuration;
using Shared;
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

namespace LogicDesigner.ViewModel
{
    public class ComponentVM : INotifyPropertyChanged, ISerializable
    {
        [NonSerialized]
        private IDisplayableNode node;

        [NonSerialized]
        private readonly Command removeCommand;

        private readonly ConfigurationLogic config;

        private double xCoord;
        private double yCoord;
        private readonly string uniqueName;

        public event PropertyChangedEventHandler PropertyChanged;

        public event EventHandler<FieldComponentEventArgs> ComponentPropertyChanged;

        public ComponentVM(IDisplayableNode realComponent, string uniqueName, 
            Command setPinCommand, Command removeCommand, ConfigurationLogic configurationLogic)
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
                if(pin != null)
                {
                    this.OutputPinsVM.Add(new PinVM(pin, false, setPinCommand, this, this.config.PinActiveColor, this.config.PinPassiveColor));
                    //this.OutputPinsVM.Add(new PinVM(pin, false, setPinCommand));
                }

            }
            
            foreach (var pin in this.node.Inputs)
            {
                if (pin != null)
                {
                    this.InputPinsVM.Add(new PinVM(pin, true, setPinCommand, this, this.config.PinActiveColor, this.config.PinPassiveColor));
                    //this.InputPinsVM.Add(new PinVM(pin, true, setPinCommand));
                }
            }
        }

        public ComponentVM()
        {

        }

        public string Label
        {
            get { return this.node.Label; }
        }

        public string Name
        {
            get { return this.uniqueName; }
        }

        public string TextValue
        {
            get { return this.node.Outputs.ElementAt(0).Value.ToString(); }
        }

        public string Identifier
        {
            get => this.uniqueName;
        }


        public double XCoord
        {
            get { return this.xCoord; }
            set
            {
                this.xCoord = value;
                this.FireOnPropertyChanged();
            }
        }

        public double YCoord
        {
            get { return this.yCoord; }
            set
            {
                this.yCoord = value;
                this.FireOnPropertyChanged();
            }
        }

        public BitmapSource Image
        {
            get
            {
                try
                {
                    return Imaging.CreateBitmapSourceFromHBitmap(this.Picture.GetHbitmap(), IntPtr.Zero, 
                        Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

        public Bitmap Picture
        {
            get { return this.node.Picture; }
        }

        public bool IsInField
        {
            get;
            set;
        }

        public IDisplayableNode Node
        {
            get
            {
                return this.node;
            }
        }

        public string Description
        {
            get
            {
                return this.node.Description;
            }
        }

        public Command RemoveComponentCommand
        {
            get
            {
                return this.removeCommand;
            }
        }

        public ObservableCollection<PinVM> OutputPinsVM
        {
            get;
        }

        public ObservableCollection<PinVM> InputPinsVM
        {
            get;
        }

        public void Activate()
        {
            this.node.Activate();
        }

        public void Execute()
        {
            this.node.Execute();
        }

        protected virtual void FireOnComponentPropertyChanged(ComponentVM componentVM)
        {
            this.ComponentPropertyChanged?.Invoke(this, new FieldComponentEventArgs(componentVM));
        }

        protected void OnPictureChanged(object sender, EventArgs e)
        {
            this.FireOnPropertyChanged(nameof(this.Picture));
            this.FireOnComponentPropertyChanged(this);
        }

        protected void FireOnPropertyChanged([CallerMemberName]string name = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        internal ComponentVM(SerializationInfo info, StreamingContext context)
        {
            this.node = (IDisplayableNode)info.GetValue(nameof(this.Node), typeof(IDisplayableNode));

            this.XCoord = (double)info.GetValue(nameof(this.XCoord), typeof(double));
            this.YCoord = (double)info.GetValue(nameof(this.YCoord), typeof(double));
            this.IsInField = (bool)info.GetValue(nameof(this.IsInField), typeof(bool));

            this.uniqueName = (string)info.GetValue(nameof(this.Name), typeof(string));

            this.removeCommand = (Command)info.GetValue(nameof(this.RemoveComponentCommand), typeof(Command));
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(this.Node), this.Node, this.Node.GetType());

            info.AddValue(nameof(this.XCoord), this.XCoord, this.XCoord.GetType());
            info.AddValue(nameof(this.YCoord), this.YCoord, this.YCoord.GetType());
            info.AddValue(nameof(this.IsInField), this.IsInField, this.IsInField.GetType());

            info.AddValue(nameof(this.Name), this.Name, this.Name.GetType());

            info.AddValue(nameof(this.RemoveComponentCommand), this.RemoveComponentCommand, 
                this.RemoveComponentCommand.GetType());
        }

    }
}
