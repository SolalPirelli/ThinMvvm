using ThinMvvm.Tests.TestInfrastructure;
using Xunit;

namespace ThinMvvm.Tests
{
    public sealed class KeyValueStoreExtensionsTests
    {
        [Fact]
        public void GetReturnsValueIfPresent()
        {
            var store = new InMemoryKeyValueStore();
            store.Set( "X", 42 );

            var result = store.Get( "X", 10 );

            Assert.Equal( 42, result );
        }

        [Fact]
        public void GetReturnsDefaultValueIfAbsent()
        {
            var store = new InMemoryKeyValueStore();

            var result = store.Get( "X", 10 );

            Assert.Equal( 10, result );
        }
    }
}