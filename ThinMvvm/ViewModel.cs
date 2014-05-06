// Copyright (c) Solal Pirelli 2014
// See License.txt file for more details

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ThinMvvm
{
    /// <summary>
    /// Concrete implementation of IViewModel, inheriting from ObservableObject.
    /// </summary>
    /// <typeparam name="TParameter">The type of the ViewModel's constructor parameter, or <see cref="NoParameter" /> if it does not have one.</typeparam>
    public abstract class ViewModel<TParameter> : ObservableObject, IViewModel<TParameter>
    {
        private Dictionary<string, ICommand> _cache = new Dictionary<string, ICommand>();

        /// <summary>
        /// Executed when the user navigates to the ViewModel.
        /// </summary>
        public virtual void OnNavigatedTo() { }

        /// <summary>
        /// Executed when the user navigates from the ViewModel.
        /// </summary>
        public virtual void OnNavigatedFrom() { }


        /// <summary>
        /// Gets or creates a Command without parameter that will be unique to this ViewModel.
        /// </summary>
        /// <param name="execute">The action to execute when the command is executed.</param>
        /// <param name="canExecute">Optional. The predicate indicating whether the command can be executed.</param>
        /// <param name="name">Optional. Should not be provieded as it uses compiler functionality. The command name.</param>
        protected Command GetCommand( Action execute, Expression<Func<bool>> canExecute = null, [CallerMemberName] string name = "" )
        {
            if ( !_cache.ContainsKey( name ) )
            {
                _cache.Add( name, new Command( this, execute, canExecute ) );
            }

            return (Command) _cache[name];
        }

        /// <summary>
        /// Gets or creates a Command that will be unique to this ViewModel.
        /// </summary>
        /// <param name="execute">The action to execute when the command is executed.</param>
        /// <param name="canExecute">Optional. The predicate indicating whether the command can be executed.</param>
        /// <param name="name">Optional. Should not be provieded as it uses compiler functionality. The command name.</param>
        protected Command<T> GetCommand<T>( Action<T> execute, Expression<Func<T, bool>> canExecute = null, [CallerMemberName] string name = "" )
        {
            if ( !_cache.ContainsKey( name ) )
            {
                _cache.Add( name, new Command<T>( this, execute, canExecute ) );
            }

            return (Command<T>) _cache[name];
        }

        // N.B.: The following two methods cannot be called GetCommand because since the return type of a function is not part of its
        //       signature, overload resolution cannot decide whether a parameterless method is an Action or a Func<T>.

        /// <summary>
        /// Gets or creates an AsyncCommand without parameter that will be unique to this ViewModel.
        /// </summary>
        /// <param name="execute">The action to execute when the command is executed.</param>
        /// <param name="canExecute">Optional. The predicate indicating whether the command can be executed.</param>
        /// <param name="name">Optional. Should not be provieded as it uses compiler functionality. The command name.</param>
        protected AsyncCommand GetAsyncCommand( Func<Task> execute, Expression<Func<bool>> canExecute = null, [CallerMemberName] string name = "" )
        {
            if ( !_cache.ContainsKey( name ) )
            {
                _cache.Add( name, new AsyncCommand( this, execute, canExecute ) );
            }

            return (AsyncCommand) _cache[name];
        }

        /// <summary>
        /// Gets or creates an AsyncCommand that will be unique to this ViewModel.
        /// </summary>
        /// <param name="execute">The action to execute when the command is executed.</param>
        /// <param name="canExecute">Optional. The predicate indicating whether the command can be executed.</param>
        /// <param name="name">Optional. Should not be provieded as it uses compiler functionality. The command name.</param>
        protected AsyncCommand<T> GetAsyncCommand<T>( Func<T, Task> execute, Expression<Func<T, bool>> canExecute = null, [CallerMemberName] string name = "" )
        {
            if ( !_cache.ContainsKey( name ) )
            {
                _cache.Add( name, new AsyncCommand<T>( this, execute, canExecute ) );
            }

            return (AsyncCommand<T>) _cache[name];
        }
    }
}