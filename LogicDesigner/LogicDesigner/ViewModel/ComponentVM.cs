using LogicDesigner.Commands;
using Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace LogicDesigner.ViewModel
{
    public class ComponentVM : INotifyPropertyChanged
    {
        private IDisplayableNode node;
        private Command activateCommand;
        private int xCoord;
        private int yCoord;

        public event PropertyChangedEventHandler PropertyChanged;

        public ComponentVM(IDisplayableNode node, Command activateCommand)
        {
            this.node = node;
            this.activateCommand = activateCommand;
        }

        public string Label
        {
            get { return this.node.Label; }
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

        public bool IsInField
        {
            get;
            set;
        }

        public object Node
        {
            get
            {
                return this.node;
            }
        }

        public void Activate()
        {
            this.node.Activate();
            this.FireOnPropertyChanged();
            // clicked -> changed -> should be actualized
        }

        protected void FireOnPropertyChanged([CallerMemberName]string name = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

    }
}
