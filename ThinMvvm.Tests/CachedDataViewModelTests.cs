// Copyright (c) Solal Pirelli 2014
// See License.txt file for more details

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
        private sealed class TestCache : IDataCache
        {
            // pretend it's a static cache, as a real one would be
            public static readonly Dictionary<Type, Dictionary<long, Tuple<object, DateTimeOffset>>> Data
                = new Dictionary<Type, Dictionary<long, Tuple<object, DateTimeOffset>>>();

            public Task<CachedData<T>> GetAsync<T>( Type owner, long id )
            {
                if ( Data.ContainsKey( owner ) && Data[owner].ContainsKey( id ) )
                {
                    if ( DateTimeOffset.Now <= Data[owner][id].Item2 )
                    {
                        return Task.FromResult( new CachedData<T>( (T) Data[owner][id].Item1 ) );
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
        }

        private sealed class TestCachedDataViewModel : CachedDataViewModel<NoParameter, int>
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
        public async Task CacheStatusOnErrorAfterSuccessfulLoadIsUsed()
        {
            var vm = new TestCachedDataViewModel
            {
                GetDataMethod = ( _, __ ) => CachedTask.Create( () => Task.FromResult( 0 ) ),
                HandleDataMethod = ( _, __ ) => true
            };

            await vm.OnNavigatedToAsync();

            vm.GetDataMethod = ( _, __ ) => CachedTask.Create<int>( () => { throw new Exception(); } );

            vm.OnNavigatedTo();

            Assert.AreEqual( CacheStatus.Used, vm.CacheStatus );
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



        [TestMethod]
        public async Task HandleDataIsCalledOnLiveData()
        {
            bool called = false;
            var vm = new TestCachedDataViewModel
            {
                GetDataMethod = ( _, __ ) => CachedTask.Create( () => Task.FromResult( 0 ) ),
                HandleDataMethod = ( _, __ ) => { called = true; return true; }
            };

            await vm.OnNavigatedToAsync();

            Assert.IsTrue( called );
        }

        [TestMethod]
        public async Task HandleDataIsNotCalledTwiceOnCachedData()
        {
            var vm = new TestCachedDataViewModel
            {
                GetDataMethod = ( _, __ ) => CachedTask.Create( () => Task.FromResult( 0 ) ),
                HandleDataMethod = ( _, __ ) => true
            };

            await vm.OnNavigatedToAsync();

            bool called = false;
            vm.GetDataMethod = ( _, __ ) => CachedTask.NoNewData<int>();
            vm.HandleDataMethod = ( _, __ ) => { called = true; return true; };

            await vm.RefreshCommand.ExecuteAsync();

            Assert.IsFalse( called );
        }

        [TestMethod]
        public async Task HandleDataIsCalledOnDataThatMustNotBeCached()
        {
            bool called = false;
            var vm = new TestCachedDataViewModel
            {
                GetDataMethod = ( _, __ ) => CachedTask.DoNotCache( () => Task.FromResult( 0 ) ),
                HandleDataMethod = ( _, __ ) => { called = true; return true; }
            };

            await vm.OnNavigatedToAsync();

            Assert.IsTrue( called );
        }

        [TestMethod]
        public async Task SeparateTaskIdsAreRespected()
        {
            var vm = new TestCachedDataViewModel
            {
                GetDataMethod = ( _, __ ) => CachedTask.Create( () => Task.FromResult( 0 ), 0 ),
                HandleDataMethod = ( _, __ ) => { return true; }
            };

            await vm.OnNavigatedToAsync();

            var source = new TaskCompletionSource<int>();

            vm.GetDataMethod = ( _, __ ) => CachedTask.Create( () => source.Task, 1 );

            vm.OnNavigatedTo();

            Assert.AreNotEqual( CacheStatus.UsedTemporarily, vm.CacheStatus );

            source.SetResult( 0 );
        }

        [TestMethod]
        public async Task ExpirationDateIsRespected()
        {
            var vm = new TestCachedDataViewModel
            {
                GetDataMethod = ( _, __ ) => CachedTask.Create( () => Task.FromResult( 0 ), expirationDate: DateTime.Now.AddMilliseconds( 50 ) ),
                HandleDataMethod = ( _, __ ) => { return true; }
            };

            await vm.OnNavigatedToAsync();
            await Task.Delay( 100 );

            var source = new TaskCompletionSource<int>();

            vm.GetDataMethod = ( _, __ ) => CachedTask.Create( () => source.Task );

            var _unused = vm.RefreshCommand.ExecuteAsync();

            Assert.AreNotEqual( CacheStatus.UsedTemporarily, vm.CacheStatus );

            source.SetResult( 0 );
        }

        [TestMethod]
        [ExpectedException( typeof( ArgumentException ) )]
        public void ErrorWhenExpirationDateIsInPast()
        {
            CachedTask.Create( () => Task.FromResult( 0 ), expirationDate: DateTime.Now.AddSeconds( -1 ) );
        }
    }
}