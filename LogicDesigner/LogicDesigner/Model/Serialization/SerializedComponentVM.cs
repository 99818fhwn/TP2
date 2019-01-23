using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LogicDesigner.ViewModel;

namespace LogicDesigner.Model.Serialization
{
    public class SerializedComponentVM
    {
        public double XPos { get; set; }
        public double YPos { get; set; }
        public string AssemblyPath { get; set; }
        public string UniqueName { get; set; }

        public SerializedComponentVM(double xpos, double ypos, string path, string id)
        {
            this.XPos = xpos;
            this.YPos = ypos;
            this.AssemblyPath = path;
            this.UniqueName = id;
        }

        public SerializedComponentVM(ComponentVM baseVm, string assemblyPath)
        {
            this.XPos = baseVm.XCoord;
            this.YPos = baseVm.YCoord;
            this.AssemblyPath = assemblyPath;
            this.UniqueName = baseVm.Identifier;
        }

        public SerializedComponentVM()
        {
        }
    }
}
