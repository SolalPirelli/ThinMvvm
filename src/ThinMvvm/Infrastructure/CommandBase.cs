using System;
using System.ComponentModel;
using System.Windows.Input;

namespace ThinMvvm.Infrastructure
{
    /// <summary>
    /// Infrastructure.
    /// Base class for commands, implementing <see cref="ICommand" /> with an additional event raised on execution.
    /// </summary>
    [EditorBrowsable( EditorBrowsableState.Advanced )]
    public abstract class CommandBase : ICommand
    {
        /// <summary>
        /// Occurs when the command is executed.
        /// </summary>
        public event EventHandler<CommandExecutedEventArgs> Executed;

        /// <summary>
        /// Occurs when changes occur that affect whether or not the command can execute.
        /// </summary>
        public event EventHandler CanExecuteChanged;


        /// <summary>
        /// Triggers the <see cref="CanExecuteChanged" /> event.
        /// </summary>
        public void OnCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke( this, EventArgs.Empty );
        }


        /// <summary>
        /// Triggers the <see cref="Executed" /> event with the specified argument.
        /// </summary>
        /// <param name="argument">The argument.</param>
        protected void OnExecuted( object argument )
        {
            Executed?.Invoke( this, new CommandExecutedEventArgs( argument ) );
        }


        /// <summary>
        /// Indicates whether the command can execute.
        /// </summary>
        /// <param name="argument">The command argument.</param>
        /// <returns>A value indicating whether the command can execute.</returns>
        protected abstract bool CanExecute( object argument );

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="argument">The command argument.</param>
        protected abstract void Execute( object argument );


        // Explicit ICommand implementation, so that consumers use the correct methods
        // for direct calls on subclasses, e.g. Execute() if there's no parameter.
        // This is mostly useful for testing code, since UI frameworks will use them as ICommands.

        bool ICommand.CanExecute( object argument )
        {
            return CanExecute( argument );
        }

        void ICommand.Execute( object argument )
        {
            Execute( argument );
        }
    }
}