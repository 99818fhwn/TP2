

namespace LogicDesigner.ViewModel
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Threading;
    using LogicDesigner.Commands;
    using LogicDesigner.Model;
    using LogicDesigner.Model.Configuration;
    using LogicDesigner.Model.Serialization;
    using Shared;

    public class ProgramMngVM : INotifyPropertyChanged
    {
        private ProgramManager programManager;
        private ObservableCollection<ConnectionVM> connectionsVM;
        private ObservableCollection<ComponentVM> nodesVMInField;
        private ObservableCollection<ComponentRepresentationVM> selectableComponents;
        private int uniqueNodeId;
        private PinVM selectedOutputPin;
        private PinVM selectedInputPin;
        private Stack<Tuple<ObservableCollection<ConnectionVM>, ObservableCollection<ComponentVM>>> undoHistoryStack;
        private Stack<Tuple<ObservableCollection<ConnectionVM>, ObservableCollection<ComponentVM>>> redoHistoryStack;

        public event EventHandler<FieldComponentEventArgs> FieldComponentAdded;
        //public event EventHandler<EventArgs> PreFieldComponentAdded;
        public event EventHandler<FieldComponentEventArgs> FieldComponentRemoved;
        public event EventHandler<FieldComponentEventArgs> FieldComponentChanged;
        public event EventHandler<PinVMConnectionChangedEventArgs> PinsConnected;
        public event EventHandler<PinVMConnectionChangedEventArgs> PinsDisconnected;
        public event PropertyChangedEventHandler PropertyChanged;

        private ComponentVM selectedFieldComponent;
        private int newUniqueConnectionId;

        private readonly Command removeCommand;
        private readonly Command setPinCommand;
        private readonly Command addCommand;
        private readonly ConfigurationLogic config;

        public ProgramMngVM()
        {
            this.programManager = new ProgramManager();
            this.programManager.Watcher.Created += this.NewModuleAdded;
            this.programManager.StepFinished += this.RefreshVM;

            this.config = new ConfigurationLogic();

            this.StartCommand = new Command(obj =>
            {
                Dispatcher.CurrentDispatcher.Invoke(() => Task.Run(() =>
                {
                    this.programManager.Run();
                }));
            });

            this.StepCommand = new Command(obj =>
            {
                if (!this.programManager.Stop)
                {
                    this.programManager.RunLoop(0); // step
                }
            });

            this.StopCommand = new Command(obj =>
            {
                this.programManager.StopProgram();
            });

            this.setPinCommand = new Command(obj =>
            {
                var pin = obj as PinVM;
                this.SetSelectedPin(pin);
            });

            this.removeCommand = new Command(obj =>
            {
                var nodeInFieldVM = obj as ComponentVM;

                // Warum das foreach?
                foreach (var n in this.programManager.FieldNodes)
                {
                    if (nodeInFieldVM.Node == n)
                    {
                        this.programManager.FieldNodes.Remove(n);
                        this.nodesVMInField.Remove(nodeInFieldVM);
                        this.OnFieldComponentRemoved(this, new FieldComponentEventArgs(nodeInFieldVM));
                        this.RemoveDeletedComponentConnections(nodeInFieldVM);
                        this.UpdateUndoHistory(); ////Not sure if i have to update the program manager !!!!!!!!
                        break;
                    }
                }

                ////Or i just place it here ... but then the model isnt synced
                this.FireOnComponentVMRemoved(nodeInFieldVM);
                this.programManager.FieldNodes.Remove(nodeInFieldVM.Node);
            });

            this.addCommand = new Command(obj =>
            {
                var representationNode = obj as ComponentRepresentationVM;
                //this.PreFieldComponentAdded(this, new EventArgs());
                var realComponent = representationNode.Node;
                var newGenerateComp = (IDisplayableNode)Activator.CreateInstance(realComponent.GetType());
                this.programManager.FieldNodes.Add(newGenerateComp);

                var compVM = new ComponentVM(newGenerateComp, this.CreateUniqueName(realComponent), this.setPinCommand,
                    this.removeCommand, this.config);
                this.nodesVMInField.Add(compVM);
                this.FireOnFieldComponentAdded(compVM);
                this.UpdateUndoHistory();
            });

            this.UndoCommand = new Command(obj =>
            {
                if (this.undoHistoryStack.Count > 0)
                {
                    var history = this.undoHistoryStack.Pop();
                    this.redoHistoryStack.Push(history);
                    var differencesComps = new List<ComponentVM>(this.NodesVMInField.Except(history.Item2));
                    differencesComps.AddRange(new List<ComponentVM>(history.Item2.Except(this.NodesVMInField)));

                    if (differencesComps.Count == 0 && this.undoHistoryStack.Count > 0)
                    {
                        history = this.undoHistoryStack.Pop();
                        this.redoHistoryStack.Push(history);
                        differencesComps = new List<ComponentVM>(this.NodesVMInField.Except(history.Item2));
                        differencesComps.AddRange(new List<ComponentVM>(history.Item2.Except(this.NodesVMInField)));
                    }

                    foreach (var item in differencesComps)
                    {
                        if (!history.Item2.Contains(item))
                        {
                            this.OnFieldComponentRemoved(this, new FieldComponentEventArgs(item));
                            this.RemoveDeletedComponentConnections(item);
                        }
                        else
                        {
                            this.FireOnFieldComponentAdded(item);
                        }
                    }

                    var differencesConnects = new List<ConnectionVM>(this.ConnectionsVM.Except(history.Item1));
                    differencesConnects.AddRange(new List<ConnectionVM>(history.Item1.Except(this.ConnectionsVM)));

                    foreach (var item in differencesConnects)
                    {
                        if (!history.Item1.Contains(item))
                        {
                            this.OnPinsDisconnected(this, new PinsConnectedEventArgs(item.OutputPin.Pin, item.InputPin.Pin));
                        }
                        else
                        {
                            this.OnPinsConnected(this, new PinVMConnectionChangedEventArgs(item));
                        }
                    }

                    this.ConnectionsVM = new ObservableCollection<ConnectionVM>(history.Item1);
                    this.NodesVMInField = new ObservableCollection<ComponentVM>(history.Item2);
                    //this.programManager.ConnectedOutputInputPairs = this.ConnectionsVM.Select(x => new Tuple<IPin, IPin>(x.))
                }
            });

            this.RedoCommand = new Command(obj =>
            {
                if (this.redoHistoryStack.Count > 0)
                {
                    var futureHistory = this.redoHistoryStack.Pop();
                    this.undoHistoryStack.Push(futureHistory);
                    var differencesComps = new List<ComponentVM>(this.NodesVMInField.Except(futureHistory.Item2));
                    differencesComps.AddRange(new List<ComponentVM>(futureHistory.Item2.Except(this.NodesVMInField)));

                    if (differencesComps.Count == 0 && this.redoHistoryStack.Count > 0)
                    {
                        futureHistory = this.redoHistoryStack.Pop();
                        this.undoHistoryStack.Push(futureHistory);
                        differencesComps = new List<ComponentVM>(this.NodesVMInField.Except(futureHistory.Item2));
                        differencesComps.AddRange(new List<ComponentVM>(futureHistory.Item2.Except(this.NodesVMInField)));
                    }

                    foreach (var item in differencesComps)
                    {
                        if (!futureHistory.Item2.Contains(item))
                        {
                            this.OnFieldComponentRemoved(this, new FieldComponentEventArgs(item));
                            this.RemoveDeletedComponentConnections(item);
                        }
                        else
                        {
                            this.FireOnFieldComponentAdded(item);
                        }
                    }

                    var differencesConnects = new List<ConnectionVM>(this.ConnectionsVM.Except(futureHistory.Item1));
                    differencesConnects.AddRange(new List<ConnectionVM>(futureHistory.Item1.Except(this.ConnectionsVM)));

                    foreach (var item in differencesConnects)
                    {
                        if (!futureHistory.Item1.Contains(item))
                        {
                            this.OnPinsDisconnected(this, new PinsConnectedEventArgs(item.OutputPin.Pin, item.InputPin.Pin));
                        }
                        else
                        {
                            this.OnPinsConnected(this, new PinVMConnectionChangedEventArgs(item));
                        }
                    }

                    this.ConnectionsVM = new ObservableCollection<ConnectionVM>(futureHistory.Item1);
                    this.NodesVMInField = new ObservableCollection<ComponentVM>(futureHistory.Item2);
                }
            });

            var nodesInField = this.programManager.FieldNodes.Select(node => new ComponentVM(node,
                this.CreateUniqueName(node), this.setPinCommand, this.removeCommand, this.config
                ));

            this.nodesVMInField = new ObservableCollection<ComponentVM>(nodesInField);

            var nodesToChoose = this.programManager.SerializationPathInfo.Select(
                node => new ComponentRepresentationVM(this.addCommand, node.Item1, node.Item2));

            this.SelectableComponents = new ObservableCollection<ComponentRepresentationVM>(nodesToChoose);

            var connections = this.programManager.ConnectedOutputInputPairs.Select(conn =>
            new ConnectionVM(new PinVM(conn.Item1, false, this.setPinCommand),
            new PinVM(conn.Item2, true, this.setPinCommand), this.NewUniqueConnectionId()));

            this.connectionsVM = new ObservableCollection<ConnectionVM>(connections);
            this.undoHistoryStack = new Stack<Tuple<ObservableCollection<ConnectionVM>, ObservableCollection<ComponentVM>>>();
            this.redoHistoryStack = new Stack<Tuple<ObservableCollection<ConnectionVM>, ObservableCollection<ComponentVM>>>();
            this.UpdateUndoHistory();
            this.programManager.PinsDisconnected += this.OnPinsDisconnected;
        }

        private void FireOnComponentVMRemoved(ComponentVM item)
        {
            this.FieldComponentRemoved?.Invoke(this, new FieldComponentEventArgs(item));
        }

        private void RemoveDeletedComponentConnections(ComponentVM removedComponentVM)
        {
            foreach (var pinVM in removedComponentVM.OutputPinsVM)
            {
                for (int i = 0; i < this.connectionsVM.Count(); i++)
                {
                    var conn = this.connectionsVM[i];
                    if (pinVM == conn.OutputPin)
                    {
                        this.programManager.RemoveConnection(conn.OutputPin.Pin, conn.InputPin.Pin);
                        this.OnPinsDisconnected(this, new PinsConnectedEventArgs(conn.OutputPin.Pin, conn.InputPin.Pin));
                        this.connectionsVM.Remove(conn);
                    }
                }
            }

            foreach (var pinVM in removedComponentVM.InputPinsVM)
            {
                for (int i = 0; i < this.connectionsVM.Count(); i++)
                {
                    var conn = this.connectionsVM[i];
                    if (pinVM == conn.InputPin)
                    {
                        this.programManager.RemoveConnection(conn.OutputPin.Pin, conn.InputPin.Pin);
                        this.OnPinsDisconnected(this, new PinsConnectedEventArgs(conn.OutputPin.Pin, conn.InputPin.Pin));
                        this.connectionsVM.Remove(conn);
                    }
                }
            }

        }

        private string NewUniqueConnectionId()
        {
            string s = "Connection" + this.newUniqueConnectionId.ToString();
            this.newUniqueConnectionId++;
            return s;
        }

        public Command StartCommand
        {
            get;
            private set;
        }

        public Command StepCommand
        {
            get;
            private set;
        }

        public Command StopCommand
        {
            get;
            private set;
        }

        public Command UndoCommand
        {
            get;
            private set;
        }
        public Command RedoCommand
        {
            get;
            private set;
        }

        public ComponentVM SelectedFieldComponent
        {
            get
            {
                return this.selectedFieldComponent;
            }

            set
            {
                this.selectedFieldComponent = value;
                this.FireOnPropertyChanged();
            }
        }


        public ObservableCollection<ComponentVM> NodesVMInField
        {
            get
            {
                return this.nodesVMInField;
            }
            private set
            {
                this.nodesVMInField = value;
                this.FireOnPropertyChanged();
            }
        }

        public ObservableCollection<ComponentRepresentationVM> SelectableComponents
        {
            get
            {
                return this.selectableComponents;
            }
            set
            {
                this.selectableComponents = value;
                this.FireOnPropertyChanged();
            }
        }

        public ObservableCollection<ConnectionVM> ConnectionsVM
        {
            get
            {
                return this.connectionsVM;
            }
            set
            {
                this.connectionsVM = value;
            }
        }

        /// <summary>
        /// Gets the paste command.
        /// </summary>
        /// <value>
        /// The paste command.
        /// </value>
        public Command PasteCommand
        {
            get => new Command(new Action<object>((input) =>
            {
                MessageBox.Show("Se Paste Wörks!");
            }));
        }

        /// <summary>
        /// Gets the copy command.
        /// </summary>
        /// <value>
        /// The copy command.
        /// </value>
        public Command CopyCommand
        {
            get => new Command(new Action<object>((input) =>
            {

            }));
        }

        public void UpdateUndoHistory()
        {
            var oldHistory = new Tuple<ObservableCollection<ConnectionVM>, ObservableCollection<ComponentVM>>(
                new ObservableCollection<ConnectionVM>(this.ConnectionsVM),
                new ObservableCollection<ComponentVM>(this.NodesVMInField));
            this.undoHistoryStack.Push(oldHistory);
            this.redoHistoryStack.Clear();
        }

        public void SetSelectedPin(PinVM value)
        {
            value.Active = (value.Active == true) ? false : true;

            if (this.selectedOutputPin == value || this.selectedInputPin == value)
            {
                this.selectedInputPin = null;
                this.selectedOutputPin = null;
            }
            else
            {
                if (!value.IsInputPin)
                {
                    this.selectedOutputPin = value;
                }
                else
                {
                    this.selectedInputPin = value;
                }

                if (this.selectedOutputPin != null && this.selectedInputPin != null)
                {
                    this.ConnectPins(this.selectedOutputPin, this.selectedInputPin);
                }
            }
        }

        public void FireOnFieldComponentAdded(ComponentVM addedComponent)
        {
            this.FieldComponentAdded?.Invoke(this, new FieldComponentEventArgs(addedComponent));
        }

        public void OnFieldComponentRemoved(object sender, FieldComponentEventArgs e)
        {
            this.FieldComponentRemoved?.Invoke(this, e);
        }

        private void ConnectPins(PinVM selectedOutputPin, PinVM selectedInputPin)
        {
            if (this.programManager.ConnectPins(selectedOutputPin.Pin, selectedInputPin.Pin))
            {
                var conn = new ConnectionVM(selectedOutputPin, selectedInputPin,
                    this.NewUniqueConnectionId());
                this.connectionsVM.Add(conn);
                this.UpdateUndoHistory(); ////If connect successful update history
                this.OnPinsConnected(this, new PinVMConnectionChangedEventArgs(conn));
            }

            this.selectedInputPin.Active = false;
            this.selectedOutputPin.Active = false;

            this.selectedInputPin = null;
            this.selectedOutputPin = null;
        }

        /// <summary>
        /// Refreshes the view models, in case a step in program manager finished.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data in this case not necesary.</param>
        private void RefreshVM(object sender, EventArgs e)
        {
            var oldTemp = this.SelectedFieldComponent;
            this.SelectedFieldComponent = null;
            this.SelectedFieldComponent = oldTemp;
        }

        /// <summary>
        /// Creates an unique name by adding a serial number to the label ending.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns>The identifier will be returned.</returns>
        private string CreateUniqueName(IDisplayableNode node)
        {
            this.uniqueNodeId++;
            return this.CreateNameTag(node.Label, this.uniqueNodeId.ToString());
        }

        /// <summary>
        /// Triggers the the initialization prozess to refresh all Selectable components in view.
        /// </summary>
        /// <param name="sender">The sender of the event, program manager.</param>
        /// <param name="e">The <see cref="FileSystemEventArgs"/> instance containing the event data.</param>
        private void NewModuleAdded(object sender, FileSystemEventArgs e)
        {
            //this.programManager = new ProgramManager();
            //this.programManager.Watcher.Created += NewModuleAdded;
            this.programManager.InitializeNodesToChooseFromVoid();

            //var nodesToChoose = this.programManager.PossibleNodesToChooseFrom.Select(node => new ComponentRepresentationVM(this.addCommand, node));
            var nodesToChoose = this.programManager.SerializationPathInfo.Select(node => new ComponentRepresentationVM(this.addCommand, node.Item1, node.Item2));

            // hässlich, aber konnte keinen besseren Weg finden
            //App.Current.Dispatcher.Invoke((Action)delegate // <--- HERE
            //{
            //    this.SelectableComponents.Clear();
            //});
            //foreach (var item in nodesToChoose)
            //{
            //    App.Current.Dispatcher.BeginInvoke((Action)delegate // <--- HERE
            //    {
            //        this.SelectableComponents.Add(item);
            //    });
            //}

            App.Current.Dispatcher.Invoke(() =>
            this.SelectableComponents = new ObservableCollection<ComponentRepresentationVM>(nodesToChoose));
        }

        /// <summary>
        /// Creates the name tag from Label and can add additional string to the end, it removes all chars except the letters.
        /// </summary>
        /// <param name="preName">Name of the element.</param>
        /// <param name="additional">The additional string ending.</param>
        /// <returns></returns>
        public string CreateNameTag(string preName, string additional)
        {
            return Regex.Replace(preName, "[^A-Za-z]", string.Empty) + additional;
        }

        ///// <summary>
        ///// Initializes a new instance of the <see cref="ProgramMngVM"/> class.
        ///// </summary>
        ///// <param name="old"> The ProgramMngVM which values should be copied. </param>
        //public ProgramMngVM(ProgramMngVM old)
        //{
        //    this.nodesVMInField = new ObservableCollection<ComponentVM>(old.NodesVMInField); ////Can be solved by  new ObservableCollection<ComponentVM>(old.nodesVMInField);
        //    this.SelectedFieldComponent = old.SelectedFieldComponent;
        //    this.SelectableComponents = old.SelectableComponents;
        //    this.programManager = new ProgramManager(old.programManager);
        //    this.StartCommand = new Command(obj =>
        //    {
        //        Dispatcher.CurrentDispatcher.Invoke(() => Task.Run(() =>
        //        {
        //            this.programManager.Run();
        //        }));
        //    });

        //    this.StepCommand = new Command(obj =>
        //    {
        //        this.programManager.RunLoop(0); // step
        //    });

        //    this.StopCommand = new Command(obj =>
        //    {
        //        this.programManager.StopProgram();
        //    });
        //}

        public void OnPinsConnected(object sender, PinVMConnectionChangedEventArgs e)
        {
            this.PinsConnected?.Invoke(this, e);
        }

        public void OnPinsDisconnected(object sender, PinsConnectedEventArgs e)
        {
            var conn = this.connectionsVM?.Where(c => c.OutputPin.Pin == e.OutputPin && c.InputPin.Pin == e.InputPin).FirstOrDefault();
            this.PinsDisconnected?.Invoke(this, new PinVMConnectionChangedEventArgs(conn));
            this.connectionsVM.Remove(conn);

        }

        protected virtual void FireOnPropertyChanged([CallerMemberName]string name = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public void SaveStatus(string path)
        {
            SerializationLogic serializer = new SerializationLogic();

            List<Tuple<ComponentVM, string>> serializationTuples = new List<Tuple<ComponentVM, string>>();
            for (int i = 0; i < this.NodesVMInField.Count; i++)
            {
                bool found = false;
                for (int j = 0; j < this.programManager.SerializationPathInfo.Count; j++)
                {
                    if (!found && this.NodesVMInField[i].Node.Label == this.programManager.SerializationPathInfo.ElementAt(j).Item1.Label)
                    {
                        serializationTuples.Add(new Tuple<ComponentVM, string>(this.NodesVMInField[i], this.programManager.SerializationPathInfo.ElementAt(j).Item2));
                        found = true;
                    }
                }
            }
            serializer.SerializeComponent(path, serializationTuples, (ICollection<ConnectionVM>)this.ConnectionsVM);

        }

        public Tuple<List<ConnectionVM>, List<ComponentVM>> LoadStatus(string path)
        {
            SerializationLogic serializer = new SerializationLogic();
            var testResult = (SerializedObject)serializer.DeserializeObject(path);

            List<Tuple<IDisplayableNode, string>> loadedNodes = new List<Tuple<IDisplayableNode, string>>();
            List<ComponentVM> reconstructedCompVMs = new List<ComponentVM>();
            foreach (var result in testResult.Components)
            {
                if (!loadedNodes.Any(node => node.Item2 == result.AssemblyPath))
                {
                    foreach (var component in NodesLoader.LoadSingleAssembly(result.AssemblyPath))
                    {
                        loadedNodes.Add(component);
                        var tempVM = new ComponentVM(component.Item1, this.CreateUniqueName(component.Item1), this.setPinCommand, this.removeCommand, this.config);
                        tempVM.XCoord = result.XPos;
                        tempVM.YCoord = result.YPos;
                        reconstructedCompVMs.Add(tempVM);
                    }
                }
            }

            List<ConnectionVM> reconstructedConns = new List<ConnectionVM>();
            foreach (var connection in testResult.Connections)
            {
                var inparent = loadedNodes.Find(node => node.Item1.Label == connection.InputParentID);
                var outparent = loadedNodes.Find(node => node.Item1.Label == connection.OutputParentID);
                // Labelvergleich wirkt ziemlich ranzig
                if (inparent != null && outparent != null)
                {
                    var inPin = inparent.Item1.Inputs.ToList().Find(pin => pin.Label == connection.InputPinID);
                    var outPin = outparent.Item1.Outputs.ToList().Find(pin => pin.Label == connection.OutputPinID);

                    var inCompVM = new ComponentVM(inparent.Item1, this.CreateUniqueName(inparent.Item1), this.setPinCommand, this.removeCommand, this.config);
                    var outCompVM = new ComponentVM(outparent.Item1, this.CreateUniqueName(outparent.Item1), this.setPinCommand, this.removeCommand, this.config);

                    var tempIn = new PinVM(inPin, true, this.setPinCommand, inCompVM, this.config.PinActiveColor, this.config.PinPassiveColor);
                    var tempOut = new PinVM(outPin, false, this.setPinCommand, outCompVM, this.config.PinActiveColor, this.config.PinPassiveColor);

                    var tempConnection = new ConnectionVM(tempOut, tempIn, this.NewUniqueConnectionId());
                    tempConnection.InputPin.XPosition = connection.InputX;
                    tempConnection.InputPin.YPosition = connection.InputY;

                    tempConnection.OutputPin.XPosition = connection.OutputX;
                    tempConnection.OutputPin.YPosition = connection.OutputY;
                    reconstructedConns.Add(tempConnection);
                }
            }

            App.Current.Dispatcher.Invoke((Action)delegate // <--- HERE
            {
                this.programManager.ConnectedOutputInputPairs = new List<Tuple<IPin, IPin>>();
                this.ConnectionsVM = new ObservableCollection<ConnectionVM>();
                var tempCopy = new ObservableCollection<ComponentVM>(this.NodesVMInField);
                foreach (var existingComponent in tempCopy)
                {
                    this.FieldComponentRemoved?.Invoke(this, new FieldComponentEventArgs(existingComponent));
                    this.NodesVMInField.Remove(existingComponent);
                    // Insert visual remove
                }
            });

            return new Tuple<List<ConnectionVM>, List<ComponentVM>>(reconstructedConns, reconstructedCompVMs);
        }

        public void AddLoadedComponent(ComponentVM loadedComponent)
        {
            NodesVMInField.Add(loadedComponent);
            programManager.FieldNodes.Add(loadedComponent.Node);
            this.FieldComponentAdded?.Invoke(this, new FieldComponentEventArgs(loadedComponent));
        }

        public void AddLoadedConnection(ConnectionVM loadedConnection)
        {
            ConnectionsVM.Add(loadedConnection);
            programManager.ConnectPins(loadedConnection.OutputPin.Pin, loadedConnection.InputPin.Pin);
            this.PinsConnected?.Invoke(this, new PinVMConnectionChangedEventArgs(loadedConnection));
        }

        /// <summary>
        /// Fires the on component vm changed, this seemes obsolete because the event when fired already contains the entire component.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="FieldComponentEventArgs"/> instance containing the event data.</param>
        protected virtual void FireOnComponentVMChanged(object sender, FieldComponentEventArgs e)
        {
            this.FieldComponentChanged?.Invoke(this, e);
        }

        public void RemoveConnectionVM(string id)
        {
            foreach (var conn in this.connectionsVM)
            {
                if (conn.ConnectionId == id)
                {
                    this.programManager.RemoveConnection(conn.OutputPin.Pin, conn.InputPin.Pin);
                    this.connectionsVM.Remove(conn);
                    break;
                }
            }
        }
    }
}
