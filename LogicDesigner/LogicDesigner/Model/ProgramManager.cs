﻿using LogicDesigner.ViewModel;
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
        private readonly string logFileName;

        /// <summary>
        /// The possible nodes to choose from
        /// </summary>
        private ICollection<IDisplayableNode> possibleNodesToChooseFrom;
        private readonly string path;
        public event EventHandler<PinsConnectedEventArgs> PinsDisconnected;
        public event EventHandler<PinsConnectedEventArgs> ConnectionUpdated;

        public ICollection<Tuple<IDisplayableNode, string>> SerializationPathInfo { get; set; }

        private readonly string componentDirectory;

        private readonly string logDirectory;

        public FileSystemWatcher Watcher { get; set; }

        public event EventHandler StepFinished;

        public ProgramManager()
        {
            //this.path = path;
            //this.connectedOutputInputPairs = new List<Tuple<IPin, IPin>>();
            this.componentDirectory = "Components";
            this.logDirectory = "LogFiles";
            this.ConnectedOutputInputPairs = new List<Tuple<IPin, IPin>>();
            this.RunActive = false;
            this.Delay = 1000; // milli sec = 1 sec
            this.fieldNodes = new List<IDisplayableNode>();
            this.InitializeNodesToChooseFromVoid();
            this.SerializationPathInfo = this.InitializeNodesToChooseFrom();

            if (!Directory.Exists(Path.GetDirectoryName(this.componentDirectory)))
            {
                List<IDisplayableNode> moduleList = new List<IDisplayableNode>();
                foreach (var module in this.SerializationPathInfo)
                {
                    moduleList.Add(module.Item1);
                }
                this.possibleNodesToChooseFrom = (ICollection<IDisplayableNode>)moduleList;
            }

            if (!Directory.Exists(Path.GetDirectoryName(this.componentDirectory)))
            {
                Directory.CreateDirectory(this.componentDirectory);
            }

            if (!Directory.Exists(Path.GetDirectoryName(this.logDirectory)))
            {
                Directory.CreateDirectory(this.logDirectory);
                this.logFileName = "Log_" + DateTime.Now.ToString("ddd d MMM yyyy HH mm ss").Replace(" ", "_") + ".txt";
                if (!File.Exists(Path.Combine(this.logDirectory, this.logFileName)))
                {
                    using (File.Create(Path.Combine(this.logDirectory, this.logFileName))) { };
                }

                this.WriteToLog(new string[] { "Log initialized" });
            }

            this.Watcher = new FileSystemWatcher(this.componentDirectory);
            this.Watcher.IncludeSubdirectories = true;
            this.Watcher.EnableRaisingEvents = true;
            this.Watcher.Filter = "";
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProgramManager"/> class.
        /// </summary>
        /// <param name="old">The old.</param>
        public ProgramManager(ProgramManager old)
        {
            this.Delay = old.Delay;
            this.FieldNodes = old.FieldNodes;
            this.logFileName = old.logFileName;
            this.possibleNodesToChooseFrom = old.PossibleNodesToChooseFrom;
            this.RunActive = old.RunActive;
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
            set
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
            private set
            {
                this.possibleNodesToChooseFrom = value;
            }
        }

        public int Delay
        {
            get;
            private set;
        }

        public bool RunActive
        {
            get;
            private set;
        }

        private ICollection<Tuple<IDisplayableNode, string>> InitializeNodesToChooseFrom()
        {
            return new NodesLoader().GetNodes(this.componentDirectory);
        }

        public void InitializeNodesToChooseFromVoid()
        {
            var moduleList = new List<IDisplayableNode>();

            foreach (var module in new NodesLoader().GetNodes(this.componentDirectory))
            {
                moduleList.Add(module.Item1);
            }

            this.PossibleNodesToChooseFrom = moduleList;
        }

        public void Run()
        {
            while (this.RunActive)
            {
                this.RunLoop(this.Delay);
            }
        }

        public void RunLoop(int delay)
        {
            try
            {
                foreach (var t in this.ConnectedOutputInputPairs)
                {
                    t.Item2.Value.Current = t.Item1.Value.Current;
                }

                foreach (INode node in this.fieldNodes)
                {
                    if (!this.RunActive)
                    {
                        return;
                    }

                    foreach (var t in this.ConnectedOutputInputPairs)
                    {
                        if (!this.RunActive)
                        {
                            return;
                        }

                        t.Item2.Value.Current = t.Item1.Value.Current;

                        this.OnConnectionUpdated(t.Item1, t.Item2);

                        //if(t.Item2.Value.Current.GetType() == typeof(bool))
                        //{
                        //    if((bool)t.Item2.Value.Current == true)
                        //    {
                        //        // fire event on connection change color to VM

                        //    }
                        //}
                        //else if (t.Item2.Value.Current.GetType() == typeof(string))
                        //{
                        //    if (!string.IsNullOrEmpty((string)t.Item2.Value.Current))
                        //    {
                        //        // fire event on connection change color to VM
                        //        this.OnConnectionUpdated(t.Item2, t.Item1);
                        //    }
                        //}
                        //else if (t.Item2.Value.Current.GetType() == typeof(int))
                        //{
                        //    if ((int)t.Item2.Value.Current != 0)
                        //    {
                        //        // fire event on connection change color to VM
                        //        this.OnConnectionUpdated(t.Item2, t.Item1);
                        //    }
                        //}
                    }

                    node.Execute();
                    Task.Delay(delay);
                }

                this.FireOnStepFinished();
            }
            catch (Exception e)
            {
                List<string> message = new List<string>() { "Error:" , "Time: " + DateTime.Now.ToString("H:mm:ss")
                    , "Source: " + e.Source, "ErrorType:" + e.GetType().ToString() , "ErrorMessage: " + e.Message, "" };
                this.WriteToLog(message.ToArray());
            }
        }

        public void WriteToLog(string[] logMessage)
        {
            File.AppendAllLines(Path.Combine(this.logDirectory, this.logFileName), logMessage);
        }

        protected virtual void FireOnStepFinished()
        {
            this.StepFinished?.Invoke(this, new EventArgs());
        }

        protected virtual void OnConnectionUpdated(IPin output, IPin input)
        {
            this.ConnectionUpdated?.Invoke(this, new PinsConnectedEventArgs(output, input));
        }

        public void Step(INode node)
        {
            if (!this.RunActive)
            {
                node.Execute();
                Task.Delay(this.Delay);
            }
        }

        public void SetActive()
        {
            this.RunActive = true;
        }

        public void StopActive()
        {
            this.RunActive = false;
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

        public void RemoveConnection(IPin outputPin, IPin inputPin)
        {
            foreach (var conn in this.connectedOutputInputPairs)
            {
                if (conn.Item1 == outputPin && conn.Item2 == inputPin)
                {
                    this.connectedOutputInputPairs.Remove(conn);
                    break;
                }
            }
        }
    }
}
