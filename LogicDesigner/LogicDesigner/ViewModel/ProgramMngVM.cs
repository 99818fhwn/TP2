using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
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

        public event EventHandler<FieldComponentEventArgs> FieldComponentAdded;
        public event EventHandler<EventArgs> PreFieldComponentAdded;
        public event EventHandler<FieldComponentEventArgs> FieldComponentRemoved;
        public event EventHandler<FieldComponentEventArgs> FieldComponentChanged;

        private readonly Command activateCommand;
        private readonly Command executeCommand;
        private readonly Command removeCommand;
        private readonly Command addCommand;

        public ProgramMngVM()
        {
            this.programManager = new ProgramManager();
            this.programManager.Watcher.Created += NewModuleAdded;

            this.activateCommand = new Command(obj =>
            {
                var nodeInFieldVM = obj as ComponentVM;
                nodeInFieldVM.Activate();
            });

            this.executeCommand = new Command(obj =>
            {
                var nodeInFieldVM = obj as ComponentVM;
                nodeInFieldVM.Execute();
            });

            this.removeCommand = new Command(obj =>
            {
                // null reference exception
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
                // null reference exception
                var representationNode = obj as ComponentRepresentationVM;
                this.PreFieldComponentAdded(this, new EventArgs());
                var realComponent = representationNode.Node;
                var newGenerateComp = (IDisplayableNode)Activator.CreateInstance(realComponent.GetType());////Create new Component
                this.programManager.FieldNodes.Add(newGenerateComp);
                var compVM = new ComponentVM(newGenerateComp, CreateUniqueName(realComponent), this.executeCommand, this.activateCommand, this.removeCommand);
                this.nodesVMInField.Add(compVM);
                OnFieldComponentCreated(this, new FieldComponentEventArgs(compVM));
            });

            var nodesInField = this.programManager.FieldNodes.Select(node => new ComponentVM(node,
                CreateUniqueName(node), this.activateCommand, this.executeCommand, this.removeCommand
                ));////seems suspicious for unnecessary inputs

            this.nodesVMInField = new ObservableCollection<ComponentVM>(nodesInField);

            var nodesToChoose = this.programManager.PossibleNodesToChooseFrom.Select(
                node => new ComponentRepresentationVM(this.addCommand, node));

            this.SelectableComponents = new ObservableCollection<ComponentRepresentationVM>(nodesToChoose);
            this.nodesVMInField = new ObservableCollection<ComponentVM>(nodesInField);
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
            //foreach (var node in old.nodesVMInField) ////Will be obsolete.
            //{
            //    this.nodesVMInField.Add(node);
            //}
            this.SelectableComponents = old.SelectableComponents;
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
