﻿using Shared;
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

namespace LogicDesigner.Model
{
    public class ProgramManager
    {
        /// <summary>
        /// The field nodes
        /// </summary>
        private ICollection<IDisplayableNode> fieldNodes;

        /// <summary>
        /// The possible nodes to choose from
        /// </summary>
        private readonly ICollection<IDisplayableNode> possibleNodesToChooseFrom;

        public FileSystemWatcher Watcher { get; set; }

        public ProgramManager()
        {
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
            // test - connect pins
            //for (int i = 0; i < this.possibleNodesToChooseFrom.Count(); i++)
            //{
            //    for (int g = 0; g < this.possibleNodesToChooseFrom.Count(); g++)
            //    {
            //        try
            //        {
            //            this.ConnectPins(this.possibleNodesToChooseFrom.ElementAt(i).Outputs.ElementAt(0),
            //            this.possibleNodesToChooseFrom.ElementAt(g).Inputs.ElementAt(0));
            //        }
            //        catch(ArgumentOutOfRangeException)
            //        {

            //        }
            //    }
            //}
        }

        public ProgramManager (ProgramManager old)
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
            while(!this.Stop)
            {
                this.RunCircle();
            }
        }

        public void RunCircle()
        {
            foreach(INode node in this.fieldNodes)
            {
                if(!this.Stop)
                {
                    node.Execute();
                    //MessageBox.Show("Step made");
                    Thread.Sleep(this.Delay);

                    //this.Step();
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
