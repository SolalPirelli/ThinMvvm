using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using ThinMvvm.Data.Infrastructure;

namespace ThinMvvm.Data
{
    /// <summary>
    /// Base class for data sources that fetch paginated items.
    /// </summary>
    /// <typeparam name="TData">The data type.</typeparam>
    /// <typeparam name="TToken">The pagination token type.</typeparam>
    public abstract class PaginatedDataSource<TData, TToken> : ObservableObject, IDataSource
    {
        // Locks changes to all properties, to ensure atomicity.
        private readonly object _lock;
        // Creates tokens for fetch operations.
        private readonly CancellationTokenCreator _cancellationTokens;

        // Cache and its associated metadata creator. May be null, but will be changed only once.
        private Cache _cache;
        private Func<Optional<TToken>, CacheMetadata> _cacheMetadataCreator;

        // Values before the transformation is applied.
        private List<DataChunk<TData>> _originalValues;
        // Writable version of the values after transformation.
        private ObservableCollection<DataChunk<TData>> _writeableValues;
        // Read-only values, publicly visible.
        private ReadOnlyObservableCollection<DataChunk<TData>> _values;

        // Current pagination token.
        private Optional<TToken> _paginationToken;

        // Current status.
        private DataSourceStatus _status;


        /// <summary>
        /// Gets the values loaded by the source, if any.
        /// </summary>
        public ReadOnlyObservableCollection<DataChunk<TData>> Data
        {
            get { return _values; }
            private set { Set( ref _values, value ); }
        }

        /// <summary>
        /// Gets the source's status.
        /// </summary>
        public DataSourceStatus Status
        {
            get { return _status; }
            private set { Set( ref _status, value ); }
        }

        /// <summary>
        /// Gets a value indicating whether there is more data to be fetched.
        /// If this is <c>true</c>, calling <see cref="FetchMoreAsync" /> will add a chunk to <see cref="Data" />.
        /// </summary>
        public bool CanFetchMore
        {
            get { return _paginationToken != default( Optional<TToken> ); }
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="PaginatedDataSource{TItem, TToken}" /> class.
        /// </summary>
        protected PaginatedDataSource()
        {
            _lock = new object();
            _cancellationTokens = new CancellationTokenCreator();
        }


        /// <summary>
        /// Asynchronously refreshes the data.
        /// </summary>
        /// <returns>A task that represents the refresh operation.</returns>
        public Task RefreshAsync()
        {
            return FetchAsync( false );
        }

        /// <summary>
        /// Asynchronously fetches more data.
        /// </summary>
        /// <returns>A task that representa the fetch operation.</returns>
        public Task FetchMoreAsync()
        {
            if( !CanFetchMore )
            {
                throw new InvalidOperationException( $"Cannot call {nameof( FetchMoreAsync )} when {nameof( CanFetchMore )} is false." );
            }

            return FetchAsync( true );
        }


        /// <summary>
        /// Asynchronously fetches data, using the specified pagination and cancellation tokens.
        /// </summary>
        /// <param name="paginationToken">The pagination token, if any.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the fetch operation.</returns>
        protected abstract Task<PaginatedData<TData, TToken>> FetchAsync( Optional<TToken> paginationToken, CancellationToken cancellationToken );

        /// <summary>
        /// Transforms the specified values into new ones if necessary.
        /// </summary>
        /// <param name="values">The values.</param>
        /// <param name="isIncremental">
        /// Whether the transformation is incremental.
        /// If <c>true</c>, the return value of this method will be added to the source's current value.
        /// Otherwise, the return value of this method will replace the source's current value.
        /// </param>
        /// <returns>Either the existing values if no transformation was needed, or new values.</returns>
        protected virtual TData Transform( TData values, bool isIncremental )
        {
            return values;
        }

        /// <summary>
        /// Updates the values by re-applying <see cref="Transform" />.
        /// This method will not fetch any new data.
        /// </summary>
        protected void UpdateValues()
        {
            if( _originalValues == null )
            {
                throw new InvalidOperationException( $"{nameof( UpdateValues )} can only be called after data has been successfully loaded." );
            }

            lock( _lock )
            {
                // TODO: Use e.g. versioning to avoid having the transform in the lock.
                var transformed = new List<DataChunk<TData>>();
                for( int n = 0; n < _originalValues.Count; n++ )
                {
                    transformed.Add( DataLoader.Transform( _originalValues[n], items => Transform( items, n == 0 ) ) );
                }

                _writeableValues = new ObservableCollection<DataChunk<TData>>( transformed );
                Data = new ReadOnlyObservableCollection<DataChunk<TData>>( _writeableValues );

                OnPropertyChanged( string.Empty );
            }
        }

        /// <summary>
        /// Enables caching for this source.
        /// </summary>
        /// <param name="id">The source's ID.</param>
        /// <param name="dataStore">The data store for cached values.</param>
        /// <param name="metadataCreator">The metadata creator, if any.</param>
        protected void EnableCache( string id, IDataStore dataStore, Func<Optional<TToken>, CacheMetadata> metadataCreator = null )
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
                throw new InvalidOperationException( "Caching has already been enabled." );
            }

            // The rest of the code assumes that if _cache is set then _cacheMetadataCreator also is.
            _cacheMetadataCreator = metadataCreator ?? ( _ => CacheMetadata.Default );
            _cache = new Cache( id, dataStore );
        }


        /// <summary>
        /// Asynchronously fetches data, incrementally or not.
        /// </summary>
        private async Task FetchAsync( bool isIncremental )
        {
            Status = isIncremental ? DataSourceStatus.LoadingMore : DataSourceStatus.Loading;

            var paginationToken = isIncremental ? _paginationToken : default( Optional<TToken> );
            var cancellationToken = _cancellationTokens.CreateAndCancelPrevious();

            var chunk = await DataLoader.LoadAsync( () => FetchAsync( paginationToken, cancellationToken ) );

            if( _cache != null )
            {
                chunk = await DataLoader.CacheAsync( chunk, _cache, () => _cacheMetadataCreator( paginationToken ) );
            }

            var itemsChunk = new DataChunk<TData>( chunk.Value == null ? default( TData ) : chunk.Value.Value, chunk.Status, chunk.Errors );
            var transformedChunk = DataLoader.Transform( itemsChunk, items => Transform( items, isIncremental ) );

            lock( _lock )
            {
                if( !cancellationToken.IsCancellationRequested )
                {
                    _paginationToken = chunk.Value?.Token ?? default( Optional<TToken> );
                    OnPropertyChanged( nameof( CanFetchMore ) );

                    if( isIncremental )
                    {
                        _originalValues.Add( itemsChunk );
                        _writeableValues.Add( transformedChunk );
                    }
                    else
                    {
                        _originalValues = new List<DataChunk<TData>> { itemsChunk };
                        _writeableValues = new ObservableCollection<DataChunk<TData>> { transformedChunk };
                        Data = new ReadOnlyObservableCollection<DataChunk<TData>>( _writeableValues );
                    }

                    Status = DataSourceStatus.Loaded;
                }
            }
        }


        /// <summary>
        /// Infrastructure.
        /// Gets the data loaded by the source.
        /// </summary>
        IReadOnlyList<IDataChunk> IDataSource.Data => Data;
    }
}