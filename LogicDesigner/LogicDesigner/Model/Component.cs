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
            this.Label = info.GetString(nameof(Label));
            this.Description = info.GetString(nameof(Description));
            this.Type = (NodeType) info.GetValue(nameof(Type), typeof(NodeType));
            this.Inputs = (List<IPin>)info.GetValue(nameof(Inputs), typeof(List<IPin>));
            this.Outputs = (List<IPin>)info.GetValue(nameof(Outputs), typeof(List<IPin>));
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(Inputs), Inputs, Inputs.GetType());
            info.AddValue(nameof(Outputs), Outputs, Outputs.GetType());
            info.AddValue(nameof(Label), Label, Label.GetType());
            info.AddValue(nameof(Description), Description, Description.GetType());
            info.AddValue(nameof(Type), Type, Type.GetType());
        }
    }
}
