using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ThinMvvm.Data.Infrastructure;
using ThinMvvm.Tests.TestInfrastructure;
using Xunit;

namespace ThinMvvm.Data.Tests
{
    public sealed class DataSourceTests
    {
        public sealed class Basic
        {
            private sealed class IntDataSource : DataSource<int>
            {
                public Func<CancellationToken, Task<int>> Fetch;


                public IntDataSource( Func<CancellationToken, Task<int>> fetch )
                {
                    Fetch = fetch;
                }


                protected override Task<int> FetchAsync( CancellationToken cancellationToken )
                    => Fetch( cancellationToken );
            }

            [Fact]
            public void InitialState()
            {
                var source = new IntDataSource( _ => Task.FromResult( 0 ) );

                Assert.Null( source.Data );
                Assert.Equal( DataSourceStatus.None, source.Status );

                Assert.Null( ( (IDataSource) source ).Data );
                Assert.False( ( (IDataSource) source ).CanFetchMore );
            }

            [Fact]
            public async Task RefreshInProgress()
            {
                var taskSource = new TaskCompletionSource<int>();
                var source = new IntDataSource( _ => taskSource.Task );

                var hits = new List<string>();
                source.PropertyChanged += ( _, e ) =>
                {
                    hits.Add( e.PropertyName );

                    if( e.PropertyName == nameof( IDataSource.Status ) && hits.Count == 0 )
                    {
                        Assert.Null( source.Data );
                        Assert.Equal( DataSourceStatus.Loading, source.Status );

                        Assert.Null( ( (IDataSource) source ).Data );
                        Assert.False( ( (IDataSource) source ).CanFetchMore );
                    }
                };

                var task = source.RefreshAsync();

                Assert.Equal( new[] { nameof( IDataSource.Status ) }, hits );

                taskSource.SetResult( 0 );
                await task;
            }

            [Fact]
            public async Task SuccessfulRefresh()
            {
                var taskSource = new TaskCompletionSource<int>();
                var source = new IntDataSource( _ => taskSource.Task );

                var task = source.RefreshAsync();

                var hits = new List<string>();
                source.PropertyChanged += ( _, e ) =>
                {
                    hits.Add( e.PropertyName );

                    if( e.PropertyName == nameof( IntDataSource.Status ) )
                    {
                        Assert.Equal( new DataChunk<int>( 42, DataStatus.Normal, default( DataErrors ) ), source.Data );
                        Assert.Equal( DataSourceStatus.Loaded, source.Status );

                        Assert.Equal<IDataChunk>( new[] { source.Data }, ( (IDataSource) source ).Data );
                        Assert.False( ( (IDataSource) source ).CanFetchMore );
                    }
                };

                taskSource.SetResult( 42 );
                await task;

                Assert.Equal( new[] { nameof( IDataSource.Data ), nameof( IDataSource.Status ) }, hits );
            }

            [Fact]
            public async Task FailedRefresh()
            {
                var ex = new MyException();
                var taskSource = new TaskCompletionSource<int>();
                var source = new IntDataSource( _ => taskSource.Task );

                var task = source.RefreshAsync();

                var hits = new List<string>();
                source.PropertyChanged += ( _, e ) =>
                {
                    hits.Add( e.PropertyName );

                    if( e.PropertyName == nameof( IntDataSource.Status ) )
                    {
                        Assert.Equal( new DataChunk<int>( 0, DataStatus.Error, new DataErrors( ex, null, null ) ), source.Data );
                        Assert.Equal( DataSourceStatus.Loaded, source.Status );

                        Assert.Equal<IDataChunk>( new[] { source.Data }, ( (IDataSource) source ).Data );
                        Assert.False( ( (IDataSource) source ).CanFetchMore );
                    }
                };

                taskSource.SetException( ex );
                await task;

                Assert.Equal( new[] { nameof( IDataSource.Data ), nameof( IDataSource.Status ) }, hits );
            }

            [Fact]
            public async Task RefreshCancelsOlderCall()
            {
                var tokens = new List<CancellationToken>();
                var source = new IntDataSource( t => { tokens.Add( t ); return Task.FromResult( 42 ); } );

                await source.RefreshAsync();
                await source.RefreshAsync();

                Assert.Equal( 2, tokens.Count );
                Assert.True( tokens[0].IsCancellationRequested );
                Assert.False( tokens[1].IsCancellationRequested );
            }

            [Fact]
            public async Task SlowEarlyRefreshDoesNotOverrideLaterOne()
            {
                var taskSource = new TaskCompletionSource<int>();
                var source = new IntDataSource( _ => taskSource.Task );

                // Call 1 begins
                var task = source.RefreshAsync();

                source.Fetch = _ => Task.FromResult( 42 );
                // Call 2 begins
                var task2 = source.RefreshAsync();

                // Call 1 completes
                taskSource.SetResult( 100 );
                // Wait for both
                await task;
                await task2;

                Assert.Equal( new DataChunk<int>( 42, DataStatus.Normal, default( DataErrors ) ), source.Data );
                Assert.Equal( DataSourceStatus.Loaded, source.Status );
            }

