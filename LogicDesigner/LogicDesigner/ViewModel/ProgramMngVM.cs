﻿//-----------------------------------------------------------------------
// <copyright file="ProgramMngVM.cs" company="FH">
//     Company copyright tag.
// </copyright>
// <summary>Contains the program manager view model class.</summary>
//-----------------------------------------------------------------------
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

    /// <summary>
    /// The main class.
    /// </summary>
    /// <seealso cref="System.ComponentModel.INotifyPropertyChanged" />
    public class ProgramMngVM : INotifyPropertyChanged
    {
        /// <summary>
        /// The remove command.
        /// </summary>
        private readonly Command removeCommand;

        /// <summary>
        /// The set pin command.
        /// </summary>
        private readonly Command setPinCommand;

        /// <summary>
        /// The add command.
        /// </summary>
        private readonly Command addCommand;

        /// <summary>
        /// The configuration.
        /// </summary>
        private readonly ConfigurationLogic config;

        /// <summary>
        /// The start button path.
        /// </summary>
        private string startButtonPath;

        /// <summary>
        /// The step button path.
        /// </summary>
        private string stepButtonPath;

        /// <summary>
        /// The stop button path.
        /// </summary>
        private string stopButtonPath;

        /// <summary>
        /// The program manager.
        /// </summary>
        private ProgramManager programManager;

        /// <summary>
        /// The connections.
        /// </summary>
        private ObservableCollection<ConnectionVM> connectionsVM;

        /// <summary>
        /// The nodes in field.
        /// </summary>
        private ObservableCollection<ComponentVM> nodesVMInField;

        /// <summary>
        /// The selectable components.
        /// </summary>
        private ObservableCollection<ComponentRepresentationVM> selectableComponents;

        /// <summary>
        /// The unique node identifier.
        /// </summary>
        private int uniqueNodeId;

        /// <summary>
        /// The unique pin identifier set by the Component and given by the program manager view model.
        /// </summary>
        private int uniquePinId;

        /// <summary>
        /// The selected output pin.
        /// </summary>
        private PinVM selectedOutputPin;

        /// <summary>
        /// The selected input pin.
        /// </summary>
        private PinVM selectedInputPin;

        /// <summary>
        /// The selected component in the work field.
        /// </summary>
        private ComponentVM selectedFieldComponent;

        /// <summary>
        /// The new unique connection identifier.
        /// </summary>
        private int newUniqueConnectionId;

        /// <summary>
        /// The undo history stack.
        /// </summary>
        private Stack<Tuple<ObservableCollection<ConnectionVM>, ObservableCollection<ComponentVM>>> undoHistoryStack;

        /// <summary>
        /// The redo history stack.
        /// </summary>
        private Stack<Tuple<ObservableCollection<ConnectionVM>, ObservableCollection<ComponentVM>>> redoHistoryStack;

        /// <summary>
        /// The loop task that runs the simulation.
        /// </summary>
        private Task loopTask;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProgramMngVM"/> class.
        /// </summary>
        public ProgramMngVM()
        {
            this.programManager = new ProgramManager();
            this.programManager.Watcher.Created += this.NewModuleAdded;
            this.programManager.StepFinished += this.RefreshVM;

            this.config = new ConfigurationLogic();
            this.StartButtonPath = @"\ButtonPictures\start.png";
            this.StepButtonPath = @"\ButtonPictures\step.png";
            this.StopButtonPath = @"\ButtonPictures\stop.png";

            this.StartCommand = new Command(obj =>
            {
                this.StartButtonPath = @"\ButtonPictures\start_pressed.png";

                Dispatcher.CurrentDispatcher.Invoke(() =>
                this.loopTask = new Task(() =>
                    {
                        if (!this.programManager.RunActive)
                        {
                            this.programManager.SetActive();
                            this.programManager.Run();
                            this.RestoreSaveState();
                        }
                    })).Start();
            });

            this.StepCommand = new Command(obj =>
            {
                if (!this.programManager.RunActive)
                {
                    this.programManager.SetActive();
                    this.programManager.RunLoop(0); // step
                    this.programManager.StopActive();
                }
            });

            this.StopCommand = new Command(obj =>
            {
                this.StartButtonPath = @"\ButtonPictures\start.png";

                this.StopWaitForTask();
            });

            this.setPinCommand = new Command(obj =>
            {
                var pin = obj as PinVM;
                this.SetSelectedPin(pin);
            });

            this.removeCommand = new Command(obj =>
            {
                this.StopWaitForTask();
                var nodeInFieldVM = obj as ComponentVM;

                foreach (var n in this.programManager.FieldNodes)
                {
                    if (nodeInFieldVM.Node == n)
                    {
                        this.RestoreSaveState();
                        this.programManager.FieldNodes.Remove(n);
                        this.nodesVMInField.Remove(nodeInFieldVM);
                        this.OnFieldComponentRemoved(this, new FieldComponentEventArgs(nodeInFieldVM));
                        this.RemoveDeletedComponentConnections(nodeInFieldVM);
                        this.UpdateUndoHistory(); // Not sure if i have to update the program manager!
                        this.SetSaveState();
                        break;
                    }
                }

                this.FireOnComponentVMRemoved(nodeInFieldVM);
                this.programManager.FieldNodes.Remove(nodeInFieldVM.Node);
            });

            this.addCommand = new Command(obj =>
            {
                this.StopWaitForTask();
                var representationNode = obj as ComponentRepresentationVM;
                this.RestoreSaveState();
                var realComponent = representationNode.Node;
                var newGenerateComp = (IDisplayableNode)Activator.CreateInstance(realComponent.GetType());
                this.programManager.FieldNodes.Add(newGenerateComp);

                var compVM = new ComponentVM(newGenerateComp, this.CreateUniqueName(realComponent), this.GetUniqueNumber(), this.setPinCommand, this.removeCommand, this.config);
                this.nodesVMInField.Add(compVM);
                this.FireOnFieldComponentAdded(compVM);
                this.UpdateUndoHistory();
                this.SetSaveState();
            });

            this.UndoCommand = new Command(obj =>
            {
                this.StopWaitForTask();
                if (this.undoHistoryStack.Count > 0)
                {
                    this.RestoreSaveState();

                    var history = this.undoHistoryStack.Pop();
                    this.redoHistoryStack.Push(history);
                    var differencesComps = new List<ComponentVM>(this.NodesVMInField.Except(history.Item2));
                    differencesComps.AddRange(new List<ComponentVM>(history.Item2.Except(this.NodesVMInField)));
                    var differencesConnects = new List<ConnectionVM>(this.ConnectionsVM.Except(history.Item1));
                    differencesConnects.AddRange(new List<ConnectionVM>(history.Item1.Except(this.ConnectionsVM)));

                    if (differencesComps.Count == 0 && this.undoHistoryStack.Count > 0 && differencesConnects.Count == 0)
                    {
                        history = this.undoHistoryStack.Pop();
                        this.redoHistoryStack.Push(history);
                        differencesComps = new List<ComponentVM>(this.NodesVMInField.Except(history.Item2));
                        differencesComps.AddRange(new List<ComponentVM>(history.Item2.Except(this.NodesVMInField)));
                        differencesConnects = new List<ConnectionVM>(this.ConnectionsVM.Except(history.Item1));
                        differencesConnects.AddRange(new List<ConnectionVM>(history.Item1.Except(this.ConnectionsVM)));
                    }

                    foreach (var item in differencesComps)
                    {
                        if (!history.Item2.Contains(item))
                        {
                            this.OnFieldComponentRemoved(this, new FieldComponentEventArgs(item));
                        }
                        else
                        {
                            this.FireOnFieldComponentAdded(item);
                        }
                    }

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
                    this.programManager.ConnectedOutputInputPairs = this.ConnectionsVM.Select(x => new Tuple<IPin, IPin>(x.InputPin.Pin, x.OutputPin.Pin)).ToList();
                    this.programManager.FieldNodes = this.NodesVMInField.Select(x => x.Node).ToList();
                    this.SetSaveState();
                }
            });

            this.RedoCommand = new Command(obj =>
            {
                this.StopWaitForTask();
                if (this.redoHistoryStack.Count > 0)
                {
                    this.RestoreSaveState();

                    var futureHistory = this.redoHistoryStack.Pop();
                    this.undoHistoryStack.Push(futureHistory);
                    var differencesComps = new List<ComponentVM>(this.NodesVMInField.Except(futureHistory.Item2));
                    differencesComps.AddRange(new List<ComponentVM>(futureHistory.Item2.Except(this.NodesVMInField)));
                    var differencesConnects = new List<ConnectionVM>(this.ConnectionsVM.Except(futureHistory.Item1));
                    differencesConnects.AddRange(new List<ConnectionVM>(futureHistory.Item1.Except(this.ConnectionsVM)));

                    if (differencesComps.Count == 0 && this.redoHistoryStack.Count > 0 && differencesConnects.Count == 0)
                    {
                        futureHistory = this.redoHistoryStack.Pop();
                        this.undoHistoryStack.Push(futureHistory);
                        differencesComps = new List<ComponentVM>(this.NodesVMInField.Except(futureHistory.Item2));
                        differencesComps.AddRange(new List<ComponentVM>(futureHistory.Item2.Except(this.NodesVMInField)));
                        differencesConnects = new List<ConnectionVM>(this.ConnectionsVM.Except(futureHistory.Item1));
                        differencesConnects.AddRange(new List<ConnectionVM>(futureHistory.Item1.Except(this.ConnectionsVM)));
                    }

                    foreach (var item in differencesComps)
                    {
                        if (!futureHistory.Item2.Contains(item))
                        {
                            this.OnFieldComponentRemoved(this, new FieldComponentEventArgs(item));
                        }
                        else
                        {
                            this.FireOnFieldComponentAdded(item);
                        }
                    }

                    // differencesConnects.AddRange(new List<ConnectionVM>(futureHistory.Item1.Except(this.ConnectionsVM)));
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
                    this.programManager.ConnectedOutputInputPairs = this.ConnectionsVM.Select(x => new Tuple<IPin, IPin>(x.OutputPin.Pin, x.InputPin.Pin)).ToList();
                    this.programManager.FieldNodes = this.NodesVMInField.Select(x => x.Node).ToList();
                    this.SetSaveState();
                }
            });

            var nodesInField = this.programManager.FieldNodes.Select(node => new ComponentVM(
                node,
                this.CreateUniqueName(node),
                this.GetUniqueNumber(),
                this.setPinCommand,
                this.removeCommand,
                this.config));

            this.nodesVMInField = new ObservableCollection<ComponentVM>(nodesInField);

            var nodesToChoose = this.programManager.SerializationPathInfo.Select(
                node => new ComponentRepresentationVM(this.addCommand, node.Item1, node.Item2));

            this.SelectableComponents = new ObservableCollection<ComponentRepresentationVM>(nodesToChoose);

            var connections = this.programManager.ConnectedOutputInputPairs.Select(conn => new ConnectionVM(
                new PinVM(
                    conn.Item1,
                    this.GetUniqueNumberFromEnumerator(),
                    false,
                    this.setPinCommand),
                new PinVM(conn.Item2, this.GetUniqueNumberFromEnumerator(), true, this.setPinCommand),
                this.NewUniqueConnectionId(),
                this.config.LinePassiveColor));

            this.connectionsVM = new ObservableCollection<ConnectionVM>(connections);
            this.undoHistoryStack = new Stack<Tuple<ObservableCollection<ConnectionVM>, ObservableCollection<ComponentVM>>>();
            this.redoHistoryStack = new Stack<Tuple<ObservableCollection<ConnectionVM>, ObservableCollection<ComponentVM>>>();
            this.UpdateUndoHistory();
            this.SetSaveState();
            this.programManager.PinsDisconnected += this.OnPinsDisconnected;
            this.programManager.ConnectionUpdated += this.OnConnectionUpdated;
        }

        /// <summary>
        /// Occurs when field component is added.
        /// </summary>
        public event EventHandler<FieldComponentEventArgs> FieldComponentAdded;

        /// <summary>
        /// Occurs when field component is removed.
        /// </summary>
        public event EventHandler<FieldComponentEventArgs> FieldComponentRemoved;

        /// <summary>
        /// Occurs when field component is changed.
        /// </summary>
        public event EventHandler<FieldComponentEventArgs> FieldComponentChanged;

        /// <summary>
        /// Occurs when pins get connected.
        /// </summary>
        public event EventHandler<PinVMConnectionChangedEventArgs> PinsConnected;

        /// <summary>
        /// Occurs when pins get disconnected.
        /// </summary>
        public event EventHandler<PinVMConnectionChangedEventArgs> PinsDisconnected;

        /// <summary>
        /// Occurs when connection view model is updated.
        /// </summary>
        public event EventHandler<PinVMConnectionChangedEventArgs> ConnectionVMUpdated;

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Gets or sets the start button path.
        /// </summary>
        /// <value>
        /// The start button path.
        /// </value>
        public string StartButtonPath
        {
            get
            {
                return this.startButtonPath;
            }

            set
            {
                this.startButtonPath = value;
                this.FireOnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the step button path.
        /// </summary>
        /// <value>
        /// The step button path.
        /// </value>
        public string StepButtonPath
        {
            get
            {
                return this.stepButtonPath;
            }

            set
            {
                this.stepButtonPath = value;
                this.FireOnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the stop button path.
        /// </summary>
        /// <value>
        /// The stop button path.
        /// </value>
        public string StopButtonPath
        {
            get
            {
                return this.stopButtonPath;
            }

            set
            {
                this.stopButtonPath = value;
                this.FireOnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets the start command.
        /// </summary>
        /// <value>
        /// The start command.
        /// </value>
        public Command StartCommand
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the step command.
        /// </summary>
        /// <value>
        /// The step command.
        /// </value>
        public Command StepCommand
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the stop command.
        /// </summary>
        /// <value>
        /// The stop command.
        /// </value>
        public Command StopCommand
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the undo command.
        /// </summary>
        /// <value>
        /// The undo command.
        /// </value>
        public Command UndoCommand
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the redo command.
        /// </summary>
        /// <value>
        /// The redo command.
        /// </value>
        public Command RedoCommand
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets the selected field component.
        /// </summary>
        /// <value>
        /// The selected field component.
        /// </value>
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

        /// <summary>
        /// Gets the nodes in field.
        /// </summary>
        /// <value>
        /// The nodes in field.
        /// </value>
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

        /// <summary>
        /// Gets or sets the selectable components.
        /// </summary>
        /// <value>
        /// The selectable components.
        /// </value>
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

        /// <summary>
        /// Gets or sets the connections.
        /// </summary>
        /// <value>
        /// The connections.
        /// </value>
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

        /// <summary>
        /// Gets or sets the save point.
        /// </summary>
        /// <value>
        /// The save point to restore from.
        /// </value>
        public Tuple<ObservableCollection<ConnectionVM>, ObservableCollection<ComponentVM>> SavePoint { get; set; }

        /// <summary>
        /// Updates the undo history.
        /// </summary>
        public void UpdateUndoHistory()
        {
            var oldHistory = new Tuple<ObservableCollection<ConnectionVM>, ObservableCollection<ComponentVM>>(
                new ObservableCollection<ConnectionVM>(this.ConnectionsVM),
                new ObservableCollection<ComponentVM>(this.NodesVMInField));
            this.undoHistoryStack.Push(oldHistory);
            this.redoHistoryStack.Clear();
        }

        /// <summary>
        /// Sets the selected pin.
        /// </summary>
        /// <param name="value">The value.</param>
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

        /// <summary>
        /// Clears the field.
        /// </summary>
        public void ClearField()
        {
            this.programManager.StopActive();

            for (int i = this.NodesVMInField.Count - 1; i >= 0; i--)
            {
                this.removeCommand.Execute(this.NodesVMInField[i]);
            }
        }

        /// <summary>
        /// Fires the on field component added event.
        /// </summary>
        /// <param name="addedComponent">The added component.</param>
        public void FireOnFieldComponentAdded(ComponentVM addedComponent)
        {
            this.FieldComponentAdded?.Invoke(this, new FieldComponentEventArgs(addedComponent));
        }

        /// <summary>
        /// Called when field component is removed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="LogicDesigner.ViewModel.FieldComponentEventArgs" /> instance containing the event data.</param>
        public void OnFieldComponentRemoved(object sender, FieldComponentEventArgs e)
        {
            this.FieldComponentRemoved?.Invoke(this, e);
        }

        /// <summary>
        /// Creates the name tag from Label and can add additional string to the end, it removes all chars except the letters.
        /// </summary>
        /// <param name="preName">Name of the element.</param>
        /// <param name="additional">The additional string ending.</param>
        /// <returns>The nametag of the component.</returns>
        public string CreateNameTag(string preName, string additional)
        {
            return Regex.Replace(preName, "[^A-Za-z]", string.Empty) + additional;
        }

        /// <summary>
        /// Called when pins get connected.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="LogicDesigner.ViewModel.PinVMConnectionChangedEventArgs" /> instance containing the event data.</param>
        public void OnPinsConnected(object sender, PinVMConnectionChangedEventArgs e)
        {
            this.PinsConnected?.Invoke(this, e);
        }

        /// <summary>
        /// Called when pins get disconnected.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="LogicDesigner.ViewModel.PinsConnectedEventArgs" /> instance containing the event data.</param>
        public void OnPinsDisconnected(object sender, PinsConnectedEventArgs e)
        {
            var conn = this.connectionsVM?.Where(c => c.OutputPin.Pin == e.OutputPin && c.InputPin.Pin == e.InputPin).FirstOrDefault();
            this.PinsDisconnected?.Invoke(this, new PinVMConnectionChangedEventArgs(conn));
            this.connectionsVM.Remove(conn);
        }

        /// <summary>
        /// Saves the status.
        /// </summary>
        /// <param name="path">The path where the save file will be stored.</param>
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

        /// <summary>
        /// Loads the status.
        /// </summary>
        /// <param name="path">The path from where the save file is loaded.</param>
        /// <returns>The tuple.</returns>
        public Tuple<List<ConnectionVM>, List<ComponentVM>> LoadStatus(string path)
        {
            SerializationLogic serializer = new SerializationLogic();
            var testResult = (SerializedObject)serializer.DeserializeObject(path);

            List<Tuple<IDisplayableNode, string>> loadedNodes = new List<Tuple<IDisplayableNode, string>>();
            List<ComponentVM> reconstructedCompVMs = new List<ComponentVM>();
            foreach (var result in testResult.Components)
            {
                foreach (var component in NodesLoader.LoadSingleAssembly(result.AssemblyPath, this.config.ModulePath))
                {
                    loadedNodes.Add(component);
                    var tempVM = new ComponentVM(component.Item1, result.UniqueName, this.ExtraxtIDsFromCompVM(result.InputPutputIDs), this.setPinCommand, this.removeCommand, this.config);
                    tempVM.XCoord = result.XPos;
                    tempVM.YCoord = result.YPos;
                    reconstructedCompVMs.Add(tempVM);
                }
            }

            List<ConnectionVM> reconstructedConns = new List<ConnectionVM>();
            foreach (var connection in testResult.Connections)
            {
                var inparent = reconstructedCompVMs.Find(node => node.Name == connection.InputParentID);
                var outparent = reconstructedCompVMs.Find(node => node.Name == connection.OutputParentID);
                var tempConnection = new ConnectionVM(outparent.OutputPinsVM.First(x => connection.IdNumberOutPin == x.IDNumber), inparent.InputPinsVM.First(x => connection.IdNumberInPin == x.IDNumber), connection.ConnectionID, this.config.LinePassiveColor);

                tempConnection.InputPin.XPosition = connection.InputX;
                tempConnection.InputPin.YPosition = connection.InputY;

                tempConnection.OutputPin.XPosition = connection.OutputX;
                tempConnection.OutputPin.YPosition = connection.OutputY;
                reconstructedConns.Add(tempConnection);
            }

            App.Current.Dispatcher.Invoke(() => // <--- HERE
            {
                this.newUniqueConnectionId = reconstructedConns.Count > 0 ? reconstructedConns.Select(x => int.Parse(Regex.Replace(x.ConnectionId, @"[^\d*]", string.Empty))).Max() + 1 : 0;
                this.uniqueNodeId = reconstructedConns.Count > 0 ? reconstructedCompVMs.Select(x => int.Parse(Regex.Replace(x.Name, @"[^\d*]", string.Empty))).Max() + 1 : 0;
                var tempOutpuPinIDMax = reconstructedCompVMs.Select(x => x.OutputPinsVM.Count > 0 ? x.OutputPinsVM.Select(y => y.IDNumber)?.ToList().Max() : 0).Max();
                var tempInoutPinIDMax = reconstructedCompVMs.Select(x => x.InputPinsVM.Count > 0 ? x.InputPinsVM.Select(y => y.IDNumber)?.ToList().Max() : 0).Max();
                var actual = tempInoutPinIDMax >= tempOutpuPinIDMax ? tempInoutPinIDMax : tempOutpuPinIDMax;
                if (actual == null)
                {
                    this.uniquePinId = 0;
                }
                else
                {
                    this.uniquePinId = (int)actual + 1;
                }

                this.programManager.ConnectedOutputInputPairs = new List<Tuple<IPin, IPin>>();
                this.ConnectionsVM = new ObservableCollection<ConnectionVM>();
                var tempCopy = new ObservableCollection<ComponentVM>(this.NodesVMInField);
                foreach (var existingComponent in tempCopy)
                {
                    this.FieldComponentRemoved?.Invoke(this, new FieldComponentEventArgs(existingComponent));
                    this.NodesVMInField.Remove(existingComponent);

                    // Insert visual remove.
                }
            });

            return new Tuple<List<ConnectionVM>, List<ComponentVM>>(reconstructedConns, reconstructedCompVMs);
        }

        /// <summary>
        /// Adds the loaded component.
        /// </summary>
        /// <param name="loadedComponent">The loaded component.</param>
        public void AddLoadedComponent(ComponentVM loadedComponent)
        {
            this.NodesVMInField.Add(loadedComponent);
            this.programManager.FieldNodes.Add(loadedComponent.Node);
            this.FieldComponentAdded?.Invoke(this, new FieldComponentEventArgs(loadedComponent));
        }

        /// <summary>
        /// Adds the loaded connection.
        /// </summary>
        /// <param name="loadedConnection">The loaded connection.</param>
        public void AddLoadedConnection(ConnectionVM loadedConnection)
        {
            this.ConnectionsVM.Add(loadedConnection);
            this.programManager.ConnectPins(loadedConnection.OutputPin.Pin, loadedConnection.InputPin.Pin);
            this.PinsConnected?.Invoke(this, new PinVMConnectionChangedEventArgs(loadedConnection));
        }

        /// <summary>
        /// Removes the connection.
        /// </summary>
        /// <param name="id">The identifier.</param>
        public void RemoveConnectionVM(string id)
        {
            this.StopWaitForTask();
            foreach (var conn in this.connectionsVM)
            {
                if (conn.ConnectionId == id)
                {
                    this.RestoreSaveState();
                    this.programManager.RemoveConnection(conn.OutputPin.Pin, conn.InputPin.Pin);
                    this.connectionsVM.Remove(conn);
                    this.UpdateUndoHistory();
                    this.PinsDisconnected?.Invoke(this, new PinVMConnectionChangedEventArgs(conn));
                    this.SetSaveState();
                    break;
                }
            }
        }

        /// <summary>
        /// Stops the task and waits for it and sets the button picture.
        /// </summary>
        public void StopWaitForTask()
        {
            this.programManager.StopActive();
            this.StartButtonPath = @"\ButtonPictures\start.png";
            if (this.loopTask?.Status == TaskStatus.Running)
            {
            }
        }

        /// <summary>
        /// Sets the state of back to the save point.
        /// </summary>
        public void SetSaveState()
        {
            this.SavePoint = new Tuple<ObservableCollection<ConnectionVM>, ObservableCollection<ComponentVM>>(new ObservableCollection<ConnectionVM>(this.ConnectionsVM), new ObservableCollection<ComponentVM>(this.NodesVMInField));
        }

        /// <summary>
        /// Fires the on component removed event.
        /// </summary>
        /// <param name="item">The component that was removed.</param>
        protected virtual void FireOnComponentVMRemoved(ComponentVM item)
        {
            this.FieldComponentRemoved?.Invoke(this, new FieldComponentEventArgs(item));
        }

        /// <summary>
        /// Fired when a property changed.
        /// </summary>
        /// <param name="name">The name of the property.</param>
        protected virtual void FireOnPropertyChanged([CallerMemberName]string name = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        /// <summary>
        /// Fires the on component changed, this is obsolete because the event when fired already contains the entire component.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="FieldComponentEventArgs"/> instance containing the event data.</param>
        protected virtual void FireOnComponentVMChanged(object sender, FieldComponentEventArgs e)
        {
            this.FieldComponentChanged?.Invoke(this, e);
        }

        /// <summary>
        /// Gets the unique number from by calling the enumerator next move.
        /// </summary>
        /// <returns>Returns the current value of the enumeration.</returns>
        private int GetUniqueNumberFromEnumerator()
        {
            this.GetUniqueNumber().MoveNext();
            return this.GetUniqueNumber().Current;
        }

        /// <summary>
        /// Extracts the identifiers from a serialized component view model.
        /// </summary>
        /// <param name="pinIDs">The pin id's.</param>
        /// <returns>The next integer identifier from the array.</returns>
        private IEnumerator<int> ExtraxtIDsFromCompVM(int[] pinIDs)
        {
            foreach (var id in pinIDs)
            {
                yield return id;
            }
        }

        /// <summary>
        /// Restores the state by loading all default values of the pins.
        /// </summary>
        private void RestoreSaveState()
        {
            foreach (var c in this.SavePoint.Item1)
            {
                c.InputPin.Pin.Value.Current = c.InputPin.InitialValue;
                c.OutputPin.Pin.Value.Current = c.OutputPin.InitialValue;
            }

            this.ConnectionsVM = this.SavePoint.Item1;
            foreach (var n in this.SavePoint.Item2)
            {
                foreach (var input in n.InputPinsVM)
                {
                    input.Pin.Value.Current = input.InitialValue;
                }

                foreach (var output in n.OutputPinsVM)
                {
                    output.Pin.Value.Current = output.InitialValue;
                }
            }

            this.NodesVMInField = this.SavePoint.Item2;

            this.programManager.ConnectedOutputInputPairs = this.ConnectionsVM.Select(x => new Tuple<IPin, IPin>(x.OutputPin.Pin, x.InputPin.Pin)).ToList();
            this.programManager.FieldNodes = this.NodesVMInField.Select(x => x.Node).ToList();
        }

        /// <summary>
        /// Called when connection is updated.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="PinsConnectedEventArgs"/> instance containing the event data.</param>
        private void OnConnectionUpdated(object sender, PinsConnectedEventArgs e)
        {
            var conn = this.connectionsVM?.Where(
                a => a.OutputPin.Pin == e.OutputPin && a.InputPin.Pin == e.InputPin).FirstOrDefault();

            var hmm = conn;

            var type = conn.OutputPin.Pin.Value.Current.GetType();

            if (e.OutputPin.Value.Current.Equals(Activator.CreateInstance(type)))
            {
                conn.LineColor = Color.FromArgb(
                    this.config.LinePassiveColor.R,
                    this.config.LinePassiveColor.G,
                    this.config.LinePassiveColor.B);
            }
            else
            {
                conn.LineColor = Color.FromArgb(
                    this.config.LineActiveColor.R,
                    this.config.LineActiveColor.G,
                    this.config.LineActiveColor.B);
            }

            this.ConnectionVMUpdated?.Invoke(this, new PinVMConnectionChangedEventArgs(conn));
        }

        /// <summary>
        /// Removes the deleted component connections.
        /// </summary>
        /// <param name="removedComponentVM">The removed component view model.</param>
        private void RemoveDeletedComponentConnections(ComponentVM removedComponentVM)
        {
            foreach (var outputPinVM in removedComponentVM.OutputPinsVM)
            {
                for (int i = this.connectionsVM.Count() - 1; i >= 0; i--)
                {
                    var conn = this.connectionsVM[i];

                    if (outputPinVM == conn.OutputPin)
                    {
                        this.programManager.RemoveConnection(conn.OutputPin.Pin, conn.InputPin.Pin);
                        this.OnPinsDisconnected(this, new PinsConnectedEventArgs(conn.OutputPin.Pin, conn.InputPin.Pin));
                        this.connectionsVM.Remove(conn);
                    }
                }
            }

            foreach (var pinVM in removedComponentVM.InputPinsVM)
            {
                for (int i = this.connectionsVM.Count() - 1; i >= 0; i--)
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

        /// <summary>
        /// Connects the pins.
        /// </summary>
        /// <param name="selectedOutputPin">The selected output pin.</param>
        /// <param name="selectedInputPin">The selected input pin.</param>
        private void ConnectPins(PinVM selectedOutputPin, PinVM selectedInputPin)
        {
            this.StopWaitForTask();
            this.RestoreSaveState();

            if (this.programManager.ConnectPins(selectedOutputPin.Pin, selectedInputPin.Pin))
            {
                var conn = new ConnectionVM(
                    selectedOutputPin,
                    selectedInputPin,
                    this.NewUniqueConnectionId(),
                this.config.LinePassiveColor);
                this.connectionsVM.Add(conn);
                this.UpdateUndoHistory(); // If connect successful update history
                this.SetSaveState();
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
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data in this case not used.</param>
        private void RefreshVM(object sender, EventArgs e)
        {
            var oldTemp = this.SelectedFieldComponent;
            this.SelectedFieldComponent = null;
            this.SelectedFieldComponent = oldTemp;
        }

        /// <summary>
        /// Creates an unique name by adding a serial number to the label ending.
        /// </summary>
        /// <param name="node">The node that will contain the identifier.</param>
        /// <returns>The identifier will be returned.</returns>
        private string CreateUniqueName(IDisplayableNode node)
        {
            this.uniqueNodeId++;
            return this.CreateNameTag(node.Label, this.uniqueNodeId.ToString());
        }

        /// <summary>
        /// Gets the unique next number.
        /// </summary>
        /// <returns>The next integer.</returns>
        private IEnumerator<int> GetUniqueNumber()
        {
            for (int i = 0; i < int.MaxValue; i++)
            {
                yield return this.uniquePinId++;
            }
        }

        /// <summary>
        /// Triggers the initialization process to refresh all selectable components in view.
        /// </summary>
        /// <param name="sender">The sender of the event, program manager.</param>
        /// <param name="e">The <see cref="FileSystemEventArgs"/> instance containing the event data.</param>
        private void NewModuleAdded(object sender, FileSystemEventArgs e)
        {
            this.programManager.InitializeNodesToChooseFromVoid();

            var nodesToChoose = this.programManager.SerializationPathInfo.Select(node => new ComponentRepresentationVM(this.addCommand, node.Item1, node.Item2));

            App.Current.Dispatcher.Invoke(() =>
            this.SelectableComponents = new ObservableCollection<ComponentRepresentationVM>(nodesToChoose));
        }

        /// <summary>
        /// Occurs when pins are connected.
        /// </summary>
        /// <returns>Returns a new id.</returns>    
        private string NewUniqueConnectionId()
        {
            string s = "Connection" + this.newUniqueConnectionId.ToString();
            this.newUniqueConnectionId++;
            return s;
        }
    }
}
