﻿using Shared;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComponentLibrary
{
    public class ANDGate : IDisplayableNode
    {
        public ANDGate()
        {
            this.Inputs = new List<IPin>();
            this.Outputs = new List<IPin>();
            this.Label = "AND";
            this.Description = "If all inputs are true, the output is true";
            this.Picture = null;
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

        public event EventHandler PictureChanged;

        public void Execute()
        {
            if (!this.Inputs.Any(x => (bool)x.Value.Value == false))
            {
                foreach (var o in this.Outputs)
                {
                    o.Value.Value = true;
                }
            }
        }
    }
}