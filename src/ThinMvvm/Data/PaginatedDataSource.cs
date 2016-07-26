using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using ThinMvvm.Data.Infrastructure;

namespace ThinMvvm.Data
{
    /// <summary>
    /// Base class for data sources that fetch paginated values.
    /// </summary>
    /// <typeparam name="TValue">The value type.</typeparam>
    /// <typeparam name="TToken">The pagination token type.</typeparam>
    public abstract class PaginatedDataSource<TValue, TToken> : ObservableObject, IDataSource
    {
        // Locks changes to all properties, to ensure atomicity.
        private readonly object _lock;
        // Creates tokens for fetch operations.
        private readonly CancellationTokenHolder _cancellationTokens;
        // Data version, used to avoid overwriting newer data in UpdateValues
        private uint _version;

        // Cache and its associated metadata creator. May be null, but will be changed only once.
        private Cache _cache;
        private Func<Optional<TToken>, CacheMetadata> _cacheMetadataCreator;

        // Values before the transformation is applied.
        private List<DataChunk<TValue>> _originalValues;
        // Writable version of the values after transformation.
        private ObservableCollection<DataChunk<TValue>> _writeableValues;
        // Read-only values, publicly visible.
        private ReadOnlyObservableCollection<DataChunk<TValue>> _values;

        // Current pagination token.
        private Optional<TToken> _paginationToken;

        // Current status.
        private DataSourceStatus _status;


        /// <summary>
        /// Gets the data loaded by the source, if any.
        /// </summary>
        public ReadOnlyObservableCollection<DataChunk<TValue>> Data
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
        /// Initializes a new instance of the <see cref="PaginatedDataSource{TValue, TToken}" /> class.
        /// </summary>
        protected PaginatedDataSource()
        {
            _lock = new object();
            _cancellationTokens = new CancellationTokenHolder();
            _version = 0;
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
        protected abstract Task<PaginatedData<TValue, TToken>> FetchAsync( Optional<TToken> paginationToken, CancellationToken cancellationToken );

        /// <summary>
        /// Asynchronously transforms the specified value, if necessary.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="isIncremental">
        /// Whether the transformation is incremental.
        /// If <c>true</c>, the return value of this method will be added to the source's current value.
        /// Otherwise, the return value of this method will replace the source's current value.
        /// </param>
        /// <returns>A task that represents the transform operation..</returns>
        protected virtual Task<TValue> TransformAsync( TValue value, bool isIncremental )
        {
            return Task.FromResult( value );
        }

        /// <summary>
        /// Asynchronously updates the values by re-applying <see cref="Transform" />.
        /// This method will not fetch any new data.
        /// </summary>
        /// <returns>A task that represents the update operation.</returns>
        protected async Task UpdateValuesAsync()
        {
            if( _originalValues == null )
            {
                throw new InvalidOperationException( $"{nameof( UpdateValuesAsync )} can only be called after data has been successfully loaded." );
            }

            var version = _version;

            var transformed = new List<DataChunk<TValue>>();
            for( int n = 0; n < _originalValues.Count; n++ )
            {
                transformed.Add( await DataOperations.TransformAsync( _originalValues[n], items => TransformAsync( items, n == 0 ) ) );
            }


            lock( _lock )
            {
                if( version == _version )
                {
                    _writeableValues = new ObservableCollection<DataChunk<TValue>>( transformed );
                    Data = new ReadOnlyObservableCollection<DataChunk<TValue>>( _writeableValues );

                    OnPropertyChanged( string.Empty );
                }
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

            var chunk = await DataOperations.FetchAsync( () => FetchAsync( paginationToken, cancellationToken ) );

            if( _cache != null )
            {
                chunk = await DataOperations.CacheAsync( chunk, _cache, () => _cacheMetadataCreator( paginationToken ) );
            }

            var itemsChunk = new DataChunk<TValue>( chunk.Value == null ? default( TValue ) : chunk.Value.Value, chunk.Status, chunk.Errors );
            var transformedChunk = await DataOperations.TransformAsync( itemsChunk, items => TransformAsync( items, isIncremental ) );

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
                        _originalValues = new List<DataChunk<TValue>> { itemsChunk };
                        _writeableValues = new ObservableCollection<DataChunk<TValue>> { transformedChunk };
                        Data = new ReadOnlyObservableCollection<DataChunk<TValue>>( _writeableValues );
                    }

                    Status = DataSourceStatus.Loaded;
                    _version++;
                }
            }
        }


        // Explicitly implemented to provide a typed value instead.
        IReadOnlyList<IDataChunk> IDataSource.Data => Data;
    }
}