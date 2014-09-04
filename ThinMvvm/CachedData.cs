// Copyright (c) Solal Pirelli 2014
// See License.txt file for more details

using System;

namespace ThinMvvm
{
    /// <summary>
    /// Data cached by an <see cref="IDataCache" />.
    /// </summary>
    /// <typeparam name="T">The data type.</typeparam>
    public sealed class CachedData<T>
    {
        private readonly T _data;

        /// <summary>
        /// Gets a value indicating whether data was in the cache.
        /// </summary>
        public bool HasData { get; private set; }

        /// <summary>
        /// Gets the cached data, or throws if there is none.
        /// </summary>
        public T Data
        {
            get
            {
                if ( HasData )
                {
                    return _data;
                }
                throw new InvalidOperationException( "Cannot get non-existent data." );
            }
        }

        /// <summary>
        /// Creates a new instance of the <see cref="CachedData{T}" /> class, representing a lack of data.
        /// </summary>
        public CachedData()
        {
            HasData = false;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="CachedData{T}" /> class with data.
        /// </summary>
        /// <param name="data"></param>
        public CachedData( T data )
        {
            HasData = true;
            _data = data;
        }
    }
}