// Copyright (c) Solal Pirelli 2014
// See License.txt file for more details

using System;
using System.Globalization;
using System.Linq.Expressions;
using System.Windows.Input;
using ThinMvvm.Internals;

namespace ThinMvvm
{
    /// <summary>
    /// Synchronous <see cref="ICommand"/> without a parameter.
    /// </summary>
    public sealed class Command : CommandBase, ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool> _canExecute;

        /// <summary>
        /// Initializes a new instance of the <see cref="Command" /> class with the specified action and optional condition.
        /// </summary>
        /// <param name="owner">The command's owner.</param>
        /// <param name="execute">The action to execute when the command is executed.</param>
        /// <param name="canExecute">Optional. The predicate indicating whether the command can be executed.</param>
        internal Command( object owner, Action execute, Expression<Func<bool>> canExecute = null )
            : base( owner, canExecute )
        {
            _execute = execute;
            _canExecute = canExecute == null ? null : canExecute.Compile();
        }

        /// <summary>
        /// Defines the method that determines whether the command can execute in its current state.
        /// </summary>
        /// <returns>True if this command can be executed; otherwise, false.</returns>
        public bool CanExecute()
        {
            return ( (ICommand) this ).CanExecute( null );
        }

        /// <summary>
        /// Defines the method to be called when the command is invoked.
        /// </summary>
        public void Execute()
        {
            ( (ICommand) this ).Execute( null );
        }

        #region ICommand implementation
        /// <summary>
        /// Defines the method that determines whether the command can execute in its current state.
        /// </summary>
        /// <param name="parameter">Ignored parameter.</param>
        /// <returns>True if this command can be executed; otherwise, false.</returns>
        bool ICommand.CanExecute( object parameter )
        {
            return _canExecute == null || _canExecute();
        }

        /// <summary>
        /// Defines the method to be called when the command is invoked.
        /// </summary>
        /// <param name="parameter">Ignored parameter.</param>
        void ICommand.Execute( object parameter )
        {
            OnExecuted( null );
            _execute();
        }
        #endregion
    }

    /// <summary>
    /// Synchronous <see cref="ICommand"/> with a parameter.
    /// </summary>
    /// <typeparam name="T">The parameter type.</typeparam>
    public sealed class Command<T> : CommandBase, ICommand
    {
        private readonly Action<T> _execute;
        private readonly Func<T, bool> _canExecute;

        /// <summary>
        /// Initializes a new instance of the <see cref="Command{T}" /> class with the specified action and optional condition.
        /// </summary>
        /// <param name="owner">The command's owner.</param>
        /// <param name="execute">The action to execute when the command is executed.</param>
        /// <param name="canExecute">Optional. The predicate indicating whether the command can be executed.</param>
        internal Command( object owner, Action<T> execute, Expression<Func<T, bool>> canExecute = null )
            : base( owner, canExecute )
        {
            _execute = execute;
            _canExecute = canExecute == null ? null : canExecute.Compile();
        }

        /// <summary>
        /// Defines the method that determines whether the command can execute in its current state.
        /// </summary>
        /// <param name="parameter">Data used by the command.</param>
        /// <returns>True if this command can be executed; otherwise, false.</returns>
        public bool CanExecute( T parameter )
        {
            return _canExecute == null || _canExecute( parameter );
        }

        /// <summary>
        /// Defines the method to be called when the command is invoked.
        /// </summary>
        /// <param name="parameter">Data used by the command.</param>
        public void Execute( T parameter )
        {
            OnExecuted( parameter );
            _execute( parameter );
        }

        #region ICommand implementation
        /// <summary>
        /// Defines the method that determines whether the command can execute in its current state.
        /// </summary>
        /// <param name="parameter">Data used by the command.</param>
        /// <returns>True if this command can be executed; otherwise, false.</returns>
        bool ICommand.CanExecute( object parameter )
        {
            if ( parameter is T )
            {
                return CanExecute( (T) parameter );
            }

            return false;
        }

        /// <summary>
        /// Defines the method to be called when the command is invoked.
        /// </summary>
        /// <param name="parameter">Data used by the command.</param>
        void ICommand.Execute( object parameter )
        {
            if ( !( parameter is T ) )
            {
                throw new ArgumentException( string.Format( CultureInfo.InvariantCulture,
                                                            "Wrong parameter type. Expected {0}, got {1}.",
                                                            typeof( T ).FullName, parameter.GetType().FullName ),
                                             "parameter" );
            }

            Execute( (T) parameter );
        }
        #endregion
    }
}