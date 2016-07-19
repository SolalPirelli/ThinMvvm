using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ThinMvvm.Tests.TestInfrastructure;
using Xunit;

namespace ThinMvvm.Data.Tests
{
    /// <summary>
    /// Tests for <see cref="PaginatedDataSource{TItem, TToken}" />.
    /// </summary>
    public sealed class PaginatedDataSourceTests
    {
        private static PaginatedData<int, int> Paginated( int result, Optional<int> token = default( Optional<int> ) )
        {
            return new PaginatedData<int, int>( new[] { result }, token );
        }


        public sealed class Basic
        {
            private sealed class IntDataSource : PaginatedDataSource<int, int>
            {
                public Func<Optional<int>, CancellationToken, Task<PaginatedData<int, int>>> Fetch;


                public IntDataSource( Func<Optional<int>, CancellationToken, Task<PaginatedData<int, int>>> fetch )
                {
                    Fetch = fetch;
                }


                protected override Task<PaginatedData<int, int>> FetchAsync( Optional<int> paginationToken, CancellationToken cancellationToken )
                    => Fetch( paginationToken, cancellationToken );
            }

            [Fact]
            public void InitialState()
            {
                var source = new IntDataSource( ( _, __ ) => Task.FromResult( Paginated( 0 ) ) );

                Assert.Equal( null, source.Values );
                Assert.Equal( null, source.LastException );
                Assert.Equal( DataStatus.None, source.Status );
            }

            [Fact]
            public async Task RefreshInProgress()
            {
                var taskSource = new TaskCompletionSource<PaginatedData<int, int>>();
                var source = new IntDataSource( ( _, __ ) => taskSource.Task );

                DataStatus? status = null;
                source.PropertyChanged += ( _, e ) =>
                {
                    if( e.PropertyName == nameof( IntDataSource.Status ) )
                    {
                        status = source.Status;
                    }
                };

                var task = source.RefreshAsync();

                Assert.Equal( null, source.Values );
                Assert.Equal( null, source.LastException );
                Assert.Equal( DataStatus.Loading, source.Status );

                Assert.Equal( source.Status, status );

                taskSource.SetResult( Paginated( 0 ) );
                await task;
            }

            [Fact]
            public async Task SuccessfulRefresh()
            {
                var source = new IntDataSource( ( _, __ ) => Task.FromResult( Paginated( 42 ) ) );

                DataStatus? status = null;
                int countAfterStatus = 0;
                source.PropertyChanged += ( _, e ) =>
                {
                    if( e.PropertyName == nameof( IntDataSource.Status ) )
                    {
                        status = source.Status;
                        countAfterStatus = 0;
                    }
                    else
                    {
                        countAfterStatus++;
                    }
                };

                await source.RefreshAsync();

                Assert.Equal( new[] { 42 }, source.Values );
                Assert.Equal( null, source.LastException );
                Assert.Equal( DataStatus.Loaded, source.Status );

                Assert.Equal( source.Status, status );
                Assert.Equal( 0, countAfterStatus );
            }

            [Fact]
            public async Task FailedRefresh()
            {
                var ex = new MyException();
                var source = new IntDataSource( ( _, __ ) => TaskEx.FromException<PaginatedData<int, int>>( ex ) );

                DataStatus? status = null;
                int countAfterStatus = 0;
                source.PropertyChanged += ( _, e ) =>
                {
                    if( e.PropertyName == nameof( IntDataSource.Status ) )
                    {
                        status = source.Status;
                        countAfterStatus = 0;
                    }
                    else
                    {
                        countAfterStatus++;
                    }
                };

                await source.RefreshAsync();

                Assert.Equal( null, source.Values );
                Assert.Equal( ex, source.LastException );
                Assert.Equal( DataStatus.NoData, source.Status );

                Assert.Equal( source.Status, status );
                Assert.Equal( 0, countAfterStatus );
            }

            [Fact]
            public async Task RefreshCancelsOlderCall()
            {
                var tokens = new List<CancellationToken>();
                var source = new IntDataSource( ( _, t ) => { tokens.Add( t ); return Task.FromResult( Paginated( 0 ) ); } );

                await source.RefreshAsync();
                await source.RefreshAsync();

                Assert.Equal( 2, tokens.Count );
                Assert.True( tokens[0].IsCancellationRequested );
                Assert.False( tokens[1].IsCancellationRequested );
            }

