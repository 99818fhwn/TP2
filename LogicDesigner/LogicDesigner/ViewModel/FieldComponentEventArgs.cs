using System;

namespace LogicDesigner.ViewModel
{
    public class FieldComponentEventArgs : EventArgs
    {
        public FieldComponentEventArgs(ComponentVM component)
        {
            this.Component = component;
        }

        public ComponentVM Component
        {
            get;
            private set;
        }
    }
}