using Shared;
using SharedClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwitchComponent
{
    public class Switch : IDisplayableNode
    {
        public Switch()
        {
            this.Inputs = new List<IPin>();
            this.Outputs = new List<IPin>();
            this.Label = "OR";
            this.Description = "If one or more inputs are true, the output is true";
            this.Picture = Properties.Resources.SwitchOpen;
            this.Type = NodeType.Logic;
            this.IsClosed = false;
            this.Inputs.Add(new GenericPin<bool>(new GenericValue<bool>(false), "Pin1"));
            this.Outputs.Add(new GenericPin<bool>(new GenericValue<bool>(false), "Pin2"));
        }

        public bool IsClosed
        {
            get; set;
        }

        public ICollection<IPin> Inputs
        {
            get;
        }

        public ICollection<IPin> Outputs
        {
            get;
        }

        public string Label
        {
            get;
        }

        public string Description
        {
            get;
        }

        public System.Drawing.Bitmap Picture
        {
            get;
            private set;
        }

        public NodeType Type
        {
            get;
        }

        public event EventHandler PictureChanged;

        public void Activate()
        {
            if (this.IsClosed)
            {
                this.IsClosed = false;
                this.Picture = Properties.Resources.SwitchClosed;
                this.FireOnPictureChanged();
            }
            else
            {
                this.IsClosed = true;
                this.Picture = Properties.Resources.SwitchOpen;
                this.FireOnPictureChanged();
            }
        }

        protected virtual void FireOnPictureChanged()
        {
            this.PictureChanged?.Invoke(this, new EventArgs());
        }

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
    }
}
