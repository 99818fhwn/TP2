// -----------------------------------------------------------------------     
// <copyright file="ProgramManager.cs" company="FHWN">    
// Copyright (c) FHWN. All rights reserved.    
// </copyright>    
// <summary>Containg the program manager class.</summary>
// -----------------------------------------------------------------------
namespace LogicDesigner.Model
{
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
    using LogicDesigner.Model.Configuration;
    using LogicDesigner.ViewModel;
    using Shared;
    using SharedClasses;

    /// <summary>
    /// The program manager class.
    /// </summary>
    public class ProgramManager
    {
        /// <summary>
        /// The log file name.
        /// </summary>
        private readonly string logFileName;
        
        /// <summary>
        /// The path.
        /// </summary>
        private readonly string path;

        /// <summary>
        /// The component directory.
        /// </summary>
        private readonly string componentDirectory;

        /// <summary>
        /// The configuration.
        /// </summary>
        private readonly ConfigurationLogic config;

        /// <summary>
        /// The log directory
        /// </summary>
        private readonly string logDirectory;

        /// <summary>
        /// The field nodes
        /// </summary>
        private ICollection<IDisplayableNode> fieldNodes;

        /// <summary>
        /// The connected output input pairs.
        /// </summary>
        private List<Tuple<IPin, IPin>> connectedOutputInputPairs;
        
        /// <summary>
        /// The possible nodes to choose from.
        /// </summary>
        private ICollection<IDisplayableNode> possibleNodesToChooseFrom;
                
        /// <summary>
        /// Initializes a new instance of the <see cref="ProgramManager"/> class.
        /// </summary>
        public ProgramManager()
        {
            // this.path = path;
            // this.connectedOutputInputPairs = new List<Tuple<IPin, IPin>>();
            this.config = new ConfigurationLogic();
            this.componentDirectory = this.config.ModulePath;
            this.logDirectory = this.config.LogPath;
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
                    using (File.Create(Path.Combine(this.logDirectory, this.logFileName)))
                    {
                    }
                }

                this.WriteToLog(new string[] { "Log initialized" });
            }

            this.Watcher = new FileSystemWatcher(this.componentDirectory);
            this.Watcher.IncludeSubdirectories = true;
            this.Watcher.EnableRaisingEvents = true;
            this.Watcher.Filter = string.Empty;
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
        
        /// <summary>
        /// Occurs when pins get disconnected.
        /// </summary>
        public event EventHandler<PinsConnectedEventArgs> PinsDisconnected;

        /// <summary>
        /// Occurs when connection is updated.
        /// </summary>
        public event EventHandler<PinsConnectedEventArgs> ConnectionUpdated;

        /// <summary>
        /// Occurs when a step finished.
        /// </summary>
        public event EventHandler StepFinished;
        
        /// <summary>
        /// Gets or sets the serialization path information.
        /// </summary>
        /// <value>
        /// The serialization path information.
        /// </value>
        public ICollection<Tuple<IDisplayableNode, string>> SerializationPathInfo { get; set; }

        /// <summary>
        /// Gets or sets the watcher.
        /// </summary>
        /// <value>
        /// The watcher.
        /// </value>
        public FileSystemWatcher Watcher { get; set; }

        /// <summary>
        /// Gets or sets the connected output input pairs.
        /// </summary>
        /// <value>
        /// The connected output input pairs.
        /// </value>
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

        /// <summary>
        /// Gets or sets the field nodes.
        /// </summary>
        /// <value>
        /// The field nodes.
        /// </value>
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

        /// <summary>
        /// Gets the possible nodes to choose from.
        /// </summary>
        /// <value>
        /// The possible nodes to choose from.
        /// </value>
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

        /// <summary>
        /// Gets the delay.
        /// </summary>
        /// <value>
        /// The delay.
        /// </value>
        public int Delay
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a value indicating whether run is active.
        /// </summary>
        /// <value>
        /// <c>true</c> if run is active; otherwise, <c>false</c>.
        /// </value>
        public bool RunActive
        {
            get;
            private set;
        }

