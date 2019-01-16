using Shared;
using SharedClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StringDisplayComponent
{
    public class StringDisplay : IDisplayableNode
    {
        public StringDisplay()
        {
            this.Inputs = new List<IPin>();
            this.Outputs = new List<IPin>();
            this.Label = "StringDisplay";
            const string V = "If int input has a value it will beconverted to string and " +
                             "writen in the output, the integer will be prefered in case of int and string input are active";
            this.Description = V;
            this.Picture = Properties.Resources.StringDisplay;
            this.Type = NodeType.Logic;
            this.Pin1 = new GenericPin<string>(new GenericValue<string>(""), "Pin1");
            this.Inputs.Add(this.Pin1);
            this.Pin2 = new GenericPin<int>(new GenericValue<int>(-1), "Pin2");
            this.Inputs.Add(this.Pin2);
            this.Outputs.Add(new GenericPin<string>(new GenericValue<string>(""), "Pin3"));
        }

        public IPin Pin1
        {
            get; set;
        }

        public IPin Pin2
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
            if ((int)this.Pin2.Value.Current != -1)
            {
                foreach (var p in this.Outputs)
                {
                    p.Value.Current = ((char)(int)this.Pin2.Value.Current).ToString();
                }
            }
            else
            {
                foreach (var p in this.Outputs)
                {
                    p.Value.Current = (string)this.Pin1.Value.Current;
                }
            }

        }
    }
}
