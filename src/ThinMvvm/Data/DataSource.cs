using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using ThinMvvm.Data.Infrastructure;

namespace ThinMvvm.Data
{
    /// <summary>
    /// Base class for data sources that fetch a single value.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    public abstract class DataSource<T> : ObservableObject, IDataSource
    {
        // Locks changes to all properties, to ensure atomicity.
        private readonly object _lock;
        // Creates tokens for refresh operations.
        private readonly CancellationTokenHolder _cancellationTokens;
        // Data version, used to avoid overwriting newer data in UpdateValues
        private uint _version;

        // Cache and its associated metadata creator. May be null, but will be changed only once.
        private Cache _cache;
        private Func<CacheMetadata> _cacheMetadataCreator;

        // Data before the transformation is applied.
        private DataChunk<T> _originalData;
        // Data after the transformation, publicly visible.
        private DataChunk<T> _data;
        // Data exposed to satisfy the IDataSource contract.
        private ReadOnlyCollection<IDataChunk> _rawData;

        // Current status.
        private DataSourceStatus _status;


        /// <summary>
        /// Gets the data loaded by the source, if any.
        /// </summary>
        public DataChunk<T> Data
        {
            get { return _data; }
            private set
            {
                _rawData = new ReadOnlyCollection<IDataChunk>( new[] { value } );

                Set( ref _data, value );
            }
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
        /// Initializes a new instance of the <see cref="DataSource{T}" /> class.
        /// </summary>
        protected DataSource()
        {
            _lock = new object();
            _cancellationTokens = new CancellationTokenHolder();
            _version = 0;
        }


        /// <summary>
        /// Asynchronously refreshes the data.
        /// </summary>
        /// <returns>A task that represents the refresh operation.</returns>
        public async Task RefreshAsync()
        {
            Status = DataSourceStatus.Loading;

            var token = _cancellationTokens.CreateAndCancelPrevious();

            var value = await DataOperations.FetchAsync( () => FetchAsync( token ) );

            if( _cache != null )
            {
                value = await DataOperations.CacheAsync( value, _cache, _cacheMetadataCreator );
            }

            var transformedValue = DataOperations.Transform( value, Transform );

            lock( _lock )
            {
                if( !token.IsCancellationRequested )
                {
                    _originalData = value;
                    Data = transformedValue;
                    Status = DataSourceStatus.Loaded;
                    _version++;
                }
            }
        }


        /// <summary>
        /// Asynchronously fetches data, using the specified cancellation token.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the fetch operation.</returns>
        protected abstract Task<T> FetchAsync( CancellationToken cancellationToken );

        /// <summary>
        /// Transforms the specified value, if necessary.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The transformed value, which can be the existing value if no transformation was necessary.</returns>
        protected virtual T Transform( T value )
        {
            return value;
        }

        /// <summary>
        /// Updates the value by re-applying <see cref="Transform" />.
        /// This method will not fetch any new data.
        /// </summary>
        protected void UpdateValue()
        {
            if( _originalData == null )
            {
                throw new InvalidOperationException( $"{nameof( UpdateValue )} can only be called after data has been successfully loaded." );
            }

            var version = _version;

            var transformedData = DataOperations.Transform( _originalData, Transform );

            lock( _lock )
            {
                if( version == _version )
                {
                    Data = transformedData;
                    OnPropertyChanged( nameof( Status ) );
                }
            }
        }

        /// <summary>
        /// Enables caching for the source.
        /// </summary>
        /// <param name="id">The source's ID.</param>
        /// <param name="dataStore">The data store for cached values.</param>
        /// <param name="metadataCreator">The metadata creator, if any.</param>
        protected void EnableCache( string id, IDataStore dataStore, Func<CacheMetadata> metadataCreator = null )
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
            _cacheMetadataCreator = metadataCreator ?? ( () => CacheMetadata.Default );
            _cache = new Cache( id, dataStore );
        }


        // Explicitly implemented to provide a typed value instead.
        IReadOnlyList<IDataChunk> IDataSource.Data => _rawData;

        // This class does not support pagination.
        bool IDataSource.CanFetchMore => false;

        // This class does not support pagination.
        Task IDataSource.FetchMoreAsync()
        {
            throw new NotSupportedException();
        }
    }
}