using System;
using Xunit;

namespace ThinMvvm.Data.Tests
{
    /// <summary>
    /// Tests for <see cref="CacheMetadata" />.
    /// </summary>
    public sealed class CacheMetatadaTests
    {
        [Fact]
        public void NullIdsAreAllowed()
        {
            var metadata = new CacheMetadata( null, DateTimeOffset.Now );

            Assert.Null( metadata.Id );
        }

        [Fact]
        public void NullExpirationDatesAreAllowed()
        {
            var metadata = new CacheMetadata( "id", null );

            Assert.Null( metadata.ExpirationDate );
        }

        [Fact]
        public void ConstructorSetsId()
        {
            var metadata = new CacheMetadata( "id", DateTimeOffset.Now );

            Assert.Equal( "id", metadata.Id );
        }

        [Fact]
        public void ConstructorSetsExpirationDate()
        {
            var metadata = new CacheMetadata( "id", DateTimeOffset.MaxValue );

            Assert.Equal( DateTimeOffset.MaxValue, metadata.ExpirationDate );
        }
    }
}