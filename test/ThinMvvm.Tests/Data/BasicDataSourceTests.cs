using System;
using System.Threading;
using System.Threading.Tasks;
using ThinMvvm.Data;
using ThinMvvm.Tests.TestInfrastructure;
using Xunit;

namespace ThinMvvm.Tests.Data
{
    public sealed class BasicDataSourceTests
    {
        [Fact]
        public void CannotCreateWithNullFetcher()
        {
            Assert.Throws<ArgumentNullException>( () => new BasicDataSource<int>( (Func<Task<int>>) null ) );
        }

        [Fact]
        public void CannotCreateWithNullCancellableFetcher()
        {
            Assert.Throws<ArgumentNullException>( () => new BasicDataSource<int>( (Func<CancellationToken, Task<int>>) null ) );
        }

        [Fact]
        public async Task FetcherIsUsed()
        {
            var source = new BasicDataSource<int>( () => Task.FromResult( 42 ) );

            await source.RefreshAsync();

            Assert.Equal( 42, source.Data.Value );
        }

        [Fact]
        public async Task CancellableFetcherIsUsed()
        {
            var source = new BasicDataSource<int>( _ => Task.FromResult( 42 ) );

            await source.RefreshAsync();

            Assert.Equal( 42, source.Data.Value );
        }

        [Fact]
        public async Task WithCacheWorks()
        {
            var result = Task.FromResult( 42 );
            var source = new BasicDataSource<int>( () => result )
                             .WithCache( "X", new InMemoryDataStore() );

            await source.RefreshAsync();

            result = TaskEx.FromException<int>( new MyException() );

            await source.RefreshAsync();

            Assert.Equal( DataStatus.Cached, source.Data.Status );
        }
    }
}