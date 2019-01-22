//-----------------------------------------------------------------------
// <copyright file="Command.cs" company="FH">
//     Company copyright tag.
// </copyright>
//-----------------------------------------------------------------------
namespace LogicDesigner.Commands
{
    using System;
    using System.Windows.Input;

    /// <summary>
    /// The command class.
    /// </summary>
    /// <seealso cref="System.Windows.Input.ICommand" />
    public class Command : ICommand
    {
        /// <summary>
        /// The action to execute.
        /// </summary>
        private Action<object> action;

        /// <summary>
        /// Initializes a new instance of the <see cref="Command"/> class.
        /// </summary>
        /// <param name="action">The action.</param>
        public Command(Action<object> action)
        {
            this.action = action;
        }

        /// <summary>
        /// Occurs when changes occur that affect whether or not the command should execute.
        /// </summary>
        public event EventHandler CanExecuteChanged;
        
        /// <summary>
        /// Defines the method that determines whether the command can execute in its current state.
        /// </summary>
        /// <param name="parameter">Data used by the command.  If the command does not require data to be passed, this object can be set to <see langword="null" />.</param>
        /// <returns>
        /// <see langword="true" /> if this command can be executed; otherwise, <see langword="false" />.
        /// </returns>
        public bool CanExecute(object parameter)
        {
            // if item implements IClickable interface
            return true;
        }

        /// <summary>
        /// Defines the method to be called when the command is invoked.
        /// </summary>
        /// <param name="parameter">Data used by the command.  If the command does not require data to be passed, this object can be set to <see langword="null" />.</param>
        public void Execute(object parameter)
        {
            this.action(parameter);
        }
    }
}
