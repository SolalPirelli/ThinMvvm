using System;
using System.Collections.Generic;

namespace ThinMvvm.Data
{
    /// <summary>
    /// Represents a chunk of items from a paginated source.
    /// </summary>
    /// <typeparam name="TItem">The items' type.</typeparam>
    /// <typeparam name="TToken">The pagination token's type.</typeparam>
    public sealed class PaginatedData<TItem, TToken>
    {
        /// <summary>
        /// Gets the items.
        /// </summary>
        public IReadOnlyList<TItem> Items { get; }

        /// <summary>
        /// Gets the pagination token for the next batch, if there is any.
        /// </summary>
        public Optional<TToken> Token { get; }


        /// <summary>
        /// Initializes a new instance of the <see cref="PaginatedData{TItem, TToken}" /> class
        /// with the specified items and pagination token.
        /// </summary>
        /// <param name="items">The items.</param>
        /// <param name="token">The pagination token, if any.</param>
        public PaginatedData( IReadOnlyList<TItem> items, Optional<TToken> token )
        {
            if( items == null )
            {
                throw new ArgumentNullException( nameof( items ) );
            }

            Items = items;
            Token = token;
        }
    }
}