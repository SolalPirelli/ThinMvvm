using System;
using System.Threading;
using System.Threading.Tasks;
using ThinMvvm.Data;
using ThinMvvm.Tests.TestInfrastructure;
using Xunit;

namespace ThinMvvm.Tests.Data
{
    public sealed class BasicPaginatedDataSourceTests
    {
        [Fact]
        public void CannotCreateWithNullFetcher()
        {
            Assert.Throws<ArgumentNullException>(
                () => new BasicPaginatedDataSource<int, int>( (Func<Optional<int>, Task<PaginatedData<int, int>>>) null )
            );
        }

        [Fact]
        public void CannotCreateWithNullCancellableFetcher()
        {
            Assert.Throws<ArgumentNullException>(
                () => new BasicPaginatedDataSource<int, int>( (Func<Optional<int>, CancellationToken, Task<PaginatedData<int, int>>>) null )
            );
        }

        [Fact]
        public async Task FetcherIsUsed()
        {
            var source = new BasicPaginatedDataSource<int, int>( _ => Task.FromResult( new PaginatedData<int, int>( 42, default( Optional<int> ) ) ) );

            await source.RefreshAsync();

            Assert.Equal( 42, source.Data[0].Value );
        }

        [Fact]
        public async Task CancellableFetcherIsUsed()
        {
            var source = new BasicPaginatedDataSource<int, int>(
                ( _, __ ) => Task.FromResult( new PaginatedData<int, int>( 42, default( Optional<int> ) ) )
            );

            await source.RefreshAsync();

            Assert.Equal( 42, source.Data[0].Value );
        }

        [Fact]
        public async Task WithCacheWorks()
        {
            var result = Task.FromResult( new PaginatedData<int, int>( 42, default( Optional<int> ) ) );
            var source = new BasicPaginatedDataSource<int, int>( _ => result )
                             .WithCache( "X", new InMemoryDataStore() );

            await source.RefreshAsync();

            result = TaskEx.FromException<PaginatedData<int, int>>( new MyException() );

            await source.RefreshAsync();

            Assert.Equal( DataStatus.Cached, source.Data[0].Status );
        }
    }
}