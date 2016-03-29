using System;
using System.Threading.Tasks;
using ThinMvvm.Tests.TestInfrastructure;
using Xunit;

namespace ThinMvvm.Data.Infrastructure.Tests
{
    /// <summary>
    /// Tests for <see cref="Cache" />.
    /// </summary>
    public sealed class CacheTests
    {
        [Fact]
        public void CannotCreateWithNullOwner()
        {
            Assert.Throws<ArgumentNullException>( () => new Cache( null, new InMemoryDataStore() ) );
        }

        [Fact]
        public void CannotCreateWithNullStore()
        {
            Assert.Throws<ArgumentNullException>( () => new Cache( this, null ) );
        }


        [Fact]
        public async Task ValuesAreCachedWithNoMetadata()
        {
            var cache = new Cache( this, new InMemoryDataStore() );

            await cache.StoreAsync( 42, null, null );

            var value = await cache.GetAsync<int>( null );

            Assert.Equal( new Optional<int>( 42 ), value );
        }

        [Fact]
        public async Task ValuesAreCachedWithSameMetadata()
        {
            var cache = new Cache( this, new InMemoryDataStore() );

            await cache.StoreAsync( 42, "id", null );

            var value = await cache.GetAsync<int>( "id" );

            Assert.Equal( new Optional<int>( 42 ), value );
        }

        [Fact]
        public async Task CacheIsNotUsedWhenMetadataIdDiffers()
        {
            var cache = new Cache( this, new InMemoryDataStore() );

            await cache.StoreAsync( 42, "id", null );

            var value = await cache.GetAsync<int>( "id2" );

            Assert.Equal( default( Optional<int> ), value );
        }

        [Fact]
        public async Task CacheDifferentiatesBetweenMetadataIds()
        {
            var cache = new Cache( this, new InMemoryDataStore() );

            await cache.StoreAsync( 1, "id", null );
            await cache.StoreAsync( 2, "id2", null );

            var value = await cache.GetAsync<int>( "id" );

            Assert.Equal( new Optional<int>( 1 ), value );
        }

        [Fact]
        public async Task NullAndEmptyIdsAreNotTheSame()
        {
            var cache = new Cache( this, new InMemoryDataStore() );

            await cache.StoreAsync( 1, null, null );
            await cache.StoreAsync( 2, "", null );

            var value = await cache.GetAsync<int>( null );

            Assert.Equal( new Optional<int>( 1 ), value );
        }

        [Fact]
        public async Task OldDataIsNotUsed()
        {
            var cache = new Cache( this, new InMemoryDataStore() );

            await cache.StoreAsync( 1, null, DateTimeOffset.MinValue );

            var value = await cache.GetAsync<int>( null );

            Assert.Equal( default( Optional<int> ), value );
        }

        [Fact]
        public async Task ExpirationDateChangesAreRespected()
        {
            var cache = new Cache( this, new InMemoryDataStore() );

            await cache.StoreAsync( 1, "id", DateTimeOffset.MaxValue );
            await cache.StoreAsync( 2, "id", DateTimeOffset.MinValue );

            var value = await cache.GetAsync<int>( "id" );

            Assert.Equal( default( Optional<int> ), value );
        }
    }
}