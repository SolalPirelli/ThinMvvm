using System;
using System.Windows.Input;
using Xunit;

namespace ThinMvvm.Tests
{
    public sealed class CommandsTests_Sync
    {
        [Fact]
        public void ConstructorThrowsIfExecuteIsNull()
        {
            Assert.Throws<ArgumentNullException>( () => new Command( null ) );
        }

        [Fact]
        public void ExecuteInvokesTheFunction()
        {
            bool fired = false;
            var command = new Command( () => fired = true );
            command.Execute();

            Assert.True( fired );
        }

        [Fact]
        public void ExecuteInvokesTheFunctionAsManyTimesAsItIsInvoked()
        {
            int counter = 0;
            var command = new Command( () => counter++ );

            for( int n = 0; n < 10; n++ )
            {
                command.Execute();
            }

            Assert.Equal( 10, counter );
        }

        [Fact]
        public void CanExecuteReturnsTrueIfNotProvided()
        {
            var command = new Command( () => { } );

            bool result = command.CanExecute();

            Assert.True( result );
        }

        [Fact]
        public void CanExecuteInvokesTheFunctionWhenProvided()
        {
            bool result = false;
            var command = new Command( () => { }, () => result );

            bool actual1 = command.CanExecute();

            Assert.False( actual1 );


            result = true;

            bool actual2 = command.CanExecute();

            Assert.True( actual2 );
        }

        [Fact]
        public void ICommandExecuteInvokesTheFunction()
        {
            bool fired = false;
            var command = new Command( () => fired = true );

            ( (ICommand) command ).Execute( null );

            Assert.True( fired );
        }

        [Fact]
        public void ICommandCanExecuteReturnsTrueIfNotProvided()
        {
            var command = new Command( () => { } );

            bool result = ( (ICommand) command ).CanExecute( null );

            Assert.True( result );
        }

        [Fact]
        public void ICommandCanExecuteInvokesTheFunctionWhenProvided()
        {
            bool result = false;
            var command = new Command( () => { }, () => result );

            bool actual1 = ( (ICommand) command ).CanExecute( null );

            Assert.False( actual1 );


            result = true;

            bool actual2 = ( (ICommand) command ).CanExecute( null );

            Assert.True( actual2 );
        }

        [Fact]
        public void ExecuteTriggersExecuted()
        {
            var command = new Command( () => { } );
            bool fired = false;
            command.Executed += ( _, __ ) => fired = true;

            command.Execute();

            Assert.True( fired );
        }

        [Fact]
        public void ExecutedHasNullArgument()
        {
            var command = new Command( () => { } );
            object arg = null;
            command.Executed += ( _, e ) => arg = e.Argument;

            command.Execute();

            Assert.Null( arg );
        }

        [Fact]
        public void ICommandExecuteTriggersExecuted()
        {
            var command = new Command( () => { } );
            bool fired = false;
            command.Executed += ( _, __ ) => fired = true;

            ( (ICommand) command ).Execute( null );

            Assert.True( fired );
        }

        [Fact]
        public void ICommandExecuteDoesNotPassArgumentToExecuted()
        {
            var command = new Command( () => { } );
            object arg = null;
            command.Executed += ( _, e ) => arg = e.Argument;

            ( (ICommand) command ).Execute( 42 );

            Assert.Null( arg );
        }

        [Fact]
        public void OnCanExecuteChangedWorksCorrectly()
        {
            var command = new Command( () => { } );
            bool fired = false;
            command.CanExecuteChanged += ( _, __ ) => fired = true;

            command.OnCanExecuteChanged();

            Assert.True( fired );
        }

        [Fact]
        public void OnCanExecuteChangedWorksCorrectlyWithoutListeners()
        {
            var command = new Command( () => { } );

            command.OnCanExecuteChanged();
        }
    }
}