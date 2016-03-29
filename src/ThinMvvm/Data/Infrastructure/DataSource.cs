using System;
using System.Threading;
using System.Threading.Tasks;

namespace ThinMvvm.Data.Infrastructure
{
    /// <summary>
    /// Infrastructure.
    /// Base class for data sources.
    /// </summary>
    public abstract class DataSource : ObservableObject
    {
        private readonly SemaphoreSlim _semaphore;
        private CancellationTokenSource _cancellationTokenSource;

        private Cache _cache;
        private Func<CacheMetadata> _metadataCreator;
        private CacheStatus _cacheStatus;

        private DataStatus _status;
        private Exception _lastException;


        /// <summary>
        /// Gets the source's status.
        /// 
        /// Monitoring general changes should be done through this property,
        /// which is guaranteed to change after any other properties have changed
        /// for a given operation (successful or not).
        /// </summary>
        public DataStatus Status
        {
            get { return _status; }
            internal set { Set( ref _status, value ); }
        }

        /// <summary>
        /// Gets the exception thrown by the last operation, if said operation threw.
        /// </summary>
        public Exception LastException
        {
            get { return _lastException; }
            internal set { Set( ref _lastException, value ); }
        }

        /// <summary>
        /// Gets the cache status of this source.
        /// Caching is disabled by default.
        /// </summary>
        public CacheStatus CacheStatus
        {
            get { return _cacheStatus; }
            internal set { Set( ref _cacheStatus, value ); }
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="DataSource" /> class.
        /// </summary>
        internal DataSource()
        {
            _semaphore = new SemaphoreSlim( 1 );
            _cancellationTokenSource = new CancellationTokenSource();
        }

        /// <summary>
        /// Asynchronously refreshes the data.
        /// </summary>
        /// <returns>A task that represents the refresh operation.</returns>
        public abstract Task RefreshAsync();

        /// <summary>
        /// Asynchronously attempts to fetch more data.
        /// </summary>
        /// <returns>
        /// A task that representa the fetch operation. 
        /// The value will be <c>true</c> if a fetch operation was performed (even if it failed),
        /// and <c>false</c> if the source had no more data available.
        /// </returns>
        public virtual Task<bool> TryFetchMoreAsync()
        {
            return Task.FromResult( false );
        }


        /// <summary>
        /// Enables caching for this source.
        /// </summary>
        /// <param name="dataStore">The data store for cached values.</param>
        /// <param name="metadataCreator">The metadata creator.</param>
        internal void EnableCache( IDataStore dataStore, Func<CacheMetadata> metadataCreator )
        {
            if( dataStore == null )
            {
                throw new ArgumentNullException( nameof( dataStore ) );
            }
            if( _cache != null )
            {
                throw new InvalidOperationException( $"Cannot call {nameof( EnableCache )} more than once." );
            }

            _cache = new Cache( this, dataStore );
            _metadataCreator = metadataCreator;
        }


        /// <summary>
        /// Asynchronously loads data using the specified initialization, fetch operation, and callback.
        /// </summary>
        /// <typeparam name="TData">The data type.</typeparam>
        /// <param name="initialization">The initialization to perform before fetching.</param>
        /// <param name="fetcher">The fetch operation to execute.</param>
        /// <param name="resultHandler">The callback containing the result.</param>
        /// <returns>A task that represents the loading operation.</returns>
        // The initialization action is a hack to allow resetting the pagination token
        // so that the metadata creation can be done correctly in PaginatedDataSource...
        // TODO: Find a better way to do it.
        // Also, the callback being passed the cache status is so that the paginated source
        // can choose whether to update the cache status depending on whether this is a "fetch more" call or not.
        internal async Task LoadAsync<TData>( Action initialization, 
                                              Func<CancellationToken, Task<TData>> fetcher, 
                                              Action<TData, CacheStatus> resultHandler )
        {
            // Refresh operations need to be fully atomic, i.e. nobody should be able to observe a data source
            // with status, exception and value properties that do not all match the same fetch operation.
            // A simple lock can be used to cancel the previous operation (cancelling is a synchronous),
            // but a semaphore is needed to make the fetch itself atomic, as locks cannot be used in asynchronous contexts.

            CancellationToken token;
            lock ( _cancellationTokenSource )
            {
                if( !_cancellationTokenSource.IsCancellationRequested )
                {
                    _cancellationTokenSource.Cancel();
                    _cancellationTokenSource = new CancellationTokenSource();
                }
                token = _cancellationTokenSource.Token;
            }

            await _semaphore.WaitAsync();
            try
            {
                Status = DataStatus.Loading;

                initialization();

                TData result;
                try
                {
                    result = await fetcher( token );
                }
                catch( Exception e )
                {
                    if( !token.IsCancellationRequested )
                    {
                        LastException = e;

                        if( _cache == null )
                        {
                            resultHandler( default( TData ), CacheStatus.Unused );
                        }
                        else
                        {
                            var metadata = _metadataCreator();
                            var cachedResult = default( Optional<TData> );
                            if( metadata != null )
                            {
                                cachedResult = await _cache.GetAsync<TData>( metadata.Id );
                            }

                            if( cachedResult.HasValue )
                            {
                                resultHandler( cachedResult.Value, CacheStatus.Used );
                            }
                            else
                            {
                                resultHandler( default( TData ), CacheStatus.Unused );
                            }
                        }
                    }
                    return;
                }

                if( _cache != null )
                {
                    var metadata = _metadataCreator();
                    if( metadata != null )
                    {
                        await _cache.StoreAsync( result, metadata.Id, metadata.ExpirationDate );
                    }
                }

                if( !token.IsCancellationRequested )
                {
                    LastException = null;
                    resultHandler( result, CacheStatus.Unused );
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}