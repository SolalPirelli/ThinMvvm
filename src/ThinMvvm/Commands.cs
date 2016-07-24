using System;
using System.Threading.Tasks;
using ThinMvvm.Infrastructure;

namespace ThinMvvm
{
    /// <summary>
    /// Synchronous parameterless command.
    /// </summary>
    public sealed class Command : CommandBase
    {
        private readonly Func<bool> _canExecute;
        private readonly Action _execute;


        /// <summary>
        /// Initializes a new instance of the <see cref="Command" /> class with the specified behaviors.
        /// </summary>
        /// <param name="execute">The function to execute.</param>
        /// <param name="canExecute">The execution constraints, if any.</param>
        public Command( Action execute, Func<bool> canExecute = null )
        {
            if( execute == null )
            {
                throw new ArgumentNullException( nameof( execute ) );
            }

            _canExecute = canExecute;
            _execute = execute;
        }


        /// <summary>
        /// Checks whether the command can execute.
        /// </summary>
        /// <returns>A value indicating whether the command can execute.</returns>
        public bool CanExecute()
        {
            if( _canExecute == null )
            {
                return true;
            }

            return _canExecute();
        }

        /// <summary>
        /// Executes the command.
        /// </summary>
        public void Execute()
        {
            _execute();
            OnExecuted( null );
        }


        /// <summary>
        /// Checks whether the command can execute.
        /// </summary>
        /// <param name="argument">The command argument, which is ignored.</param>
        /// <returns>A value indicating whether the command can execute.</returns>
        protected override bool CanExecute( object argument )
        {
            return CanExecute();
        }

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="argument">The command argument, which is ignored.</param>
        protected override void Execute( object argument )
        {
            Execute();
        }
    }

    /// <summary>
    /// Synchronous parameterized command.
    /// </summary>
    /// <typeparam name="T">The parameter type.</typeparam>
    public sealed class Command<T> : CommandBase
    {
        private readonly Func<T, bool> _canExecute;
        private readonly Action<T> _execute;


        /// <summary>
        /// Initializes a new instance of the <see cref="Command{T}" /> class with the specified behaviors.
        /// </summary>
        /// <param name="execute">The function to execute.</param>
        /// <param name="canExecute">The execution constraints, if any.</param>
        public Command( Action<T> execute, Func<T, bool> canExecute = null )
        {
            if( execute == null )
            {
                throw new ArgumentNullException( nameof( execute ) );
            }

            _canExecute = canExecute;
            _execute = execute;
        }


        /// <summary>
        /// Checks whether the command can execute with the specified argument.
        /// </summary>
        /// <param name="argument">The argument.</param>
        /// <returns>A value indicating whether the command can execute with the specified argument.</returns>
        public bool CanExecute( T argument )
        {
            if( _canExecute == null )
            {
                return true;
            }

            return _canExecute( argument );
        }

        /// <summary>
        /// Executes the command with the specified argument.
        /// </summary>
        /// <param name="argument">The argument.</param>
        public void Execute( T argument )
        {
            _execute( argument );
            OnExecuted( argument );
        }


        /// <summary>
        /// Checks whether the command can execute.
        /// </summary>
        /// <param name="argument">The command argument, which must be of the correct type.</param>
        /// <returns>A value indicating whether the command can execute.</returns>
        protected override bool CanExecute( object argument )
        {
            return CanExecute( (T) argument );
        }

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="argument">The command argument, which must be of the correct type.</param>
        protected override void Execute( object argument )
        {
            Execute( (T) argument );
        }
    }

    /// <summary>
    /// Asynchronous parameterless command.
    /// </summary>
    public sealed class AsyncCommand : CommandBase
    {
        private readonly Func<bool> _canExecute;
        private readonly Func<Task> _execute;
        private bool _isRunning;


        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncCommand" /> class with the specified behaviors.
        /// </summary>
        /// <param name="execute">The asynchronous function to execute.</param>
        /// <param name="canExecute">The execution constraints, if any.</param>
        public AsyncCommand( Func<Task> execute, Func<bool> canExecute = null )
        {
            if( execute == null )
            {
                throw new ArgumentNullException( nameof( execute ) );
            }

            _canExecute = canExecute;
            _execute = execute;
        }


