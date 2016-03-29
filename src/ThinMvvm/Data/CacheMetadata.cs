using System;

namespace ThinMvvm.Data
{
    /// <summary>
    /// Contains metadata for cache entries. 
    /// </summary>
    public sealed class CacheMetadata
    {
        public static readonly CacheMetadata Default = new CacheMetadata( null, null );

        /// <summary>
        /// Gets the data's ID, if any.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Gets the data's expiration date, if any.
        /// </summary>
        public DateTimeOffset? ExpirationDate { get; }


        /// <summary>
        /// Initializes a new instance of the <see cref="CacheMetadata" /> class 
        /// with some data's ID and expiration date, both of which are optional.
        /// </summary>
        /// <param name="id">The data's ID, if any.</param>
        /// <param name="expirationDate">The data's expiration date, if any.</param>
        public CacheMetadata( string id, DateTimeOffset? expirationDate )
        {
            Id = id;
            ExpirationDate = expirationDate;
        }
    }
}