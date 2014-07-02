// Copyright (c) Solal Pirelli 2014
// See License.txt file for more details

using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ThinMvvm
{
    /// <summary>
    /// Asynchronous ICommand without a parameter.
    /// </summary>
    public sealed class AsyncCommand : CommandBase, ICommand
    {
        private readonly Func<Task> _execute;
        private readonly Func<bool> _canExecute;

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncCommand" /> class with the specified action and optional condition.
        /// </summary>
        /// <param name="owner">The command's owner.</param>
        /// <param name="execute">The action to execute when the command is executed.</param>
        /// <param name="canExecute">Optional. The predicate indicating whether the command can be executed.</param>
        public AsyncCommand( object owner, Func<Task> execute, Expression<Func<bool>> canExecute = null )
            : base( owner, canExecute )
        {
            _execute = execute;
            _canExecute = canExecute == null ? null : canExecute.Compile();
        }

        /// <summary>
        /// Asynchronously executes the command.
        /// </summary>
        /// <returns>The task object representing the asynchronous operation.</returns>
        public Task ExecuteAsync()
        {
            OnExecuted();
            return _execute();
        }

        /// <summary>
        /// Defines the method that determines whether the command can execute in its current state.
        /// </summary>
        /// <returns>True if this command can be executed; otherwise, false.</returns>
        public bool CanExecute()
        {
            return ( (ICommand) this ).CanExecute( null );
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
        async void ICommand.Execute( object parameter )
        {
            await ExecuteAsync();
        }
        #endregion
    }

    /// <summary>
    /// Asynchronous ICommand with a parameter.
    /// </summary>
    /// <typeparam name="T">The parameter type.</typeparam>
    public sealed class AsyncCommand<T> : CommandBase, ICommand
    {
        private readonly Func<T, Task> _execute;
        private readonly Func<T, bool> _canExecute;

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncCommand{T}" /> class with the specified action and optional condition.
        /// </summary>
        /// <param name="owner">The command's owner.</param>
        /// <param name="execute">The action to execute when the command is executed.</param>
        /// <param name="canExecute">Optional. The predicate indicating whether the command can be executed.</param>
        public AsyncCommand( object owner, Func<T, Task> execute, Expression<Func<T, bool>> canExecute = null )
            : base( owner, canExecute )
        {
            _execute = execute;
            _canExecute = canExecute == null ? null : canExecute.Compile();
        }

        /// <summary>
        /// Asynchronously executes the command.
        /// </summary>
        /// <param name="parameter">Data used by the command.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        public Task ExecuteAsync( T parameter )
        {
            OnExecuted( parameter );
            return _execute( parameter );
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
        async void ICommand.Execute( object parameter )
        {
            if ( parameter is T )
            {
                await ExecuteAsync( (T) parameter );
            }
        }
        #endregion
    }
}