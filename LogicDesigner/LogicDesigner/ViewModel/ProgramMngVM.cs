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
        private ProgramManager programManager;
        private ObservableCollection<ComponentVM> nodesVMInField;
        private ObservableCollection<ComponentVM> possibleComponentsVMToChooseFrom;

        private Command addComponentToFieldCommand;
        private Command removeComponentFromFieldCommand;

        public event EventHandler<FieldComponentEventArgs> FieldComponentAdded;
        public event EventHandler<FieldComponentEventArgs> FieldComponentRemoved;

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

        public ProgramMngVM()
        {
            // Added by Moe
            this.programManager = new ProgramManager();

            //var observableNodes = new ObservableCollection<ComponentVM>();

            //foreach(IDisplayableNode comp in this.programManager.PossibleNodesToChooseFrom)
            //{
            //    // Null definitely needs to be replaced here - Moe
            //    observableNodes.Add(new ComponentVM(comp, null));
            //}

            //this.PossibleComponentsToChooseFrom = observableNodes;

            var nodesInField = this.programManager.FieldNodes.Select(node => new ComponentVM(node, 
                new Command(obj => {
                    var nodeInFieldVM = obj as ComponentVM;
                    nodeInFieldVM.Activate();
                })));

            this.nodesVMInField = new ObservableCollection<ComponentVM>(nodesInField);

            var nodesToChoose = this.programManager.PossibleNodesToChooseFrom.Select(
                node => new ComponentVM(node,
                new Command(obj => {
                    var nodeToChooseVM = obj as ComponentVM;
                    // find instance in the collection ?
                    // -> Found instance in the collection? (Not sure about which collection though ¯\_(ツ)_/¯ ) LG Moe
                    //ComponentVM instance = PossibleComponentsToChooseFrom.Where((current) =>
                    //{
                    //    if (current.Node == nodeToChooseVM.Node)
                    //    {
                    //        return true;
                    //    }
                    //    return false;
                    //}).First();
                    //// End of Moes code

                    nodeToChooseVM.Activate();
                })));

            this.possibleComponentsVMToChooseFrom = new ObservableCollection<ComponentVM>(nodesToChoose);
            this.nodesVMInField = new ObservableCollection<ComponentVM>(nodesInField);

            this.addComponentToFieldCommand = new Command(obj =>
            {
                var nodeInFieldVM = obj as ComponentVM;

                this.programManager.FieldNodes.Add(
                    (IDisplayableNode)Activator.CreateInstance(nodeInFieldVM.Node.GetType()));

                this.OnFieldComponentCreated(this, new FieldComponentEventArgs(this.nodesVMInField.Last()));
            });

            this.removeComponentFromFieldCommand = new Command(obj =>
            {
                var nodeInFieldVM = obj as ComponentVM;
                foreach (var n in this.programManager.FieldNodes)
                {
                    if(nodeInFieldVM.Node == n)
                    {
                        this.programManager.FieldNodes.Remove(n);
                        this.OnFieldComponentRemoved(this, new FieldComponentEventArgs(nodeInFieldVM));

                        break;
                    }
                }

                this.programManager.FieldNodes.Remove(
                (IDisplayableNode)Activator.CreateInstance(nodeInFieldVM.GetType()));
            });
        }

        public void OnFieldComponentCreated(object sender, FieldComponentEventArgs e)
        {
            this.FieldComponentAdded?.Invoke(this, e);
        }

        public void OnFieldComponentRemoved(object sender, FieldComponentEventArgs e)
        {
            this.FieldComponentRemoved?.Invoke(this, e);
        }

        public Command AddComponentCommand
        {
            get
            {
                return this.addComponentToFieldCommand;
            }
        }

        public Command RemoveComponentCommand
        {
            get
            {
                return this.removeComponentFromFieldCommand;
            }
        }
    }
}
