using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ThinMvvm.Tests.TestInfrastructure;
using Xunit;

namespace ThinMvvm.Data.Tests
{
    /// <summary>
    /// Tests for <see cref="DataSource{T}" />.
    /// </summary>
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


                protected override Task<int> FetchAsync( CancellationToken cancellationToken ) => Fetch( cancellationToken );
            }

            [Fact]
            public void InitialState()
            {
                var source = new IntDataSource( _ => Task.FromResult( 0 ) );

                Assert.False( source.CanFetchMore );
                Assert.Equal( 0, source.Value );
                Assert.Equal( source.Value, source.RawValue );
                Assert.Equal( null, source.LastException );
                Assert.Equal( DataStatus.None, source.Status );
            }

            [Fact]
            public async Task RefreshInProgress()
            {
                var taskSource = new TaskCompletionSource<int>();
                var source = new IntDataSource( _ => taskSource.Task );

                DataStatus? status = null;
                source.PropertyChanged += ( _, e ) =>
                {
                    if( e.PropertyName == nameof( IntDataSource.Status ) )
                    {
                        status = source.Status;
                    }
                };

                var task = source.RefreshAsync();

                Assert.Equal( 0, source.Value );
                Assert.Equal( source.Value, source.RawValue );
                Assert.Equal( null, source.LastException );
                Assert.Equal( DataStatus.Loading, source.Status );

                Assert.Equal( source.Status, status );

                taskSource.SetResult( 0 );
                await task;
            }

            [Fact]
            public async Task SuccessfulRefresh()
            {
                var source = new IntDataSource( _ => Task.FromResult( 42 ) );

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

                Assert.Equal( 42, source.Value );
                Assert.Equal( source.Value, source.RawValue );
                Assert.Equal( null, source.LastException );
                Assert.Equal( DataStatus.Loaded, source.Status );

                Assert.Equal( source.Status, status );
                Assert.Equal( 0, countAfterStatus );
            }

            [Fact]
            public async Task FailedRefresh()
            {
                var ex = new MyException();
                var source = new IntDataSource( _ => TaskEx.FromException<int>( ex ) );

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

                Assert.Equal( 0, source.Value );
                Assert.Equal( source.Value, source.RawValue );
                Assert.Equal( ex, source.LastException );
                Assert.Equal( DataStatus.NoData, source.Status );

                Assert.Equal( source.Status, status );
                Assert.Equal( 0, countAfterStatus );
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

                Assert.Equal( 42, source.Value );
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

                Assert.Equal( DataStatus.Loaded, source.Status );
            }

            [Fact]
            public async Task CannotFetchMore()
            {
                var source = new IntDataSource( _ => Task.FromResult( 0 ) );

                await source.RefreshAsync();

                Assert.False( source.CanFetchMore );
                await Assert.ThrowsAsync<NotSupportedException>( () => source.FetchMoreAsync() );
            }
        }

        public sealed class WithTransform
        {
            private sealed class IntDataSource : DataSource<int>
            {
                public Func<CancellationToken, Task<int>> Fetch;
                public Func<int, int> Transformer;


                public IntDataSource( Func<CancellationToken, Task<int>> fetch, Func<int, int> transformer )
                {
                    Fetch = fetch;
                    Transformer = transformer;
                }


                public new void UpdateValue() => base.UpdateValue();

                protected override Task<int> FetchAsync( CancellationToken cancellationToken ) => Fetch( cancellationToken );

                protected override int Transform( int value ) => Transformer( value );
            }

            [Fact]
            public async Task SuccessfulTransform()
            {
                var source = new IntDataSource( _ => Task.FromResult( 21 ), n => n * 2 );

                await source.RefreshAsync();

                Assert.Equal( 42, source.Value );
                Assert.Equal( null, source.LastException );
                Assert.Equal( DataStatus.Loaded, source.Status );
            }

            [Fact]
            public async Task FailedTransform()
            {
                var source = new IntDataSource( _ => Task.FromResult( 42 ), _ => { throw new MyException(); } );

                await Assert.ThrowsAsync<MyException>( source.RefreshAsync );
            }

            [Fact]
            public async Task SuccessfulUpdatedTransform()
            {
                var source = new IntDataSource( _ => Task.FromResult( 84 ), n => n * 2 );

                await source.RefreshAsync();

                source.Transformer = n => n / 2;

                source.UpdateValue();

                Assert.Equal( 42, source.Value );
                Assert.Equal( null, source.LastException );
                Assert.Equal( DataStatus.Loaded, source.Status );
            }

            [Fact]
            public async Task FailedUpdatedTransform()
            {
                var source = new IntDataSource( _ => Task.FromResult( 84 ), n => n * 2 );

                await source.RefreshAsync();

                source.Transformer = _ => { throw new MyException(); };

                Assert.Throws<MyException>( () => source.UpdateValue() );
            }

            [Fact]
            public async Task UpdateDoesNotFetchAgain()
            {
                int count = 0;
                var source = new IntDataSource( _ => { count++; return Task.FromResult( 21 ); }, n => n * 2 );

                await source.RefreshAsync();

                source.UpdateValue();

                Assert.Equal( 1, count );
            }

            [Fact]
            public void CannotUpdateValueBeforeRefreshing()
            {
                var source = new IntDataSource( _ => Task.FromResult( 0 ), n => n + 1 );

                Assert.Throws<InvalidOperationException>( () => source.UpdateValue() );
            }
        }

        public sealed class WithCache
        {
            private sealed class IntDataSource : DataSource<int>
            {
                public Func<CancellationToken, Task<int>> Fetch;


                public IntDataSource( Func<CancellationToken, Task<int>> fetch, Func<CacheMetadata> metadataCreator )
                {
                    Fetch = fetch;
                    EnableCache( "X", new InMemoryDataStore(), metadataCreator );
                }


                protected override Task<int> FetchAsync( CancellationToken cancellationToken ) => Fetch( cancellationToken );
            }

            [Fact]
            public async Task SuccessfulRefresh()
            {
                var source = new IntDataSource( _ => Task.FromResult( 42 ), null );

                await source.RefreshAsync();

                Assert.Equal( 42, source.Value );
                Assert.Equal( null, source.LastException );
                Assert.Equal( DataStatus.Loaded, source.Status );
                Assert.Equal( CacheStatus.Unused, source.CacheStatus );
            }

            [Fact]
            public async Task FailedRefresh()
            {
                var ex = new MyException();
                var source = new IntDataSource( _ => TaskEx.FromException<int>( ex ), null );

                await source.RefreshAsync();

                Assert.Equal( 0, source.Value );
                Assert.Equal( ex, source.LastException );
                Assert.Equal( DataStatus.NoData, source.Status );
                Assert.Equal( CacheStatus.Unused, source.CacheStatus );
            }

            [Fact]
            public async Task FailedRefreshAfterSuccessfulOne()
            {
                var source = new IntDataSource( _ => Task.FromResult( 42 ), null );

                await source.RefreshAsync();

                var ex = new MyException();
                source.Fetch = _ => TaskEx.FromException<int>( ex );

                await source.RefreshAsync();

                Assert.Equal( 42, source.Value );
                Assert.Equal( ex, source.LastException );
                Assert.Equal( DataStatus.Loaded, source.Status );
                Assert.Equal( CacheStatus.Used, source.CacheStatus );
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

                Assert.Equal( 0, source.Value );
                Assert.Equal( ex, source.LastException );
                Assert.Equal( DataStatus.NoData, source.Status );
                Assert.Equal( CacheStatus.Unused, source.CacheStatus );
            }

            [Fact]
            public async Task StaleDataIsNotUsed()
            {
                var source = new IntDataSource( _ => Task.FromResult( 42 ), () => new CacheMetadata( "", DateTimeOffset.MinValue ) );

                await source.RefreshAsync();

                var ex = new MyException();
                source.Fetch = _ => TaskEx.FromException<int>( ex );

                await source.RefreshAsync();

                Assert.Equal( 0, source.Value );
                Assert.Equal( ex, source.LastException );
                Assert.Equal( DataStatus.NoData, source.Status );
                Assert.Equal( CacheStatus.Unused, source.CacheStatus );
            }

            [Fact]
            public async Task CacheIsNotUsedWhenMetadataIsNull()
            {
                var source = new IntDataSource( _ => Task.FromResult( 42 ), () => null );

                await source.RefreshAsync();

                var ex = new MyException();
                source.Fetch = _ => TaskEx.FromException<int>( ex );

                await source.RefreshAsync();

                Assert.Equal( 0, source.Value );
                Assert.Equal( ex, source.LastException );
                Assert.Equal( DataStatus.NoData, source.Status );
                Assert.Equal( CacheStatus.Unused, source.CacheStatus );
            }

            [Fact]
            public async Task SuccessfulRefreshButMetadataCreatorThrows()
            {
                var ex = new MyException();
                var source = new IntDataSource( _ => Task.FromResult( 42 ), () => { throw ex; } );

                await source.RefreshAsync();

                Assert.Equal( 0, source.Value );
                Assert.Equal( ex, source.LastException );
                Assert.Equal( DataStatus.NoData, source.Status );
                Assert.Equal( CacheStatus.Unused, source.CacheStatus );
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

                var ex = new MyException();
                source.Fetch = _ => TaskEx.FromException<int>( ex );

                shouldThrow = true;
                await source.RefreshAsync();

                Assert.Equal( 0, source.Value );
                Assert.Equal( cacheEx, source.LastException );
                Assert.Equal( DataStatus.NoData, source.Status );
                Assert.Equal( CacheStatus.Unused, source.CacheStatus );
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
                public Func<int, int> Transformer;


                public IntDataSource( Func<CancellationToken, Task<int>> fetch, Func<int, int> transformer, Func<CacheMetadata> metadataCreator )
                {
                    Fetch = fetch;
                    Transformer = transformer;
                    EnableCache( "X", new InMemoryDataStore(), metadataCreator );
                }


                public new void UpdateValue() => base.UpdateValue();

                protected override Task<int> FetchAsync( CancellationToken cancellationToken ) => Fetch( cancellationToken );

                protected override int Transform( int value ) => Transformer( value );
            }


            [Fact]
            public async Task FailedRefreshAfterSuccessfulOne()
            {
                var source = new IntDataSource( _ => Task.FromResult( 21 ), n => n * 2, null );

                await source.RefreshAsync();

                var ex = new MyException();
                source.Fetch = _ => TaskEx.FromException<int>( ex );

                await source.RefreshAsync();

                Assert.Equal( 42, source.Value );
                Assert.Equal( ex, source.LastException );
                Assert.Equal( DataStatus.Loaded, source.Status );
                Assert.Equal( CacheStatus.Used, source.CacheStatus );
            }
        }
    }
}