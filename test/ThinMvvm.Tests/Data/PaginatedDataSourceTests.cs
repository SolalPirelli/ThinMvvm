using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ThinMvvm.Data.Infrastructure;
using ThinMvvm.Tests.TestInfrastructure;
using Xunit;

namespace ThinMvvm.Data.Tests
{
    public sealed class PaginatedDataSourceTests
    {
        private static PaginatedData<int, int> Paginated( int value, Optional<int> token = default( Optional<int> ) )
        {
            return new PaginatedData<int, int>( value, token );
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

                Assert.Null( source.Data );
                Assert.Equal( DataSourceStatus.None, source.Status );
                Assert.False( source.CanFetchMore );

                Assert.Null( ( (IDataSource) source ).Data );
            }

            [Fact]
            public async Task RefreshInProgress()
            {
                var taskSource = new TaskCompletionSource<PaginatedData<int, int>>();
                var source = new IntDataSource( ( _, __ ) => taskSource.Task );

                var hits = new List<string>();
                source.PropertyChanged += ( _, e ) =>
                {
                    hits.Add( e.PropertyName );

                    if( e.PropertyName == nameof( IDataSource.Status ) && hits.Count == 0 )
                    {
                        Assert.Null( source.Data );
                        Assert.Equal( DataSourceStatus.Loading, source.Status );
                        Assert.False( source.CanFetchMore );

                        Assert.Null( ( (IDataSource) source ).Data );
                    }
                };

                var task = source.RefreshAsync();

                Assert.Equal( new[] { nameof( IDataSource.Status ) }, hits );

                taskSource.SetResult( Paginated( 0 ) );
                await task;
            }

            [Fact]
            public async Task SuccessfulRefresh()
            {
                var taskSource = new TaskCompletionSource<PaginatedData<int, int>>();
                var source = new IntDataSource( ( _, __ ) => taskSource.Task );

                var task = source.RefreshAsync();

                var hits = new List<string>();
                source.PropertyChanged += ( _, e ) =>
                {
                    hits.Add( e.PropertyName );

                    if( e.PropertyName == nameof( IDataSource.Status ) )
                    {
                        Assert.Equal( new[] { new DataChunk<int>( 42, DataStatus.Normal, default( DataErrors ) ) }, source.Data );
                        Assert.Equal( DataSourceStatus.Loaded, source.Status );
                        Assert.False( source.CanFetchMore );

                        Assert.Equal( source.Data, ( (IDataSource) source ).Data );
                    }
                };

                taskSource.SetResult( Paginated( 42 ) );
                await task;

                Assert.Equal( 3, hits.Count );
                Assert.Equal( nameof( IDataSource.Status ), hits.Last() );
                Assert.Contains( nameof( IDataSource.Data ), hits );
                Assert.Contains( nameof( IDataSource.CanFetchMore ), hits );
            }

            [Fact]
            public async Task FailedRefresh()
            {
                var ex = new MyException();
                var taskSource = new TaskCompletionSource<PaginatedData<int, int>>();
                var source = new IntDataSource( ( _, __ ) => taskSource.Task );

                var task = source.RefreshAsync();

                var hits = new List<string>();
                source.PropertyChanged += ( _, e ) =>
                {
                    hits.Add( e.PropertyName );

                    if( e.PropertyName == nameof( IDataSource.Status ) )
                    {
                        Assert.Equal( new[] { new DataChunk<int>( 0, DataStatus.Error, new DataErrors( ex, null, null ) ) },
                                      source.Data );
                        Assert.Equal( DataSourceStatus.Loaded, source.Status );
                        Assert.False( source.CanFetchMore );

                        Assert.Equal( source.Data, ( (IDataSource) source ).Data );
                    }
                };

                taskSource.SetException( ex );
                await task;

                Assert.Equal( 3, hits.Count );
                Assert.Equal( nameof( IDataSource.Status ), hits.Last() );
                Assert.Contains( nameof( IDataSource.Data ), hits );
                Assert.Contains( nameof( IDataSource.CanFetchMore ), hits );
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

                Assert.Equal( new[] { new DataChunk<int>( 42, DataStatus.Normal, default( DataErrors ) ) },
                              source.Data );
                Assert.Equal( DataSourceStatus.Loaded, source.Status );
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

                Assert.Equal( new[] { new DataChunk<int>( 42, DataStatus.Normal, default( DataErrors ) ) },
                              source.Data );
                Assert.Equal( DataSourceStatus.Loaded, source.Status );
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
                await Assert.ThrowsAsync<InvalidOperationException>( source.FetchMoreAsync );
            }

