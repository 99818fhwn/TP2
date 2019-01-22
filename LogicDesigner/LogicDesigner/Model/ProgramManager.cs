using LogicDesigner.ViewModel;
using Shared;
using SharedClasses;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace LogicDesigner.Model
{
    public class ProgramManager
    {
        /// <summary>
        /// The field nodes
        /// </summary>
        private ICollection<IDisplayableNode> fieldNodes;
        private List<Tuple<IPin, IPin>> connectedOutputInputPairs;

        /// <summary>
        /// The possible nodes to choose from
        /// </summary>
        private readonly ICollection<IDisplayableNode> possibleNodesToChooseFrom;
        private readonly string path;
        public event EventHandler<PinsConnectedEventArgs> PinsDisconnected;

        private readonly string componentDirectory;

        public FileSystemWatcher Watcher { get; set; }

        public event EventHandler StepFinished;

        public ProgramManager()
        {
            //this.path = path;
            //this.connectedOutputInputPairs = new List<Tuple<IPin, IPin>>();
            this.componentDirectory = "Components";
            this.ConnectedOutputInputPairs = new List<Tuple<IPin, IPin>>();
            this.Stop = false;
            this.Delay = 1000; // milli sec = 1 sec
            this.fieldNodes = new List<IDisplayableNode>();
            this.possibleNodesToChooseFrom = this.InitializeNodesToChooseFrom();

            if (!Directory.Exists(Path.GetDirectoryName(componentDirectory)))
            {
                Directory.CreateDirectory(componentDirectory);
            }
            this.Watcher = new FileSystemWatcher(componentDirectory);
            Watcher.IncludeSubdirectories = true;
            Watcher.EnableRaisingEvents = true;
            Watcher.Filter = "";
        }

        public ProgramManager(ProgramManager old)
        {
            this.Delay = old.Delay;
            this.FieldNodes = old.FieldNodes;
            possibleNodesToChooseFrom = old.PossibleNodesToChooseFrom;
            this.Stop = old.Stop;
        }

        public List<Tuple<IPin, IPin>> ConnectedOutputInputPairs
        {
            get
            {
                return this.connectedOutputInputPairs;
            }
            set
            {
                this.connectedOutputInputPairs = value;
            }
        }

        public ICollection<IDisplayableNode> FieldNodes
        {
            get
            {
                return this.fieldNodes;
            }
            private set
            {
                this.fieldNodes = value;
            }
        }

        public ICollection<IDisplayableNode> PossibleNodesToChooseFrom
        {
            get
            {
                return this.possibleNodesToChooseFrom;
            }
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

        private ICollection<IDisplayableNode> InitializeNodesToChooseFrom()
        {
            return new NodesLoader().GetNodes(componentDirectory);
        }

        public void Run()
        {
            while (!this.Stop)
            {
                this.RunLoop(this.Delay);
            }
        }

        public void RunLoop(int delay)
        {
            foreach (var t in this.ConnectedOutputInputPairs)
            {
                t.Item2.Value.Current = t.Item1.Value.Current;
            }

            foreach (INode node in this.fieldNodes)
            {
                if (!this.Stop)
                {
                    foreach (var t in this.ConnectedOutputInputPairs)
                    {
                        t.Item2.Value.Current = t.Item1.Value.Current;
                    }

                    node.Execute();
                    Task.Delay(delay);
                }
                else
                {
                    break;
                }
            }

            this.FireOnStepFinished();
        }

        protected virtual void FireOnStepFinished()
        {
            this.StepFinished?.Invoke(this, new EventArgs());
        }

        public void Step(INode node)
        {
            if (!this.Stop)
            {
                node.Execute();
                Task.Delay(this.Delay);
            }
        }

        public void StopProgram()
        {
            this.Stop = true;
        }

        public bool ConnectPins(IPin output, IPin input)
        {
            var outputType = output.Value.Current?.GetType();
            var inputType = input.Value.Current?.GetType();

            if (outputType == null || inputType == null)
            {
                return false;
            }

            if (outputType != inputType)
            {
                return false;
            }

            this.UnConnectPin(input);
            this.UnConnectPins(output, input);
            this.ConnectedOutputInputPairs.Add(new Tuple<IPin, IPin>(output, input));

           

            return true;
        }

        private void UnConnectPin(IPin input)
        {
            foreach (var t in this.ConnectedOutputInputPairs)
            {
                if (t.Item2 == input)
                {
                    this.ConnectedOutputInputPairs.Remove(t);
                    this.OnDisconnectedPins(this, new PinsConnectedEventArgs(t.Item1, input));
                    break;
                }
            }
        }

        public void UnConnectPins(IPin output, IPin input)
        {
            foreach (var t in this.ConnectedOutputInputPairs)
            {
                if (t.Item1 == output && t.Item2 == input)
                {
                    this.ConnectedOutputInputPairs.Remove(t);
                    this.OnDisconnectedPins(this, new PinsConnectedEventArgs(output, input));
                    break;
                }
            }
        }

        protected void OnDisconnectedPins(object source, PinsConnectedEventArgs e)
        {
            this.PinsDisconnected?.Invoke(source, e);
        }
    }
}
