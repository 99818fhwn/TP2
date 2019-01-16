using Shared;
using SharedClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ORGateComponent
{
    public class ORGate : IDisplayableNode
    {
        public ORGate()
        {
            this.Inputs = new List<IPin>();
            this.Outputs = new List<IPin>();
            this.Label = "OR";
            this.Description = "If one or more inputs are true, the output is true";
            this.Picture = Properties.Resources.ORGate;
            this.Type = NodeType.Logic;
            this.Inputs.Add(new GenericPin<bool>(new GenericValue<bool>(false), "Pin1"));
            this.Inputs.Add(new GenericPin<bool>(new GenericValue<bool>(false), "Pin2"));
            this.Outputs.Add(new GenericPin<bool>(new GenericValue<bool>(false), "Pin3"));
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
        }

        public NodeType Type
        {
            get;
        }

        public event EventHandler PictureChanged;

        public void Activate()
        {
            return;
        }

        public void Execute()
        {
            if (this.Inputs.Any(x => (bool)x.Value.Current == true))
            {
                foreach (var o in this.Outputs)
                {
                    o.Value.Current = true;
                }
            }
        }
    }
}
