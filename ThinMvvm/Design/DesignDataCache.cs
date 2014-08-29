// Copyright (c) Solal Pirelli 2014
// See License.txt file for more details

using System;

namespace ThinMvvm.Design
{
    /// <summary>
    /// <see cref="IDataCache" /> implementation for design-time data.
    /// Does not cache anything.
    /// </summary>
    public sealed class DesignDataCache : IDataCache
    {
        /// <summary>
        /// Returns false.
        /// </summary>
        /// <typeparam name="T">Ignored.</typeparam>
        /// <param name="owner">Ignored.</param>
        /// <param name="id">Ignored.</param>
        /// <param name="value">Ignored.</param>
        /// <returns>False.</returns>
        public bool TryGet<T>( Type owner, long id, out T value )
        {
            value = default( T );
            return false;
        }

        /// <summary>
        /// Does nothing.
        /// </summary>
        /// <param name="owner">Ignored.</param>
        /// <param name="id">Ignored.</param>
        /// <param name="expirationDate">Ignored.</param>
        /// <param name="value">Ignored.</param>
        public void Set( Type owner, long id, DateTime expirationDate, object value ) { }
    }
}