        /// <summary>
        /// Checks whether the command can execute.
        /// </summary>
        /// <returns>A value indicating whether the command can execute.</returns>
        public bool CanExecute()
        {
            if( _isRunning )
            {
                return false;
            }

            if( _canExecute == null )
            {
                return true;
            }

            return _canExecute();
        }

        /// <summary>
        /// Asynchronously executes the command.
        /// </summary>
        /// <returns>A task that represents the command execution.</returns>
        public async Task ExecuteAsync()
        {
            _isRunning = true;
            await _execute();
            OnExecuted( null );
            _isRunning = false;
            OnCanExecuteChanged();
        }


        /// <summary>
        /// Checks whether the command can execute.
        /// </summary>
        /// <param name="argument">The command argument, which is ignored.</param>
        /// <returns>A value indicating whether the command can execute.</returns>
        protected override bool CanExecute( object argument )
        {
            return CanExecute();
        }

        /// <summary>
        /// Executes the command but does not wait until the end of the operation.
        /// </summary>
        /// <param name="argument">The command argument, which is ignored.</param>
        protected override async void Execute( object argument )
        {
            await ExecuteAsync();
        }
    }

    /// <summary>
    /// Asynchronous parameterized command.
    /// </summary>
    /// <typeparam name="T">The parameter type.</typeparam>
    public sealed class AsyncCommand<T> : CommandBase
    {
        private readonly Func<T, bool> _canExecute;
        private readonly Func<T, Task> _execute;
        private bool _isRunning;


        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncCommand{T}" /> class with the specified behaviors.
        /// </summary>
        /// <param name="execute">The asynchronous function to execute.</param>
        /// <param name="canExecute">The execution constraints, if any.</param>
        public AsyncCommand( Func<T, Task> execute, Func<T, bool> canExecute = null )
        {
            if( execute == null )
            {
                throw new ArgumentNullException( nameof( execute ) );
            }

            _canExecute = canExecute;
            _execute = execute;
        }


        /// <summary>
        /// Checks whether the command can execute with the specified argument.
        /// </summary>
        /// <param name="argument">The argument.</param>
        /// <returns>A value indicating whether the command can execute with the specified argument.</returns>
        public bool CanExecute( T argument )
        {
            if( _isRunning )
            {
                return false;
            }

            if( _canExecute == null )
            {
                return true;
            }

            return _canExecute( argument );
        }

        /// <summary>
        /// Asynchronously executes the command with the specified argument.
        /// </summary>
        /// <param name="argument">The argument.</param>
        /// <returns>A task that represents the command execution.</returns>
        public async Task ExecuteAsync( T argument )
        {
            _isRunning = true;
            await _execute( argument );
            OnExecuted( argument );
            _isRunning = false;
            OnCanExecuteChanged();
        }


        /// <summary>
        /// Checks whether the command can execute.
        /// </summary>
        /// <param name="argument">The command argument, which must be of the correct type.</param>
        /// <returns>A value indicating whether the command can execute.</returns>
        protected override bool CanExecute( object argument )
        {
            return CanExecute( (T) argument );
        }

        /// <summary>
        /// Executes the command but does not wait until the end of the operation.
        /// </summary>
        /// <param name="argument">The command argument, which must be of the correct type.</param>
        protected override void Execute( object argument )
        {
            ExecutePrivate( (T) argument );
        }

        /// <summary>
        /// Executes the command but does not wait until the end of the operation.
        /// Implementation detail, to ensure that <see cref="Execute" /> throws synchronously
        /// if its parameter is not of the correct type.
        /// </summary>
        /// <param name="argument">The command argument.</param>
        private async void ExecutePrivate( T argument )
        {
            await ExecuteAsync( argument );
        }
    }
}