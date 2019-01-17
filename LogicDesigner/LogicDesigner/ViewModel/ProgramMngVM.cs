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
        private ObservableCollection<ComponentVM> possibleComponentsToChooseFrom;

        private Command addComponentToField;
        private Command removeComponentFromField;

        public event EventHandler<FieldComponentEventArgs> FieldComponentCreated;
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
                return this.possibleComponentsToChooseFrom;
            }
        }

        public ProgramMngVM()
        {
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
                    nodeToChooseVM.Activate();
                })));

            this.nodesVMInField = new ObservableCollection<ComponentVM>(nodesInField);

            this.addComponentToField = new Command(obj =>
            {
                var nodeInFieldVM = obj as ComponentVM;

                this.programManager.FieldNodes.Add(
                    (IDisplayableNode)Activator.CreateInstance(nodeInFieldVM.Node.GetType()));

                this.OnFieldComponentCreated(this, new FieldComponentEventArgs(this.nodesVMInField.Last()));
            });

            this.addComponentToField = new Command(obj =>
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
            this.FieldComponentCreated?.Invoke(this, e);
        }

        public void OnFieldComponentRemoved(object sender, FieldComponentEventArgs e)
        {
            this.FieldComponentRemoved?.Invoke(this, e);
        }
    }
}