            [Fact]
            public async Task SlowFailedEarlyRefreshDoesNotOverrideLaterOne()
            {
                var taskSource = new TaskCompletionSource<int>();
                var source = new IntDataSource( _ => taskSource.Task );

                // Call 1 begins
                var task = source.RefreshAsync();

                source.Fetch = _ => Task.FromResult( 42 );
                // Call 2 begins
                var task2 = source.RefreshAsync();

                // Call 1 fails
                taskSource.SetException( new Exception() );
                // Wait for both
                await task;
                await task2;

                Assert.Equal( new DataChunk<int>( 42, DataStatus.Normal, default( DataErrors ) ), source.Data );
                Assert.Equal( DataSourceStatus.Loaded, source.Status );
            }

            [Fact]
            public async Task FetchMoreThrows()
            {
                var source = new IntDataSource( _ => Task.FromResult( 0 ) );

                await source.RefreshAsync();

                await Assert.ThrowsAsync<NotSupportedException>( ( (IDataSource) source ).FetchMoreAsync );
            }
        }

        public sealed class WithTransform
        {
            private sealed class IntDataSource : DataSource<int>
            {
                public Func<CancellationToken, Task<int>> Fetch;
                public Func<int, Task<int>> Transformer;


                public IntDataSource( Func<CancellationToken, Task<int>> fetch, Func<int, Task<int>> transformer )
                {
                    Fetch = fetch;
                    Transformer = transformer;
                }


                public new Task UpdateValueAsync()
                    => base.UpdateValueAsync();

                protected override Task<int> FetchAsync( CancellationToken cancellationToken )
                    => Fetch( cancellationToken );

                protected override Task<int> TransformAsync( int value )
                    => Transformer( value );
            }

            [Fact]
            public async Task CannotUpdateValueBeforeRefreshing()
            {
                var source = new IntDataSource( _ => Task.FromResult( 0 ), n => Task.FromResult( n + 1 ) );

                await Assert.ThrowsAsync<InvalidOperationException>( () => source.UpdateValueAsync() );
            }

            [Fact]
            public async Task SuccessfulTransform()
            {
                var source = new IntDataSource( _ => Task.FromResult( 21 ), n => Task.FromResult( n * 2 ) );

                await source.RefreshAsync();

                Assert.Equal( new DataChunk<int>( 42, DataStatus.Normal, default( DataErrors ) ), source.Data );
                Assert.Equal( DataSourceStatus.Loaded, source.Status );
            }

            [Fact]
            public async Task FailedTransform()
            {
                var ex = new MyException();
                var source = new IntDataSource( _ => Task.FromResult( 42 ), _ => { throw ex; } );

                await source.RefreshAsync();

                Assert.Equal( new DataChunk<int>( 0, DataStatus.Error, new DataErrors( null, null, ex ) ), source.Data );
                Assert.Equal( DataSourceStatus.Loaded, source.Status );
            }

            [Fact]
            public async Task SuccessfulUpdatedTransform()
            {
                var source = new IntDataSource( _ => Task.FromResult( 84 ), n => Task.FromResult( n ) );

                await source.RefreshAsync();

                source.Transformer = n => Task.FromResult( n / 2 );

                await source.UpdateValueAsync();

                Assert.Equal( new DataChunk<int>( 42, DataStatus.Normal, default( DataErrors ) ), source.Data );
                Assert.Equal( DataSourceStatus.Loaded, source.Status );
            }

            [Fact]
            public async Task FailedUpdatedTransform()
            {
                var source = new IntDataSource( _ => Task.FromResult( 42 ), n => Task.FromResult( n ) );

                await source.RefreshAsync();

                var ex = new MyException();
                source.Transformer = _ => { throw ex; };

                await source.UpdateValueAsync();

                Assert.Equal( new DataChunk<int>( 0, DataStatus.Error, new DataErrors( null, null, ex ) ), source.Data );
                Assert.Equal( DataSourceStatus.Loaded, source.Status );
            }

            [Fact]
            public async Task UpdateDoesNotFetchAgain()
            {
                var count = 0;
                var source = new IntDataSource( _ => { count++; return Task.FromResult( 42 ); }, n => Task.FromResult( n ) );

                await source.RefreshAsync();

                await source.UpdateValueAsync();

                Assert.Equal( 1, count );
            }

