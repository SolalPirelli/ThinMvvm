using System;
using System.Threading.Tasks;
using System.Windows.Input;
using ThinMvvm.Tests.TestInfrastructure;
using Xunit;

namespace ThinMvvm.Tests
{
    public sealed class CommandsTests_ParameterizedAsync
    {
        [Fact]
        public void ConstructorThrowsIfExecuteIsNull()
        {
            Assert.Throws<ArgumentNullException>( () => new AsyncCommand<int>( null ) );
        }

        [Fact]
        public async Task ExecuteInvokesTheFunction()
        {
            int value = 0;
            var command = new AsyncCommand<int>( n => { value = n; return TaskEx.CompletedTask; } );

            await command.ExecuteAsync( 42 );

            Assert.Equal( 42, value );
        }

        [Fact]
        public async Task ExecuteInvokesTheFunctionAsManyTimesAsItIsInvoked()
        {
            int counter = 0;
            var command = new AsyncCommand<int>( n => { counter += n; return TaskEx.CompletedTask; } );

            for( int n = 0; n < 10; n++ )
            {
                await command.ExecuteAsync( 10 );
            }

            Assert.Equal( 100, counter );
        }

        [Fact]
        public void CanExecuteReturnsTrueIfNotProvided()
        {
            var command = new AsyncCommand<int>( _ => TaskEx.CompletedTask );

            bool result = command.CanExecute( 0 );

            Assert.True( result );
        }

        [Fact]
        public void CanExecuteInvokesTheFunctionWhenProvided()
        {
            var command = new AsyncCommand<int>( _ => TaskEx.CompletedTask, n => n % 2 == 0 );

            bool actual1 = command.CanExecute( 1 );

            Assert.False( actual1 );


            bool actual2 = command.CanExecute( 2 );

            Assert.True( actual2 );
        }

        [Fact]
        public async Task CanExecuteIsFalseWhileExecuteIsRunning()
        {
            var source = new TaskCompletionSource<int>();
            var command = new AsyncCommand<int>( _ => source.Task );


            var task = command.ExecuteAsync( 0 );

            bool resultBefore = command.CanExecute( 0 );

            Assert.False( resultBefore );


            source.SetResult( 0 );
            await task;

            bool resultAfter = command.CanExecute( 0 );

            Assert.True( resultAfter );
        }


        [Fact]
        public void ICommandExecuteInvokesTheFunction()
        {
            int value = 0;
            var command = new AsyncCommand<int>( n => { value = n; return TaskEx.CompletedTask; } );

            ( (ICommand) command ).Execute( 42 );

            Assert.Equal( 42, value );
        }

        [Fact]
        public void ICommandCanExecuteReturnsTrueIfNotProvided()
        {
            var command = new AsyncCommand<int>( _ => TaskEx.CompletedTask );

            bool result = ( (ICommand) command ).CanExecute( 0 );

            Assert.True( result );
        }

        [Fact]
        public void ICommandCanExecuteInvokesTheFunctionWhenProvided()
        {
            var command = new AsyncCommand<int>( _ => TaskEx.CompletedTask, n => n % 2 == 0 );

            bool actual1 = ( (ICommand) command ).CanExecute( 1 );

            Assert.False( actual1 );


            bool actual2 = ( (ICommand) command ).CanExecute( 2 );

            Assert.True( actual2 );
        }

        [Fact]
        public void ICommandExecuteThrowsWhenParameterIsOfWrongType()
        {
            var command = new AsyncCommand<int>( _ => TaskEx.CompletedTask );

            Assert.Throws<InvalidCastException>( () => ( (ICommand) command ).Execute( "X" ) );
        }

        [Fact]
        public void ICommandCanExecuteThrowsWhenParameterIsOfWrongType()
        {
            var command = new AsyncCommand<int>( _ => TaskEx.CompletedTask );

            Assert.Throws<InvalidCastException>( () => ( (ICommand) command ).CanExecute( "X" ) );
        }

        [Fact]
        public async Task ExecuteTriggersExecuted()
        {
            var command = new AsyncCommand<int>( _ => TaskEx.CompletedTask );
            bool fired = false;
            command.Executed += ( _, __ ) => fired = true;

            await command.ExecuteAsync( 0 );

            Assert.True( fired );
        }

        [Fact]
        public async Task ExecutedHasNullArgument()
        {
            var command = new AsyncCommand<int>( _ => TaskEx.CompletedTask );
            object arg = null;
            command.Executed += ( _, e ) => arg = e.Argument;

            await command.ExecuteAsync( 42 );

            Assert.Equal( 42, arg );
        }

        [Fact]
        public void ICommandExecuteTriggersExecuted()
        {
            var command = new AsyncCommand<int>( _ => TaskEx.CompletedTask );
            bool fired = false;
            command.Executed += ( _, __ ) => fired = true;

            ( (ICommand) command ).Execute( 0 );

            Assert.True( fired );
        }

        [Fact]
        public void ICommandExecuteDoesNotPassArgumentToExecuted()
        {
            var command = new AsyncCommand<int>( _ => TaskEx.CompletedTask );
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