            [Fact]
            public async Task SlowEarlyRefreshDoesNotOverrideLaterOne()
            {
                var taskSource = new TaskCompletionSource<PaginatedData<int, int>>();
                var source = new IntDataSource( ( _, ___ ) => taskSource.Task );

                // Call 1 begins
                var task = source.RefreshAsync();

                source.Fetch = ( _, __ ) => Task.FromResult( Paginated( 42 ) );
                // Call 2 begins
                var task2 = source.RefreshAsync();

                // Call 1 completes
                taskSource.SetResult( Paginated( 100 ) );
                // Wait for both
                await task;
                await task2;

                Assert.Equal( new[] { 42 }, source.Values );
            }

            [Fact]
            public async Task SlowFailedEarlyRefreshDoesNotOverrideLaterOne()
            {
                var taskSource = new TaskCompletionSource<PaginatedData<int, int>>();
                var source = new IntDataSource( ( _, __ ) => taskSource.Task );

                // Call 1 begins
                var task = source.RefreshAsync();

                source.Fetch = ( _, __ ) => Task.FromResult( Paginated( 42 ) );
                // Call 2 begins
                var task2 = source.RefreshAsync();

                // Call 1 fails
                taskSource.SetException( new Exception() );
                // Wait for both
                await task;
                await task2;

                Assert.Equal( DataStatus.Loaded, source.Status );
            }

            [Fact]
            public async Task CanFetchMoreWhenRefreshReturnsToken()
            {
                var source = new IntDataSource( ( _, __ ) => Task.FromResult( Paginated( 1, new Optional<int>( 1 ) ) ) );

                await source.RefreshAsync();

                Assert.True( source.CanFetchMore );
            }

            [Fact]
            public async Task CannotFetchMoreWhenRefreshDoesNotReturnToken()
            {
                var source = new IntDataSource( ( _, __ ) => Task.FromResult( Paginated( 1 ) ) );

                await source.RefreshAsync();

                Assert.False( source.CanFetchMore );
            }

            [Fact]
            public async Task CannotCallFetchMoreAfterSingleRefreshWithError()
            {
                var source = new IntDataSource( ( _, __ ) => TaskEx.FromException<PaginatedData<int, int>>( new MyException() ) );
                await source.RefreshAsync();

                Assert.False( source.CanFetchMore );
            }

            [Fact]
            public async Task FetchMoreInProgress()
            {
                var source = new IntDataSource( ( _, __ ) => Task.FromResult( Paginated( 1, new Optional<int>( 1 ) ) ) );
                await source.RefreshAsync();

                var taskSource = new TaskCompletionSource<PaginatedData<int, int>>();
                source.Fetch = ( _, __ ) => taskSource.Task;

                DataStatus? status = null;
                source.PropertyChanged += ( _, e ) =>
                {
                    if( e.PropertyName == nameof( IntDataSource.Status ) )
                    {
                        status = source.Status;
                    }
                };

                var task = source.FetchMoreAsync();

                Assert.Equal( new[] { 1 }, source.Values );
                Assert.Equal( null, source.LastException );
                Assert.Equal( DataStatus.LoadingMore, source.Status );

                Assert.Equal( source.Status, status );

                taskSource.SetResult( Paginated( 1 ) );
                await task;
            }

            [Fact]
            public async Task SuccessfulFetchMore()
            {
                var source = new IntDataSource( ( _, __ ) => Task.FromResult( Paginated( 1, new Optional<int>( 1 ) ) ) );
                await source.RefreshAsync();

                source.Fetch = ( _, __ ) => Task.FromResult( Paginated( 2 ) );

                DataStatus? status = null;
                int countAfterStatus = 0;
                source.PropertyChanged += ( _, e ) =>
                {
                    if( e.PropertyName == nameof( IntDataSource.Status ) )
                    {
                        status = source.Status;
                        countAfterStatus = 0;
                    }
                    else
                    {
                        countAfterStatus++;
                    }
                };

                await source.FetchMoreAsync();

                Assert.Equal( new[] { 1, 2, }, source.Values );
                Assert.Equal( null, source.LastException );
                Assert.Equal( DataStatus.Loaded, source.Status );

                Assert.Equal( source.Status, status );
                Assert.Equal( 0, countAfterStatus );
            }

