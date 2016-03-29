using System;
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

            Assert.Equal<int>( items, data.Items );
        }

        [Fact]
        public void ConstructorSetsToken()
        {
            var data = new PaginatedData<int, int>( Array.Empty<int>(), new Optional<int>( 42 ) );

            Assert.Equal( new Optional<int>( 42 ), data.Token );
        }
    }
}