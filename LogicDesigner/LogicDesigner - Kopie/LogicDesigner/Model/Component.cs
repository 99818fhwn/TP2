using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace LogicDesigner.Model
{
    [Serializable()]
    public class Component : INode, ISerializable
    {
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

        public NodeType Type
        {
            get;
        }

        public void Activate()
        {
            throw new NotImplementedException();
        }

        public void Execute()
        {
            throw new NotImplementedException();
        }

        internal Component(SerializationInfo info, StreamingContext context)
        {
            this.Label = info.GetString(nameof(this.Label));
            this.Description = info.GetString(nameof(this.Description));
            this.Type = (NodeType) info.GetValue(nameof(this.Type), typeof(NodeType));
            this.Inputs = (List<IPin>)info.GetValue(nameof(this.Inputs), typeof(List<IPin>));
            this.Outputs = (List<IPin>)info.GetValue(nameof(this.Outputs), typeof(List<IPin>));
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(this.Inputs), this.Inputs, this.Inputs.GetType());
            info.AddValue(nameof(this.Outputs), this.Outputs, this.Outputs.GetType());
            info.AddValue(nameof(this.Label), this.Label, this.Label.GetType());
            info.AddValue(nameof(this.Description), this.Description, this.Description.GetType());
            info.AddValue(nameof(this.Type), this.Type, this.Type.GetType());
        }
    }
}
