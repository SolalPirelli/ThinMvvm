// Copyright (c) Solal Pirelli 2014
// See License.txt file for more details

using System;
using System.Threading.Tasks;

namespace ThinMvvm
{
    /// <summary>
    /// Caches one ID -> Data map per Type.
    /// </summary>
    public interface IDataCache
    {
        /// <summary>
        /// Asynchronously gets the value stored by the specified owner type, with the specified ID.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="owner">The owner type.</param>
        /// <param name="id">The ID.</param>
        /// <returns>The cached value.</returns>
        Task<CachedData<T>> GetAsync<T>( Type owner, long id );

        /// <summary>
        /// Asynchronously sets the specified value for the specified owner type, with the specified ID.
        /// </summary>
        /// <param name="owner">The owner type.</param>
        /// <param name="id">The ID.</param>
        /// <param name="expirationDate">The expiration date.</param>
        /// <param name="value">The value.</param>
        Task SetAsync( Type owner, long id, DateTimeOffset expirationDate, object value );
    }
}