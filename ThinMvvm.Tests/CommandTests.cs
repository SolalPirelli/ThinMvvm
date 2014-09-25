// Copyright (c) Solal Pirelli 2014
// See License.txt file for more details

using System.Windows.Input;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ThinMvvm.Tests
{
    [TestClass]
    public sealed class CommandTests
    {
        [TestMethod]
        public void ExecuteCallsTheProvidedExecuteMethod()
        {
            int count = 0;
            var cmd = new Command( null, () => count++ );

            cmd.Execute();

            Assert.AreEqual( 1, count, "Execute() should call the provided 'execute' parameter exactly once." );
        }

        [TestMethod]
        public void ICommandExecuteCallsTheProvidedExecuteMethod()
        {
            int count = 0;
            var cmd = new Command( null, () => count++ );

            ( (ICommand) cmd ).Execute( null );

            Assert.AreEqual( 1, count, "ICommand.Execute() should call the provided 'execute' parameter exactly once." );
        }

        [TestMethod]
        public void CanExecuteIsTrueWhenNotProvided()
        {
            var cmd = new Command( null, () => { } );

            Assert.AreEqual( true, cmd.CanExecute(), "CanExecute() should return true when the 'canExecute' parameter is not provided." );
        }

        [TestMethod]
        public void CanExecuteCallsTheProvidedCanExecuteMethod()
        {
            int n = 0;
            var cmd = new Command( null, () => { }, () => n == 42 );

            Assert.AreEqual( false, cmd.CanExecute(), "CanExecute() should call the provided 'canExecute' parameter." );
            n = 42;
            Assert.AreEqual( true, cmd.CanExecute(), "CanExecute() should call the provided 'canExecute' parameter." );
        }

        [TestMethod]
        public void ICommandCanExecuteCallsTheProvidedCanExecuteMethod()
        {
            int n = 0;
            var cmd = new Command( null, () => { }, () => n == 42 );

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

            private InpcExample Other { get; set; }

            public void TestCommand()
            {
                var cmd = new Command( null, () => { }, () => Value == 0 );
                int count = 0;

                cmd.CanExecuteChanged += ( s, e ) => count++;
                Value++;

                Assert.AreEqual( 1, count, "CanExecuteChanged should be fired exactly once when a property it uses changes." );
            }

            public void TestCommand2()
            {
                Other = new InpcExample();
                var cmd = new Command( null, () => { }, () => Other.Value == 0 );
                int count = 0;

                cmd.CanExecuteChanged += ( s, e ) => count++;
                Other.Value++;

                Assert.AreEqual( 1, count, "CanExecuteChanged should be fired exactly once when a property it uses changes." );
            }
        }

        [TestMethod]
        public void CanExecuteChangedShouldBeFiredWhenAPropertyChanges()
        {
            new InpcExample().TestCommand();
        }

        [TestMethod]
        public void CanExecuteChangedShouldBeFiredWhenNestedAPropertyChanges()
        {
            new InpcExample().TestCommand2();
        }

        [TestMethod]
        public void CanExecuteChangedShouldBeFiredWhenAPropertyOfAFieldChanges()
        {
            var ex = new InpcExample();
            var cmd = new Command( null, () => { }, () => ex.Value == 1 );
            int count = 0;

            cmd.CanExecuteChanged += ( s, e ) => count++;
            ex.Value++;

            Assert.AreEqual( 1, count, "CanExecuteChanged should be fired exactly once when a property it uses changes, even in a closure." );
        }

        [TestMethod]
        public void CanExecuteShouldWorkWithConstants()
        {
            new Command( null, () => { }, () => true );
        }
    }
}