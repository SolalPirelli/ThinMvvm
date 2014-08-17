// Copyright (c) Solal Pirelli 2014
// See License.txt file for more details

using System;
using System.ComponentModel;
using System.Linq.Expressions;

namespace ThinMvvm.Internals
{
    /// <summary>
    /// The base class for Commands.
    /// </summary>
    /// <remarks>
    /// ICommand is implemented partially to let implementers implement it explicitly
    /// so that the parameters can be ignored or of the correct type.
    /// </remarks>
    [EditorBrowsable( EditorBrowsableState.Never )]
    public abstract class CommandBase
    {
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


        #region Partial ICommand implementation
        /// <summary>
        /// Occurs when changes occur that affect whether or not the command should execute.
        /// </summary>
        public event EventHandler CanExecuteChanged;

        /// <summary>
        /// Fires the CanExecuteChanged event.
        /// </summary>
        public void OnCanExecuteChanged()
        {
            var evt = CanExecuteChanged;
            if ( evt != null )
            {
                evt( this, EventArgs.Empty );
            }
        }
        #endregion

        /// <summary>
        /// Occurs when the command is executed.
        /// </summary>
        internal event EventHandler<CommandExecutedEventArgs> Executed;

        /// <summary>
        /// Fires the Executed event.
        /// </summary>
        /// <param name="parameter">The parameter given to the command's Execute method, if any.</param>
        protected void OnExecuted( object parameter = null )
        {
            var evt = Executed;
            if ( evt != null )
            {
                evt( this, new CommandExecutedEventArgs( parameter ) );
            }
        }
    }
}