            [Fact]
            public async Task CannotCallFetchMoreAfterRefreshWithError()
            {
                var source = new IntDataSource( ( _, __ ) => TaskEx.FromException<PaginatedData<int, int>>( new MyException() ) );
                await source.RefreshAsync();

                Assert.False( source.CanFetchMore );
                await Assert.ThrowsAsync<InvalidOperationException>( source.FetchMoreAsync );
            }

            [Fact]
            public async Task FetchMoreInProgress()
            {
                var source = new IntDataSource( ( _, __ ) => Task.FromResult( Paginated( 42, new Optional<int>( 1 ) ) ) );
                await source.RefreshAsync();

                var taskSource = new TaskCompletionSource<PaginatedData<int, int>>();
                source.Fetch = ( _, __ ) => taskSource.Task;

                var hits = new List<string>();
                source.PropertyChanged += ( _, e ) =>
                {
                    hits.Add( e.PropertyName );

                    if( e.PropertyName == nameof( IntDataSource.Status ) && hits.Count == 0 )
                    {
                        Assert.Equal( new[] { new DataChunk<int>( 42, DataStatus.Normal, default( DataErrors ) ) },
                                      source.Data );
                        Assert.Equal( DataSourceStatus.LoadingMore, source.Status );
                        Assert.True( source.CanFetchMore );

                        Assert.Equal( source.Data, ( (IDataSource) source ).Data );
                    }
                };

                var task = source.FetchMoreAsync();

                Assert.Equal( new[] { nameof( IDataSource.Status ) }, hits );

                taskSource.SetResult( Paginated( 0 ) );
                await task;
            }

            [Fact]
            public async Task SuccessfulFetchMore()
            {
                var source = new IntDataSource( ( _, __ ) => Task.FromResult( Paginated( 42, new Optional<int>( 1 ) ) ) );
                await source.RefreshAsync();

                var taskSource = new TaskCompletionSource<PaginatedData<int, int>>();
                source.Fetch = ( _, __ ) => taskSource.Task;

                var task = source.FetchMoreAsync();

                var hits = new List<string>();
                source.PropertyChanged += ( _, e ) =>
                {
                    hits.Add( e.PropertyName );

                    if( e.PropertyName == nameof( IDataSource.Status ) )
                    {
                        Assert.Equal( new[] { new DataChunk<int>( 42, DataStatus.Normal, default( DataErrors ) ),
                                              new DataChunk<int>( 100, DataStatus.Normal, default( DataErrors ) ) },
                                      source.Data );
                        Assert.Equal( DataSourceStatus.Loaded, source.Status );
                        Assert.False( source.CanFetchMore );

                        Assert.Equal( source.Data, ( (IDataSource) source ).Data );
                    }
                };

                taskSource.SetResult( Paginated( 100 ) );
                await task;

                Assert.Equal( 2, hits.Count );
                Assert.Equal( nameof( IDataSource.Status ), hits.Last() );
                Assert.Contains( nameof( IDataSource.CanFetchMore ), hits );
            }

