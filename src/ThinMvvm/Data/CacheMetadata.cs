using System;
using System.Runtime.Serialization;

namespace ThinMvvm.Data
{
    /// <summary>
    /// Contains metadata for cache entries. 
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


        public static bool operator ==( CacheMetadata left, CacheMetadata right )
        {
            if( object.ReferenceEquals( left, null ) )
            {
                return object.ReferenceEquals( right, null );
            }

            return left.Equals( right );
        }

        public static bool operator !=( CacheMetadata left, CacheMetadata right )
        {
            return !( left == right );
        }


        public bool Equals( CacheMetadata other )
        {
            if( other == null )
            {
                return false;
            }
            return Id == other.Id && ExpirationDate == other.ExpirationDate;
        }

        public override bool Equals( object obj )
        {
            return Equals( obj as CacheMetadata );
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode() + 7 * ( ExpirationDate == null ? 0 : ExpirationDate.GetHashCode() );
        }
    }
}