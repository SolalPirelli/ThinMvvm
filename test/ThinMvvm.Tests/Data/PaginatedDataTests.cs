using System.IO;
using System.Runtime.Serialization.Json;
using ThinMvvm.Tests.TestInfrastructure;
using Xunit;

namespace ThinMvvm.Data.Tests
{
    /// <summary>
    /// Tests for <see cref="PaginatedData{TValue, TToken}" />.
    /// </summary>
    public sealed class PaginatedDataTests
    {
        [Fact]
        public void ConstructorSetsValuea()
        {
            var data = new PaginatedData<int, int>( 1, default( Optional<int> ) );

            Assert.Equal( 1, data.Value );
        }

        [Fact]
        public void ConstructorSetsToken()
        {
            var data = new PaginatedData<int, int>( 0, new Optional<int>( 42 ) );

            Assert.Equal( new Optional<int>( 42 ), data.Token );
        }

        [Fact]
        public void Equality()
        {
            EqualityTests.For( new PaginatedData<int, string>( 1, default( Optional<string> ) ) )
                .WithUnequal( new PaginatedData<int, string>( 1, new Optional<string>( "abc" ) ) )
                .WithUnequal( new PaginatedData<int, string>( 2, default( Optional<string> ) ) )
                .Test();

            EqualityTests.For( new PaginatedData<int, string>( 1, new Optional<string>( "abc" ) ) )
                .WithUnequal( new PaginatedData<int, string>( 0, new Optional<string>( "abc" ) ) )
                .WithUnequal( new PaginatedData<int, string>( 1, new Optional<string>( "def" ) ) )
                .Test();
        }

        [Fact]
        public void CanBeDataContractSerialized()
        {
            var data = new PaginatedData<int, string>( 42, new Optional<string>( "abc" ) );

            var serializer = new DataContractJsonSerializer( data.GetType() );
            var stream = new MemoryStream();

            serializer.WriteObject( stream, data );

            stream.Seek( 0, SeekOrigin.Begin );
            var roundtrippedData = (PaginatedData<int, string>) serializer.ReadObject( stream );

            Assert.Equal( data, roundtrippedData );
        }
    }
}