            [Fact]
            public async Task FailedFetchMore()
            {
                var source = new IntDataSource( ( _, __ ) => Task.FromResult( Paginated( 42, new Optional<int>( 1 ) ) ) );
                await source.RefreshAsync();

                var ex = new MyException();
                var taskSource = new TaskCompletionSource<PaginatedData<int, int>>();
                source.Fetch = ( _, __ ) => taskSource.Task;

                var task = source.FetchMoreAsync();

                var hits = new List<string>();
                source.PropertyChanged += ( _, e ) =>
                {
                    hits.Add( e.PropertyName );

                    if( e.PropertyName == nameof( IDataSource.Status ) )
                    {
                        Assert.Equal( new[] { new DataChunk<int>( 42, DataStatus.Normal, default( DataErrors ) ),
                                              new DataChunk<int>( 0, DataStatus.Error, new DataErrors( ex, null, null ) ) },
                                      source.Data );

                        Assert.Equal( DataSourceStatus.Loaded, source.Status );
                        Assert.False( source.CanFetchMore );

                        Assert.Equal( source.Data, ( (IDataSource) source ).Data );
                    }
                };

                taskSource.SetException( ex );
                await task;

                Assert.Equal( 2, hits.Count );
                Assert.Equal( nameof( IDataSource.Status ), hits.Last() );
                Assert.Contains( nameof( IDataSource.CanFetchMore ), hits );
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
                var source = new IntDataSource( ( _, __ ) => Task.FromResult( Paginated( 10, new Optional<int>( 1 ) ) ) );
                await source.RefreshAsync();

                var taskSource = new TaskCompletionSource<PaginatedData<int, int>>();
                source.Fetch = ( _, t ) => taskSource.Task;

                // Call 1 begins
                var task = source.FetchMoreAsync();

                source.Fetch = ( _, __ ) => Task.FromResult( Paginated( 20 ) );
                // Call 2 begins
                var task2 = source.FetchMoreAsync();

                // Call 1 completes
                taskSource.SetResult( Paginated( 30, new Optional<int>( 2 ) ) );
                // Wait for both
                await task;
                await task2;

                Assert.Equal( new[] { new DataChunk<int>( 10, DataStatus.Normal, default( DataErrors ) ),
                                      new DataChunk<int>( 20, DataStatus.Normal, default( DataErrors ) ) },
                              source.Data );
                Assert.Equal( DataSourceStatus.Loaded, source.Status );
            }

            [Fact]
            public async Task SlowFailedEarlyFetchMoreDoesNotOverrideLaterOne()
            {
                var source = new IntDataSource( ( _, __ ) => Task.FromResult( Paginated( 10, new Optional<int>( 1 ) ) ) );
                await source.RefreshAsync();

                var taskSource = new TaskCompletionSource<PaginatedData<int, int>>();
                source.Fetch = ( _, t ) => taskSource.Task;

                // Call 1 begins
                var task = source.FetchMoreAsync();

                source.Fetch = ( _, __ ) => Task.FromResult( Paginated( 20, new Optional<int>( 2 ) ) );
                // Call 2 begins
                var task2 = source.FetchMoreAsync();

                // Call 1 completes
                taskSource.SetException( new MyException() );
                // Wait for both
                await task;
                await task2;

                Assert.Equal( new[] { new DataChunk<int>( 10, DataStatus.Normal, default( DataErrors ) ),
                                      new DataChunk<int>( 20, DataStatus.Normal, default( DataErrors ) ) },
                              source.Data );
                Assert.Equal( DataSourceStatus.Loaded, source.Status );
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
                public Func<int, bool, int> Transformer;


                public IntDataSource( Func<Optional<int>, CancellationToken, Task<PaginatedData<int, int>>> fetch,
                                      Func<int, bool, int> transformer )
                {
                    Fetch = fetch;
                    Transformer = transformer;
                }


                public new void UpdateValues()
                    => base.UpdateValues();

                protected override Task<PaginatedData<int, int>> FetchAsync( Optional<int> paginationToken, CancellationToken cancellationToken )
                    => Fetch( paginationToken, cancellationToken );

                protected override int Transform( int data, bool isIncremental )
                    => Transformer( data, isIncremental );
            }


