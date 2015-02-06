// Copyright (c) 2014-15 Solal Pirelli
// See License.txt file for more details

using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ThinMvvm.Tests
{
    [TestClass]
    public sealed class AsyncCommandTests
    {
        [TestMethod]
        public async Task ExecuteAsyncCallsTheProvidedExecuteMethod()
        {
            int count = 0;
            var cmd = new AsyncCommand( null, () => { count++; return Task.FromResult( 0 ); } );

            await cmd.ExecuteAsync();

            Assert.AreEqual( 1, count, "ExecuteAsync() should call the provided 'execute' parameter exactly once." );
        }

        [TestMethod]
        public void ICommandExecuteCallsTheProvidedExecuteMethod()
        {
            int count = 0;
            var cmd = new AsyncCommand( null, () => { count++; return Task.FromResult( 0 ); } );

            ( (ICommand) cmd ).Execute( null );

            Assert.AreEqual( 1, count, "ICommand.Execute() should call the provided 'execute' parameter exactly once." );
        }

        [TestMethod]
        public void CanExecuteIsTrueWhenNotProvided()
        {
            var cmd = new AsyncCommand( null, () => Task.FromResult( 0 ) );

            Assert.AreEqual( true, cmd.CanExecute(), "CanExecute() should return true when the 'canExecute' parameter is not provided." );
        }

        [TestMethod]
        public void CanExecuteCallsTheProvidedCanExecuteMethod()
        {
            int n = 0;
            var cmd = new AsyncCommand( null, () => Task.FromResult( 0 ), () => n == 42 );

            Assert.AreEqual( false, cmd.CanExecute(), "CanExecute() should call the provided 'canExecute' parameter." );
            n = 42;
            Assert.AreEqual( true, cmd.CanExecute(), "CanExecute() should call the provided 'canExecute' parameter." );
        }

        [TestMethod]
        public void ICommandCanExecuteCallsTheProvidedCanExecuteMethod()
        {
            int n = 0;
            var cmd = new AsyncCommand( null, () => Task.FromResult( 0 ), () => n == 42 );

            Assert.AreEqual( false, ( (ICommand) cmd ).CanExecute( null ), "ICommand.CanExecute() should call the provided 'canExecute' parameter." );
            n = 42;
            Assert.AreEqual( true, ( (ICommand) cmd ).CanExecute( null ), "ICommand.CanExecute() should call the provided 'canExecute' parameter." );
        }

        private sealed class InpcExample : ObservableObject
        {
            private int _value;

            public int Value
            {
                get { return _value; }
                set { SetProperty( ref _value, value ); }
            }

            public void TestAsyncCommand()
            {
                var cmd = new AsyncCommand( null, () => Task.FromResult( 0 ), () => Value == 0 );
                int count = 0;

                cmd.CanExecuteChanged += ( s, e ) => count++;
                Value++;

                Assert.AreEqual( 1, count, "CanExecuteChanged should be fired exactly once when a property it uses changes." );
            }
        }

        [TestMethod]
        public void CanExecuteChangedShouldBeFiredWhenAPropertyChanges()
        {
            new InpcExample().TestAsyncCommand();
        }

        [TestMethod]
        public void CanExecuteChangedShouldBeFiredWhenAPropertyOfAFieldChanges()
        {
            var ex = new InpcExample();
            var cmd = new AsyncCommand( null, () => Task.FromResult( 0 ), () => ex.Value == 1 );
            int count = 0;

            cmd.CanExecuteChanged += ( s, e ) => count++;
            ex.Value++;

            Assert.AreEqual( 1, count, "CanExecuteChanged should be fired exactly once when a property it uses changes, even in a closure." );
        }

        [TestMethod]
        public void CanExecuteShouldWorkWithConstants()
        {
            new AsyncCommand( null, () => Task.FromResult( 0 ), () => true );
        }

        [TestMethod]
        public async Task ExecuteThrowsWhenExecuteAsyncThrows()
        {
            bool threw = false;
            UnhandledExceptionEventHandler handler = ( s, e ) =>
            {
                threw = true;
                Assert.IsInstanceOfType( e.ExceptionObject, typeof( InvalidTimeZoneException ) );
            };
            AppDomain.CurrentDomain.UnhandledException += handler;

            var cmd = new AsyncCommand( null, () => { throw new InvalidTimeZoneException(); } );

            ( (ICommand) cmd ).Execute( null );
            await Task.Delay( 100 );

            Assert.IsTrue( threw );

            AppDomain.CurrentDomain.UnhandledException -= handler;
        }
    }
}