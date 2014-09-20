// Copyright (c) Solal Pirelli 2014
// See License.txt file for more details

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ThinMvvm.Tests
{
    [TestClass]
    public sealed class CachedDataViewModelTests
    {
        private sealed class TestDataCache : IDataCache
        {
            private readonly Dictionary<Type, Dictionary<long, Tuple<object, DateTimeOffset>>> Data
                = new Dictionary<Type, Dictionary<long, Tuple<object, DateTimeOffset>>>();

            public Task<CachedData<T>> GetAsync<T>( Type owner, long id )
            {
                if ( Contains( owner, id ) )
                {
                    if ( DateTimeOffset.Now <= Data[owner][id].Item2 )
                    {
                        return Task.FromResult( new CachedData<T>( (T) Get( owner, id ) ) );
                    }
                }
                return Task.FromResult( new CachedData<T>() );
            }

            public Task SetAsync( Type owner, long id, DateTimeOffset expirationDate, object value )
            {
                if ( !Data.ContainsKey( owner ) )
                {
                    Data.Add( owner, new Dictionary<long, Tuple<object, DateTimeOffset>>() );
                }
                Data[owner][id] = Tuple.Create( value, expirationDate );
                return Task.FromResult( 0 );
            }

            public bool Contains( Type owner, long id )
            {
                return Data.ContainsKey( owner ) && Data[owner].ContainsKey( id );
            }

            public object Get( Type owner, long id )
            {
                return Data[owner][id].Item1;
            }
        }

        private sealed class TestCachedDataViewModel : CachedDataViewModel<NoParameter, int>
        {
            public CachedTask<int> Data { get; set; }

            public Func<int, bool> HandleDataMethod { get; set; }

            public TestCachedDataViewModel() : base( new TestDataCache() ) { }
            public TestCachedDataViewModel( IDataCache cache ) : base( cache ) { }

            protected override CachedTask<int> GetData( bool force, CancellationToken token )
            {
                return Data;
            }

            protected override bool HandleData( int data, CancellationToken token )
            {
                return HandleDataMethod( data );
            }
        }

        // Nothing -> No new data = Nothing
        [TestMethod]
        public async Task Nothing__NoNewData__Nothing()
        {
            var test = await Test.Empty()
                                 .NoNewData()
                                 .DoAsync();

            test.Data( DataStatus.Loaded )
                .Cache( CacheStatus.NoData )
                .NotInCache();
        }

        // Nothing -> Error = Nothing
        [TestMethod]
        public async Task Nothing__Error__Nothing()
        {
            var test = await Test.Empty()
                                 .Throw()
                                 .DoAsync();

            test.Data( DataStatus.Error )
                .Cache( CacheStatus.NoData )
                .NotInCache();
        }

        // Nothing -> Data, cached, handled = Cache unused, new cached
        [TestMethod]
        public async Task Nothing__Data_Cached_Handled__CacheUnused_DataCached()
        {
            var test = await Test.Empty()
                                 .Data( 1 )
                                 .Cache()
                                 .Handle()
                                 .DoAsync();

            test.Data( DataStatus.Loaded )
                .Cache( CacheStatus.Unused )
                .InCache();
        }

        // Nothing -> Data, cached, unhandled = Opted out, new not cached
        [TestMethod]
        public async Task Nothing__Data_Cached_Unhandled__OptedOut_NotCached()
        {
            var test = await Test.Empty()
                                 .Data( 1 )
                                 .Cache()
                                 .DoAsync();

            test.Data( DataStatus.Loaded )
                .Cache( CacheStatus.OptedOut )
                .NotInCache();
        }

        // Data -> No new data = Cache unused, old cached
        [TestMethod]
        public async Task Data__NoNewData__Unused_OldCached()
        {
            var test = await Test.WithData( 1 )
                                 .NoNewData()
                                 .DoAsync();

            test.Data( DataStatus.Loaded )
                .Cache( CacheStatus.Unused )
                .OldInCache();
        }

        // Data -> Error = Used, old cached
        [TestMethod]
        public async Task Data__Error__Used_OldCached()
        {
            var test = await Test.WithData( 1 )
                                 .Throw()
                                 .DoAsync();

            test.Data( DataStatus.Error )
                .Cache( CacheStatus.Used )
                .OldInCache();
        }

        // Data -> New data, cached, handled = Cache unused, new cached
        [TestMethod]
        public async Task Data__NewData_Cached_Handled__Unused_NewCached()
        {
            var test = await Test.WithData( 1 )
                                 .Data( 2 )
                                 .Cache()
                                 .Handle()
                                 .DoAsync();

            test.Data( DataStatus.Loaded )
                .Cache( CacheStatus.Unused )
                .InCache();
        }

        // Data -> New data, cached, unhandled = Opted out, old cached
        [TestMethod]
        public async Task Data__NewData_Cached_Unhandled__OptedOut_OldCached()
        {
            var test = await Test.WithData( 1 )
                                 .Data( 2 )
                                 .Cache()
                                 .DoAsync();

            test.Data( DataStatus.Loaded )
                .Cache( CacheStatus.OptedOut )
                .OldInCache();
        }

        // Data -> New data, not cached, handled = Opted out, old cached
        [TestMethod]
        public async Task Data__NewData_NotCached_Handled__OptedOut_OldCached()
        {
            var test = await Test.WithData( 1 )
                                 .Data( 2 )
                                 .Handle()
                                 .DoAsync();

            test.Data( DataStatus.Loaded )
                .Cache( CacheStatus.OptedOut )
                .OldInCache();
        }

        // Data -> New data, not cached, unhandled = Opted out, old cached
        [TestMethod]
        public async Task Data__NewData_NotCached_Unhandled__OptedOut_OldCached()
        {
            var test = await Test.WithData( 1 )
                                 .Data( 2 )
                                 .DoAsync();

            test.Data( DataStatus.Loaded )
                .Cache( CacheStatus.OptedOut )
                .OldInCache();
        }

        // Data -> New data with different ID, cached, handled -> Unused, both in cache
        [TestMethod]
        public async Task Data__NewDataDifferentId_Cached_Handled__Unused_BothCached()
        {
            var test = await Test.WithData( 1, 0 )
                                 .Data( 2, 1 )
                                 .Cache()
                                 .Handle()
                                 .DoAsync();

            test.Data( DataStatus.Loaded )
                .Cache( CacheStatus.Unused )
                .OldInCache()
                .InCache();
        }

        [TestMethod]
        public void Nothing__Data__NoData_WhileLoading()
        {
            var source = new TaskCompletionSource<int>();
            var vm = new TestCachedDataViewModel()
            {
                Data = CachedTask.Create( () => source.Task ),
                HandleDataMethod = _ => true
            };

            // use OnNavigatedTo to avoid blocking since source.Task won't finish automatically
            vm.OnNavigatedTo();

            Assert.AreEqual( DataStatus.Loading, vm.DataStatus );
            Assert.AreEqual( CacheStatus.NoData, vm.CacheStatus );

            source.SetResult( 0 );
        }

        [TestMethod]
        public async Task Data__NewData__DoesNotHandleTwice()
        {
            int count = 0;
            var vm = new TestCachedDataViewModel()
            {
                Data = CachedTask.Create( () => Task.FromResult( 0 ) ),
                HandleDataMethod = _ => { count++; return true; }
            };

            await vm.OnNavigatedToAsync();

            var source = new TaskCompletionSource<int>();
            vm.Data = CachedTask.Create( () => source.Task );

            // use OnNavigatedTo to avoid blocking since source.Task won't finish automatically
            vm.OnNavigatedTo();

            Assert.AreEqual( 1, count );

            source.SetResult( 1 );
        }

        [TestMethod]
        public async Task Data__NewData__UsedTemporarily()
        {
            var vm = new TestCachedDataViewModel()
            {
                Data = CachedTask.Create( () => Task.FromResult( 0 ) ),
                HandleDataMethod = _ => true
            };

            await vm.OnNavigatedToAsync();

            var source = new TaskCompletionSource<int>();
            vm.Data = CachedTask.Create( () => source.Task );

            // use OnNavigatedTo to avoid blocking since source.Task won't finish automatically
            vm.OnNavigatedTo();

            Assert.AreEqual( CacheStatus.UsedTemporarily, vm.CacheStatus );

            source.SetResult( 1 );
        }

        [TestMethod]
        public async Task Data__NewDataDifferentId__NoData()
        {
            var vm = new TestCachedDataViewModel()
            {
                Data = CachedTask.Create( () => Task.FromResult( 0 ), id: 0 ),
                HandleDataMethod = _ => true
            };

            await vm.OnNavigatedToAsync();

            var source = new TaskCompletionSource<int>();
            vm.Data = CachedTask.Create( () => source.Task, id: 1 );

            // use OnNavigatedTo to avoid blocking since source.Task won't finish automatically
            vm.OnNavigatedTo();

            Assert.AreEqual( CacheStatus.NoData, vm.CacheStatus );

            source.SetResult( 1 );
        }

        [TestMethod]
        public async Task Nothing__Data_CacheHasVersion__UsedTemporarily_WhileLoading()
        {
            var cache = new TestDataCache();

            await cache.SetAsync( typeof( TestCachedDataViewModel ), 0, DateTimeOffset.MaxValue, 0 );

            var source = new TaskCompletionSource<int>();
            var vm = new TestCachedDataViewModel( cache )
            {
                Data = CachedTask.Create( () => source.Task, id: 0 ),
                HandleDataMethod = _ => true
            };

            // use OnNavigatedTo to avoid blocking since source.Task won't finish automatically
            vm.OnNavigatedTo();

            Assert.AreEqual( DataStatus.Loading, vm.DataStatus );
            Assert.AreEqual( CacheStatus.UsedTemporarily, vm.CacheStatus );

            source.SetResult( 1 );
        }

        [TestMethod]
        public async Task DataInCache__Error__Used()
        {
            var cache = new TestDataCache();
            await cache.SetAsync( typeof( TestCachedDataViewModel ), 0, DateTimeOffset.MaxValue, 123 );

            var vm = new TestCachedDataViewModel( cache )
            {
                Data = CachedTask.Create<int>( () => { throw new Exception( "lalalala" ); }, id: 0 ),
                HandleDataMethod = _ => true
            };

            await vm.RefreshCommand.ExecuteAsync();

            Assert.AreEqual( CacheStatus.Used, vm.CacheStatus );
        }

        private sealed class Test
        {
            private int _data;
            private int _id;

            private int? _oldData;
            private int _oldId;

            private bool _throw;
            private bool _noNewData;
            private bool _cache;
            private bool _handle;

            private TestDataCache _dataCache;
            private TestCachedDataViewModel _vm;

            public static Test Empty()
            {
                return new Test();
            }

            public static Test WithData( int n, int id = 0 )
            {
                return new Test { _oldData = n, _oldId = id };
            }

            public Test Throw()
            {
                _throw = true;
                return this;
            }

            public Test NoNewData()
            {
                _noNewData = true;
                return this;
            }

            public Test Cache()
            {
                _cache = true;
                return this;
            }

            public Test Handle()
            {
                _handle = true;
                return this;
            }

            public Test Data( int data, int id = 0 )
            {
                _data = data;
                _id = id;
                return this;
            }

            public async Task<Test> DoAsync()
            {
                _dataCache = new TestDataCache();
                _vm = new TestCachedDataViewModel( _dataCache );

                if ( _oldData.HasValue )
                {
                    _vm.Data = CachedTask.Create( () => Task.FromResult( _oldData.Value ), id: _oldId );
                    _vm.HandleDataMethod = _ => true;

                    await _vm.OnNavigatedToAsync();
                }

                _vm.Data = _throw ? CachedTask.Create<int>( () => { throw new Exception(); } )
                       : _noNewData ? CachedTask.NoNewData<int>()
                       : _cache ? CachedTask.Create( () => Task.FromResult( _data ), id: _id )
                               : CachedTask.DoNotCache( () => Task.FromResult( _data ) );
                _vm.HandleDataMethod = n =>
                {
                    if ( _throw )
                    {
                        Assert.Fail( "HandleData should not be called when GetData has thrown." );
                    }
                    Assert.AreEqual( _data, n );
                    return _handle;
                };

                await _vm.OnNavigatedToAsync();

                return this;
            }

            public Test Data( DataStatus status )
            {
                Assert.AreEqual( status, _vm.DataStatus );
                return this;
            }

            public Test Cache( CacheStatus status )
            {
                Assert.AreEqual( status, _vm.CacheStatus );
                return this;
            }

            public Test InCache()
            {
                Assert.AreEqual( _data, _dataCache.Get( typeof( TestCachedDataViewModel ), _id ) );
                return this;
            }

            public Test OldInCache()
            {
                Assert.AreEqual( _oldData, _dataCache.Get( typeof( TestCachedDataViewModel ), _oldId ) );
                return this;
            }

            public Test NotInCache()
            {
                Assert.IsFalse( _dataCache.Contains( typeof( TestCachedDataViewModel ), _id ) );
                return this;
            }
        }
    }
}