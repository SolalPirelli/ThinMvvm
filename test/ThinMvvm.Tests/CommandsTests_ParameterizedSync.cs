using System;
using System.Windows.Input;
using Xunit;

namespace ThinMvvm.Tests
{
    /// <summary>
    /// Tests for <see cref="Command{T}" />.
    /// </summary>
    public sealed class CommandsTests_ParameterizedSync
    {
        [Fact]
        public void ConstructorThrowsIfExecuteIsNull()
        {
            Assert.Throws<ArgumentNullException>( () => new Command<int>( null ) );
        }

        [Fact]
        public void ExecuteInvokesTheFunction()
        {
            int value = 0;
            var command = new Command<int>( n => value = n );

            command.Execute( 42 );

            Assert.Equal( 42, value );
        }

        [Fact]
        public void ExecuteInvokesTheFunctionAsManyTimesAsItIsInvoked()
        {
            int counter = 0;
            var command = new Command<int>( n => counter += n );

            for( int n = 0; n < 10; n++ )
            {
                command.Execute( 10 );
            }

            Assert.Equal( 100, counter );
        }

        [Fact]
        public void CanExecuteReturnsTrueIfNotProvided()
        {
            var command = new Command<int>( _ => { } );

            bool result = command.CanExecute( 0 );

            Assert.True( result );
        }

        [Fact]
        public void CanExecuteInvokesTheFunctionWhenProvided()
        {
            var command = new Command<int>( _ => { }, n => n % 2 == 0 );

            bool actual1 = command.CanExecute( 1 );

            Assert.False( actual1 );


            bool actual2 = command.CanExecute( 2 );

            Assert.True( actual2 );
        }

        [Fact]
        public void ICommandExecuteInvokesTheFunction()
        {
            bool fired = false;
            var command = new Command<int>( _ => fired = true );

            ( (ICommand) command ).Execute( 0 );

            Assert.True( fired );
        }

        [Fact]
        public void ICommandCanExecuteReturnsTrueIfNotProvided()
        {
            var command = new Command<int>( _ => { } );

            bool result = ( (ICommand) command ).CanExecute( 0 );

            Assert.True( result );
        }

        [Fact]
        public void ICommandCanExecuteInvokesTheFunctionWhenProvided()
        {
            var command = new Command<int>( _ => { }, n => n % 2 == 0 );


            bool actual1 = ( (ICommand) command ).CanExecute( 1 );

            Assert.False( actual1 );


            bool actual2 = ( (ICommand) command ).CanExecute( 2 );

            Assert.True( actual2 );
        }

        [Fact]
        public void ICommandExecuteThrowsWhenParameterIsOfWrongType()
        {
            var command = new Command<int>( _ => { } );

            Assert.Throws<InvalidCastException>( () => ( (ICommand) command ).Execute( "X" ) );
        }

        [Fact]
        public void ICommandCanExecuteThrowsWhenParameterIsOfWrongType()
        {
            var command = new Command<int>( _ => { } );

            Assert.Throws<InvalidCastException>( () => ( (ICommand) command ).CanExecute( "X" ) );
        }

        [Fact]
        public void ExecuteTriggersExecuted()
        {
            var command = new Command<int>( _ => { } );
            bool fired = false;
            command.Executed += ( _, __ ) => fired = true;

            command.Execute( 42 );

            Assert.True( fired );
        }

        [Fact]
        public void ExecutedHasCorrectArgument()
        {
            var command = new Command<int>( _ => { } );
            object arg = null;
            command.Executed += ( _, e ) => arg = e.Argument;

            command.Execute( 42 );

            Assert.Equal( 42, arg );
        }

        [Fact]
        public void ICommandExecuteTriggersExecuted()
        {
            var command = new Command<int>( _ => { } );
            bool fired = false;
            command.Executed += ( _, __ ) => fired = true;

            ( (ICommand) command ).Execute( 42 );

            Assert.True( fired );
        }

        [Fact]
        public void ICommandExecutePassesArgumentToExecuted()
        {
            var command = new Command<int>( _ => { } );
            object arg = null;
            command.Executed += ( _, e ) => arg = e.Argument;

            ( (ICommand) command ).Execute( 42 );

            Assert.Equal( 42, arg );
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