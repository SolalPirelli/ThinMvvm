using System;
using System.Runtime.Serialization;

namespace ThinMvvm.Data
{
    /// <summary>
    /// Metadata for cache entries.
    /// </summary>
    [DataContract]
    public sealed class CacheMetadata : IEquatable<CacheMetadata>
    {
        /// <summary>
        /// Gets the default cache metadata.
        /// </summary>
        public static CacheMetadata Default { get; } = new CacheMetadata( "", null );


        /// <summary>
        /// Gets the data's ID.
        /// </summary>
        [DataMember]
        public string Id { get; private set; }

        /// <summary>
        /// Gets the data's expiration date, if any.
        /// </summary>
        [DataMember]
        public DateTimeOffset? ExpirationDate { get; private set; }


        /// <summary>
        /// Initializes a new instance of the <see cref="CacheMetadata" /> class 
        /// with the data's ID and its expiration date, the latter being optional.
        /// </summary>
        /// <param name="id">The data's ID.</param>
        /// <param name="expirationDate">The data's expiration date, if any.</param>
        public CacheMetadata( string id, DateTimeOffset? expirationDate )
        {
            if( id == null )
            {
                throw new ArgumentNullException( nameof( id ) );
            }

            Id = id;
            ExpirationDate = expirationDate;
        }


        /// <summary>
        /// Indicates whether the two instances of <see cref="CacheMetadata" /> are equal.
        /// </summary>
        /// <param name="left">The first instance.</param>
        /// <param name="right">The second instance.</param>
        /// <returns>A value indicating whether the instances are equal.</returns>
        public static bool operator ==( CacheMetadata left, CacheMetadata right )
        {
            if( object.ReferenceEquals( left, null ) )
            {
                return object.ReferenceEquals( right, null );
            }

            return left.Equals( right );
        }

        /// <summary>
        /// Indicates whether the two instances of <see cref="CacheMetadata" /> are unequal.
        /// </summary>
        /// <param name="left">The first instance.</param>
        /// <param name="right">The second instance.</param>
        /// <returns>A value indicating whether the instances are unequal.</returns>
        public static bool operator !=( CacheMetadata left, CacheMetadata right )
        {
            return !( left == right );
        }

        /// <summary>
        /// Indicates whether the <see cref="CacheMetadata" /> is equal to the specified <see cref="CacheMetadata" />.
        /// </summary>
        /// <param name="other">The other instance.</param>
        /// <returns>A value indicating whether the two instances are equal.</returns>
        public bool Equals( CacheMetadata other )
        {
            if( other == null )
            {
                return false;
            }
            return Id == other.Id && ExpirationDate == other.ExpirationDate;
        }

        /// <summary>
        /// Indicates whether the <see cref="CacheMetadata" /> is equal to the specified object.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns>A value indicating whether the two objects are equal.</returns>
        public override bool Equals( object obj )
        {
            return Equals( obj as CacheMetadata );
        }

        /// <summary>
        /// Returns the hash code of the object.
        /// </summary>
        /// <returns>The object's hash code.</returns>
        public override int GetHashCode()
        {
            return Id.GetHashCode() + 7 * ( ExpirationDate == null ? 0 : ExpirationDate.GetHashCode() );
        }
    }
}