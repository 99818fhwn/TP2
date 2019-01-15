using System.Collections.Generic;

namespace Shared
{
    public interface INode
    {
        ICollection<IPinGeneric> Inputs { get; }

        ICollection<IPinGeneric> Outputs { get; }

        void Execute();
        void Activate();

        string Label { get; }

        string Description { get; }
    }
}
