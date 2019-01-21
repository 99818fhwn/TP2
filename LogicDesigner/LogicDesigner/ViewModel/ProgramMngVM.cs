using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using LogicDesigner.Commands;
using LogicDesigner.Model;
using Shared;

namespace LogicDesigner.ViewModel
{
    public class ProgramMngVM
    {
        private ProgramManager programManager;
        private ObservableCollection<ComponentVM> nodesVMInField;
        private ObservableCollection<ComponentRepresentationVM> selectableComponents;
        private int uniqueId;
        private PinVM selectedOutputPin;
        private PinVM selectedInputPin;
        private Command setPinCommand;

        public event EventHandler<FieldComponentEventArgs> FieldComponentAdded;
        public event EventHandler<FieldComponentEventArgs> FieldComponentRemoved;
        public event EventHandler<FieldComponentEventArgs> FieldComponentChanged;

        public ProgramMngVM()
        {
            this.programManager = new ProgramManager();

            var activateCommand = new Command(obj =>
            {
                var nodeInFieldVM = obj as ComponentVM;
                nodeInFieldVM.Activate();
            });

            var executeCommand = new Command(obj =>
            {
                var nodeInFieldVM = obj as ComponentVM;
                nodeInFieldVM.Execute();
            });

            var removeCommand = new Command(obj =>
            {
                // null reference exception
                var nodeInFieldVM = obj as ComponentVM;
                foreach (var n in this.programManager.FieldNodes)
                {
                    if (nodeInFieldVM.Node == n)
                    {
                        this.programManager.FieldNodes.Remove(n);
                        this.nodesVMInField.Remove(nodeInFieldVM);
                        this.OnFieldComponentRemoved(this, new FieldComponentEventArgs(nodeInFieldVM));

                        break;
                    }
                }

                this.programManager.FieldNodes.Remove(nodeInFieldVM.Node);////Temporary fix for testing
            });

            var addCommand = new Command(obj =>
            {
                // null reference exception?
                var representationNode = obj as ComponentRepresentationVM;
                var realComponent = representationNode.Node;
                var newGenerateComp = (IDisplayableNode)Activator.CreateInstance(realComponent.GetType());////Create new Component
                this.programManager.FieldNodes.Add(newGenerateComp);
                var compVM = new ComponentVM(newGenerateComp, this.CreateUniqueName(realComponent), executeCommand, 
                    activateCommand, removeCommand);
                this.nodesVMInField.Add(compVM);
                this.OnFieldComponentCreated(this, new FieldComponentEventArgs(compVM));
            });

            var nodesInField = this.programManager.FieldNodes.Select(node => new ComponentVM(node,
                this.CreateUniqueName(node),activateCommand, executeCommand, removeCommand
                ));

            this.nodesVMInField = new ObservableCollection<ComponentVM>(nodesInField);

            var nodesToChoose = this.programManager.PossibleNodesToChooseFrom.Select(
                node => new ComponentRepresentationVM(addCommand, node));

            this.SelectableComponents = new ObservableCollection<ComponentRepresentationVM>(nodesToChoose);

            this.setPinCommand = new Command(obj =>
            {
                var pin = obj as PinVM;
                this.SetSelectedPin(pin);
            });
        }

        public void SetSelectedPin(PinVM value)
        {
            if(this.selectedOutputPin == value || this.selectedInputPin == value)
            {
                this.selectedOutputPin = null;
            }
            else
            {
                if(!this.selectedOutputPin.IsInputPin)
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
        

        private void ConnectPins(PinVM selectedOutputPin, PinVM selectedInputPin)
        {
            this.programManager.ConnectPins(selectedOutputPin.Pin, selectedInputPin.Pin);
        }

        /// <summary>
        /// Creates an unique name by adding a serial number to the label ending.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns>The identifier will be returned.</returns>
        private string CreateUniqueName(IDisplayableNode node)
        {
            this.uniqueId++;
            return this.CreateNameTag(node.Label, this.uniqueId.ToString());
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
            //foreach (var node in old.nodesVMInField) ////Will be obsolete.
            //{
            //    this.nodesVMInField.Add(node);
            //}
            //this.SelectableComponents = old.SelectableComponents;
            this.programManager = new ProgramManager(old.programManager);
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
