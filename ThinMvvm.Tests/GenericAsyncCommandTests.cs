// Copyright (c) 2014-15 Solal Pirelli
// See License.txt file for more details

using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ThinMvvm.Tests
{
    [TestClass]
    public sealed class GenericAsyncCommandTests
    {
        [TestMethod]
        public async Task ExecuteAsyncCallsTheProvidedExecuteMethod()
        {
            int count = 0;
            bool paramOk = false;
            object obj = new object();
            var cmd = new AsyncCommand<object>( null, o =>
            {
                count++;
                if ( o == obj )
                {
                    paramOk = true;
                }
                return Task.FromResult( 0 );
            } );

            await cmd.ExecuteAsync( obj );

            Assert.AreEqual( 1, count, "ExecuteAsync() should call the provided 'execute' parameter exactly once." );
            Assert.AreEqual( true, paramOk, "ExecuteAsync() should provide the correct parameter." );
        }

        [TestMethod]
        public void ICommandExecuteCallsTheProvidedExecuteMethod()
        {
            int count = 0;
            bool paramOk = false;
            object obj = new object();
            var cmd = new AsyncCommand<object>( null, o =>
            {
                count++;
                if ( o == obj )
                {
                    paramOk = true;
                }
                return Task.FromResult( 0 );
            } );

            ( (ICommand) cmd ).Execute( obj );

            Assert.AreEqual( 1, count, "ICommand.Execute() should call the provided 'execute' parameter exactly once." );
            Assert.AreEqual( true, paramOk, "ICommand.Execute() should provide the correct parameter." );
        }

        [TestMethod]
        public void CanExecuteIsTrueWhenNotProvided()
        {
            var cmd = new AsyncCommand<object>( null, _ => Task.FromResult( 0 ) );

            Assert.AreEqual( true, cmd.CanExecute( null ), "CanExecute() should return true when the 'canExecute' parameter is not provided." );
        }

        [TestMethod]
        public void CanExecuteCallsTheProvidedCanExecuteMethod()
        {
            object obj = new object();
            var cmd = new AsyncCommand<object>( null, _ => Task.FromResult( 0 ), o => o == obj );

            Assert.AreEqual( false, cmd.CanExecute( new object() ), "CanExecute() should call the provided 'canExecute' parameter with the correct parameter." );
            Assert.AreEqual( true, cmd.CanExecute( obj ), "CanExecute() should call the provided 'canExecute' parameter with the correct parameter." );
        }

        [TestMethod]
        public void ICommandCanExecuteCallsTheProvidedCanExecuteMethod()
        {
            object obj = new object();
            var cmd = new AsyncCommand<object>( null, _ => Task.FromResult( 0 ), o => o == obj );

            Assert.AreEqual( false, ( (ICommand) cmd ).CanExecute( new object() ), "ICommand.CanExecute() should call the provided 'canExecute' parameter." );
            Assert.AreEqual( true, ( (ICommand) cmd ).CanExecute( obj ), "ICommand.CanExecute() should call the provided 'canExecute' parameter." );
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
                var cmd = new AsyncCommand<object>( null, _ => Task.FromResult( 0 ), _ => Value == 0 );
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
            var cmd = new AsyncCommand<object>( null, _ => Task.FromResult( 0 ), _ => ex.Value == 1 );
            int count = 0;

            cmd.CanExecuteChanged += ( s, e ) => count++;
            ex.Value++;

            Assert.AreEqual( 1, count, "CanExecuteChanged should be fired exactly once when a property it uses changes, even in a closure." );
        }

        [TestMethod]
        public void CanExecuteShouldWorkWithConstants()
        {
            new AsyncCommand<object>( null, _ => Task.FromResult( 0 ), _ => true );
        }

        [TestMethod]
        public void CanExecuteIsAlwaysFalseIfTypeDoesNotMatch()
        {
            var cmd = new AsyncCommand<int>( null, _ => Task.FromResult( 0 ) );

            Assert.IsFalse( ( (ICommand) cmd ).CanExecute( "abc" ) );
        }

        [TestMethod]
        public async Task ErrorOnExecuteIfTypeDoesNotMatch()
        {
            bool threw = false;
            // required to catch exceptions thrown from async void
            UnhandledExceptionEventHandler handler = ( _, e ) =>
            {
                threw = true;
                Assert.IsInstanceOfType( e.ExceptionObject, typeof( ArgumentException ) );
            };
            AppDomain.CurrentDomain.UnhandledException += handler;

            ( (ICommand) new AsyncCommand<int>( null, _ => Task.FromResult( 0 ) ) ).Execute( "abc" );

            await Task.Delay( 100 );

            Assert.IsTrue( threw );

            AppDomain.CurrentDomain.UnhandledException -= handler;
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

            var cmd = new AsyncCommand<int>( null, n => { throw new InvalidTimeZoneException(); } );

            ( (ICommand) cmd ).Execute( 0 );
            await Task.Delay( 100 );

            Assert.IsTrue( threw );

            AppDomain.CurrentDomain.UnhandledException -= handler;
        }
    }
}