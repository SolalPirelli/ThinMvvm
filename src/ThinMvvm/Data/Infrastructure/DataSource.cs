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
        private bool _canFetchMore;
        private object _value;
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
        /// Gets a value indicating whether there is more data to fetch.
        /// </summary>
        public abstract bool CanFetchMore { get; }

        /// <summary>
        /// Infrastructure.
        /// Gets the last successfully retrieved value.
        /// </summary>
        /// <remarks>
        /// This property exists so that code can be written to target both kinds 
        /// of data sources, i.e. paginated and non-paginated.
        /// However, for compatibility with Windows Universal Apps, whose bindings
        /// do not support properties which hide a base property (C#'s "new"),
        /// it is not named "Value".
        /// To ensure nobody accidentally uses this property, subclasses hide it
        /// with a property marked as invisible to editors.
        /// </remarks>
        public abstract object RawValue { get; }

        /// <summary>
        /// Gets the exception thrown by the last operation, if the operation threw.
        /// </summary>
        public Exception LastException
        {
            get { return _lastException; }
            private set { Set( ref _lastException, value ); }
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
        }

        /// <summary>
        /// Asynchronously refreshes the data.
        /// </summary>
        /// <returns>A task that represents the refresh operation.</returns>
        public abstract Task RefreshAsync();

        /// <summary>
        /// Asynchronously fetches more data.
        /// </summary>
        /// <returns>A task that represents the fetch operation.</returns>
        public abstract Task FetchMoreAsync();


        /// <summary>
        /// Enables caching for this source.
        /// </summary>
        /// <param name="id">The source's ID.</param>
        /// <param name="dataStore">The data store for cached values.</param>
        /// <param name="metadataCreator">The metadata creator.</param>
        internal void EnableCache( string id, IDataStore dataStore, Func<CacheMetadata> metadataCreator )
        {
            if( id == null )
            {
                throw new ArgumentNullException( nameof( id ) );
            }
            if( dataStore == null )
            {
                throw new ArgumentNullException( nameof( dataStore ) );
            }
            if( _cache != null )
            {
                throw new InvalidOperationException( $"Cannot call {nameof( EnableCache )} more than once." );
            }

            _cache = new Cache( id, dataStore );
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
            // Refresh operations need to be fully atomic, i.e. at the end of a fetch operation
            // the status, exception and value properties must all be related.
            // Use the semaphore both as a semaphore and as a lock object, because why not?

            CancellationToken token;
            lock( _semaphore )
            {
                if( _cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested )
                {
                    _cancellationTokenSource.Cancel();
                }

                _cancellationTokenSource = new CancellationTokenSource();
                token = _cancellationTokenSource.Token;
            }

            await _semaphore.WaitAsync();
            try
            {
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
                            var cachedResult = default( Optional<TData> );

                            try
                            {
                                var metadata = _metadataCreator();
                                if( metadata != null )
                                {
                                    cachedResult = await _cache.GetAsync<TData>( metadata.Id );
                                }
                            }
                            catch( Exception e2 )
                            {
                                LastException = e2;
                                resultHandler( default( TData ), CacheStatus.Unused );
                                return;
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
                    try
                    {
                        var metadata = _metadataCreator();
                        if( metadata != null )
                        {
                            await _cache.StoreAsync( metadata.Id, result, metadata.ExpirationDate );
                        }
                    }
                    catch( Exception e )
                    {
                        LastException = e;
                        resultHandler( default( TData ), CacheStatus.Unused );
                        return;
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