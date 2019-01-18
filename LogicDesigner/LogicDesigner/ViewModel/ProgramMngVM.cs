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
        private ObservableCollection<ComponentVM> possibleComponentsVMToChooseFrom;
        private int uniqueId;

        public event EventHandler<FieldComponentEventArgs> FieldComponentAdded;
        public event EventHandler<FieldComponentEventArgs> FieldComponentRemoved;
        public event EventHandler<FieldComponentEventArgs> FieldComponentChanged;

        public ProgramMngVM()
        {
            this.programManager = new ProgramManager();

            var addCommand = new Command(obj =>
            {
                // null reference exception
                var nodeInFieldVM = obj as ComponentVM;

                this.programManager.FieldNodes.Add(
                    (IDisplayableNode)Activator.CreateInstance(nodeInFieldVM.Node.GetType()));

                this.nodesVMInField.Add(nodeInFieldVM);
                this.OnFieldComponentCreated(this, new FieldComponentEventArgs(nodeInFieldVM));
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

                this.programManager.FieldNodes.Remove(
                    (IDisplayableNode)Activator.CreateInstance(nodeInFieldVM.GetType()));
            });

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


            Func<IDisplayableNode, string> newUniqueName = new Func<IDisplayableNode, string>(node =>
            {
                this.uniqueId++;
                return this.CreateNameTag(node.Label, this.uniqueId.ToString());
            });

            var nodesInField = this.programManager.FieldNodes.Select(node => new ComponentVM(node,
                activateCommand, addCommand, executeCommand, removeCommand,
                newUniqueName(node)));

            this.nodesVMInField = new ObservableCollection<ComponentVM>(nodesInField);

            var nodesToChoose = this.programManager.PossibleNodesToChooseFrom.Select(
                node => new ComponentVM(node,
                activateCommand, addCommand, executeCommand, removeCommand,
                newUniqueName(node)));

            this.possibleComponentsVMToChooseFrom = new ObservableCollection<ComponentVM>(nodesToChoose);
            this.nodesVMInField = new ObservableCollection<ComponentVM>(nodesInField);
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
            this.nodesVMInField = new ObservableCollection<ComponentVM>(); ////Can be solved by  new ObservableCollection<ComponentVM>(old.nodesVMInField);
            foreach (var node in old.nodesVMInField) ////Will be obsolete.
            {
                this.nodesVMInField.Add(node);
            }
            this.PossibleComponentsToChooseFrom = old.PossibleComponentsToChooseFrom;
            this.programManager = new ProgramManager(old.programManager);
        }

        public ObservableCollection<ComponentVM> NodesVMInField
        {
            get
            {
                return this.nodesVMInField;
            }
        }

        public ObservableCollection<ComponentVM> PossibleComponentsToChooseFrom
        {
            get
            {
                return this.possibleComponentsVMToChooseFrom;
            }
            set
            {
                this.possibleComponentsVMToChooseFrom = value;
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
