using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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

            var setPinCommand = new Command(obj =>
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
                    removeCommand);
                this.nodesVMInField.Add(compVM);
                this.OnFieldComponentCreated(this, new FieldComponentEventArgs(compVM));
            });

            var nodesInField = this.programManager.FieldNodes.Select(node => new ComponentVM(node,
                CreateUniqueName(node), setPinCommand, this.removeCommand
                ));

            this.nodesVMInField = new ObservableCollection<ComponentVM>(nodesInField);

            var nodesToChoose = this.programManager.PossibleNodesToChooseFrom.Select(
                node => new ComponentRepresentationVM(this.addCommand, node));

            this.SelectableComponents = new ObservableCollection<ComponentRepresentationVM>(nodesToChoose);

            var connections = this.programManager.ConnectedOutputInputPairs.Select(conn =>
            new ConnectionVM(new PinVM(conn.Item1, false, setPinCommand), 
            new PinVM(conn.Item2, true, setPinCommand), this.NewUniqueConnectionId()));

            this.connectionsVM = new ObservableCollection<ConnectionVM>(connections);

            this.programManager.PinsDisconnected += this.OnPinsDisconnected;
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

        public void SetSelectedPin(PinVM value)
        {
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

        public void OnFieldComponentCreated(object sender, FieldComponentEventArgs e)
        {
            this.FieldComponentAdded?.Invoke(this, e);
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

                this.OnPinsConnected(this, new PinVMConnectionChangedEventArgs(conn));
            }

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
            return CreateNameTag(node.Label, this.uniqueNodeId.ToString());
        }

        private void NewModuleAdded(object sender, FileSystemEventArgs e)
        {
            this.programManager = new ProgramManager();
            this.programManager.Watcher.Created += NewModuleAdded;

            var nodesToChoose = this.programManager.PossibleNodesToChooseFrom.Select(node => new ComponentRepresentationVM(this.addCommand, node));

            // hässlich, aber konnte keinen besseren Weg finden
            App.Current.Dispatcher.Invoke((Action)delegate // <--- HERE
            {
                this.SelectableComponents.Clear();
            });
            foreach (var item in nodesToChoose)
            {
                App.Current.Dispatcher.BeginInvoke((Action)delegate // <--- HERE
                {
                    this.SelectableComponents.Add(item);
                });
            }
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
            serializer.SerializeObject(path, NodesVMInField);
        }

        public void LoadStatus(string path)
        {
            SerializationLogic serializer = new SerializationLogic();
            this.nodesVMInField = (ObservableCollection<ComponentVM>)serializer.DeserializeObject(path);
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
