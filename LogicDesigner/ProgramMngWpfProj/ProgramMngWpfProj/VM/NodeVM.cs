using Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProgramMngWpfProj.VM
{
    public class NodeVM : INotifyPropertyChanged
    {
        private INode node;

        public int XCoord
        {
            get;
            set;
        }

        public int YCoord
        {
            get;
            set;
        }

        public bool IsInField
        {
            get;
            set;
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