            [Fact]
            public async Task UpdateValuesDuringRefreshDoesNotUpdate()
            {
                // Scenario:
                // After the initial load, one thread starts refreshing data.
                // While data is refreshing, another thread wants to update the value.
                // The data refresh ends, then the value update ends.
                // The update result should be ignored, since its data is no longer up to date.

                var taskSource = new TaskCompletionSource<int>();
                var transformSource = new TaskCompletionSource<int>();
                var source = new IntDataSource( _ => taskSource.Task, async n =>
                {
                    if( n == 1 )
                    {
                        await transformSource.Task;
                    }

                    return 10 * n;
                } );

                // Initial fetch
                taskSource.SetResult( 1 );
                transformSource.SetResult( 0 );
                await source.RefreshAsync();

                taskSource = new TaskCompletionSource<int>();
                transformSource = new TaskCompletionSource<int>();

                // Refresh starts...
                var refreshTask = source.RefreshAsync();

                // Update starts...
                var transformTask = source.UpdateValueAsync();

                // Refresh finishes
                taskSource.SetResult( 2 );
                await refreshTask;

                // Update finishes
                transformSource.SetResult( 0 );
                await transformTask;

                Assert.Equal( 20, source.Data.Value );
            }
        }

        public sealed class WithCache
        {
            private sealed class IntDataSource : DataSource<int>
            {
                public Func<CancellationToken, Task<int>> Fetch;


                public IntDataSource( Func<CancellationToken, Task<int>> fetch, Func<CacheMetadata> metadataCreator,
                                      IDataStore dataStore = null )
                {
                    Fetch = fetch;

                    EnableCache( "X", dataStore ?? new InMemoryDataStore(), metadataCreator );
                }


                protected override Task<int> FetchAsync( CancellationToken cancellationToken )
                    => Fetch( cancellationToken );
            }

            [Fact]
            public async Task SuccessfulRefresh()
            {
                var source = new IntDataSource( _ => Task.FromResult( 42 ), null );

                await source.RefreshAsync();

                Assert.Equal( new DataChunk<int>( 42, DataStatus.Normal, default( DataErrors ) ), source.Data );
                Assert.Equal( DataSourceStatus.Loaded, source.Status );
            }

            [Fact]
            public async Task FailedRefresh()
            {
                var ex = new MyException();
                var source = new IntDataSource( _ => TaskEx.FromException<int>( ex ), null );

                await source.RefreshAsync();

                Assert.Equal( new DataChunk<int>( 0, DataStatus.Error, new DataErrors( ex, null, null ) ), source.Data );
                Assert.Equal( DataSourceStatus.Loaded, source.Status );
            }

            [Fact]
            public async Task FailedRefreshAfterSuccessfulOne()
            {
                var source = new IntDataSource( _ => Task.FromResult( 42 ), null );

                await source.RefreshAsync();

                var ex = new MyException();
                source.Fetch = _ => TaskEx.FromException<int>( ex );

                await source.RefreshAsync();

                Assert.Equal( new DataChunk<int>( 42, DataStatus.Cached, new DataErrors( ex, null, null ) ), source.Data );
                Assert.Equal( DataSourceStatus.Loaded, source.Status );
            }

            [Fact]
            public async Task RefreshesWithDifferentMetadataDoNotInterfere()
            {
                string id = "id";
                var source = new IntDataSource( _ => Task.FromResult( 42 ), () => new CacheMetadata( id, null ) );

                await source.RefreshAsync();

                var ex = new MyException();
                source.Fetch = _ => TaskEx.FromException<int>( ex );
                id = "id2";

                await source.RefreshAsync();

                Assert.Equal( new DataChunk<int>( 0, DataStatus.Error, new DataErrors( ex, null, null ) ), source.Data );
                Assert.Equal( DataSourceStatus.Loaded, source.Status );
            }

            [Fact]
            public async Task StaleDataIsNotUsed()
            {
                var source = new IntDataSource( _ => Task.FromResult( 42 ), () => new CacheMetadata( "", DateTimeOffset.MinValue ) );

                await source.RefreshAsync();

                var ex = new MyException();
                source.Fetch = _ => TaskEx.FromException<int>( ex );

                await source.RefreshAsync();

                Assert.Equal( new DataChunk<int>( 0, DataStatus.Error, new DataErrors( ex, null, null ) ), source.Data );
                Assert.Equal( DataSourceStatus.Loaded, source.Status );
            }

            [Fact]
            public async Task DataIsNotUsedWhenMetadataIsNull()
            {
                var source = new IntDataSource( _ => Task.FromResult( 42 ), () => null );

                await source.RefreshAsync();

                var ex = new MyException();
                source.Fetch = _ => TaskEx.FromException<int>( ex );

                await source.RefreshAsync();

                Assert.Equal( new DataChunk<int>( 0, DataStatus.Error, new DataErrors( ex, null, null ) ), source.Data );
                Assert.Equal( DataSourceStatus.Loaded, source.Status );
            }

