using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ThinMvvm.Tests
{
    [TestClass]
    public sealed class CachedDataViewModelTests
    {
        public sealed class TestCachedDataViewModel : CachedDataViewModel<NoParameter, int>
        {
            public Func<bool, CancellationToken, CachedTask<int>> GetDataMethod { get; set; }

            public Func<int, CancellationToken, bool> HandleDataMethod { get; set; }


            public TestCachedDataViewModel() : base( new TestCache() ) { }


            protected override CachedTask<int> GetData( bool force, CancellationToken token )
            {
                return GetDataMethod( force, token );
            }

            protected override bool HandleData( int data, CancellationToken token )
            {
                return HandleDataMethod( data, token );
            }

            private sealed class TestCache : ICache
            {
                private Dictionary<Type, Dictionary<long, Tuple<object, DateTime>>> _cache
                    = new Dictionary<Type, Dictionary<long, Tuple<object, DateTime>>>();

                public bool TryGet<T>( Type owner, long id, out T value )
                {
                    if ( _cache.ContainsKey( owner ) && _cache[owner].ContainsKey( id ) )
                    {
                        if ( DateTime.Now <= _cache[owner][id].Item2 )
                        {
                            value = (T) _cache[owner][id].Item1;
                            return true;
                        }
                    }
                    value = default( T );
                    return false;
                }

                public void Set( Type owner, long id, DateTime expirationDate, object value )
                {
                    if ( !_cache.ContainsKey( owner ) )
                    {
                        _cache.Add( owner, new Dictionary<long, Tuple<object, DateTime>>() );
                    }
                    _cache[owner][id] = Tuple.Create( value, expirationDate );
                }
            }
        }

        [TestInitialize]
        public void Initialize()
        {
            DataViewModelOptions.ClearNetworkExceptionTypes();
        }

        [TestMethod]
        public void CacheStatusOnInitializationIsNoCache()
        {
            var vm = new TestCachedDataViewModel();

            Assert.AreEqual( CacheStatus.NoCache, vm.CacheStatus );
        }

        [TestMethod]
        public async Task CacheStatusAfterErrorIsNoCache()
        {
            var vm = new TestCachedDataViewModel
            {
                GetDataMethod = ( _, __ ) => CachedTask.Create<int>( () => { throw new Exception(); } )
            };

            await vm.OnNavigatedToAsync();

            Assert.AreEqual( CacheStatus.NoCache, vm.CacheStatus );
        }

        [TestMethod]
        public async Task CacheStatusAfterNetworkErrorIsNoCache()
        {
            DataViewModelOptions.AddNetworkExceptionType( typeof( WebException ) );
            var vm = new TestCachedDataViewModel
            {
                GetDataMethod = ( _, __ ) => { throw new WebException(); }
            };

            await vm.OnNavigatedToAsync();

            Assert.AreEqual( CacheStatus.NoCache, vm.CacheStatus );
        }

        [TestMethod]
        public void CacheStatusWhenLoadingIsLoading()
        {
            var source = new TaskCompletionSource<int>();
            var vm = new TestCachedDataViewModel
            {
                GetDataMethod = ( _, __ ) => CachedTask.Create( () => source.Task )
            };

            vm.OnNavigatedTo();

            Assert.AreEqual( CacheStatus.Loading, vm.CacheStatus );

            source.SetResult( 0 );
        }

        [TestMethod]
        public async Task CacheStatusAfterSuccessfulLoadIsUnused()
        {
            var vm = new TestCachedDataViewModel
            {
                GetDataMethod = ( _, __ ) => CachedTask.Create( () => Task.FromResult( 0 ) ),
                HandleDataMethod = ( _, __ ) => true
            };

            await vm.OnNavigatedToAsync();

            Assert.AreEqual( CacheStatus.Unused, vm.CacheStatus );
        }

        [TestMethod]
        public async Task CacheStatusOnOptOutIsOptedOut()
        {
            var vm = new TestCachedDataViewModel
            {
                GetDataMethod = ( _, __ ) => CachedTask.DoNotCache( () => Task.FromResult( 0 ) ),
                HandleDataMethod = ( _, __ ) => true
            };

            await vm.OnNavigatedToAsync();

            Assert.AreEqual( CacheStatus.OptedOut, vm.CacheStatus );
        }


        [TestMethod]
        public async Task CacheStatusWhenHandleDateReturnsFalseIsOptedOut()
        {
            var vm = new TestCachedDataViewModel
            {
                GetDataMethod = ( _, __ ) => CachedTask.Create( () => Task.FromResult( 0 ) ),
                HandleDataMethod = ( _, __ ) => false
            };

            await vm.OnNavigatedToAsync();

            Assert.AreEqual( CacheStatus.OptedOut, vm.CacheStatus );
        }

        [TestMethod]
        public async Task CacheStatusWhenLoadingAfterSuccessfulLoadIsUsedTemporarily()
        {
            var vm = new TestCachedDataViewModel
            {
                GetDataMethod = ( _, __ ) => CachedTask.Create( () => Task.FromResult( 0 ) ),
                HandleDataMethod = ( _, __ ) => true
            };

            await vm.OnNavigatedToAsync();

            var source = new TaskCompletionSource<int>();
            vm.GetDataMethod = ( _, __ ) => CachedTask.Create( () => source.Task );

            vm.OnNavigatedTo();

            Assert.AreEqual( CacheStatus.UsedTemporarily, vm.CacheStatus );

            source.SetResult( 0 );
        }

        [TestMethod]
        public async Task CacheStatusOnErrorAfterSuccessfulLoadIsNoCache()
        {
            var vm = new TestCachedDataViewModel
            {
                GetDataMethod = ( _, __ ) => CachedTask.Create( () => Task.FromResult( 0 ) ),
                HandleDataMethod = ( _, __ ) => true
            };

            await vm.OnNavigatedToAsync();

            vm.GetDataMethod = ( _, __ ) => CachedTask.Create<int>( () => { throw new Exception(); } );

            vm.OnNavigatedTo();

            Assert.AreEqual( CacheStatus.NoCache, vm.CacheStatus );
        }

        [TestMethod]
        public async Task CacheStatusOnNetworkErrorAfterSuccessfulLoadIsNoCache()
        {
            DataViewModelOptions.AddNetworkExceptionType( typeof( WebException ) );
            var vm = new TestCachedDataViewModel
            {
                GetDataMethod = ( _, __ ) => CachedTask.Create( () => Task.FromResult( 0 ) ),
                HandleDataMethod = ( _, __ ) => true
            };

            await vm.OnNavigatedToAsync();

            vm.GetDataMethod = ( _, __ ) => CachedTask.Create<int>( () => { throw new WebException(); } );

            vm.OnNavigatedTo();

            Assert.AreEqual( CacheStatus.Used, vm.CacheStatus );
        }
    }
}