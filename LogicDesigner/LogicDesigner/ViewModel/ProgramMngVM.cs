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
using LogicDesigner.Model.Serialization;
using Shared;

namespace LogicDesigner.ViewModel
{
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
        public event EventHandler<EventArgs> PreFieldComponentAdded;
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

        public ProgramMngVM()
        {
            this.programManager = new ProgramManager();
            this.programManager.Watcher.Created += this.NewModuleAdded;
            this.programManager.StepFinished += this.RefreshVM;

            this.StartCommand = new Command(obj =>
            {
                Dispatcher.CurrentDispatcher.Invoke(() => Task.Run(() =>
                {
                    this.programManager.Run();
                }));
            });

            this.StepCommand = new Command(obj =>
            {
                this.programManager.RunLoop(0); // step
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
                foreach (var n in this.programManager.FieldNodes)
                {
                    if (nodeInFieldVM.Node == n)
                    {
                        this.programManager.FieldNodes.Remove(n);
                        this.nodesVMInField.Remove(nodeInFieldVM);
                        this.OnFieldComponentRemoved(this, new FieldComponentEventArgs(nodeInFieldVM));
                        this.RemoveDeletedComponentConnections(nodeInFieldVM);
                        break;
                    }
                }

                this.programManager.FieldNodes.Remove(nodeInFieldVM.Node);
            });

            this.addCommand = new Command(obj =>
            {
                var representationNode = obj as ComponentRepresentationVM;
                this.PreFieldComponentAdded(this, new EventArgs());
                var realComponent = representationNode.Node;
                var newGenerateComp = (IDisplayableNode)Activator.CreateInstance(realComponent.GetType());
                this.programManager.FieldNodes.Add(newGenerateComp);

                var compVM = new ComponentVM(newGenerateComp, this.CreateUniqueName(realComponent), setPinCommand, 
                    this.removeCommand);
                this.UpdateUndoHistory();
                this.nodesVMInField.Add(compVM);
                this.FireOnFieldComponentAdded(compVM);
            });

            this.UndoCommand = new Command(obj =>
           {
               if (this.undoHistoryStack.Count > 0)
               {
                   var history = this.undoHistoryStack.Pop();

                   var differencesComps = this.NodesVMInField.Except(history.Item2);

                   foreach (var item in differencesComps)
                   {
                       if (!history.Item2.Contains(item))
                       {
                           this.FireOnComponentVMRemoved(item);
                       }
                       else
                       {
                           this.FireOnFieldComponentAdded(item);
                       }
                   }

                   var differencesConnects = this.ConnectionsVM.Except(history.Item1);

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
                   this.redoHistoryStack.Push(history);
               }
           });
            //    this.OnFieldComponentCreated(this, new FieldComponentEventArgs(compVM));
            //});

            var nodesInField = this.programManager.FieldNodes.Select(node => new ComponentVM(node,
                this.CreateUniqueName(node), setPinCommand, this.removeCommand
                ));

            this.nodesVMInField = new ObservableCollection<ComponentVM>(nodesInField);

            var nodesToChoose = this.programManager.SerializationPathInfo.Select(
                node => new ComponentRepresentationVM(this.addCommand, node.Item1, node.Item2));

            this.SelectableComponents = new ObservableCollection<ComponentRepresentationVM>(nodesToChoose);

            var connections = this.programManager.ConnectedOutputInputPairs.Select(conn =>
            new ConnectionVM(new PinVM(conn.Item1, false, setPinCommand), 
            new PinVM(conn.Item2, true, setPinCommand), this.NewUniqueConnectionId()));

            this.connectionsVM = new ObservableCollection<ConnectionVM>(connections);
            this.undoHistoryStack = new Stack<Tuple<ObservableCollection<ConnectionVM>, ObservableCollection<ComponentVM>>>();
            this.redoHistoryStack = new Stack<Tuple<ObservableCollection<ConnectionVM>, ObservableCollection<ComponentVM>>>();
            this.programManager.PinsDisconnected += this.OnPinsDisconnected;
        }

        private void FireOnComponentVMAdded(ComponentVM item)
        {
            throw new NotImplementedException();
        }

