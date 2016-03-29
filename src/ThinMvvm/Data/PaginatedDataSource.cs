using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        private ReadOnlyObservableCollection<TItem> _value;
        private List<TItem> _originalValue;


        /// <summary>
        /// Gets the last successfully retrieved value.
        /// </summary>
        public ReadOnlyObservableCollection<TItem> Value
        {
            get { return _value; }
            private set { Set( ref _value, value ); }
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
                () => _paginationToken = default( Optional<TToken> ),
                t => FetchAsync( _paginationToken, t ),
                ( result, cacheStatus ) =>
                {
                    CacheStatus = cacheStatus;
                    if( CacheStatus == CacheStatus.Used || LastException == null )
                    {
                        _originalValue = new List<TItem>( result.Items );
                        _paginationToken = result.Token;
                        UpdateValue( _originalValue, false );
                        Status = DataStatus.Loaded;
                    }
                    else
                    {
                        _originalValue = null;
                        Value = null;
                        Status = DataStatus.NoData;
                    }
                }
            );
        }

        /// <summary>
        /// Asynchronously attempts to fetch more data.
        /// </summary>
        /// <returns>
        /// A task that representa the fetch operation. 
        /// The value will be <c>true</c> if a fetch operation was performed (even if it failed),
        /// and <c>false</c> if the source had no more data available.
        /// </returns>
        public override async Task<bool> TryFetchMoreAsync()
        {
            if( !_paginationToken.HasValue )
            {
                return false;
            }

            await LoadAsync(
                () => { }, // No initialization needed
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
                        UpdateValue( result.Items, true );
                    }

                    Status = DataStatus.Loaded;
                }
            );

            return true;
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
        /// Updates the value by re-applying <see cref="Transform" />.
        /// This method will not fetch any new data.
        /// </summary>
        protected void UpdateValue()
        {
            if( _originalValue == null )
            {
                throw new InvalidOperationException( $"{nameof( UpdateValue )} can only be called after data has been successfully loaded." );
            }

            UpdateValue( _originalValue, false );
        }

        /// <summary>
        /// Enables caching for this source.
        /// </summary>
        /// <param name="dataStore">The data store for cached values.</param>
        /// <param name="metadataCreator">The metadata creator, if any.</param>
        protected void EnableCache( IDataStore dataStore, Func<Optional<TToken>, CacheMetadata> metadataCreator = null )
        {
            if( metadataCreator == null )
            {
                EnableCache( dataStore, () => CacheMetadata.Default );
            }
            else
            {
                EnableCache( dataStore, () => metadataCreator( _paginationToken ) );
            }
        }


        /// <summary>
        /// Updates the value by re-applying the transform on the specified values.
        /// </summary>
        private void UpdateValue( IReadOnlyList<TItem> items, bool isIncremental )
        {
            var value = Transform( items, isIncremental );

            if( isIncremental )
            {
                foreach( var item in value )
                {
                    _writeableValue.Add( item );
                }
            }
            else
            {
                _writeableValue = new ObservableCollection<TItem>( value );
                Value = new ReadOnlyObservableCollection<TItem>( _writeableValue );
            }
        }
    }
}