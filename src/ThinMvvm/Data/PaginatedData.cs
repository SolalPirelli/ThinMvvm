using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ThinMvvm.Data
{
    /// <summary>
    /// Value from a paginated source.
    /// </summary>
    /// <typeparam name="TValue">The value type.</typeparam>
    /// <typeparam name="TToken">The pagination token's type.</typeparam>
    [DataContract]
    public sealed class PaginatedData<TValue, TToken> : IEquatable<PaginatedData<TValue, TToken>>
    {
        /// <summary>
        /// Gets the value.
        /// </summary>
        [DataMember]
        public TValue Value { get; private set; }

        /// <summary>
        /// Gets the pagination token for the next batch, if any.
        /// </summary>
        [DataMember]
        public Optional<TToken> Token { get; private set; }


        /// <summary>
        /// Initializes a new instance of the <see cref="PaginatedData{TValue, TToken}" /> class
        /// with the specified value and pagination token.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="token">The pagination token, if any.</param>
        public PaginatedData( TValue value, Optional<TToken> token )
        {
            Value = value;
            Token = token;
        }


        /// <summary>
        /// Indicates whether the two instances of <see cref="PaginatedData{TValue, TToken}" /> are equal.
        /// </summary>
        /// <param name="left">The first instance.</param>
        /// <param name="right">The second instance.</param>
        /// <returns>A value indicating whether the instances are equal.</returns>
        public static bool operator ==( PaginatedData<TValue, TToken> left, PaginatedData<TValue, TToken> right )
        {
            if( object.ReferenceEquals( left, null ) )
            {
                return object.ReferenceEquals( right, null );
            }
            return left.Equals( right );
        }

        /// <summary>
        /// Indicates whether the two instances of <see cref="PaginatedData{TValue, TToken}" /> are unequal.
        /// </summary>
        /// <param name="left">The first instance.</param>
        /// <param name="right">The second instance.</param>
        /// <returns>A value indicating whether the instances are unequal.</returns>
        public static bool operator !=( PaginatedData<TValue, TToken> left, PaginatedData<TValue, TToken> right )
        {
            return !( left == right );
        }

        /// <summary>
        /// Indicates whether the <see cref="PaginatedData{TValue, TToken}" /> is equal to the specified <see cref="PaginatedData{TValue, TToken}" />.
        /// </summary>
        /// <param name="other">The other instance.</param>
        /// <returns>A value indicating whether the two instances are equal.</returns>
        public bool Equals( PaginatedData<TValue, TToken> other )
        {
            if( other == null )
            {
                return false;
            }

            return EqualityComparer<TValue>.Default.Equals( Value, other.Value ) && Token == other.Token;
        }

        /// <summary>
        /// Indicates whether the <see cref="PaginatedData{TValue, TToken}" /> is equal to the specified object.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns>A value indicating whether the two objects are equal.</returns>
        public override bool Equals( object obj )
        {
            return Equals( obj as PaginatedData<TValue, TToken> );
        }

        /// <summary>
        /// Returns the hash code of the object.
        /// </summary>
        /// <returns>The object's hash code.</returns>
        public override int GetHashCode()
        {
            return Value.GetHashCode() + 7 * Token.GetHashCode();
        }
    }
}