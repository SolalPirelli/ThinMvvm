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
    /// Extension methods for <see cref="ICommandOwner" /> that enable command creation and caching.
    /// </summary>
    public static class ICommandOwnerExtensions
    {
        private static readonly ConditionalWeakTable<ICommandOwner, Dictionary<string, ICommand>> _commands
            = new ConditionalWeakTable<ICommandOwner, Dictionary<string, ICommand>>();

        /// <summary>
        /// Gets or creates a <see cref="Command" /> with the specified action and optional condition.
        /// </summary>
        /// <param name="owner">The command owner.</param>
        /// <param name="execute">The action to execute when the command is executed.</param>
        /// <param name="canExecute">Optional. The predicate indicating whether the command can be executed.</param>
        /// <param name="name">Optional. Should not be provided as it uses compiler functionality. The command name.</param>
        public static Command GetCommand( this ICommandOwner owner, Action execute, Expression<Func<bool>> canExecute = null, [CallerMemberName] string name = "" )
        {
            if ( owner == null )
            {
                throw new ArgumentNullException( "owner" );
            }
            if ( execute == null )
            {
                throw new ArgumentNullException( "execute" );
            }

            var ownerCommands = _commands.GetOrCreateValue( owner );
            if ( !ownerCommands.ContainsKey( name ) )
            {
                ownerCommands.Add( name, new Command( owner, execute, canExecute ) );
            }

            return (Command) ownerCommands[name];
        }

        /// <summary>
        /// Gets or creates a <see cref="Command{T}" /> with the specified action and optional condition.
        /// </summary>
        /// <typeparam name="T">The command's parameter type.</typeparam>
        /// <param name="owner">The command owner.</param>
        /// <param name="execute">The action to execute when the command is executed.</param>
        /// <param name="canExecute">Optional. The predicate indicating whether the command can be executed.</param>
        /// <param name="name">Optional. Should not be provided as it uses compiler functionality. The command name.</param>
        public static Command<T> GetCommand<T>( this ICommandOwner owner, Action<T> execute, Expression<Func<T, bool>> canExecute = null, [CallerMemberName] string name = "" )
        {
            if ( owner == null )
            {
                throw new ArgumentNullException( "owner" );
            }
            if ( execute == null )
            {
                throw new ArgumentNullException( "execute" );
            }

            var ownerCommands = _commands.GetOrCreateValue( owner );
            if ( !ownerCommands.ContainsKey( name ) )
            {
                ownerCommands.Add( name, new Command<T>( owner, execute, canExecute ) );
            }

            return (Command<T>) ownerCommands[name];
        }

        /// <summary>
        /// Gets or creates an <see cref="AsyncCommand" /> with the specified action and optional condition.
        /// </summary>
        /// <param name="owner">The command owner.</param>
        /// <param name="execute">The action to execute when the command is executed.</param>
        /// <param name="canExecute">Optional. The predicate indicating whether the command can be executed.</param>
        /// <param name="name">Optional. Should not be provided as it uses compiler functionality. The command name.</param>
        public static AsyncCommand GetAsyncCommand( this ICommandOwner owner, Func<Task> execute, Expression<Func<bool>> canExecute = null, [CallerMemberName] string name = "" )
        {
            if ( owner == null )
            {
                throw new ArgumentNullException( "owner" );
            }
            if ( execute == null )
            {
                throw new ArgumentNullException( "execute" );
            }

            var ownerCommands = _commands.GetOrCreateValue( owner );
            if ( !ownerCommands.ContainsKey( name ) )
            {
                ownerCommands.Add( name, new AsyncCommand( owner, execute, canExecute ) );
            }

            return (AsyncCommand) ownerCommands[name];
        }

        /// <summary>
        /// Gets or creates an <see cref="AsyncCommand{T}" /> with the specified action and optional condition.
        /// </summary>
        /// <typeparam name="T">The command's parameter type.</typeparam>
        /// <param name="owner">The command owner.</param>
        /// <param name="execute">The action to execute when the command is executed.</param>
        /// <param name="canExecute">Optional. The predicate indicating whether the command can be executed.</param>
        /// <param name="name">Optional. Should not be provided as it uses compiler functionality. The command name.</param>
        public static AsyncCommand<T> GetAsyncCommand<T>( this ICommandOwner owner, Func<T, Task> execute, Expression<Func<T, bool>> canExecute = null, [CallerMemberName] string name = "" )
        {
            if ( owner == null )
            {
                throw new ArgumentNullException( "owner" );
            }
            if ( execute == null )
            {
                throw new ArgumentNullException( "execute" );
            }

            var ownerCommands = _commands.GetOrCreateValue( owner );
            if ( !ownerCommands.ContainsKey( name ) )
            {
                ownerCommands.Add( name, new AsyncCommand<T>( owner, execute, canExecute ) );
            }

            return (AsyncCommand<T>) ownerCommands[name];
        }
    }
}