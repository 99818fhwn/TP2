using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LogicDesigner.ViewModel;

namespace LogicDesigner.Model.Serialization
{
    public class SerializedObject
    {
        public List<SerializedComponentVM> Components { get; set; }
        public List<SerializedConnectionVM> Connections { get; set; }
        //public List<string> AssemblyPaths { get; set; }

       // public SerializedComponent(ICollection<SerializedComponentVM> vms, ICollection<string> paths, ICollection<ConnectionVM> connections)
        public SerializedObject(ICollection<SerializedComponentVM> vms, ICollection<SerializedConnectionVM> connections)
        {
            this.Components = new List<SerializedComponentVM>(vms);
            this.Connections = new List<SerializedConnectionVM>(connections);
            //this.AssemblyPaths = new List<string>(paths);
        }

        public SerializedObject()
        {
            this.Components = new List<SerializedComponentVM>();
            this.Connections = new List<SerializedConnectionVM>();
            //this.AssemblyPaths = new List<string>();
        }
    }
}
