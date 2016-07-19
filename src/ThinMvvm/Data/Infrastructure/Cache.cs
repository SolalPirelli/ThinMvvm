using System;
using System.Threading.Tasks;

namespace ThinMvvm.Data.Infrastructure
{
    /// <summary>
    /// Caches data and retrieves it when no data is present.
    /// </summary>
    public sealed class Cache
    {
        private readonly IDataStore _store;
        private readonly string _idPrefix;
        private readonly string _expirationDatePrefix;


        /// <summary>
        /// Initializes a new instance of the <see cref="Cache{T}" /> class, with the specified ID,
        /// using the specified data store, and optionally using metadata provided by the specified function.
        /// If no metadata provider is specified, all data will be considered to have the same ID, and to never expire.
        /// </summary>
        /// <param name="id">The cache ID.</param>
        /// <param name="store">The data store to use for cached data.</param>
        /// <param name="metadataProvider">The metadata provider, if any.</param>
        public Cache( string id, IDataStore store )
        {
            if( id == null )
            {
                throw new ArgumentNullException( nameof( id ) );
            }
            if( store == null )
            {
                throw new ArgumentNullException( nameof( store ) );
            }

            _store = store;
            _idPrefix = "Cache_" + id + "_";
            _expirationDatePrefix = _idPrefix + "Date_";
        }


        public async Task<Optional<T>> GetAsync<T>( string id )
        {
            if( id == null )
            {
                throw new ArgumentNullException( nameof( id ) );
            }

            var storedExpirationDate = await _store.LoadAsync<DateTimeOffset>( _expirationDatePrefix + id );
            if( storedExpirationDate.HasValue && storedExpirationDate.Value > DateTimeOffset.UtcNow )
            {
                return await _store.LoadAsync<T>( _idPrefix + id );
            }

            return default( Optional<T> );
        }

        public async Task StoreAsync( string id, object value, DateTimeOffset? expirationDate )
        {
            if( id == null )
            {
                throw new ArgumentNullException( nameof( id ) );
            }

            // Technically, null is treated the same as the max representable date here,
            // but that doesn't matter because of how absurdly far away in the future it is.
            // If you're reading this in the year 9999... Yay! This lib is still relevant!
            await _store.StoreAsync( _expirationDatePrefix + id, expirationDate ?? DateTimeOffset.MaxValue );
            await _store.StoreAsync( _idPrefix + id, value );
        }
    }
}