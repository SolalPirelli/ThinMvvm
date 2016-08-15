using System;
using System.Threading.Tasks;
using ThinMvvm.Data.Infrastructure;

namespace ThinMvvm.Data
{
    /// <summary>
    /// Base class for input forms.
    /// </summary>
    /// <typeparam name="T">The input type.</typeparam>
    public abstract class Form<T> : ObservableObject
    {
        // Locks methods, since neither initialization nor submission can be called concurrently.
        private readonly object _lock;

        // Current input (which is most likely a mutable class).
        private T _input;
        // Current status.
        private FormStatus _status;
        // Current error (if any).
        private Exception _error;


        /// <summary>
        /// Gets the form's input.
        /// </summary>
        public T Input
        {
            get { return _input; }
            private set { Set( ref _input, value ); }
        }

        /// <summary>
        /// Gets the form's status.
        /// </summary>
        public FormStatus Status
        {
            get { return _status; }
            private set { Set( ref _status, value ); }
        }

        /// <summary>
        /// Gets the error that occurred during the last operation, if any.
        /// </summary>
        public Exception Error
        {
            get { return _error; }
            private set { Set( ref _error, value ); }
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="Form{T}" /> class.
        /// </summary>
        public Form()
        {
            _lock = new object();
        }


        /// <summary>
        /// Asynchronously initializes the form.
        /// </summary>
        /// <returns>A task that represents the initialization operation.</returns>
        public async Task InitializeAsync()
        {
            lock( _lock )
            {
                if( Status != FormStatus.None )
                {
                    throw new InvalidOperationException( "Cannot initialize a form more than once." );
                }

                Status = FormStatus.Initializing;
            }

            var result = await DataOperations.FetchAsync( LoadInitialInputAsync );

            if( result.Status == DataStatus.Normal )
            {
                Input = result.Value;
                Error = null;
                Status = FormStatus.Initialized;
            }
            else
            {
                Error = result.Errors.Fetch;
                Status = FormStatus.None;
            }
        }

        /// <summary>
        /// Asynchronously submits the form.
        /// </summary>
        /// <returns>A task that represents the submit operation.</returns>
        public async Task SubmitAsync()
        {
            lock( _lock )
            {
                if( Status == FormStatus.None || Status == FormStatus.Initializing || Status == FormStatus.Submitting )
                {
                    throw new InvalidOperationException( "Cannot submit a form before it was initialized." );
                }

                Status = FormStatus.Submitting;
            }

            Error = await DataOperations.DoAsync( () => SubmitAsync( Input ) );
            Status = FormStatus.Submitted;
        }


        /// <summary>
        /// Asynchronously loads the initial input value.
        /// </summary>
        /// <returns>A task that represents the load operation.</returns>
        protected abstract Task<T> LoadInitialInputAsync();

        /// <summary>
        /// Asynchronously submits the specified input.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>A task that represents the submit operation.</returns>
        protected abstract Task SubmitAsync( T input );
    }
}