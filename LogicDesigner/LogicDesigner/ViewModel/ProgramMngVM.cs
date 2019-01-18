using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LogicDesigner.Commands;
using LogicDesigner.Model;
using Shared;

namespace LogicDesigner.ViewModel
{
    public class ProgramMngVM
    {
        // A random comment appeared
        private ProgramManager programManager;
        private ObservableCollection<ComponentVM> nodesVMInField;
        private ObservableCollection<ComponentVM> possibleComponentsVMToChooseFrom;

        //private Command addComponentToFieldCommand;
        //private Command removeComponentFromFieldCommand;

        public event EventHandler<FieldComponentEventArgs> FieldComponentAdded;
        public event EventHandler<FieldComponentEventArgs> FieldComponentRemoved;

        public ProgramMngVM()
        {
            this.programManager = new ProgramManager();

            var addCommand = new Command(obj =>
            {
                var nodeInFieldVM = obj as ComponentVM;

                this.programManager.FieldNodes.Add(
                    (IDisplayableNode)Activator.CreateInstance(nodeInFieldVM.Node.GetType()));

                this.OnFieldComponentCreated(this, new FieldComponentEventArgs(nodeInFieldVM));
            });

            var removeCommand = new Command(obj =>
            {
                var nodeInFieldVM = obj as ComponentVM;
                foreach (var n in this.programManager.FieldNodes)
                {
                    if (nodeInFieldVM.Node == n)
                    {
                        this.programManager.FieldNodes.Remove(n);
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

            var nodesInField = this.programManager.FieldNodes.Select(node => new ComponentVM(node,
                activateCommand, addCommand, removeCommand));

            this.nodesVMInField = new ObservableCollection<ComponentVM>(nodesInField);

            var nodesToChoose = this.programManager.PossibleNodesToChooseFrom.Select(
                node => new ComponentVM(node,
                activateCommand, addCommand, removeCommand));

            this.possibleComponentsVMToChooseFrom = new ObservableCollection<ComponentVM>(nodesToChoose);
            this.nodesVMInField = new ObservableCollection<ComponentVM>(nodesInField);

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProgramMngVM"/> class.
        /// </summary>
        /// <param name="old"> The ProgramMngVM which values should be copied. </param>
        public ProgramMngVM(ProgramMngVM old)
        {
            this.nodesVMInField = new ObservableCollection<ComponentVM>();
            foreach(var node in old.nodesVMInField)
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
    }
}
