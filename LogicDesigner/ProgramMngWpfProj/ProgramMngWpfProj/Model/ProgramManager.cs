using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ProgramMngWpfProj.Model
{
    public class ProgramManager
    {
        private ICollection<INode> fieldNodes;
        private readonly ICollection<INode> possibleNodesToChooseFrom;

        public ProgramManager()
        {
            this.Stop = false;
            this.Delay = 1000; // milli sec = 1 sec
            this.fieldNodes = new List<INode>();
            this.possibleNodesToChooseFrom = this.InitializeNodesToChooseFrom();
        }

        private ICollection<INode> InitializeNodesToChooseFrom()
        {
            return new NodesLoader().GetNodes("Components");
        }

        public int Delay
        {
            get;
            private set;
        }
        public bool Stop
        {
            get;
            private set;
        }

        public void Run()
        {
            while(!this.Stop)
            {
                this.Step();
            }
        }

        public void Step()
        {
            foreach(INode node in this.fieldNodes)
            {
                node.Execute();
                Thread.Sleep(this.Delay);
            }
        }

        public void StopProgram()
        {
            this.Stop = true;
        }

        public bool ConnectPines(IPinGeneric output, IPinGeneric input)
        {
            var outputType = output.Value.Value.GetType().GetGenericTypeDefinition();
            var inputType = input.Value.Value.GetType().GetGenericTypeDefinition();

            if(outputType != inputType)
            {
                return false;
            }
            
                // if no value in both nodes - new value reference
            if(output.Value.Value == null && input.Value.Value == null)
            {
                IValue instance = (IValue)Activator.CreateInstance(outputType);

                output.Value.Value = instance;
                input.Value.Value = output.Value.Value; // value or value value
                
            }
            else if(output.Value.Value == null && input.Value.Value != null)
            {
                output.Value.Value = input.Value.Value;
            }
            else if (input.Value.Value != null && output.Value.Value == null)
            {
                input.Value.Value = output.Value.Value;
            }

            return true;
        }
    }
}
