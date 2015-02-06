// Copyright (c) 2014-15 Solal Pirelli
// See License.txt file for more details

using System;
using System.Threading.Tasks;

namespace ThinMvvm.Design
{
    /// <summary>
    /// <see cref="IDataCache" /> implementation for design-time data.
    /// Does not cache anything.
    /// </summary>
    public sealed class DesignDataCache : IDataCache
    {
        /// <summary>
        /// Returns no data.
        /// </summary>
        /// <typeparam name="T">Ignored.</typeparam>
        /// <param name="owner">Ignored.</param>
        /// <param name="id">Ignored.</param>
        /// <returns>No data.</returns>
        public Task<CachedData<T>> GetAsync<T>( Type owner, long id )
        {
            return Task.FromResult( new CachedData<T>() );
        }

        /// <summary>
        /// Does nothing.
        /// </summary>
        /// <param name="owner">Ignored.</param>
        /// <param name="id">Ignored.</param>
        /// <param name="expirationDate">Ignored.</param>
        /// <param name="value">Ignored.</param>
        /// <returns>An already-completed task.</returns>
        public Task SetAsync( Type owner, long id, DateTimeOffset expirationDate, object value )
        {
            return Task.FromResult( 0 );
        }
    }
}