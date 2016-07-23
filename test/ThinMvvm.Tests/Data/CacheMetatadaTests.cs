using System;
using System.IO;
using System.Runtime.Serialization.Json;
using ThinMvvm.Tests.TestInfrastructure;
using Xunit;

namespace ThinMvvm.Data.Tests
{
    public sealed class CacheMetatadaTests
    {
        [Fact]
        public void NullIdIsNotAllowed()
        {
            Assert.Throws<ArgumentNullException>( () => new CacheMetadata( null, DateTimeOffset.Now ) );
        }

        [Fact]
        public void NullExpirationDateIsAllowed()
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

        [Fact]
        public void Equality()
        {
            EqualityTests.For( new CacheMetadata( "id", null ) )
                .WithEqual( new CacheMetadata( "id", null ) )
                .WithUnequal( new CacheMetadata( "id2", null ) )
                .WithUnequal( new CacheMetadata( "id", DateTimeOffset.MaxValue ) )
                .Test();

            EqualityTests.For( new CacheMetadata( "id", DateTimeOffset.MaxValue ) )
                .WithEqual( new CacheMetadata( "id", DateTimeOffset.MaxValue ) )
                .Test();
        }

        // While this is not required, it's easy to implement and there's no reason to prevent it
        [Fact]
        public void CanBeDataContractSerialized()
        {
            var metadata = new CacheMetadata( "abc", new DateTimeOffset( 2000, 1, 2, 3, 4, 5, TimeSpan.FromHours( 6 ) ) );

            var serializer = new DataContractJsonSerializer( metadata.GetType() );
            var stream = new MemoryStream();

            serializer.WriteObject( stream, metadata );

            stream.Seek( 0, SeekOrigin.Begin );
            var roundtrippedMetadata = (CacheMetadata) serializer.ReadObject( stream );

            Assert.Equal( metadata.Id, roundtrippedMetadata.Id );
            Assert.Equal( metadata.ExpirationDate, roundtrippedMetadata.ExpirationDate );
        }
    }
}