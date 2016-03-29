using System;
using System.Threading.Tasks;

namespace ThinMvvm.Data.Infrastructure
{
    /// <summary>
    /// Caches data and retrieves it when no data is present.
    /// </summary>
    public sealed class Cache
    {
        // "Namespace" the default ID to ensure nobody accidentally uses it
        private const string DefaultId = "#ThinMvvm_Default";

        private readonly IDataStore _store;
        private readonly string _idPrefix;
        private readonly string _expirationDatePrefix;


        /// <summary>
        /// Initializes a new instance of the <see cref="Cache{T}" /> class, belonging to the specified owner,
        /// using the specified data store, and optionally using metadata provided by the specified function.
        /// If no metadata provider is specified, all data will be considered to have the same ID, and to never expire.
        /// </summary>
        /// <param name="owner">The object that owns this cache.</param>
        /// <param name="store">The data store to use for cached data.</param>
        /// <param name="metadataProvider">The metadata provider, if any.</param>
        public Cache( object owner, IDataStore store )
        {
            if( owner == null )
            {
                throw new ArgumentNullException( nameof( owner ) );
            }
            if( store == null )
            {
                throw new ArgumentNullException( nameof( store ) );
            }

            _store = store;
            _idPrefix = "cache:" + owner.GetType().FullName + ".";
            _expirationDatePrefix = _idPrefix + "ExpirationDate.";
        }


        public async Task<Optional<T>> GetAsync<T>( string id )
        {
            if( id == null )
            {
                id = DefaultId;
            }

            var storedExpirationDate = await _store.LoadAsync<DateTimeOffset>( _expirationDatePrefix + id );
            if( storedExpirationDate.HasValue && storedExpirationDate.Value > DateTimeOffset.UtcNow )
            {
                return await _store.LoadAsync<T>( _idPrefix + id );
            }

            return default( Optional<T> );
        }

        public async Task StoreAsync( object value, string id, DateTimeOffset? expirationDate )
        {
            if( id == null )
            {
                id = DefaultId;
            }

            // Technically, null is treated the same as the max representable date here,
            // but that doesn't matter because of how absurdly far away in the future it is.
            // If you're reading this in the year 9999... Yay! This lib is still relevant!
            await _store.StoreAsync( _expirationDatePrefix + id, expirationDate ?? DateTimeOffset.MaxValue );
            await _store.StoreAsync( _idPrefix + id, value );
        }
    }
}