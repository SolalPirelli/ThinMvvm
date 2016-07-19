using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using ThinMvvm.Data.Infrastructure;

namespace ThinMvvm.Data
{
    /// <summary>
    /// Base class for data sources that fetch paginated chunks of items.
    /// </summary>
    /// <typeparam name="TItem">The item type.</typeparam>
    /// <typeparam name="TToken">The pagination token type.</typeparam>
    public abstract class PaginatedDataSource<TItem, TToken> : DataSource
    {
        private Optional<TToken> _paginationToken;
        private ObservableCollection<TItem> _writeableValue;
        private ReadOnlyObservableCollection<TItem> _values;
        private List<TItem> _originalValue;


        /// <summary>
        /// Gets a value indicating whether there is more data to fetch.
        /// </summary>
        public override bool CanFetchMore
        {
            get { return _paginationToken != default( Optional<TToken> ); }
        }

        /// <summary>
        /// Infrastructure.
        /// Do not use this property; use <see cref="Values" /> instead.
        /// </summary>
        [EditorBrowsable( EditorBrowsableState.Never )]
        public override object RawValue
        {
            get { return Values; }
        }

        /// <summary>
        /// Gets the last successfully retrieved values.
        /// </summary>
        public ReadOnlyObservableCollection<TItem> Values
        {
            get { return _values; }
            private set { Set( ref _values, value ); }
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="PaginatedDataSource{TItem, TToken}" /> class.
        /// </summary>
        protected PaginatedDataSource()
        {
            // Nothing. This constructor is there for its access modifier only.
        }


        /// <summary>
        /// Asynchronously refreshes the data.
        /// </summary>
        /// <returns>A task that represents the refresh operation.</returns>
        public override sealed Task RefreshAsync()
        {
            return LoadAsync(
                () =>
                {
                    Status = DataStatus.Loading;
                    _paginationToken = default( Optional<TToken> );
                },
                t => FetchAsync( _paginationToken, t ),
                ( result, cacheStatus ) =>
                {
                    CacheStatus = cacheStatus;
                    if( CacheStatus == CacheStatus.Used || LastException == null )
                    {
                        _originalValue = new List<TItem>( result.Items );
                        _paginationToken = result.Token;
                        UpdateValues( _originalValue, false );
                        Status = DataStatus.Loaded;
                    }
                    else
                    {
                        _originalValue = null;
                        Values = null;
                        Status = DataStatus.NoData;
                    }
                }
            );
        }

        /// <summary>
        /// Asynchronously fetches more data.
        /// </summary>
        /// <returns>A task that representa the fetch operation.</returns>
        public override async Task FetchMoreAsync()
        {
            if( !CanFetchMore )
            {
                return;
            }

            await LoadAsync(
                () => Status = DataStatus.LoadingMore,
                t => FetchAsync( _paginationToken, t ),
                ( result, cacheStatus ) =>
                {
                    // Don't overwrite an existing 'used' status
                    if( cacheStatus == CacheStatus.Used )
                    {
                        CacheStatus = cacheStatus;
                    }

                    if( CacheStatus == CacheStatus.Used || LastException == null )
                    {
                        _paginationToken = result.Token;
                        _originalValue.AddRange( result.Items );
                        UpdateValues( result.Items, true );
                    }

                    Status = DataStatus.Loaded;
                }
            );
        }


        /// <summary>
        /// Asynchronously fetches data, using the specified pagination and cancellation tokens.
        /// </summary>
        /// <param name="paginationToken">The pagination token, if any.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the fetch operation.</returns>
        protected abstract Task<PaginatedData<TItem, TToken>> FetchAsync( Optional<TToken> paginationToken, CancellationToken cancellationToken );

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
        protected virtual IEnumerable<TItem> Transform( IReadOnlyList<TItem> values, bool isIncremental )
        {
            return values;
        }

        /// <summary>
        /// Updates the values by re-applying <see cref="Transform" />.
        /// This method will not fetch any new data.
        /// </summary>
        protected void UpdateValues()
        {
            if( _originalValue == null )
            {
                throw new InvalidOperationException( $"{nameof( UpdateValues )} can only be called after data has been successfully loaded." );
            }

            UpdateValues( _originalValue, false );
        }

        /// <summary>
        /// Enables caching for this source.
        /// </summary>
        /// <param name="id">The source's ID.</param>
        /// <param name="dataStore">The data store for cached values.</param>
        /// <param name="metadataCreator">The metadata creator, if any.</param>
        protected void EnableCache( string id, IDataStore dataStore, Func<Optional<TToken>, CacheMetadata> metadataCreator = null )
        {
            if( metadataCreator == null )
            {
                EnableCache( id, dataStore, () => CacheMetadata.Default );
            }
            else
            {
                EnableCache( id, dataStore, () => metadataCreator( _paginationToken ) );
            }
        }


        /// <summary>
        /// Updates the value by re-applying the transform on the specified values.
        /// </summary>
        private void UpdateValues( IReadOnlyList<TItem> items, bool isIncremental )
        {
            var values = Transform( items, isIncremental );
            if( values == null )
            {
                throw new InvalidOperationException( "The transformed values cannot be null." );
            }

            if( isIncremental )
            {
                foreach( var item in values )
                {
                    _writeableValue.Add( item );
                }
            }
            else
            {
                _writeableValue = new ObservableCollection<TItem>( values );
                Values = new ReadOnlyObservableCollection<TItem>( _writeableValue );
            }
        }
    }
}