            [Fact]
            public async Task FailedFetchMore()
            {
                var source = new IntDataSource( ( _, __ ) => Task.FromResult( Paginated( 1, new Optional<int>( 1 ) ) ) );
                await source.RefreshAsync();

                var ex = new MyException();
                source.Fetch = ( _, __ ) => TaskEx.FromException<PaginatedData<int, int>>( ex );

                DataStatus? status = null;
                int countAfterStatus = 0;
                source.PropertyChanged += ( _, e ) =>
                {
                    if( e.PropertyName == nameof( IntDataSource.Status ) )
                    {
                        status = source.Status;
                        countAfterStatus = 0;
                    }
                    else
                    {
                        countAfterStatus++;
                    }
                };

                await source.FetchMoreAsync();

                Assert.Equal( new[] { 1 }, source.Values );
                Assert.Equal( ex, source.LastException );
                Assert.Equal( DataStatus.Loaded, source.Status );

                Assert.Equal( source.Status, status );
                Assert.Equal( 0, countAfterStatus );
            }

            [Fact]
            public async Task FetchMoreCancelsOlderCall()
            {
                var source = new IntDataSource( ( _, __ ) => Task.FromResult( Paginated( 1, new Optional<int>( 1 ) ) ) );

                await source.RefreshAsync();

                var tokens = new List<CancellationToken>();
                source.Fetch = ( _, t ) => { tokens.Add( t ); return Task.FromResult( Paginated( 2, new Optional<int>( 2 ) ) ); };

                await source.FetchMoreAsync();
                await source.FetchMoreAsync();

                Assert.Equal( 2, tokens.Count );
                Assert.True( tokens[0].IsCancellationRequested );
                Assert.False( tokens[1].IsCancellationRequested );
            }

            [Fact]
            public async Task SlowEarlyFetchMoreDoesNotOverrideLaterOne()
            {
                var source = new IntDataSource( ( _, __ ) => Task.FromResult( Paginated( 1, new Optional<int>( 1 ) ) ) );
                await source.RefreshAsync();

                var taskSource = new TaskCompletionSource<PaginatedData<int, int>>();
                source.Fetch = ( _, t ) => taskSource.Task;

                // Call 1 begins
                var task = source.FetchMoreAsync();

                source.Fetch = ( _, __ ) => Task.FromResult( Paginated( 2 ) );
                // Call 2 begins
                var task2 = source.FetchMoreAsync();

                // Call 1 completes
                taskSource.SetResult( Paginated( 3, new Optional<int>( 2 ) ) );
                // Wait for both
                await task;
                await task2;

                Assert.Equal( new[] { 1, 2 }, source.Values );
                Assert.Equal( null, source.LastException );
                Assert.Equal( DataStatus.Loaded, source.Status );
            }

            [Fact]
            public async Task SlowFailedEarlyFetchMoreDoesNotOverrideLaterOne()
            {
                var source = new IntDataSource( ( _, __ ) => Task.FromResult( Paginated( 1, new Optional<int>( 1 ) ) ) );
                await source.RefreshAsync();

                var taskSource = new TaskCompletionSource<PaginatedData<int, int>>();
                source.Fetch = ( _, t ) => taskSource.Task;

                // Call 1 begins
                var task = source.FetchMoreAsync();

                source.Fetch = ( _, __ ) => Task.FromResult( Paginated( 2, new Optional<int>( 2 ) ) );
                // Call 2 begins
                var task2 = source.FetchMoreAsync();

                // Call 1 completes
                taskSource.SetException( new MyException() );
                // Wait for both
                await task;
                await task2;

                Assert.Equal( new[] { 1, 2 }, source.Values );
                Assert.Equal( null, source.LastException );
                Assert.Equal( DataStatus.Loaded, source.Status );
            }

            [Fact]
            public async Task RefreshUsesDefaultPaginationToken()
            {
                Optional<int> paginationToken = default( Optional<int> );
                var source = new IntDataSource( ( pt, _ ) => { paginationToken = pt; return Task.FromResult( Paginated( 1 ) ); } );

                await source.RefreshAsync();

                Assert.Equal( default( Optional<int> ), paginationToken );
            }