            [Fact]
            public async Task SuccessfulTransform()
            {
                var source = new IntDataSource( ( _, __ ) => Task.FromResult( Paginated( 21 ) ), ( n, _ ) => n * 2 );

                await source.RefreshAsync();

                Assert.Equal( new[] { new DataChunk<int>( 42, DataStatus.Normal, default( DataErrors ) ) }, source.Data );
                Assert.Equal( DataSourceStatus.Loaded, source.Status );
            }

            [Fact]
            public async Task FailedTransform()
            {
                var ex = new MyException();
                var source = new IntDataSource( ( _, __ ) => Task.FromResult( Paginated( 42 ) ), ( _, __ ) => { throw ex; } );

                await source.RefreshAsync();

                Assert.Equal( new[] { new DataChunk<int>( 0, DataStatus.Error, new DataErrors( null, null, ex ) ) }, source.Data );
                Assert.Equal( DataSourceStatus.Loaded, source.Status );
            }

            [Fact]
            public async Task SuccessfulUpdatedTransform()
            {
                var source = new IntDataSource( ( _, __ ) => Task.FromResult( Paginated( 84 ) ), ( n, _ ) => n * 2 );

                await source.RefreshAsync();

                source.Transformer = ( n, _ ) => n / 2;

                source.UpdateValues();

                Assert.Equal( new[] { new DataChunk<int>( 42, DataStatus.Normal, default( DataErrors ) ) }, source.Data );
                Assert.Equal( DataSourceStatus.Loaded, source.Status );
            }

            [Fact]
            public async Task FailedUpdatedTransform()
            {
                var source = new IntDataSource( ( _, __ ) => Task.FromResult( Paginated( 21 ) ), ( n, _ ) => n * 2 );

                await source.RefreshAsync();

                var ex = new MyException();
                source.Transformer = ( _, __ ) => { throw ex; };

                source.UpdateValues();

                Assert.Equal( new[] { new DataChunk<int>( 0, DataStatus.Error, new DataErrors( null, null, ex ) ) }, source.Data );
                Assert.Equal( DataSourceStatus.Loaded, source.Status );
            }

            [Fact]
            public async Task UpdateDoesNotFetchAgain()
            {
                int count = 0;
                var source = new IntDataSource( ( _, __ ) => { count++; return Task.FromResult( Paginated( 21 ) ); }, ( n, _ ) => n * 2 );

                await source.RefreshAsync();

                source.UpdateValues();

                Assert.Equal( 1, count );
            }

            [Fact]
            public async Task SuccessfulTransformOnFetchMore()
            {
                var source = new IntDataSource( ( _, __ ) => Task.FromResult( Paginated( 1, new Optional<int>( 1 ) ) ), ( n, _ ) => n * 2 );

                await source.RefreshAsync();

                source.Fetch = ( _, __ ) => Task.FromResult( Paginated( 2 ) );

                await source.FetchMoreAsync();

                Assert.Equal( new[] { new DataChunk<int>( 2, DataStatus.Normal, default( DataErrors ) ),
                                      new DataChunk<int>( 4, DataStatus.Normal, default( DataErrors ) ) },
                              source.Data );
                Assert.Equal( DataSourceStatus.Loaded, source.Status );
            }

            [Fact]
            public async Task SuccessfulUpdatedTransformOnFetchMore()
            {
                var source = new IntDataSource( ( _, __ ) => Task.FromResult( Paginated( 1, new Optional<int>( 0 ) ) ), ( n, _ ) => n * 2 );

                await source.RefreshAsync();

                source.Fetch = ( _, __ ) => Task.FromResult( Paginated( 2 ) );

                await source.FetchMoreAsync();

                source.Transformer = ( n, _ ) => n * 10;

                source.UpdateValues();

                Assert.Equal( new[] { new DataChunk<int>( 10, DataStatus.Normal, default( DataErrors ) ),
                                      new DataChunk<int>( 20, DataStatus.Normal, default( DataErrors ) ) },
                              source.Data );
                Assert.Equal( DataSourceStatus.Loaded, source.Status );
            }

