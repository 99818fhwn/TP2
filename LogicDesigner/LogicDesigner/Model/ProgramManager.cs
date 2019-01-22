using Shared;
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
        private List<Tuple<IPin, IPin>> ConnectedOutputInputPairs;

        /// <summary>
        /// The possible nodes to choose from
        /// </summary>
        private readonly ICollection<IDisplayableNode> possibleNodesToChooseFrom;

        public FileSystemWatcher Watcher { get; set; }

        public ProgramManager()
        {
            this.ConnectedOutputInputPairs = new List<Tuple<IPin, IPin>>();
            this.Stop = false;
            this.Delay = 1000; // milli sec = 1 sec
            this.fieldNodes = new List<IDisplayableNode>();
            this.possibleNodesToChooseFrom = this.InitializeNodesToChooseFrom();

            string path = "Components";
            if (!Directory.Exists(Path.GetDirectoryName(path)))
            {
                Directory.CreateDirectory(path);
            }
            this.Watcher = new FileSystemWatcher(path);
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
            return new NodesLoader().GetNodes("Components");
        }

        public void Run()
        {
            //var t = Task.Run(() =>
            //{
            //    this.RunCircle();
            //});

            while (!this.Stop)
            {
                //Dispatcher.CurrentDispatcher.BeginInvoke(new ThreadStart(() =>
                //{
                //    this.RunCircle();
                //}));

                //Dispatcher.CurrentDispatcher.BeginInvoke(new ThreadStart(t.Start));

                this.RunCircle();
            }
        }

        public void RunCircle()
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

                    //var tsk = Task.Run(() =>
                    //{
                    //    node.Execute();
                    //});
                    

                    //Dispatcher.CurrentDispatcher.BeginInvoke(new ThreadStart(tsk.Start));

                    node.Execute();
                    Thread.Sleep(this.Delay);
                }
                else
                {
                    break;
                }
            }
        }

        public void Step(INode node)
        {
            if (!this.Stop)
            {
                node.Execute();
                //MessageBox.Show("Step made");
                //Thread.Sleep(this.Delay);
            }
        }

        public void StopProgram()
        {
            this.Stop = true;
        }

        public bool ConnectPins(IPin output, IPin input)
        {
            var outputType = output.Value.Current.GetType();
            var inputType = input.Value.Current.GetType();

            if (outputType != inputType)
            {
                return false;
            }

            this.UnConnectPins(output, input);
            this.ConnectedOutputInputPairs.Add(new Tuple<IPin, IPin>(output, input));

            //if(output.Value.Current == null && input.Value.Current == null)
            //{
            //    //IValue instanc = (IValue)Activator.CreateInstance(outputType);

            //    //output.Value.Current = instanc;

            //    var instance = Activator.CreateInstance(outputType);

            //    output.Value.Current = instance;

            //    input.Value.Current = output.Value.Current;
            //    input.Value = output.Value;

            //    return true;
            //}

            //input.Value = output.Value;

            //input.Value = Activator.CreateInstance(input.Value.GetType());
            //input.Value.Current = output.Value.Current;

            // test
            //try
            //{
            //    output.Value.Current = true;
            //}
            //catch(Exception)
            //{

            //}

            return true;
        }

        public void UnConnectPins(IPin output, IPin input)
        {
            foreach (var t in this.ConnectedOutputInputPairs)
            {
                if (t.Item1 == output && t.Item2 == input)
                {
                    this.ConnectedOutputInputPairs.Remove(t);
                    break;
                }
            }
        }
    }
}
