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
        private ObservableCollection<ComponentVM> nodesVMInField;
        private ObservableCollection<ComponentRepresentationVM> selectableComponents;
        private int uniqueId;
        private PinVM selectedOutputPin;
        private PinVM selectedInputPin;

        public event EventHandler<FieldComponentEventArgs> FieldComponentAdded;
        public event EventHandler<EventArgs> PreFieldComponentAdded;
        public event EventHandler<FieldComponentEventArgs> FieldComponentRemoved;
        public event EventHandler<FieldComponentEventArgs> FieldComponentChanged;
        public event EventHandler<PinsConnectedEventArgs> PinsConnected;
        public event PropertyChangedEventHandler PropertyChanged;

        private readonly Command activateCommand;
        private readonly Command executeCommand;
        private readonly Command removeCommand;
        private readonly Command addCommand;

        public ProgramMngVM()
        {
            this.programManager = new ProgramManager();
            this.programManager.Watcher.Created += NewModuleAdded;

            this.StartCommand = new Command(obj =>
            {
               Task.Run(() => {
                   this.programManager.Run();
               });
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
                SetSelectedPin(pin);
            });

            this.activateCommand = new Command(obj =>
            {
                var nodeInFieldVM = obj as ComponentVM;
                nodeInFieldVM.Activate();
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
                        OnFieldComponentRemoved(this, new FieldComponentEventArgs(nodeInFieldVM));

                        break;
                    }
                }

                this.programManager.FieldNodes.Remove(nodeInFieldVM.Node);////Temporary fix for testing
            });

            this.addCommand = new Command(obj =>
            {
                var representationNode = obj as ComponentRepresentationVM;
                this.PreFieldComponentAdded(this, new EventArgs());
                var realComponent = representationNode.Node;
                var newGenerateComp = (IDisplayableNode)Activator.CreateInstance(realComponent.GetType());
                this.programManager.FieldNodes.Add(newGenerateComp);
                var compVM = new ComponentVM(newGenerateComp, CreateUniqueName(realComponent), setPinCommand,
                    this.activateCommand, this.removeCommand);
                this.nodesVMInField.Add(compVM);
                OnFieldComponentCreated(this, new FieldComponentEventArgs(compVM));
            });

            var nodesInField = this.programManager.FieldNodes.Select(node => new ComponentVM(node,
                CreateUniqueName(node), this.activateCommand, this.executeCommand, this.removeCommand
                ));

            this.nodesVMInField = new ObservableCollection<ComponentVM>(nodesInField);

            var nodesToChoose = this.programManager.PossibleNodesToChooseFrom.Select(
                node => new ComponentRepresentationVM(this.addCommand, node));

            this.SelectableComponents = new ObservableCollection<ComponentRepresentationVM>(nodesToChoose);
        }

        public void SetSelectedPin(PinVM value)
        {
            if (this.selectedOutputPin == value || this.selectedInputPin == value)
            {
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
                    ConnectPins(this.selectedOutputPin, this.selectedInputPin);
                }
            }
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

        private void ConnectPins(PinVM selectedOutputPin, PinVM selectedInputPin)
        {
            if (this.programManager.ConnectPins(selectedOutputPin.Pin, selectedInputPin.Pin))
            {
                OnPinsConnected(this, new PinsConnectedEventArgs(selectedOutputPin, selectedInputPin));
            }

            this.selectedInputPin = null;
            this.selectedOutputPin = null;

        }

        /// <summary>
        /// Creates an unique name by adding a serial number to the label ending.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns>The identifier will be returned.</returns>
        private string CreateUniqueName(IDisplayableNode node)
        {
            this.uniqueId++;
            return CreateNameTag(node.Label, this.uniqueId.ToString());
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

        private ComponentVM selectedFieldComponent;

        public ComponentVM SelectedFieldComponent
        {
            get
            {
                return this.selectedFieldComponent;
            }

            set
            {
                this.selectedFieldComponent = value;
                FireOnPropertyChanged();
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


        public void OnFieldComponentCreated(object sender, FieldComponentEventArgs e)
        {
            this.FieldComponentAdded?.Invoke(this, e);
        }

        public void OnFieldComponentRemoved(object sender, FieldComponentEventArgs e)
        {
            this.FieldComponentRemoved?.Invoke(this, e);
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

        public void OnPinsConnected(object sender, PinsConnectedEventArgs e)
        {
            this.PinsConnected?.Invoke(this, e);
        }

        protected virtual void FireOnPropertyChanged([CallerMemberName]string name = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
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
    }
}
