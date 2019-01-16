using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared;

namespace ComponentLibrary
{
    public class BoolANDGate : IDisplayableNode
    {
        public BoolANDGate()
        {
            this.Inputs = new List<IPin>();
            this.Outputs = new List<IPin>();
            this.Label = "AND";
            this.Description = "If all inputs are true, the output is true";
            this.Picture = Properties.Resources.ANDGate;
            this.Type = NodeType.Logic;
            this.Inputs.Add(new BooleanPin(new BooleanValue(false), "Pin1"));
            this.Inputs.Add(new BooleanPin(new BooleanValue(false), "Pin2"));
            this.Outputs.Add(new BooleanPin(new BooleanValue(false), "Pin3"));
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
            throw new NotImplementedException();
        }

        public void Execute()
        {
            if (!this.Inputs.Any(x => (bool)x.Value.Current == false))
            {
                foreach (var o in this.Outputs)
                {
                    o.Value.Current = false;
                }
            }
        }
    }
}
