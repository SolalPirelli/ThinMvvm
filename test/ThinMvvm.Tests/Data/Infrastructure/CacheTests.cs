using System;
using System.Threading.Tasks;
using ThinMvvm.Data.Infrastructure;
using ThinMvvm.Tests.TestInfrastructure;
using Xunit;

namespace ThinMvvm.Tests.Data.Infrastructure
{
    public sealed class CacheTests
    {
        [Fact]
        public void CannotCreateWithNullId()
        {
            Assert.Throws<ArgumentNullException>( () => new Cache( null, new InMemoryDataStore() ) );
        }

        [Fact]
        public void CannotCreateWithNullStore()
        {
            Assert.Throws<ArgumentNullException>( () => new Cache( "X", null ) );
        }

        [Fact]
        public async Task CannotGetWithNullId()
        {
            var cache = new Cache( "X", new InMemoryDataStore() );

            await Assert.ThrowsAsync<ArgumentNullException>( () => cache.GetAsync<int>( null ) );
        }

        [Fact]
        public async Task CannotStoreWithNullId()
        {
            var cache = new Cache( "X", new InMemoryDataStore() );

            await Assert.ThrowsAsync<ArgumentNullException>( () => cache.StoreAsync( null, 1, null ) );
        }


        [Fact]
        public async Task ValuesAreCachedWithNoMetadata()
        {
            var cache = new Cache( "X", new InMemoryDataStore() );

            await cache.StoreAsync( "Y", 42, null );

            var value = await cache.GetAsync<int>( "Y" );

            Assert.Equal( new Optional<int>( 42 ), value );
        }

        [Fact]
        public async Task ValuesAreCachedWithSameMetadata()
        {
            var cache = new Cache( "X", new InMemoryDataStore() );

            await cache.StoreAsync( "id", 42, null );

            var value = await cache.GetAsync<int>( "id" );

            Assert.Equal( new Optional<int>( 42 ), value );
        }

        [Fact]
        public async Task CacheIsNotUsedWhenMetadataIdDiffers()
        {
            var cache = new Cache( "X", new InMemoryDataStore() );

            await cache.StoreAsync( "id", 42, null );

            var value = await cache.GetAsync<int>( "id2" );

            Assert.Equal( default( Optional<int> ), value );
        }

        [Fact]
        public async Task CacheDifferentiatesBetweenMetadataIds()
        {
            var cache = new Cache( "X", new InMemoryDataStore() );

            await cache.StoreAsync( "id", 1, null );
            await cache.StoreAsync( "id2", 2, null );

            var value = await cache.GetAsync<int>( "id" );

            Assert.Equal( new Optional<int>( 1 ), value );
        }

        [Fact]
        public async Task OldDataIsNotUsed()
        {
            var cache = new Cache( "X", new InMemoryDataStore() );

            await cache.StoreAsync( "Y", 1, DateTimeOffset.MinValue );

            var value = await cache.GetAsync<int>( "Y" );

            Assert.Equal( default( Optional<int> ), value );
        }

        [Fact]
        public async Task ExpirationDateChangesAreRespected()
        {
            var cache = new Cache( "X", new InMemoryDataStore() );

            await cache.StoreAsync( "id", 1, DateTimeOffset.MaxValue );
            await cache.StoreAsync( "id", 2, DateTimeOffset.MinValue );

            var value = await cache.GetAsync<int>( "id" );

            Assert.Equal( default( Optional<int> ), value );
        }
    }
}