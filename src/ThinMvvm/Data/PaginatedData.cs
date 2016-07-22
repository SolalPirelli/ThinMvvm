using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ThinMvvm.Data
{
    /// <summary>
    /// Represents a chunk of data from a paginated source.
    /// </summary>
    /// <typeparam name="TValue">The value type.</typeparam>
    /// <typeparam name="TToken">The pagination token's type.</typeparam>
    [DataContract]
    public sealed class PaginatedData<TValue, TToken> : IEquatable<PaginatedData<TValue, TToken>>
    {
        /// <summary>
        /// Gets the data.
        /// </summary>
        [DataMember]
        public TValue Value { get; private set; }

        /// <summary>
        /// Gets the pagination token for the next batch, if there is any.
        /// </summary>
        [DataMember]
        public Optional<TToken> Token { get; private set; }


        /// <summary>
        /// Initializes a new instance of the <see cref="PaginatedData{TData, TToken}" /> class
        /// with the specified value and pagination token.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="token">The pagination token, if any.</param>
        public PaginatedData( TValue value, Optional<TToken> token )
        {
            Value = value;
            Token = token;
        }


        public static bool operator ==( PaginatedData<TValue, TToken> left, PaginatedData<TValue, TToken> right )
        {
            if( object.ReferenceEquals( left, null ) )
            {
                return object.ReferenceEquals( right, null );
            }
            return left.Equals( right );
        }

        public static bool operator !=( PaginatedData<TValue, TToken> left, PaginatedData<TValue, TToken> right )
        {
            return !( left == right );
        }

        public bool Equals( PaginatedData<TValue, TToken> other )
        {
            if( other == null )
            {
                return false;
            }

            return EqualityComparer<TValue>.Default.Equals( Value, other.Value ) && Token == other.Token;
        }

        public override bool Equals( object obj )
        {
            return Equals( obj as PaginatedData<TValue, TToken> );
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode() + 7 * Token.GetHashCode();
        }
    }
}