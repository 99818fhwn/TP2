using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogicDesigner.Model.Serialization
{
    public class SerializedConnectionVM
    {
        public string InputPinID { get; set; }
        public string OutputPinID { get; set; }
        public string InputParentID { get; set; }
        public string OutputParentID { get; set; }

        public double InputX { get; set; }
        public double InputY { get; set; }

        public double OutputX { get; set; }
        public double OutputY { get; set; }


        public SerializedConnectionVM(string input, string output, string inputParentID, string outputParentID, 
            double inputX, double inputY, double outputX, double outputY)
        {
            this.InputPinID = input;
            this.OutputPinID = output;
            this.InputParentID = inputParentID;
            this.OutputParentID = outputParentID;

            this.InputX = inputX;
            this.InputY = inputY;

            this.OutputX = outputX;
            this.OutputY = outputY;
        }

        public SerializedConnectionVM()
        {
        }
    }
}
