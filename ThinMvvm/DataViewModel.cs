// Copyright (c) Solal Pirelli 2014
// See License.txt file for more details

using System;
using System.Threading;
using System.Threading.Tasks;
using ThinMvvm.Logging;

namespace ThinMvvm
{
    /// <summary>
    /// ViewModel that loads data.
    /// </summary>
    public abstract class DataViewModel<TArg> : ViewModel<TArg>, IDisposable
    {
        // Lock to ensure cancellation doesn't cause race conditions
        private readonly object _lock = new object();
        // Cancellation source to avoid the problem of older tasks finishing after younger ones, and replacing data
        private CancellationTokenSource _cancellationSource;
        // Flag to force refresh on the first load
        private bool _firstRun;

        private DataStatus _dataStatus;

        /// <summary>
        /// Gets the cancellation token currently used by the ViewModel.
        /// </summary>
        protected CancellationToken CurrentCancellationToken
        {
            get { return _cancellationSource.Token; }
        }

        /// <summary>
        /// Gets the data status of the ViewModel.
        /// </summary>
        public DataStatus DataStatus
        {
            get { return _dataStatus; }
            private set { SetProperty( ref _dataStatus, value ); }
        }

        /// <summary>
        /// Command executed to update all data.
        /// </summary>
        [LogId( "Refresh" )]
        public AsyncCommand RefreshCommand
        {
            get { return GetAsyncCommand( () => TryRefreshAsync( true ), () => DataStatus != DataStatus.Loading ); }
        }


        /// <summary>
        /// Creates a new DataViewModel.
        /// </summary>
        protected DataViewModel()
        {
            _cancellationSource = new CancellationTokenSource();
            _firstRun = true;
        }


        /// <summary>
        /// Occurs after the user navigated to the ViewModel.
        /// </summary>
        public virtual async Task OnNavigatedToAsync()
        {
            await TryRefreshAsync( _firstRun );
            _firstRun = false;
        }

        /// <summary>
        /// Refreshes the data.
        /// </summary>
        /// <param name="force">Whether to force the data refresh.</param>
        /// <param name="token">The token used to cancel the refresh.</param>
        protected virtual Task RefreshAsync( bool force, CancellationToken token )
        {
            return Task.FromResult( 0 );
        }

        /// <summary>
        /// Attempts to refresh the data.
        /// </summary>
        /// <param name="force">Whether to force the data refresh.</param>
        protected Task TryRefreshAsync( bool force )
        {
            return TryExecuteAsync( tok => RefreshAsync( force, tok ) );
        }

        /// <summary>
        /// Attempts to execute the specified asynchronous action.
        /// </summary>
        protected async Task TryExecuteAsync( Func<CancellationToken, Task> action )
        {
            lock ( _lock )
            {
                if ( !_cancellationSource.IsCancellationRequested )
                {
                    _cancellationSource.Cancel();
                }
                _cancellationSource = new CancellationTokenSource();
            }

            DataStatus = DataStatus.Loading;

            var token = _cancellationSource.Token;

            try
            {
                await action( token );
            }
            catch ( Exception e )
            {
                if ( !token.IsCancellationRequested )
                {
                    if ( DataViewModelOptions.IsNetworkException( e ) )
                    {
                        DataStatus = DataStatus.NetworkError;
                    }
                    else
                    {
                        DataStatus = DataStatus.Error;
                    }
                }
            }

            if ( DataStatus == DataStatus.Loading && !token.IsCancellationRequested )
            {
                DataStatus = DataStatus.DataLoaded;
            }
        }

        #region ViewModel overrides
        /// <summary>
        /// Occurs when the user navigates to the ViewModel.
        /// Do not call this method from a derived class.
        /// </summary>
        public async override void OnNavigatedTo()
        {
            await OnNavigatedToAsync();
        }
        #endregion

        #region IDisposable implementation
        /// <summary>
        /// Destroys the DataViewModel.
        /// </summary>
        ~DataViewModel()
        {
            Dispose( false );
        }

        /// <summary>
        /// Disposes of the DataViewModel.
        /// </summary>
        public void Dispose()
        {
            Dispose( true );
            GC.SuppressFinalize( this );
        }

        /// <summary>
        /// Disposes of the DataViewModel.
        /// </summary>
        protected virtual void Dispose( bool onlyManaged )
        {
            _cancellationSource.Cancel();
            _cancellationSource.Dispose();
        }
        #endregion
    }
}