        private void FireOnComponentVMRemoved(ComponentVM item)
        {
            throw new NotImplementedException();
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

        private void UpdateUndoHistory()
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
                this.UpdateUndoHistory(); ////If connect successful update history
                this.connectionsVM.Add(conn);

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

        /// <summary>
        /// Initializes a new instance of the <see cref="ProgramMngVM"/> class.
        /// </summary>
        /// <param name="old"> The ProgramMngVM which values should be copied. </param>
        public ProgramMngVM(ProgramMngVM old)
        {
            this.nodesVMInField = new ObservableCollection<ComponentVM>(old.NodesVMInField); ////Can be solved by  new ObservableCollection<ComponentVM>(old.nodesVMInField);
            this.SelectedFieldComponent = old.SelectedFieldComponent;
            this.SelectableComponents = old.SelectableComponents;
            this.programManager = new ProgramManager(old.programManager);
            this.StartCommand = new Command(obj =>
            {
                Dispatcher.CurrentDispatcher.Invoke(() => Task.Run(() =>
                {
                    this.programManager.Run();
                }));
            });

            this.StepCommand = new Command(obj =>
            {
                this.programManager.RunLoop(0); // step
            });

            this.StopCommand = new Command(obj =>
            {
                this.programManager.StopProgram();
            });
        }

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
            for (int i = 0; i< this.NodesVMInField.Count; i++)
            {
                bool found = false;
                for (int j = 0; j < programManager.SerializationPathInfo.Count; j++)
                {
                    if (!found && NodesVMInField[i].Node.Label == programManager.SerializationPathInfo.ElementAt(j).Item1.Label)
                    {
                        serializationTuples.Add(new Tuple<ComponentVM, string>(NodesVMInField[i], programManager.SerializationPathInfo.ElementAt(j).Item2));
                        found = true;
                    }
                }
            }
            serializer.SerializeComponent(path, serializationTuples, (ICollection<ConnectionVM>)ConnectionsVM);

        }

        public Tuple<List<ConnectionVM>, List<ComponentVM>> LoadStatus(string path)
        {
            SerializationLogic serializer = new SerializationLogic();
            var testResult = (SerializedObject)serializer.DeserializeObject(path);

            List<Tuple<IDisplayableNode, string>> loadedNodes = new List<Tuple<IDisplayableNode, string>>();
            List<ComponentVM> reconstructedCompVMs = new List<ComponentVM>();
            foreach(var result in testResult.Components)
            {
                if (!loadedNodes.Any(node => node.Item2 == result.AssemblyPath))
                {
                    foreach (var component in NodesLoader.LoadSingleAssembly(result.AssemblyPath))
                    {
                        loadedNodes.Add(component);
                        var tempVM = new ComponentVM(component.Item1, CreateUniqueName(component.Item1), setPinCommand, removeCommand);
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
                if (inparent != null &&  outparent != null)
                {
                    var inPin = inparent.Item1.Inputs.ToList().Find(pin => pin.Label == connection.InputPinID);
                    var outPin = outparent.Item1.Outputs.ToList().Find(pin => pin.Label == connection.OutputPinID);

                    var inCompVM = new ComponentVM(inparent.Item1, CreateUniqueName(inparent.Item1) , setPinCommand, removeCommand);
                    var outCompVM = new ComponentVM(outparent.Item1, CreateUniqueName(outparent.Item1) , setPinCommand, removeCommand);


                    var tempIn = new PinVM(inPin, true, setPinCommand, inCompVM);
                    var tempOut = new PinVM(outPin, false, setPinCommand, outCompVM);

                    var tempConnection = new ConnectionVM(tempOut, tempIn, this.NewUniqueConnectionId());
                    reconstructedConns.Add(tempConnection);
                }
            }

            return new Tuple<List<ConnectionVM>, List<ComponentVM>>(reconstructedConns, reconstructedCompVMs);
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
            foreach(var conn in this.connectionsVM)
            {
                if(conn.ConnectionId == id)
                {
                    this.programManager.RemoveConnection(conn.OutputPin.Pin, conn.InputPin.Pin);
                    this.connectionsVM.Remove(conn);
                    break;
                }
            }
        }
    }
}
