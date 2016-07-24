using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace ThinMvvm.Data.Infrastructure
{
    /// <summary>
    /// Caches data and retrieves it when no data is present.
    /// Supports expiration dates.
    /// </summary>
    [EditorBrowsable( EditorBrowsableState.Advanced )]
    public sealed class Cache
    {
        private readonly IDataStore _store;
        private readonly string _idPrefix;
        private readonly string _expirationDatePrefix;


        /// <summary>
        /// Initializes a new instance of the <see cref="Cache" /> class with the specified ID,
        /// using the specified data store.
        /// </summary>
        /// <param name="id">The cache ID.</param>
        /// <param name="store">The data store to use.</param>
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


        /// <summary>
        /// Asynchronously fetches the value associated with the specified ID.
        /// Only returns a value if it is present and has not passed its expiration date.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="id">The ID.</param>
        /// <returns>A task that represents the fetch operation.</returns>
        public async Task<Optional<T>> GetAsync<T>( string id )
        {
            if( id == null )
            {
                throw new ArgumentNullException( nameof( id ) );
            }

            var storedExpirationDate = await _store.LoadAsync<DateTimeOffset>( _expirationDatePrefix + id );
            if( storedExpirationDate.HasValue )
            {
                if( storedExpirationDate.Value > DateTimeOffset.UtcNow )
                {
                    return await _store.LoadAsync<T>( _idPrefix + id );
                }
                else
                {
                    await _store.DeleteAsync( _expirationDatePrefix + id );
                    await _store.DeleteAsync( _idPrefix + id );
                }
            }

            return default( Optional<T> );
        }

        /// <summary>
        /// Asynchronously stores the specified value with the specified ID and optional expiration date.
        /// </summary>
        /// <param name="id">The ID.</param>
        /// <param name="value">The value.</param>
        /// <param name="expirationDate">The value's expiration date, if any.</param>
        /// <returns>A task that represents the store operation.</returns>
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