            [Fact]
            public async Task FetchMoreUsesPreviouslyReturnedPaginationToken()
            {
                var source = new IntDataSource( ( _, __ ) => Task.FromResult( Paginated( 1, new Optional<int>( 1 ) ) ) );
                await source.RefreshAsync();

                Optional<int> paginationToken = default( Optional<int> );
                source.Fetch = ( pt, _ ) => { paginationToken = pt; return Task.FromResult( Paginated( 2 ) ); };

                await source.FetchMoreAsync();

                Assert.Equal( new Optional<int>( 1 ), paginationToken );
            }

            [Fact]
            public async Task RefreshAfterFetchMoreUsesDefaultPaginationToken()
            {
                var source = new IntDataSource( ( pt, __ ) => Task.FromResult( Paginated( 1, new Optional<int>( 1 ) ) ) );

                await source.RefreshAsync();
                await source.FetchMoreAsync();

                Optional<int> paginationToken = default( Optional<int> );
                source.Fetch = ( pt, __ ) => { paginationToken = pt; return Task.FromResult( Paginated( 2, new Optional<int>( 2 ) ) ); };

                await source.RefreshAsync();

                Assert.Equal( default( Optional<int> ), paginationToken );
            }
        }

        public sealed class WithTransform
        {
            private sealed class IntDataSource : PaginatedDataSource<int, int>
            {
                public Func<Optional<int>, CancellationToken, Task<PaginatedData<int, int>>> Fetch;
                public Func<IReadOnlyList<int>, bool, IEnumerable<int>> Transformer;


                public IntDataSource( Func<Optional<int>, CancellationToken, Task<PaginatedData<int, int>>> fetch,
                                      Func<IReadOnlyList<int>, bool, IEnumerable<int>> transformer )
                {
                    Fetch = fetch;
                    Transformer = transformer;
                }


                protected override Task<PaginatedData<int, int>> FetchAsync( Optional<int> paginationToken, CancellationToken cancellationToken )
                    => Fetch( paginationToken, cancellationToken );

                protected override IEnumerable<int> Transform( IReadOnlyList<int> values, bool isIncremental )
                    => Transformer( values, isIncremental );

                public new void UpdateValues()
                    => base.UpdateValues();
            }


            [Fact]
            public async Task SuccessfulTransform()
            {
                var source = new IntDataSource( ( _, __ ) => Task.FromResult( Paginated( 21 ) ), ( v, _ ) => v.Select( n => n * 2 ) );

                await source.RefreshAsync();

                Assert.Equal( new[] { 42 }, source.Values );
                Assert.Equal( null, source.LastException );
                Assert.Equal( DataStatus.Loaded, source.Status );
            }

            [Fact]
            public async Task FailedTransform()
            {
                var source = new IntDataSource( ( _, __ ) => Task.FromResult( Paginated( 42 ) ), ( _, __ ) => { throw new MyException(); } );

                await Assert.ThrowsAsync<MyException>( source.RefreshAsync );
            }

            [Fact]
            public async Task SuccessfulUpdatedTransform()
            {
                var source = new IntDataSource( ( _, __ ) => Task.FromResult( Paginated( 84 ) ), ( v, _ ) => v.Select( n => n * 2 ) );

                await source.RefreshAsync();

                source.Transformer = ( v, _ ) => v.Select( n => n / 2 );

                source.UpdateValues();

                Assert.Equal( new[] { 42 }, source.Values );
                Assert.Equal( null, source.LastException );
                Assert.Equal( DataStatus.Loaded, source.Status );
            }

            [Fact]
            public async Task FailedUpdatedTransform()
            {
                var source = new IntDataSource( ( _, __ ) => Task.FromResult( Paginated( 21 ) ), ( v, _ ) => v.Select( n => n * 2 ) );

                await source.RefreshAsync();

                source.Transformer = ( _, __ ) => { throw new MyException(); };

                Assert.Throws<MyException>( () => source.UpdateValues() );
            }

            [Fact]
            public async Task UpdateDoesNotFetchAgain()
            {
                int count = 0;
                var source = new IntDataSource( ( _, __ ) => { count++; return Task.FromResult( Paginated( 21 ) ); }, ( v, _ ) => v.Select( n => n * 2 ) );

                await source.RefreshAsync();

                source.UpdateValues();

                Assert.Equal( 1, count );
            }

