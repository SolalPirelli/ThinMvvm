using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;
using ThinMvvm.Tests.TestInfrastructure;
using Xunit;

namespace ThinMvvm.Data.Tests
{
    /// <summary>
    /// Tests for <see cref="PaginatedData{TItem, TToken}" />.
    /// </summary>
    public sealed class PaginatedDataTests
    {
        [Fact]
        public void CannotCreatePaginatedDataWithNullItems()
        {
            Assert.Throws<ArgumentNullException>( () => new PaginatedData<int, int>( null, new Optional<int>( 0 ) ) );
        }

        [Fact]
        public void ConstructorSetsItems()
        {
            var items = new[] { 1, 2, 3 };
            var data = new PaginatedData<int, int>( items, default( Optional<int> ) );

            Assert.Equal( items, data.Items );
        }

        [Fact]
        public void ConstructorSetsToken()
        {
            var data = new PaginatedData<int, int>( new int[0], new Optional<int>( 42 ) );

            Assert.Equal( new Optional<int>( 42 ), data.Token );
        }

        [Fact]
        public void Equality()
        {
            EqualityTests.For( new PaginatedData<int, string>( new int[0], default( Optional<string> ) ) )
                .WithEqual( new PaginatedData<int, string>( new int[0], default( Optional<string> ) ) )
                .WithUnequal( new PaginatedData<int, string>( new int[0], new Optional<string>( "abc" ) ) )
                .WithUnequal( new PaginatedData<int, string>( new[] { 1, 2, 3 }, default( Optional<string> ) ) )
                .WithUnequal( new PaginatedData<int, string>( new[] { 1, 2, 3 }, new Optional<string>( "abc" ) ) )
                .Test();

            EqualityTests.For( new PaginatedData<int, string>( new[] { 1, 2, 3 }, default( Optional<string> ) ) )
                .WithEqual( new PaginatedData<int, string>( new List<int>( new[] { 1, 2, 3 } ), default( Optional<string> ) ) )
                .WithUnequal( new PaginatedData<int, string>( new[] { 3, 2, 1 }, default( Optional<string> ) ) )
                .WithUnequal( new PaginatedData<int, string>( new[] { 1, 2, 3 }, new Optional<string>( "abc" ) ) )
                .Test();

            EqualityTests.For( new PaginatedData<int, string>( new[] { 1, 2, 3 }, new Optional<string>( "abc" ) ) )
                .WithEqual( new PaginatedData<int, string>( new List<int>( new[] { 1, 2, 3 } ), new Optional<string>( "abc" ) ) )
                .WithUnequal( new PaginatedData<int, string>( new[] { 3, 2, 1 }, new Optional<string>( "abc" ) ) )
                .Test();
        }

        [Fact]
        public void CanBeDataContractSerialized()
        {
            var data = new PaginatedData<int, string>( new[] { 1, 2, 3 }, new Optional<string>( "abc" ) );

            var serializer = new DataContractJsonSerializer( data.GetType() );
            var stream = new MemoryStream();

            serializer.WriteObject( stream, data );

            stream.Seek( 0, SeekOrigin.Begin );
            var roundtrippedData = (PaginatedData<int, string>) serializer.ReadObject( stream );

            Assert.Equal( data.Items, roundtrippedData.Items );
            Assert.Equal( data.Token, roundtrippedData.Token );
        }
    }
}