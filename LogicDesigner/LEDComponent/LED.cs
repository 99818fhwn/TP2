using Shared;
using SharedClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LEDComponent
{
    public class LED : IDisplayableNode
    {
        public LED()
        {
            this.Inputs = new List<IPin>();
            this.Outputs = new List<IPin>();
            this.IsOn = false;
            this.Label = "LED";
            this.Description = "If input is true, the LED is on";
            this.Picture = Properties.Resources.LEDOff;
            this.Type = NodeType.Logic;
            this.Inputs.Add(new GenericPin<bool>(new GenericValue<bool>(false), "Pin1"));
        }

        public bool IsOn
        {
            get;
            set;
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
            if (this.IsOn)
            {
                this.IsOn = false;
                this.Picture = Properties.Resources.SwitchClosed;
                this.FireOnPictureChanged();
            }
            else
            {
                this.IsOn = true;
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
            if (this.Inputs.Any(x => (bool)x.Value.Current == true))
            {
                this.IsOn = true;
                this.Picture = Properties.Resources.SwitchOpen;
                this.FireOnPictureChanged();
            }
            else
            {

            }
        }
    }
}
