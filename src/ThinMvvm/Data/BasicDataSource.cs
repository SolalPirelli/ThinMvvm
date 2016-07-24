using System;
using System.Threading;
using System.Threading.Tasks;

namespace ThinMvvm.Data
{
    /// <summary>
    /// Basic implementation of <see cref="DataSource{T}" />.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    public sealed class BasicDataSource<T> : DataSource<T>
    {
        private readonly Func<CancellationToken, Task<T>> _fetcher;


        /// <summary>
        /// Initializes a new instance of the <see cref="BasicDataSource{T}" /> class with the specified data fetching function.
        /// </summary>
        /// <param name="fetcher">The cancellable data fetching function.</param>
        public BasicDataSource( Func<CancellationToken, Task<T>> fetcher )
        {
            if( fetcher == null )
            {
                throw new ArgumentNullException( nameof( fetcher ) );
            }

            _fetcher = fetcher;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BasicDataSource{T}" /> class with the specified data fetching function.
        /// </summary>
        /// <param name="fetcher">The data fetching function.</param>
        public BasicDataSource( Func<Task<T>> fetcher )
        {
            if( fetcher == null )
            {
                throw new ArgumentNullException( nameof( fetcher ) );
            }

            _fetcher = _ => fetcher();
        }


        /// <summary>
        /// Enables caching for the source.
        /// </summary>
        /// <param name="id">The source's ID.</param>
        /// <param name="dataStore">The data store for cached values.</param>
        /// <param name="metadataCreator">The metadata creator, if any.</param>
        /// <returns>The source itself, for fluent usage.</returns>
        public BasicDataSource<T> WithCache( string id, IDataStore dataStore, Func<CacheMetadata> metadataCreator = null )
        {
            EnableCache( id, dataStore, metadataCreator );
            return this;
        }


        /// <summary>
        /// Asynchronously fetches data, using the specified cancellation token.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the fetch operation.</returns>
        protected override Task<T> FetchAsync( CancellationToken cancellationToken )
        {
            return _fetcher( cancellationToken );
        }
    }
}