        /// <summary>
        /// Initializes the nodes to choose from void.
        /// </summary>
        public void InitializeNodesToChooseFromVoid()
        {
            var moduleList = new List<IDisplayableNode>();

            foreach (var module in new NodesLoader().GetNodes(this.componentDirectory))
            {
                moduleList.Add(module.Item1);
            }

            this.PossibleNodesToChooseFrom = moduleList;
        }

        /// <summary>
        /// Runs this instance.
        /// </summary>
        public void Run()
        {
            while (this.RunActive)
            {
                this.RunLoop(this.Delay);
            }
        }

        /// <summary>
        /// Runs the loop.
        /// </summary>
        /// <param name="delay">The delay.</param>
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
                    }

                    node.Execute();
                    Task.Delay(delay);
                }

                this.FireOnStepFinished();
            }
            catch (Exception e)
            {
                List<string> message = new List<string>()
                {
                    "Error:",
                    "Time: " + DateTime.Now.ToString("H:mm:ss"),
                    "Source: " + e.Source,
                    "ErrorType:" + e.GetType().ToString(),
                    "ErrorMessage: " + e.Message
                };

                this.WriteToLog(message.ToArray());
            }
        }

        /// <summary>
        /// Writes to log.
        /// </summary>
        /// <param name="logMessage">The log message.</param>
        public void WriteToLog(string[] logMessage)
        {
            File.AppendAllLines(Path.Combine(this.logDirectory, this.logFileName), logMessage);
        }
        
        /// <summary>
        /// Steps the specified node.
        /// </summary>
        /// <param name="node">The node.</param>
        public void Step(INode node)
        {
            if (!this.RunActive)
            {
                node.Execute();
                Task.Delay(this.Delay);
            }
        }

        /// <summary>
        /// Sets to active.
        /// </summary>
        public void SetActive()
        {
            this.RunActive = true;
        }

        /// <summary>
        /// Stops the active.
        /// </summary>
        public void StopActive()
        {
            this.RunActive = false;
        }

        /// <summary>
        /// Connects the pins.
        /// </summary>
        /// <param name="output">The output.</param>
        /// <param name="input">The input.</param>
        /// <returns>Connection status.</returns>
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
        
        /// <summary>
        /// Disconnects pins.
        /// </summary>
        /// <param name="output">The output.</param>
        /// <param name="input">The input.</param>
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

        /// <summary>
        /// Removes the connection.
        /// </summary>
        /// <param name="outputPin">The output pin.</param>
        /// <param name="inputPin">The input pin.</param>
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

        /// <summary>
        /// Fires the on step finished.
        /// </summary>
        protected virtual void FireOnStepFinished()
        {
            this.StepFinished?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// Called when connection is updated.
        /// </summary>
        /// <param name="output">The output.</param>
        /// <param name="input">The input.</param>
        protected virtual void OnConnectionUpdated(IPin output, IPin input)
        {
            this.ConnectionUpdated?.Invoke(this, new PinsConnectedEventArgs(output, input));
        }

        /// <summary>
        /// Called when pins get disconnected.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="e">The <see cref="LogicDesigner.ViewModel.PinsConnectedEventArgs" /> instance containing the event data.</param>
        protected void OnDisconnectedPins(object source, PinsConnectedEventArgs e)
        {
            this.PinsDisconnected?.Invoke(source, e);
        }

        /// <summary>
        /// Initializes the nodes to choose from.
        /// </summary>
        /// <returns>Possible nodes to chose from.</returns>
        private ICollection<Tuple<IDisplayableNode, string>> InitializeNodesToChooseFrom()
        {
            return new NodesLoader().GetNodes(this.componentDirectory);
        }

        /// <summary>
        /// Disconnects a pin.
        /// </summary>
        /// <param name="input">The input.</param>
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
    }
}
