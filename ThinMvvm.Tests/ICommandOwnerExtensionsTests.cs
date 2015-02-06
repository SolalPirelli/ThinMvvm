// Copyright (c) 2014-15 Solal Pirelli
// See License.txt file for more details

using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ThinMvvm.Tests
{
    [TestClass]
    public sealed class ICommandOwnerExtensionsTests
    {
        private class TestCommandOwner : ICommandOwner
        {
            public int Counter { get; private set; }

            public Command IncrementCommand
            {
                get { return this.GetCommand( () => Counter++ ); }
            }

            public Command<int> IncrementByCommand
            {
                get { return this.GetCommand<int>( n => Counter += n ); }
            }

            public AsyncCommand AsyncIncrementCommand
            {
                get { return this.GetAsyncCommand( () => { Counter++; return Task.FromResult( 0 ); } ); }
            }

            public AsyncCommand<int> AsyncIncrementByCommand
            {
                get { return this.GetAsyncCommand<int>( n => { Counter += n; return Task.FromResult( 0 ); } ); }
            }
        }

        [TestMethod]
        public void GetCommandWorks()
        {
            var owner = new TestCommandOwner();

            owner.IncrementCommand.Execute();

            Assert.AreEqual( 1, owner.Counter );
        }

        [TestMethod]
        public void GenericGetCommandWorks()
        {
            var owner = new TestCommandOwner();

            owner.IncrementByCommand.Execute( 2 );

            Assert.AreEqual( 2, owner.Counter );
        }

        [TestMethod]
        public async Task GetAsyncCommandWorks()
        {
            var owner = new TestCommandOwner();

            await owner.AsyncIncrementCommand.ExecuteAsync();

            Assert.AreEqual( 1, owner.Counter );
        }

        [TestMethod]
        public async Task GenericGetAsyncCommandWorks()
        {
            var owner = new TestCommandOwner();

            await owner.AsyncIncrementByCommand.ExecuteAsync( 2 );

            Assert.AreEqual( 2, owner.Counter );
        }

        [TestMethod]
        public void GetCommandIsCached()
        {
            var owner = new TestCommandOwner();

            Assert.AreEqual( owner.IncrementCommand, owner.IncrementCommand );
        }

        [TestMethod]
        public void GenericGetCommandIsCached()
        {
            var owner = new TestCommandOwner();

            Assert.AreEqual( owner.IncrementByCommand, owner.IncrementByCommand );
        }

        [TestMethod]
        public void GetAsyncCommandIsCached()
        {
            var owner = new TestCommandOwner();

            Assert.AreEqual( owner.AsyncIncrementCommand, owner.AsyncIncrementCommand );
        }

        [TestMethod]
        public void GenericGetAsyncCommandIsCached()
        {
            var owner = new TestCommandOwner();

            Assert.AreEqual( owner.AsyncIncrementByCommand, owner.AsyncIncrementByCommand );
        }

        [TestMethod]
        [ExpectedException( typeof( ArgumentNullException ) )]
        public void GetCommandValidatesOwnerParameter()
        {
            ICommandOwnerExtensions.GetCommand( null, () => { } );
        }

        [TestMethod]
        [ExpectedException( typeof( ArgumentNullException ) )]
        public void GetCommandValidatesActionParameter()
        {
            ICommandOwnerExtensions.GetCommand( new TestCommandOwner(), null );
        }

        [TestMethod]
        [ExpectedException( typeof( ArgumentNullException ) )]
        public void GenericGetCommandValidatesOwnerParameter()
        {
            ICommandOwnerExtensions.GetCommand<int>( null, n => { } );
        }

        [TestMethod]
        [ExpectedException( typeof( ArgumentNullException ) )]
        public void GenericGetCommandValidatesActionParameter()
        {
            ICommandOwnerExtensions.GetCommand<int>( new TestCommandOwner(), null );
        }

        [TestMethod]
        [ExpectedException( typeof( ArgumentNullException ) )]
        public void GetAsyncCommandValidatesOwnerParameter()
        {
            ICommandOwnerExtensions.GetAsyncCommand( null, () => Task.FromResult( 0 ) );
        }

        [TestMethod]
        [ExpectedException( typeof( ArgumentNullException ) )]
        public void GetAsyncCommandValidatesActionParameter()
        {
            ICommandOwnerExtensions.GetAsyncCommand( new TestCommandOwner(), null );
        }

        [TestMethod]
        [ExpectedException( typeof( ArgumentNullException ) )]
        public void GenericGetAsyncCommandValidatesOwnerParameter()
        {
            ICommandOwnerExtensions.GetAsyncCommand<int>( null, n => Task.FromResult( 0 ) );
        }

        [TestMethod]
        [ExpectedException( typeof( ArgumentNullException ) )]
        public void GenericGetAsyncCommandValidatesActionParameter()
        {
            ICommandOwnerExtensions.GetAsyncCommand<int>( new TestCommandOwner(), null );
        }
    }
}