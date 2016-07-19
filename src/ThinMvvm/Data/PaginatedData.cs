using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace ThinMvvm.Data
{
    /// <summary>
    /// Represents a chunk of items from a paginated source.
    /// </summary>
    /// <typeparam name="TItem">The items' type.</typeparam>
    /// <typeparam name="TToken">The pagination token's type.</typeparam>
    [DataContract]
    public sealed class PaginatedData<TItem, TToken> : IEquatable<PaginatedData<TItem, TToken>>
    {
        /// <summary>
        /// Gets the items.
        /// </summary>
        /// <remarks>
        /// This property is a ReadOnlyCollection instead of the more abstract IReadOnlyList,
        /// so that data contract serialization can work properly without having to add known types.
        /// </remarks>
        [DataMember]
        public ReadOnlyCollection<TItem> Items { get; private set; }

        /// <summary>
        /// Gets the pagination token for the next batch, if there is any.
        /// </summary>
        [DataMember]
        public Optional<TToken> Token { get; private set; }


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

            Items = new ReadOnlyCollection<TItem>( new List<TItem>( items ) );
            Token = token;
        }


        public static bool operator ==( PaginatedData<TItem, TToken> left, PaginatedData<TItem, TToken> right )
        {
            if( object.ReferenceEquals( left, null ) )
            {
                return object.ReferenceEquals( right, null );
            }
            return left.Equals( right );
        }

        public static bool operator !=( PaginatedData<TItem, TToken> left, PaginatedData<TItem, TToken> right )
        {
            return !( left == right );
        }

        public bool Equals( PaginatedData<TItem, TToken> other )
        {
            if( other == null )
            {
                return false;
            }
            if( Items.Count != other.Items.Count || Token != other.Token )
            {
                return false;
            }

            for( int n = 0; n < Items.Count; n++ )
            {
                if( !EqualityComparer<TItem>.Default.Equals( Items[n], other.Items[n] ) )
                {
                    return false;
                }
            }

            return true;
        }

        public override bool Equals( object obj )
        {
            return Equals( obj as PaginatedData<TItem, TToken> );
        }

        public override int GetHashCode()
        {
            // Don't take the items themselves into account, slow and not needed to satisfy the contract
            return Items.Count + 7 * Token.GetHashCode();
        }
    }
}