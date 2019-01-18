﻿using LogicDesigner.Commands;
using Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace LogicDesigner.ViewModel
{
    public class ComponentVM : INotifyPropertyChanged
    {
        private IDisplayableNode node;
        private readonly Command activateCommand;
        private readonly Command addCommand;
        private readonly Command removeCommand;
        private readonly Command executeCommand;
        private int xCoord;
        private int yCoord;
        private readonly string uniqueName;

        public event PropertyChangedEventHandler PropertyChanged;

        public event EventHandler<FieldComponentEventArgs> SpeacialPropertyChanged;

        public ComponentVM(IDisplayableNode node, Command activateCommand, Command addCommand, 
            Command executeCommand, Command removeCommand, string uniqueName)
        {
            this.node = node;
            this.activateCommand = activateCommand;
            this.addCommand = addCommand;
            this.executeCommand = executeCommand;
            this.removeCommand = removeCommand;
            this.uniqueName = uniqueName;

            node.PictureChanged += this.OnPictureChanged;
        }

        protected virtual void FireOnComponentPropertyChanged(ComponentVM componentVM)
        {
            this.SpeacialPropertyChanged?.Invoke(this, new FieldComponentEventArgs(componentVM));
        }

        internal void OnPictureChanged(object sender, EventArgs e)
        {
            this.FireOnPropertyChanged(nameof(this.Picture));
            this.FireOnComponentPropertyChanged(this);
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

        public int XCoord
        {
            get { return this.xCoord; }
            set
            {
                this.xCoord = value;
                this.FireOnPropertyChanged();
            }
        }

        public int YCoord
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

        public Command AddComponentCommand
        {
            get
            {
                return this.addCommand;
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

        public Command ExecuteCommand
        {
            get
            {
                return this.executeCommand;
            }
        }

        public void Activate()
        {
            this.node.Activate();
        }

        public void Execute()
        {
            this.node.Execute();
        }

        protected void FireOnPropertyChanged([CallerMemberName]string name = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

    }
}
