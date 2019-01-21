using LogicDesigner.Commands;
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

namespace LogicDesigner.ViewModel
{
    public class ComponentVM : INotifyPropertyChanged, ISerializable
    {
        private IDisplayableNode node;
        private readonly Command activateCommand;
        private readonly Command removeCommand;
       // private readonly Command executeCommand;
        private double xCoord;
        private double yCoord;
        private readonly string uniqueName;

        public event PropertyChangedEventHandler PropertyChanged;

        public event EventHandler<FieldComponentEventArgs> ComponentPropertyChanged;

        //public ComponentVM(IDisplayableNode node, Command activateCommand, 
        //    Command executeCommand, Command removeCommand, string uniqueName)
        //{
        //    this.node = node;
        //    this.activateCommand = activateCommand;
        //    this.executeCommand = executeCommand;
        //    this.removeCommand = removeCommand;
        //    this.uniqueName = uniqueName;

        //    node.PictureChanged += this.OnPictureChanged;
        //}

        public ComponentVM(IDisplayableNode realComponent, string uniqueName, 
            Command setPinCommand, Command activateCommand, Command removeCommand)
        {
            this.node = realComponent;
            this.uniqueName = uniqueName;
            this.activateCommand = activateCommand;
            this.removeCommand = removeCommand;

            this.node.PictureChanged += this.OnPictureChanged;

            this.OutputPinsVM = new ObservableCollection<PinVM>();
            this.InputPinsVM = new ObservableCollection<PinVM>();

            foreach (var pin in this.node.Outputs)
            {
                if(pin != null)
                {
                    this.OutputPinsVM.Add(new PinVM(pin, false, setPinCommand));
                }

            }

            foreach (var pin in this.node.Inputs)
            {
                if (pin != null)
                {
                    this.InputPinsVM.Add(new PinVM(pin, true, setPinCommand));
                }
            }
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

        public Command RemoveComponentCommand
        {
            get
            {
                return this.removeCommand;
            }
        }

        public Command ActivateComponentCommand
        {
            get
            {
                return this.activateCommand;
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

        //public Command ExecuteCommand
        //{
        //    get
        //    {
        //        return this.executeCommand;
        //    }
        //}

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

            this.activateCommand = (Command)info.GetValue(nameof(this.ActivateComponentCommand), typeof(Command));
            this.removeCommand = (Command)info.GetValue(nameof(this.RemoveComponentCommand), typeof(Command));
            //this.executeCommand = (Command)info.GetValue(nameof(this.ExecuteCommand), typeof(Command));
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(this.Node), this.Node, this.Node.GetType());

            info.AddValue(nameof(this.XCoord), this.XCoord, this.XCoord.GetType());
            info.AddValue(nameof(this.YCoord), this.YCoord, this.YCoord.GetType());
            info.AddValue(nameof(this.IsInField), this.IsInField, this.IsInField.GetType());

            info.AddValue(nameof(this.Name), this.Name, this.Name.GetType());

            info.AddValue(nameof(this.ActivateComponentCommand), this.ActivateComponentCommand, this.ActivateComponentCommand.GetType());
            //info.AddValue(nameof(this.ExecuteCommand), this.ExecuteCommand, this.ExecuteCommand.GetType());
            info.AddValue(nameof(this.RemoveComponentCommand), this.RemoveComponentCommand, this.RemoveComponentCommand.GetType());
        }

    }
}
