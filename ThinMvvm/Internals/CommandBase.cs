// Copyright (c) 2014-15 Solal Pirelli
// See License.txt file for more details

using System;
using System.ComponentModel;
using System.Linq.Expressions;

namespace ThinMvvm.Internals
{
    /// <summary>
    /// The base class for commands.
    /// </summary>
    [EditorBrowsable( EditorBrowsableState.Never )]
    public abstract class CommandBase
    {
        private readonly WeakEvent _canExecuteChanged;
        private readonly WeakEvent _executed;

        /// <summary>
        /// Gets the object that owns the command.
        /// </summary>
        internal object Owner { get; private set; }


        /// <summary>
        /// Initializes a new instance of the <see cref="CommandBase" /> class with the specified predicate.
        /// </summary>
        /// <param name="owner">The command's owner.</param>
        /// <param name="canExecute">The predicate indicating whether the command can be executed, or null to always execute it.</param>
        protected CommandBase( object owner, Expression canExecute )
        {
            _canExecuteChanged = new WeakEvent();
            _executed = new WeakEvent();

            Owner = owner;

            if ( canExecute == null )
            {
                return;
            }

            foreach ( var obsProp in ObservablePropertyVisitor.GetObservablePropertyAccesses( canExecute ) )
            {
                obsProp.Item1.ListenToProperty( obsProp.Item2, OnCanExecuteChanged );
            }
        }


        /// <summary>
        /// Occurs when changes occur that affect whether or not the command should execute.
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add { _canExecuteChanged.Add( value ); }
            remove { _canExecuteChanged.Remove( value ); }
        }

        /// <summary>
        /// Fires the CanExecuteChanged event.
        /// </summary>
        public void OnCanExecuteChanged()
        {
            _canExecuteChanged.Raise( this, EventArgs.Empty );
        }


        /// <summary>
        /// Occurs when the command is executed.
        /// </summary>
        internal event EventHandler<CommandExecutedEventArgs> Executed
        {
            add { _executed.Add( value ); }
            remove { _executed.Remove( value ); }
        }

        /// <summary>
        /// Fires the Executed event.
        /// </summary>
        /// <param name="parameter">The parameter given to the command's Execute method, if any.</param>
        protected void OnExecuted( object parameter )
        {
            _executed.Raise( this, new CommandExecutedEventArgs( parameter ) );
        }
    }
}