            [Fact]
            public async Task TransformOnRefreshHasCorrectIncrementalFlag()
            {
                bool? isIncremental = null;
                var source = new IntDataSource(
                    ( _, __ ) => Task.FromResult( Paginated( 1 ) ),
                    ( _, ii ) => { isIncremental = ii; return 2; }
                );

                await source.RefreshAsync();

                Assert.False( isIncremental );
            }

            [Fact]
            public async Task TransformOnFetchMoreHasCorrectIncrementalFlag()
            {
                var source = new IntDataSource(
                    ( _, __ ) => Task.FromResult( Paginated( 1, new Optional<int>( 1 ) ) ),
                    ( n, _ ) => n * 2
                );

                await source.RefreshAsync();

                bool? isIncremental = null;
                source.Transformer = ( _, ii ) => { isIncremental = ii; return 2; };

                await source.FetchMoreAsync();

                Assert.True( isIncremental );
            }

            [Fact]
            public void CannotUpdateValueBeforeRefreshing()
            {
                var source = new IntDataSource( ( _, __ ) => Task.FromResult( Paginated( 0 ) ), ( n, _ ) => n );

                Assert.Throws<InvalidOperationException>( () => source.UpdateValues() );
            }
        }

        public sealed class WithCache
        {
            private sealed class IntDataSource : PaginatedDataSource<int, int>
            {
                public Func<Optional<int>, CancellationToken, Task<PaginatedData<int, int>>> Fetch;


                public IntDataSource( Func<Optional<int>, CancellationToken, Task<PaginatedData<int, int>>> fetch,
                                      Func<Optional<int>, CacheMetadata> metadataCreator,
                                      IDataStore dataStore = null )
                {
                    Fetch = fetch;
                    EnableCache( "X", dataStore ?? new InMemoryDataStore(), metadataCreator );
                }


                public new void UpdateValues()
                    => base.UpdateValues();

                protected override Task<PaginatedData<int, int>> FetchAsync( Optional<int> paginationToken, CancellationToken cancellationToken )
                    => Fetch( paginationToken, cancellationToken );
            }

            [Fact]
            public async Task SuccessfulRefresh()
            {
                var source = new IntDataSource( ( _, __ ) => Task.FromResult( Paginated( 42 ) ), null );

                await source.RefreshAsync();

                Assert.Equal( new[] { new DataChunk<int>( 42, DataStatus.Normal, default( DataErrors ) ) }, source.Data );
                Assert.Equal( DataSourceStatus.Loaded, source.Status );
            }

            [Fact]
            public async Task FailedRefresh()
            {
                var ex = new MyException();
                var source = new IntDataSource( ( _, __ ) => TaskEx.FromException<PaginatedData<int, int>>( ex ), null );

                await source.RefreshAsync();

                Assert.Equal( new[] { new DataChunk<int>( 0, DataStatus.Error, new DataErrors( ex, null, null ) ) }, source.Data );
                Assert.Equal( DataSourceStatus.Loaded, source.Status );
            }

            [Fact]
            public async Task FailedRefreshAfterSuccessfulOne()
            {
                var source = new IntDataSource( ( _, __ ) => Task.FromResult( Paginated( 42 ) ), null );

                await source.RefreshAsync();

                var ex = new MyException();
                source.Fetch = ( _, __ ) => TaskEx.FromException<PaginatedData<int, int>>( ex );

                await source.RefreshAsync();

                Assert.Equal( new[] { new DataChunk<int>( 42, DataStatus.Cached, new DataErrors( ex, null, null ) ) }, source.Data );
                Assert.Equal( DataSourceStatus.Loaded, source.Status );
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

                Assert.Equal( new[] { new DataChunk<int>( 0, DataStatus.Error, new DataErrors( ex, null, null ) ) }, source.Data );
                Assert.Equal( DataSourceStatus.Loaded, source.Status );
            }

