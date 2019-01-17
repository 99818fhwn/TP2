using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogicDesigner.ViewModel
{
    public class ProgramMngVM
    {
        private ObservableCollection<ComponentVM> nodesVMInField;
        private ObservableCollection<ComponentVM> possibleComponentsToChooseFrom;
        private Command addComponentToField;
        private Command removeComponentFromField;

        public ObservableCollection<ComponentVM> PossibleComponentsToChooseFrom
        {
            get
            {
                return this.possibleComponentsToChooseFrom;
            }
        }
    }
}
