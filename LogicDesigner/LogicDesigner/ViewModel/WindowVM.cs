using System;
using System.Collections.Generic;
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

        public object ComponentGrid { get; private set; }

        public WindowVM()
        {
        }

        public void GenerateComponents()
        {

        }

        ///// <summary>
        ///// Called when the button is pressed down.
        ///// </summary>
        ///// <param name="sender">The sender.</param>
        ///// <param name="e">The <see cref="MouseButtonEventArgs"/> instance containing the event data.</param>
        //private void ComponentMouseDown(object sender, MouseButtonEventArgs e)
        //{
        //    Button button = (Button)sender;

        //    if (this.componentPosition == null)
        //    {
        //        this.componentPosition = button.TransformToAncestor(VisualTreeHelper.GetParent(button)).Transform(new Point(0, 0));
        //    }

        //    Point mousePosition = Mouse.GetPosition(ComponentGrid);

        //    this.deltaX = mousePosition.X - this.componentPosition.Value.X;
        //    this.deltaY = mousePosition.Y - this.componentPosition.Value.Y;

        //    this.isMoving = true;
        //}

        ///// <summary>
        ///// Called when the button is pressed up.
        ///// </summary>
        ///// <param name="sender">The sender.</param>
        ///// <param name="e">The <see cref="MouseButtonEventArgs"/> instance containing the event data.</param>
        //private void ComponentMouseUp(object sender, MouseButtonEventArgs e)
        //{
        //    this.translateTransform = SampleComponent.RenderTransform as TranslateTransform;
        //    isMoving = false;
        //}

        ///// <summary>
        ///// Called when the button is moved.
        ///// </summary>
        ///// <param name="sender">The sender.</param>
        ///// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
        //private void ComponentMouseMove(object sender, MouseEventArgs e)
        //{
        //    if (!this.isMoving)
        //    {
        //        return;
        //    }

        //    var mousePosition = Mouse.GetPosition(ComponentGrid);

        //    var offsetX = (translateTransform == null ? componentPosition.Value.X : componentPosition.Value.X - translateTransform.X) + deltaX - mousePosition.X;
        //    var offsetY = (translateTransform == null ? componentPosition.Value.Y : componentPosition.Value.Y - translateTransform.Y) + deltaY - mousePosition.Y;

        //    this.SampleComponent.RenderTransform = new TranslateTransform(-offsetX, -offsetY);
        //}
    }
}

