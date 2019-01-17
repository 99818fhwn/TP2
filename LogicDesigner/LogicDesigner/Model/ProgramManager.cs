using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LogicDesigner.Model
{
    public class ProgramManager
    {
        private ICollection<IDisplayableNode> fieldNodes;
        private readonly ICollection<IDisplayableNode> possibleNodesToChooseFrom;

        public ProgramManager()
        {
            this.Stop = false;
            this.Delay = 1000; // milli sec = 1 sec
            this.fieldNodes = new List<IDisplayableNode>();
            this.possibleNodesToChooseFrom = this.InitializeNodesToChooseFrom();

            // test
            for(int i = 0; i < this.possibleNodesToChooseFrom.Count(); i++)
            {
                for (int g = 0; g < this.possibleNodesToChooseFrom.Count(); g++)
                {
                    this.ConnectPins(this.possibleNodesToChooseFrom.ElementAt(i).Outputs.ElementAt(0),
                    this.possibleNodesToChooseFrom.ElementAt(g).Inputs.ElementAt(0));
                }
            }

            this.ConnectPins(this.possibleNodesToChooseFrom.Last().Outputs.ElementAt(0),
                    this.possibleNodesToChooseFrom.Last().Inputs.ElementAt(0));
        }

        private ICollection<IDisplayableNode> InitializeNodesToChooseFrom()
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

        public bool ConnectPins(IPin output, IPin input)
        {
            var outputType = output.Value.Current.GetType();
            //var outputType = output.Value.Current.GetType().GetGenericTypeDefinition();
            var inputType = input.Value.Current.GetType();

            if(outputType != inputType)
            {
                return false;
            }
            
             // if no value in both nodes - new value reference
             // not null - int bool no null value 
            if(output.Value.Current == null && input.Value.Current == null)
            {
                IValue instance = (IValue)Activator.CreateInstance(outputType);

                output.Value.Current = instance;
                input.Value.Current = output.Value.Current;
                return true;
            }

            input.Value.Current = output.Value.Current;

            //else if(output.Value.Current == null && input.Value.Current != null)
            //{
            //    output.Value.Current = input.Value.Current;
            //}
            //else if (input.Value.Current != null && output.Value.Current == null)
            //{
            //    input.Value.Current = output.Value.Current;
            //}

            return true;
        }
    }
}
