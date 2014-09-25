// Copyright (c) Solal Pirelli 2014
// See License.txt file for more details

using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ThinMvvm.Tests
{
    [TestClass]
    public sealed class DataViewModelTests
    {
        private sealed class TestDataViewModel : DataViewModel<NoParameter>
        {
            public Func<bool, CancellationToken, Task> RefreshAsyncMethod { get; set; }

            public CancellationToken PublicCurrentCancellationToken
            {
                get { return CurrentCancellationToken; }
            }

            protected override Task RefreshAsync( bool force, CancellationToken token )
            {
                return RefreshAsyncMethod( force, token );
            }

            public new Task TryRefreshAsync( bool force )
            {
                return base.TryRefreshAsync( force );
            }

            public Task TryExecuteNull()
            {
                return TryExecuteAsync( null );
            }
        }

        [TestInitialize]
        public void Initialize()
        {
            DataViewModelOptions.ClearNetworkExceptionTypes();
        }

        [TestMethod]
        public void DataStatusIsNoDataAfterInitialization()
        {
            var vm = new TestDataViewModel();

            Assert.AreEqual( DataStatus.NoData, vm.DataStatus );
        }

        [TestMethod]
        public async Task OnNavigatedToForcesRefreshTheFirstTime()
        {
            bool forced = false;
            var vm = new TestDataViewModel
            {
                RefreshAsyncMethod = ( f, _ ) => { forced = f; return Task.FromResult( 0 ); }
            };

            await vm.OnNavigatedToAsync();

            Assert.AreEqual( forced, true );
        }

        [TestMethod]
        public async Task OnNavigatedToDoesNotForcesRefreshSubsequentTimes()
        {
            int forcedCount = 0;
            int count = 0;
            var vm = new TestDataViewModel
            {
                RefreshAsyncMethod = ( f, _ ) => { count++; forcedCount += f ? 1 : 0; return Task.FromResult( 0 ); }
            };

            await vm.OnNavigatedToAsync();
            await vm.OnNavigatedToAsync();

            Assert.AreEqual( forcedCount, 1 );
            Assert.AreEqual( count, 2 );
        }

        [TestMethod]
        public async Task TryRefreshAsyncCallsRefreshAsync()
        {
            int forcedCount = 0;
            int count = 0;
            var vm = new TestDataViewModel
            {
                RefreshAsyncMethod = ( f, _ ) => { count++; forcedCount += f ? 1 : 0; return Task.FromResult( 0 ); }
            };

            await vm.TryRefreshAsync( false );

            Assert.AreEqual( forcedCount, 0 );
            Assert.AreEqual( count, 1 );
        }

        [TestMethod]
        public async Task TryRefreshAsyncCallsRefreshAsyncAndForcesWhenAsked()
        {
            int forcedCount = 0;
            var vm = new TestDataViewModel
            {
                RefreshAsyncMethod = ( f, _ ) => { forcedCount += f ? 1 : 0; return Task.FromResult( 0 ); }
            };

            await vm.TryRefreshAsync( true );
            Assert.AreEqual( forcedCount, 1 );
        }

        [TestMethod]
        public void DataStatusIsLoadingThenDataLoadedOnSuccessfulRefresh()
        {
            var source = new TaskCompletionSource<int>();
            var vm = new TestDataViewModel
            {
                RefreshAsyncMethod = ( _, __ ) => source.Task
            };

            vm.OnNavigatedTo();

            Assert.AreEqual( DataStatus.Loading, vm.DataStatus );

            source.SetResult( 0 );

            Assert.AreEqual( DataStatus.Loaded, vm.DataStatus );
        }

        [TestMethod]
        public async Task DataStatusIsErrorOnErrorDuringLoad()
        {
            var vm = new TestDataViewModel
            {
                RefreshAsyncMethod = ( _, __ ) => { throw new Exception(); }
            };

            await vm.OnNavigatedToAsync();

            Assert.AreEqual( DataStatus.Error, vm.DataStatus );
        }

        [TestMethod]
        public async Task DataStatusIsNetworkErrorOnNetworkErrorDuringLoad()
        {
            DataViewModelOptions.AddNetworkExceptionType( typeof( WebException ) );
            var vm = new TestDataViewModel
            {
                RefreshAsyncMethod = ( _, __ ) => { throw new WebException(); }
            };

            await vm.OnNavigatedToAsync();

            Assert.AreEqual( DataStatus.NetworkError, vm.DataStatus );
        }

        [TestMethod]
        public async Task DataViewModelOptionsNetworkExceptionTypesAreRespected()
        {
            DataViewModelOptions.AddNetworkExceptionType( typeof( DuplicateWaitObjectException ) );
            var vm = new TestDataViewModel
            {
                RefreshAsyncMethod = ( _, __ ) => { throw new DuplicateWaitObjectException(); }
            };

            await vm.OnNavigatedToAsync();

            Assert.AreEqual( DataStatus.NetworkError, vm.DataStatus );
        }

        [TestMethod]
        public async Task NetworkExceptionsIncludeSubtypesOfRegisteredExceptions()
        {
            DataViewModelOptions.AddNetworkExceptionType( typeof( Exception ) );
            var vm = new TestDataViewModel
            {
                RefreshAsyncMethod = ( _, __ ) => { throw new InvalidOperationException(); }
            };

            await vm.OnNavigatedToAsync();

            Assert.AreEqual( DataStatus.NetworkError, vm.DataStatus );
        }

        [TestMethod]
        [ExpectedException( typeof( ArgumentException ) )]
        public void CannotAddNonExceptionsToNetworkExceptionTypes()
        {
            DataViewModelOptions.AddNetworkExceptionType( typeof( string ) );
        }

        [TestMethod]
        public async Task CurrentCancellationTokenExists()
        {
            var vm = new TestDataViewModel
            {
                RefreshAsyncMethod = ( _, __ ) => Task.FromResult( 0 )
            };

            Assert.IsNotNull( vm.PublicCurrentCancellationToken );

            await vm.RefreshCommand.ExecuteAsync();

            Assert.IsNotNull( vm.PublicCurrentCancellationToken );
        }

        [TestMethod]
        public void DisposeWorks()
        {
            new TestDataViewModel().Dispose();
        }

        [TestMethod]
        [ExpectedException( typeof( ArgumentNullException ) )]
        public void ErrorOnAddingNullNetworkExceptionType()
        {
            DataViewModelOptions.AddNetworkExceptionType( null );
        }


        [TestMethod]
        [ExpectedException( typeof( ArgumentNullException ) )]
        public async Task ErrorOnTryExecuteNullAction()
        {
            await new TestDataViewModel().TryExecuteNull();
        }
    }
}