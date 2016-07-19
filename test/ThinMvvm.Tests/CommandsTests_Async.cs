using System;
using System.Threading.Tasks;
using System.Windows.Input;
using ThinMvvm.Tests.TestInfrastructure;
using Xunit;

namespace ThinMvvm.Tests
{
    /// <summary>
    /// Tests for <see cref="AsyncCommand" />.
    /// </summary>
    public sealed class CommandsTests_Async
    {
        [Fact]
        public void ConstructorThrowsIfExecuteIsNull()
        {
            Assert.Throws<ArgumentNullException>( () => new AsyncCommand( null ) );
        }

        [Fact]
        public async Task ExecuteInvokesTheFunction()
        {
            bool fired = false;
            var command = new AsyncCommand( () => { fired = true; return TaskEx.CompletedTask; } );

            await command.ExecuteAsync();

            Assert.True( fired );
        }

        [Fact]
        public async Task ExecuteInvokesTheFunctionAsManyTimesAsItIsInvoked()
        {
            int counter = 0;
            var command = new AsyncCommand( () => { counter++; return TaskEx.CompletedTask; } );

            for( int n = 0; n < 10; n++ )
            {
                await command.ExecuteAsync();
            }

            Assert.Equal( 10, counter );
        }

        [Fact]
        public void CanExecuteReturnsTrueIfNotProvided()
        {
            var command = new AsyncCommand( () => TaskEx.CompletedTask );

            bool result = command.CanExecute();

            Assert.True( result );
        }

        [Fact]
        public void CanExecuteInvokesTheFunctionWhenProvided()
        {
            bool result = false;
            var command = new AsyncCommand( () => TaskEx.CompletedTask, () => result );

            bool actual1 = command.CanExecute();

            Assert.False( actual1 );


            result = true;

            bool actual2 = command.CanExecute();

            Assert.True( actual2 );
        }

        [Fact]
        public async Task CanExecuteIsFalseWhileExecuteIsRunning()
        {
            var source = new TaskCompletionSource<int>();
            var command = new AsyncCommand( () => source.Task );


            var task = command.ExecuteAsync();

            bool resultBefore = command.CanExecute();

            Assert.False( resultBefore );


            source.SetResult( 0 );
            await task;

            bool resultAfter = command.CanExecute();

            Assert.True( resultAfter );
        }


        [Fact]
        public void ICommandExecuteInvokesTheFunction()
        {
            bool fired = false;
            var command = new AsyncCommand( () => { fired = true; return TaskEx.CompletedTask; } );

            ( (ICommand) command ).Execute( null );

            Assert.True( fired );
        }

        [Fact]
        public void ICommandCanExecuteReturnsTrueIfNotProvided()
        {
            var command = new AsyncCommand( () => TaskEx.CompletedTask );

            bool result = ( (ICommand) command ).CanExecute( null );

            Assert.True( result );
        }

        [Fact]
        public void ICommandCanExecuteInvokesTheFunctionWhenProvided()
        {
            bool result = false;
            var command = new AsyncCommand( () => TaskEx.CompletedTask, () => result );

            bool actual1 = ( (ICommand) command ).CanExecute( null );

            Assert.False( actual1 );


            result = true;

            bool actual2 = ( (ICommand) command ).CanExecute( null );

            Assert.True( actual2 );
        }

        [Fact]
        public async Task ExecuteTriggersExecuted()
        {
            var command = new AsyncCommand( () => TaskEx.CompletedTask );
            bool fired = false;
            command.Executed += ( _, __ ) => fired = true;

            await command.ExecuteAsync();

            Assert.True( fired );
        }

        [Fact]
        public async Task ExecutedHasNullArgument()
        {
            var command = new AsyncCommand( () => TaskEx.CompletedTask );
            object arg = null;
            command.Executed += ( _, e ) => arg = e.Argument;

            await command.ExecuteAsync();

            Assert.Null( arg );
        }

        [Fact]
        public void ICommandExecuteTriggersExecuted()
        {
            var command = new AsyncCommand( () => TaskEx.CompletedTask );
            bool fired = false;
            command.Executed += ( _, __ ) => fired = true;

            ( (ICommand) command ).Execute( null );

            Assert.True( fired );
        }

        [Fact]
        public void ICommandExecuteDoesNotPassArgumentToExecuted()
        {
            var command = new AsyncCommand( () => TaskEx.CompletedTask );
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