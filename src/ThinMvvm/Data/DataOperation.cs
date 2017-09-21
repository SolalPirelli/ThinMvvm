using System;
using System.Threading.Tasks;

namespace ThinMvvm.Data
{
    /// <summary>
    /// Base class for operations that operate on data, such as a form that sends data to a server.
    /// </summary>
    public abstract class DataOperation : ObservableObject
    {
        // Locks the execute methods to ensure there are no concurrent modification
        private readonly object _lock;
        // Whether the operation is currently executing
        private bool _isLoading;
        // Error thrown by the last execution
        private Exception _error;


        /// <summary>
        /// Gets a value indicating whether the operation is currently executing.
        /// </summary>
        public bool IsLoading
        {
            get { return _isLoading; }
            private set { Set( ref _isLoading, value ); }
        }

        /// <summary>
        /// Gets the error thrown by the last run of the execution, if any.
        /// </summary>
        public Exception Error
        {
            get { return _error; }
            private set { Set( ref _error, value ); }
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="DataOperation" /> class.
        /// </summary>
        public DataOperation()
        {
            _lock = new object();
        }


        /// <summary>
        /// Asynchronously executes the specified action.
        /// </summary>
        /// <param name="action">The asynchronous action to execute.</param>
        /// <returns>A task that represents the execute action.</returns>
        protected async Task DoAsync( Func<Task> action )
        {
            lock( _lock )
            {
                if( IsLoading )
                {
                    throw new InvalidOperationException( $"{nameof( DoAsync )} cannot be called concurrently." );
                }

                IsLoading = true;
            }

            try
            {
                await action();

                Error = null;
            }
            catch( Exception e )
            {
                Error = e;
            }

            IsLoading = false;
        }
    }
}