            [Fact]
            public async Task SuccessfulRefreshButMetadataCreatorThrows()
            {
                var ex = new MyException();
                var source = new IntDataSource( _ => Task.FromResult( 42 ), () => { throw ex; } );

                await source.RefreshAsync();

                Assert.Equal( new DataChunk<int>( 42, DataStatus.Normal, new DataErrors( null, ex, null ) ), source.Data );
                Assert.Equal( DataSourceStatus.Loaded, source.Status );
            }

            [Fact]
            public async Task FailedRefreshAfterSuccessfulOneButMetadataCreatorThrows()
            {
                bool shouldThrow = false;
                var cacheEx = new MyException();
                var source = new IntDataSource( _ => Task.FromResult( 42 ), () =>
                {
                    if( shouldThrow )
                    {
                        throw cacheEx;
                    }

                    return CacheMetadata.Default;
                } );

                await source.RefreshAsync();

                var fetchEx = new MyException();
                source.Fetch = _ => TaskEx.FromException<int>( fetchEx );
                shouldThrow = true;

                await source.RefreshAsync();

                Assert.Equal( new DataChunk<int>( 0, DataStatus.Error, new DataErrors( fetchEx, cacheEx, null ) ), source.Data );
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
                var source = new IntDataSource( _ => Task.FromResult( 42 ), null, store );

                await source.RefreshAsync();

                var ex = new MyException();
                source.Fetch = _ => TaskEx.FromException<int>( ex );

                await source.RefreshAsync();

                Assert.Equal( new DataChunk<int>( 0, DataStatus.Error, new DataErrors( ex, store.Exception, null ) ), source.Data );
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
                var source = new IntDataSource( _ => Task.FromResult( 42 ), null, store );

                await source.RefreshAsync();

                Assert.Equal( new DataChunk<int>( 42, DataStatus.Normal, new DataErrors( null, store.Exception, null ) ), source.Data );
                Assert.Equal( DataSourceStatus.Loaded, source.Status );
            }


            private sealed class DataSourceWithNullCacheStore : DataSource<int>
            {
                public DataSourceWithNullCacheStore()
                {
                    EnableCache( "X", null );
                }

                protected override Task<int> FetchAsync( CancellationToken cancellationToken )
                {
                    return Task.FromResult( 0 );
                }
            }

            [Fact]
            public void NullCacheStoreFails()
            {
                Assert.Throws<ArgumentNullException>( () => new DataSourceWithNullCacheStore() );
            }


            private sealed class DataSourceWithNullCacheId : DataSource<int>
            {
                public DataSourceWithNullCacheId()
                {
                    EnableCache( null, new InMemoryDataStore() );
                }

                protected override Task<int> FetchAsync( CancellationToken cancellationToken )
                {
                    return Task.FromResult( 0 );
                }
            }

            [Fact]
            public void NullCacheIdFails()
            {
                Assert.Throws<ArgumentNullException>( () => new DataSourceWithNullCacheId() );
            }


            private sealed class DataSourceEnablingCacheTwice : DataSource<int>
            {
                public DataSourceEnablingCacheTwice()
                {
                    EnableCache( "X", new InMemoryDataStore() );
                    EnableCache( "X", new InMemoryDataStore() );
                }

                protected override Task<int> FetchAsync( CancellationToken cancellationToken )
                {
                    return Task.FromResult( 0 );
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
            private sealed class IntDataSource : DataSource<int>
            {
                public Func<CancellationToken, Task<int>> Fetch;
                public Func<int, Task<int>> Transformer;


                public IntDataSource( Func<CancellationToken, Task<int>> fetch, Func<int, Task<int>> transformer, Func<CacheMetadata> metadataCreator )
                {
                    Fetch = fetch;
                    Transformer = transformer;
                    EnableCache( "X", new InMemoryDataStore(), metadataCreator );
                }


                public new Task UpdateValueAsync()
                    => base.UpdateValueAsync();

                protected override Task<int> FetchAsync( CancellationToken cancellationToken )
                    => Fetch( cancellationToken );

                protected override Task<int> TransformAsync( int value )
                    => Transformer( value );
            }


            [Fact]
            public async Task FailedRefreshAfterSuccessfulOne()
            {
                var source = new IntDataSource( _ => Task.FromResult( 21 ), n => Task.FromResult( n * 2 ), null );

                await source.RefreshAsync();

                var ex = new MyException();
                source.Fetch = _ => TaskEx.FromException<int>( ex );

                await source.RefreshAsync();

                Assert.Equal( new DataChunk<int>( 42, DataStatus.Cached, new DataErrors( ex, null, null ) ), source.Data );
                Assert.Equal( DataSourceStatus.Loaded, source.Status );
            }
        }
    }
}