            [Fact]
            public async Task StaleDataIsNotUsed()
            {
                var source = new IntDataSource( ( _, __ ) => Task.FromResult( Paginated( 42 ) ), _ => new CacheMetadata( "", DateTimeOffset.MinValue ) );

                await source.RefreshAsync();

                var ex = new MyException();
                source.Fetch = ( _, __ ) => TaskEx.FromException<PaginatedData<int, int>>( ex );

                await source.RefreshAsync();

                Assert.Equal( new[] { new DataChunk<int>( 0, DataStatus.Error, new DataErrors( ex, null, null ) ) }, source.Data );
                Assert.Equal( DataSourceStatus.Loaded, source.Status );
            }

            [Fact]
            public async Task CacheIsNotUsedWhenMetadataIsNull()
            {
                var source = new IntDataSource( ( _, __ ) => Task.FromResult( Paginated( 42 ) ), _ => null );

                await source.RefreshAsync();

                var ex = new MyException();
                source.Fetch = ( _, __ ) => TaskEx.FromException<PaginatedData<int, int>>( ex );

                await source.RefreshAsync();

                Assert.Equal( new[] { new DataChunk<int>( 0, DataStatus.Error, new DataErrors( ex, null, null ) ) }, source.Data );
                Assert.Equal( DataSourceStatus.Loaded, source.Status );
            }

            [Fact]
            public async Task CacheIsUsedForFetchMore()
            {
                int counter = 1;
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

                Assert.Equal( new[] { new DataChunk<int>( 1, DataStatus.Cached, new DataErrors( ex, null, null ) ),
                                      new DataChunk<int>( 2, DataStatus.Cached, new DataErrors( ex, null, null ) ) },
                              source.Data );
                Assert.Equal( DataSourceStatus.Loaded, source.Status );
            }

            [Fact]
            public async Task SuccessfulRefreshButMetadataCreatorThrows()
            {
                var ex = new MyException();
                var source = new IntDataSource( ( _, __ ) => Task.FromResult( Paginated( 42 ) ), _ => { throw ex; } );

                await source.RefreshAsync();

                Assert.Equal( new[] { new DataChunk<int>( 42, DataStatus.Normal, new DataErrors( null, ex, null ) ) }, source.Data );
                Assert.Equal( DataSourceStatus.Loaded, source.Status );
            }

            [Fact]
            public async Task FailedRefreshAfterSuccessfulOneButMetadataCreatorThrows()
            {
                bool shouldThrow = false;
                var cacheEx = new MyException();
                var source = new IntDataSource( ( _, __ ) => Task.FromResult( Paginated( 42 ) ), _ =>
                {
                    if( shouldThrow )
                    {
                        throw cacheEx;
                    }

                    return CacheMetadata.Default;
                } );

                await source.RefreshAsync();

                var fetchEx = new MyException();
                source.Fetch = ( _, __ ) => TaskEx.FromException<PaginatedData<int, int>>( fetchEx );
                shouldThrow = true;

                await source.RefreshAsync();

                Assert.Equal( new[] { new DataChunk<int>( 0, DataStatus.Error, new DataErrors( fetchEx, cacheEx, null ) ) }, source.Data );
                Assert.Equal( DataSourceStatus.Loaded, source.Status );
            }


            private sealed class DataStoreFailingToRead : IDataStore
            {
                public readonly Exception Exception = new MyException();


                public Task<Optional<T>> LoadAsync<T>( string id )
                {
                    return TaskEx.FromException<Optional<T>>( Exception );
                }

                public Task StoreAsync<T>( string id, T data )
                {
                    return TaskEx.CompletedTask;
                }

                public Task DeleteAsync( string id )
                {
                    throw new NotSupportedException();
                }
            }