            [Fact]
            public async Task SuccessfulTransformOnFetchMore()
            {
                var source = new IntDataSource( ( _, __ ) => Task.FromResult( Paginated( 1, new Optional<int>( 1 ) ) ),
                                                ( v, _ ) => v.Select( n => n * 2 ) );

                await source.RefreshAsync();

                source.Fetch = ( _, __ ) => Task.FromResult( Paginated( 2 ) );

                await source.FetchMoreAsync();

                Assert.Equal( new[] { 2, 4 }, source.Values );
                Assert.Equal( null, source.LastException );
                Assert.Equal( DataStatus.Loaded, source.Status );
            }

            [Fact]
            public async Task SuccessfulUpdatedTransformOnFetchMore()
            {
                var source = new IntDataSource(
                    ( _, __ ) => Task.FromResult( Paginated( 1, new Optional<int>( 1 ) ) ),
                    ( v, _ ) => v.Select( n => n * 2 )
                );

                await source.RefreshAsync();

                source.Fetch = ( _, __ ) => Task.FromResult( Paginated( 2 ) );

                await source.FetchMoreAsync();

                source.Transformer = ( v, _ ) => v.Select( n => n * 10 );

                source.UpdateValues();

                Assert.Equal( new[] { 10, 20 }, source.Values );
                Assert.Equal( null, source.LastException );
                Assert.Equal( DataStatus.Loaded, source.Status );
            }

            [Fact]
            public async Task TransformOnRefreshHasCorrectIncrementalFlag()
            {
                bool? isIncremental = null;
                var source = new IntDataSource(
                    ( _, __ ) => Task.FromResult( Paginated( 1 ) ),
                    ( v, ii ) => { isIncremental = ii; return v.Select( n => n * 2 ); }
                );

                await source.RefreshAsync();

                Assert.False( isIncremental );
            }

            [Fact]
            public async Task TransformOnFetchMoreHasCorrectIncrementalFlag()
            {
                var source = new IntDataSource(
                    ( _, __ ) => Task.FromResult( Paginated( 1, new Optional<int>( 1 ) ) ),
                    ( v, _ ) => v.Select( n => n * 2 )
                );

                await source.RefreshAsync();

                bool? isIncremental = null;
                source.Transformer = ( v, ii ) => { isIncremental = ii; return v.Select( n => n * 2 ); };

                await source.FetchMoreAsync();

                Assert.True( isIncremental );
            }

            [Fact]
            public void CannotUpdateValueBeforeRefreshing()
            {
                var source = new IntDataSource( ( _, __ ) => Task.FromResult( Paginated( 0 ) ), ( v, _ ) => v.Select( n => n + 1 ) );

                Assert.Throws<InvalidOperationException>( () => source.UpdateValues() );
            }
        }

        public sealed class WithCache
        {
            private sealed class IntDataSource : PaginatedDataSource<int, int>
            {
                public Func<Optional<int>, CancellationToken, Task<PaginatedData<int, int>>> Fetch;


                public IntDataSource( Func<Optional<int>, CancellationToken, Task<PaginatedData<int, int>>> fetch,
                                      Func<Optional<int>, CacheMetadata> metadataCreator )
                {
                    Fetch = fetch;
                    EnableCache( "X", new InMemoryDataStore(), metadataCreator );
                }


                protected override Task<PaginatedData<int, int>> FetchAsync( Optional<int> paginationToken, CancellationToken cancellationToken )
                    => Fetch( paginationToken, cancellationToken );

                public new void UpdateValues()
                    => base.UpdateValues();
            }

            [Fact]
            public async Task SuccessfulRefresh()
            {
                var source = new IntDataSource( ( _, __ ) => Task.FromResult( Paginated( 42 ) ), null );

                await source.RefreshAsync();

                Assert.Equal( new[] { 42 }, source.Values );
                Assert.Equal( null, source.LastException );
                Assert.Equal( DataStatus.Loaded, source.Status );
                Assert.Equal( CacheStatus.Unused, source.CacheStatus );
            }

            [Fact]
            public async Task FailedRefresh()
            {
                var ex = new MyException();
                var source = new IntDataSource( ( _, __ ) => TaskEx.FromException<PaginatedData<int, int>>( ex ), null );

                await source.RefreshAsync();

                Assert.Equal( null, source.Values );
                Assert.Equal( ex, source.LastException );
                Assert.Equal( DataStatus.NoData, source.Status );
                Assert.Equal( CacheStatus.Unused, source.CacheStatus );
            }

