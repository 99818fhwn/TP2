using Shared;
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
            this.Picture = ;
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
            if (true)
            {

            }
        }
    }
}