            [Fact]
            public async Task CacheFailsToRead()
            {
                var store = new DataStoreFailingToRead();
                var source = new IntDataSource( ( _, __ ) => Task.FromResult( Paginated( 42 ) ), null, store );

                await source.RefreshAsync();

                var ex = new MyException();
                source.Fetch = ( _, __ ) => TaskEx.FromException<PaginatedData<int, int>>( ex );

                await source.RefreshAsync();

                Assert.Equal( new[] { new DataChunk<int>( 0, DataStatus.Error, new DataErrors( ex, store.Exception, null ) ) }, source.Data );
                Assert.Equal( DataSourceStatus.Loaded, source.Status );
            }


            private sealed class DataStoreFailingToWrite : IDataStore
            {
                public readonly Exception Exception = new MyException();


                public Task<Optional<T>> LoadAsync<T>( string id )
                {
                    return Task.FromResult( default( Optional<T> ) );
                }

                public Task StoreAsync<T>( string id, T data )
                {
                    return TaskEx.FromException<T>( Exception );
                }

                public Task DeleteAsync( string id )
                {
                    throw new NotSupportedException();
                }
            }

            [Fact]
            public async Task CacheFailsToWrite()
            {
                var store = new DataStoreFailingToWrite();
                var source = new IntDataSource( ( _, __ ) => Task.FromResult( Paginated( 42 ) ), null, store );

                await source.RefreshAsync();

                Assert.Equal( new[] { new DataChunk<int>( 42, DataStatus.Normal, new DataErrors( null, store.Exception, null ) ) }, source.Data );
                Assert.Equal( DataSourceStatus.Loaded, source.Status );
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
                public Func<int, bool, int> Transformer;


                public IntDataSource( Func<Optional<int>, CancellationToken, Task<PaginatedData<int, int>>> fetch,
                                      Func<int, bool, int> transformer,
                                      Func<Optional<int>, CacheMetadata> metadataCreator )
                {
                    Fetch = fetch;
                    Transformer = transformer;
                    EnableCache( "X", new InMemoryDataStore(), metadataCreator );
                }


                protected override Task<PaginatedData<int, int>> FetchAsync( Optional<int> paginationToken, CancellationToken cancellationToken )
                    => Fetch( paginationToken, cancellationToken );

                protected override int Transform( int data, bool isIncremental )
                    => Transformer( data, isIncremental );

                public new void UpdateValues()
                    => base.UpdateValues();
            }

            [Fact]
            public async Task FailedRefreshAfterSuccessfulOne()
            {
                var source = new IntDataSource( ( _, __ ) => Task.FromResult( Paginated( 21 ) ), ( n, _ ) => n * 2, null );

                await source.RefreshAsync();

                var ex = new MyException();
                source.Fetch = ( _, __ ) => TaskEx.FromException<PaginatedData<int, int>>( ex );

                await source.RefreshAsync();

                Assert.Equal( new[] { new DataChunk<int>( 42, DataStatus.Cached, new DataErrors( ex, null, null ) ) }, source.Data );
                Assert.Equal( DataSourceStatus.Loaded, source.Status );
            }


            [Fact]
            public async Task FailedFetchMoreAfterSuccessfulOne()
            {
                int counter = 1;
                var source = new IntDataSource(
                    ( _, __ ) => Task.FromResult( Paginated( counter, new Optional<int>( counter ) ) ),
                    ( n, _ ) => n * 2,
                    pt => new CacheMetadata( pt.HasValue ? pt.Value.ToString() : "-", null )
                );

                await source.RefreshAsync();
                counter++;
                await source.FetchMoreAsync();

                var ex = new MyException();
                source.Fetch = ( _, __ ) => TaskEx.FromException<PaginatedData<int, int>>( ex );

                await source.RefreshAsync();
                await source.FetchMoreAsync();

                Assert.Equal( new[] { new DataChunk<int>( 2, DataStatus.Cached, new DataErrors( ex, null, null ) ),
                                      new DataChunk<int>( 4, DataStatus.Cached, new DataErrors( ex, null, null ) ) },
                              source.Data );
                Assert.Equal( DataSourceStatus.Loaded, source.Status );
            }
        }
    }
}