            [Fact]
            public async Task FailedRefreshAfterSuccessfulOne()
            {
                var source = new IntDataSource( ( _, __ ) => Task.FromResult( Paginated( 42 ) ), null );

                await source.RefreshAsync();

                var ex = new MyException();
                source.Fetch = ( _, __ ) => TaskEx.FromException<PaginatedData<int, int>>( ex );

                await source.RefreshAsync();

                Assert.Equal( new[] { 42 }, source.Values );
                Assert.Equal( ex, source.LastException );
                Assert.Equal( DataStatus.Loaded, source.Status );
                Assert.Equal( CacheStatus.Used, source.CacheStatus );
            }

            [Fact]
            public async Task RefreshesWithDifferentMetadataDoNotInterfere()
            {
                string id = "id";
                var source = new IntDataSource( ( _, __ ) => Task.FromResult( Paginated( 42 ) ), _ => new CacheMetadata( id, null ) );

                await source.RefreshAsync();

                var ex = new MyException();
                source.Fetch = ( _, __ ) => TaskEx.FromException<PaginatedData<int, int>>( ex );
                id = "id2";

                await source.RefreshAsync();

                Assert.Equal( null, source.Values );
                Assert.Equal( ex, source.LastException );
                Assert.Equal( DataStatus.NoData, source.Status );
                Assert.Equal( CacheStatus.Unused, source.CacheStatus );
            }

            [Fact]
            public async Task StaleDataIsNotUsed()
            {
                var source = new IntDataSource( ( _, __ ) => Task.FromResult( Paginated( 42 ) ), _ => new CacheMetadata( "", DateTimeOffset.MinValue ) );

                await source.RefreshAsync();

                var ex = new MyException();
                source.Fetch = ( _, __ ) => TaskEx.FromException<PaginatedData<int, int>>( ex );

                await source.RefreshAsync();

                Assert.Equal( null, source.Values );
                Assert.Equal( ex, source.LastException );
                Assert.Equal( DataStatus.NoData, source.Status );
                Assert.Equal( CacheStatus.Unused, source.CacheStatus );
            }

            [Fact]
            public async Task CacheIsNotUsedWhenMetadataIsNull()
            {
                var source = new IntDataSource( ( _, __ ) => Task.FromResult( Paginated( 42 ) ), _ => null );

                await source.RefreshAsync();

                var ex = new MyException();
                source.Fetch = ( _, __ ) => TaskEx.FromException<PaginatedData<int, int>>( ex );

                await source.RefreshAsync();

                Assert.Equal( null, source.Values );
                Assert.Equal( ex, source.LastException );
                Assert.Equal( DataStatus.NoData, source.Status );
                Assert.Equal( CacheStatus.Unused, source.CacheStatus );
            }

            [Fact]
            public async Task CacheIsUsedForFetchMore()
            {
                int counter = 0;
                var source = new IntDataSource(
                    ( _, __ ) => Task.FromResult( Paginated( counter, new Optional<int>( counter ) ) ),
                    pt => new CacheMetadata( pt.HasValue ? pt.Value.ToString() : "-", null )
                );

                await source.RefreshAsync();
                counter++;
                await source.FetchMoreAsync();

                var ex = new MyException();
                source.Fetch = ( _, __ ) => TaskEx.FromException<PaginatedData<int, int>>( ex );

                await source.RefreshAsync();
                await source.FetchMoreAsync();

                Assert.Equal( new[] { 0, 1 }, source.Values );
                Assert.Equal( ex, source.LastException );
                Assert.Equal( DataStatus.Loaded, source.Status );
                Assert.Equal( CacheStatus.Used, source.CacheStatus );
            }


            private sealed class DataSourceWithNullCacheStore : PaginatedDataSource<int, int>
            {
                public DataSourceWithNullCacheStore()
                {
                    EnableCache( "X", null );
                }

                protected override Task<PaginatedData<int, int>> FetchAsync( Optional<int> paginationToken, CancellationToken cancellationToken )
                {
                    return Task.FromResult( Paginated( 0 ) );
                }
            }

