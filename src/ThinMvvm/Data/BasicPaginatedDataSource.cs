using System;
using System.Threading;
using System.Threading.Tasks;

namespace ThinMvvm.Data
{
    /// <summary>
    /// Basic implementation of <see cref="DataSource{T}" />.
    /// </summary>
    /// <typeparam name="TValue">The value type.</typeparam>
    /// <typeparam name="TToken">The pagination token type.</typeparam>
    public sealed class BasicPaginatedDataSource<TValue, TToken> : PaginatedDataSource<TValue, TToken>
    {
        private readonly Func<Optional<TToken>, CancellationToken, Task<PaginatedData<TValue, TToken>>> _fetcher;


        /// <summary>
        /// Initializes a new instance of the <see cref="BasicPaginatedDataSource{TValue, TToken}" /> class 
        /// with the specified data fetching function.
        /// </summary>
        /// <param name="fetcher">The cancellable data fetching function.</param>
        public BasicPaginatedDataSource( Func<Optional<TToken>, CancellationToken, Task<PaginatedData<TValue, TToken>>> fetcher )
        {
            if( fetcher == null )
            {
                throw new ArgumentNullException( nameof( fetcher ) );
            }

            _fetcher = fetcher;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BasicPaginatedDataSource{TValue, TToken}" /> class 
        /// with the specified data fetching function.
        /// </summary>
        /// <param name="fetcher">The data fetching function.</param>
        public BasicPaginatedDataSource( Func<Optional<TToken>, Task<PaginatedData<TValue, TToken>>> fetcher )
        {
            if( fetcher == null )
            {
                throw new ArgumentNullException( nameof( fetcher ) );
            }

            _fetcher = ( token, _ ) => fetcher( token );
        }


        /// <summary>
        /// Enables caching for the source.
        /// </summary>
        /// <param name="id">The source's ID.</param>
        /// <param name="dataStore">The data store for cached values.</param>
        /// <param name="metadataCreator">The metadata creator, if any.</param>
        /// <returns>The source itself, for fluent usage.</returns>
        public BasicPaginatedDataSource<TValue, TToken> WithCache( string id, IDataStore dataStore, Func<Optional<TToken>, CacheMetadata> metadataCreator = null )
        {
            EnableCache( id, dataStore, metadataCreator );
            return this;
        }


        /// <summary>
        /// Asynchronously fetches data, using the specified pagination and cancellation tokens.
        /// </summary>
        /// <param name="paginationToken">The pagination token, if any.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the fetch operation.</returns>
        protected override Task<PaginatedData<TValue, TToken>> FetchAsync( Optional<TToken> paginationToken, CancellationToken cancellationToken )
        {
            return _fetcher( paginationToken, cancellationToken );
        }
    }
}