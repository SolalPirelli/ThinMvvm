using System;
using System.Threading;
using System.Threading.Tasks;
using ThinMvvm.Data.Infrastructure;

namespace ThinMvvm.Data
{
    /// <summary>
    /// Base class for data sources that fetch a single value.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    public abstract class DataSource<T> : DataSource
    {
        private T _value;
        private Optional<T> _originalValue;
        

        /// <summary>
        /// Gets the last successfully retrieved value.
        /// </summary>
        public T Value
        {
            get { return _value; }
            private set { Set( ref _value, value ); }
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="DataSource{T}" /> class.
        /// </summary>
        protected DataSource()
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
                () => { }, // No initialization needed
                FetchAsync,
                ( result, cacheStatus ) =>
                {
                    CacheStatus = cacheStatus;
                    if( CacheStatus == CacheStatus.Used || LastException == null )
                    {
                        _originalValue = new Optional<T>( result );
                        UpdateValue();
                        Status = DataStatus.Loaded;
                    }
                    else
                    {
                        _originalValue = default( Optional<T> );
                        Value = default( T );
                        Status = DataStatus.NoData;
                    }
                } 
            );
        }


        /// <summary>
        /// Asynchronously fetches data, using the specified cancellation token.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the fetch operation.</returns>
        protected abstract Task<T> FetchAsync( CancellationToken cancellationToken );

        /// <summary>
        /// Transforms the specified value into a new one if necessary.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>Either the existing value if no transformation was needed, or a new value.</returns>
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
            if( !_originalValue.HasValue )
            {
                throw new InvalidOperationException( $"{nameof( UpdateValue )} can only be called after data has been successfully loaded." );
            }

            Value = Transform( _originalValue.Value );
        }

        /// <summary>
        /// Enables caching for this source.
        /// </summary>
        /// <param name="dataStore">The data store for cached values.</param>
        /// <param name="metadataCreator">The metadata creator, if any.</param>
        protected new void EnableCache( IDataStore dataStore, Func<CacheMetadata> metadataCreator = null )
        {
            base.EnableCache( dataStore, metadataCreator ?? ( () => CacheMetadata.Default ) );
        }
    }
}