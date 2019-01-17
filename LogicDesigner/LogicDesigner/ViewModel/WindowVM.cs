using Shared;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace LogicDesigner.ViewModel
{
    public class WindowVM
    {
        /// <summary>
        /// If a component is beeing dragged.
        /// </summary>
        private bool isMoving;

        /// <summary>
        /// The component position.
        /// </summary>
        private Point? componentPosition;

        /// <summary>
        /// The delta of the clicked x position on the component and the mouse x position.
        /// </summary>
        private double deltaX;

        /// <summary>
        /// The delta of the clicked y position on the component and the mouse y position.
        /// </summary>
        private double deltaY;

        /// <summary>
        /// The translated transform.
        /// </summary>
        private TranslateTransform translateTransform;
        
        public WindowVM()
        {
            this.PossibleComponents = new ObservableCollection<IDisplayableNode>();
        }

        public void GenerateComponents()
        {

        }

        public ObservableCollection<IDisplayableNode> PossibleComponents { get; set; }


    }
}

