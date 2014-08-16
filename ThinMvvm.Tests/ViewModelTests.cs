// Copyright (c) Solal Pirelli 2014
// See License.txt file for more details

using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ThinMvvm.Tests
{
    [TestClass]
    public sealed class ViewModelTests
    {
        private class TestViewModel : ViewModel<NoParameter>
        {
            public int Counter { get; private set; }

            public Command IncrementCommand
            {
                get { return GetCommand( () => Counter++ ); }
            }

            public Command<int> IncrementByCommand
            {
                get { return GetCommand<int>( n => Counter += n ); }
            }

            public AsyncCommand AsyncIncrementCommand
            {
                get { return GetAsyncCommand( () => { Counter++; return Task.FromResult( 0 ); } ); }
            }

            public AsyncCommand<int> AsyncIncrementByCommand
            {
                get { return GetAsyncCommand<int>( n => { Counter += n; return Task.FromResult( 0 ); } ); }
            }
        }

        [TestMethod]
        public void GetCommandWorks()
        {
            var vm = new TestViewModel();

            vm.IncrementCommand.Execute();

            Assert.AreEqual( 1, vm.Counter );
        }

        [TestMethod]
        public void GenericGetCommandWorks()
        {
            var vm = new TestViewModel();

            vm.IncrementByCommand.Execute( 2 );

            Assert.AreEqual( 2, vm.Counter );
        }

        [TestMethod]
        public async Task GetAsyncCommandWorks()
        {
            var vm = new TestViewModel();

            await vm.AsyncIncrementCommand.ExecuteAsync();

            Assert.AreEqual( 1, vm.Counter );
        }

        [TestMethod]
        public async Task GenericGetAsyncCommandWorks()
        {
            var vm = new TestViewModel();

            await vm.AsyncIncrementByCommand.ExecuteAsync( 2 );

            Assert.AreEqual( 2, vm.Counter );
        }

        [TestMethod]
        public void GetCommandIsCached()
        {
            var vm = new TestViewModel();

            Assert.AreEqual( vm.IncrementCommand, vm.IncrementCommand );
        }

        [TestMethod]
        public void GenericGetCommandIsCached()
        {
            var vm = new TestViewModel();

            Assert.AreEqual( vm.IncrementByCommand, vm.IncrementByCommand );
        }

        [TestMethod]
        public void GetAsyncCommandIsCached()
        {
            var vm = new TestViewModel();

            Assert.AreEqual( vm.AsyncIncrementCommand, vm.AsyncIncrementCommand );
        }

        [TestMethod]
        public void GenericGetAsyncCommandIsCached()
        {
            var vm = new TestViewModel();

            Assert.AreEqual( vm.AsyncIncrementByCommand, vm.AsyncIncrementByCommand );
        }

        [TestMethod]
        public void OnNavigatedToWorks()
        {
            new TestViewModel().OnNavigatedTo();
        }

        [TestMethod]
        public void OnNavigatedFromWorks()
        {
            new TestViewModel().OnNavigatedFrom();
        }
    }
}