            [Fact]
            public void NullCacheStoreFails()
            {
                Assert.Throws<ArgumentNullException>( () => new DataSourceWithNullCacheStore() );
            }


            private sealed class DataSourceWithNullCacheId : PaginatedDataSource<int, int>
            {
                public DataSourceWithNullCacheId()
                {
                    EnableCache( null, new InMemoryDataStore() );
                }

                protected override Task<PaginatedData<int, int>> FetchAsync( Optional<int> paginationToken, CancellationToken cancellationToken )
                {
                    return Task.FromResult( Paginated( 0 ) );
                }
            }

            [Fact]
            public void NullCacheIdFails()
            {
                Assert.Throws<ArgumentNullException>( () => new DataSourceWithNullCacheId() );
            }


            private sealed class DataSourceEnablingCacheTwice : PaginatedDataSource<int, int>
            {
                public DataSourceEnablingCacheTwice()
                {
                    EnableCache( "X", new InMemoryDataStore() );
                    EnableCache( "X", new InMemoryDataStore() );
                }

                protected override Task<PaginatedData<int, int>> FetchAsync( Optional<int> paginationToken, CancellationToken cancellationToken )
                {
                    return Task.FromResult( Paginated( 0 ) );
                }
            }

            [Fact]
            public void EnablingCacheTwiceFails()
            {
                Assert.Throws<InvalidOperationException>( () => new DataSourceEnablingCacheTwice() );
            }
        }

        public sealed class WithTransformAndCache
        {

            private sealed class IntDataSource : PaginatedDataSource<int, int>
            {
                public Func<Optional<int>, CancellationToken, Task<PaginatedData<int, int>>> Fetch;
                public Func<IReadOnlyList<int>, bool, IEnumerable<int>> Transformer;


                public IntDataSource( Func<Optional<int>, CancellationToken, Task<PaginatedData<int, int>>> fetch,
                                      Func<IReadOnlyList<int>, bool, IEnumerable<int>> transformer,
                                      Func<Optional<int>, CacheMetadata> metadataCreator )
                {
                    Fetch = fetch;
                    Transformer = transformer;
                    EnableCache( "X", new InMemoryDataStore(), metadataCreator );
                }


                protected override Task<PaginatedData<int, int>> FetchAsync( Optional<int> paginationToken, CancellationToken cancellationToken )
                    => Fetch( paginationToken, cancellationToken );

                protected override IEnumerable<int> Transform( IReadOnlyList<int> values, bool isIncremental )
                    => Transformer( values, isIncremental );

                public new void UpdateValues()
                    => base.UpdateValues();
            }

            [Fact]
            public async Task FailedRefreshAfterSuccessfulOne()
            {
                var source = new IntDataSource( ( _, __ ) => Task.FromResult( Paginated( 21 ) ), ( v, _ ) => v.Select( n => n * 2 ), null );

                await source.RefreshAsync();

                var ex = new MyException();
                source.Fetch = ( _, __ ) => TaskEx.FromException<PaginatedData<int, int>>( ex );

                await source.RefreshAsync();

                Assert.Equal( new[] { 42 }, source.Values );
                Assert.Equal( ex, source.LastException );
                Assert.Equal( DataStatus.Loaded, source.Status );
                Assert.Equal( CacheStatus.Used, source.CacheStatus );
            }


            [Fact]
            public async Task FailedFetchMoreAfterSuccessfulOne()
            {
                int counter = 1;
                var source = new IntDataSource(
                    ( _, __ ) => Task.FromResult( Paginated( counter, new Optional<int>( counter ) ) ),
                    ( v, _ ) => v.Select( n => n * 2 ),
                    pt => new CacheMetadata( pt.HasValue ? pt.Value.ToString() : "-", null )
                );

                await source.RefreshAsync();
                counter++;
                await source.FetchMoreAsync();

                var ex = new MyException();
                source.Fetch = ( _, __ ) => TaskEx.FromException<PaginatedData<int, int>>( ex );

                await source.RefreshAsync();
                await source.FetchMoreAsync();

                Assert.Equal( new[] { 2, 4 }, source.Values );
                Assert.Equal( ex, source.LastException );
                Assert.Equal( DataStatus.Loaded, source.Status );
                Assert.Equal( CacheStatus.Used, source.CacheStatus );
            }
        }
    }
}