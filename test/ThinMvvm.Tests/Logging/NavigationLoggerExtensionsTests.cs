using System;
using System.Threading.Tasks;
using ThinMvvm.Infrastructure;
using ThinMvvm.Logging;
using ThinMvvm.Tests.TestInfrastructure;
using Xunit;

namespace ThinMvvm.Tests.Logging
{
    public sealed class NavigationLoggerExtensionsTests
    {
        private sealed class MyViewModel : ViewModel<NoParameter> { }

        private sealed class Logger : ILogger
        {
            public string Event;

            public void LogNavigation( string viewModelId, bool isArriving )
            {
                Log( $"NAVIGATION: {viewModelId} {isArriving}" );
            }

            public void LogEvent( string viewModelId, string eventId, string label )
            {
                Log( $"EVENT: {viewModelId} {eventId} {( label == null ? "-" : label )}" );
            }

            public void LogError( string name, Exception exception )
            {
                throw new NotSupportedException();
            }

            private void Log( string text )
            {
                if( Event != null )
                {
                    throw new InvalidOperationException( "Already logged one event." );
                }

                Event = text;
            }
        }

        [Fact]
        public void CannotRegisterWithNullLogger()
        {
            Assert.Throws<ArgumentNullException>( () => NavigationLoggerExtensions.Register( null, new MyViewModel(), "VM" ) );
        }

        [Fact]
        public void CannotRegisterWithNullViewModel()
        {
            Assert.Throws<ArgumentNullException>( () => NavigationLoggerExtensions.Register( new Logger(), null, "VM" ) );
        }

        [Fact]
        public void CannotRegisterWithNullId()
        {
            Assert.Throws<ArgumentNullException>( () => NavigationLoggerExtensions.Register( new Logger(), new MyViewModel(), null ) );
        }

        [Fact]
        public async Task NavigatedTo()
        {
            IViewModel vm = new MyViewModel();
            var logger = new Logger();

            logger.Register( vm, "myVM" );

            await vm.OnNavigatedToAsync( NavigationKind.Forwards );

            Assert.Equal( "NAVIGATION: myVM True", logger.Event );
        }

        [Fact]
        public async Task NavigatedFrom()
        {
            IViewModel vm = new MyViewModel();
            var logger = new Logger();

            logger.Register( vm, "myVM" );

            await vm.OnNavigatedFromAsync( NavigationKind.Forwards );

            Assert.Equal( "NAVIGATION: myVM False", logger.Event );
        }

        [Fact]
        public void CannotRegisterNullParameterlessCommand()
        {
            var registration = new Logger().Register( new MyViewModel(), "myVM" );

            Assert.Throws<ArgumentNullException>( () => registration.WithCommand( (Command) null, "myCommand" ) );
        }

        [Fact]
        public void CannotRegisterParameterlessCommandWithNullId()
        {
            var registration = new Logger().Register( new MyViewModel(), "myVM" );

            Assert.Throws<ArgumentNullException>( () => registration.WithCommand( new Command( () => { } ), null ) );
        }

        [Fact]
        public void ParameterlessCommandWithoutLabel()
        {
            var vm = new MyViewModel();
            var logger = new Logger();
            var command = new Command( () => { } );

            logger.Register( vm, "myVM" )
                .WithCommand( command, "myCommand" );

            command.Execute();

            Assert.Equal( "EVENT: myVM myCommand -", logger.Event );
        }

        [Fact]
        public void ParameterlessCommandWithLabel()
        {
            var vm = new MyViewModel();
            var logger = new Logger();
            var command = new Command( () => { } );

            logger.Register( vm, "myVM" )
                .WithCommand( command, "myCommand", () => "hello" );

            command.Execute();

            Assert.Equal( "EVENT: myVM myCommand hello", logger.Event );
        }

        [Fact]
        public void CannotRegisterNullCommand()
        {
            var registration = new Logger().Register( new MyViewModel(), "myVM" );

            Assert.Throws<ArgumentNullException>( () => registration.WithCommand( (Command<int>) null, "myCommand" ) );
        }

        [Fact]
        public void CannotRegisterCommandWithNullId()
        {
            var registration = new Logger().Register( new MyViewModel(), "myVM" );

            Assert.Throws<ArgumentNullException>( () => registration.WithCommand( new Command<int>( _ => { } ), null ) );
        }

        [Fact]
        public void CommandWithoutLabel()
        {
            var vm = new MyViewModel();
            var logger = new Logger();
            var command = new Command<int>( _ => { } );

            logger.Register( vm, "myVM" )
                .WithCommand( command, "myCommand" );

            command.Execute( 42 );

            Assert.Equal( "EVENT: myVM myCommand -", logger.Event );
        }

        [Fact]
        public void CommandWithLabel()
        {
            var vm = new MyViewModel();
            var logger = new Logger();
            var command = new Command<int>( _ => { } );

            logger.Register( vm, "myVM" )
                .WithCommand( command, "myCommand", n => $"n:{n}" );

            command.Execute( 42 );

            Assert.Equal( "EVENT: myVM myCommand n:42", logger.Event );
        }

        [Fact]
        public void CannotRegisterNullParameterlessAsyncCommand()
        {
            var registration = new Logger().Register( new MyViewModel(), "myVM" );

            Assert.Throws<ArgumentNullException>( () => registration.WithCommand( (AsyncCommand) null, "myCommand" ) );
        }

        [Fact]
        public void CannotRegisterParameterlessAsyncCommandWithNullId()
        {
            var registration = new Logger().Register( new MyViewModel(), "myVM" );

            Assert.Throws<ArgumentNullException>( () => registration.WithCommand( new AsyncCommand( () => TaskEx.CompletedTask ), null ) );
        }

        [Fact]
        public async Task ParameterlessAsyncCommandWithoutLabel()
        {
            var vm = new MyViewModel();
            var logger = new Logger();
            var command = new AsyncCommand( () => TaskEx.CompletedTask );

            logger.Register( vm, "myVM" )
                .WithCommand( command, "myCommand" );

            await command.ExecuteAsync();

            Assert.Equal( "EVENT: myVM myCommand -", logger.Event );
        }

        [Fact]
        public async Task ParameterlessAsyncCommandWithLabel()
        {
            var vm = new MyViewModel();
            var logger = new Logger();
            var command = new AsyncCommand( () => TaskEx.CompletedTask );

            logger.Register( vm, "myVM" )
                .WithCommand( command, "myCommand", () => "hello" );

            await command.ExecuteAsync();

            Assert.Equal( "EVENT: myVM myCommand hello", logger.Event );
        }

        [Fact]
        public void CannotRegisterNullAsyncCommand()
        {
            var registration = new Logger().Register( new MyViewModel(), "myVM" );

            Assert.Throws<ArgumentNullException>( () => registration.WithCommand( (AsyncCommand<int>) null, "myCommand" ) );
        }

        [Fact]
        public void CannotRegisterAsyncCommandWithNullId()
        {
            var registration = new Logger().Register( new MyViewModel(), "myVM" );

            Assert.Throws<ArgumentNullException>( () => registration.WithCommand( new AsyncCommand<int>( _ => Task.FromResult( 0 ) ), null ) );
        }

        [Fact]
        public async Task AsyncCommandWithoutLabel()
        {
            var vm = new MyViewModel();
            var logger = new Logger();
            var command = new AsyncCommand<int>( _ => TaskEx.CompletedTask );

            logger.Register( vm, "myVM" )
                .WithCommand( command, "myCommand" );

            await command.ExecuteAsync( 42 );

            Assert.Equal( "EVENT: myVM myCommand -", logger.Event );
        }

        [Fact]
        public async Task AsyncCommandWithLabel()
        {
            var vm = new MyViewModel();
            var logger = new Logger();
            var command = new AsyncCommand<int>( _ => TaskEx.CompletedTask );

            logger.Register( vm, "myVM" )
                .WithCommand( command, "myCommand", n => $"n:{n}" );

            await command.ExecuteAsync( 42 );

            Assert.Equal( "EVENT: myVM myCommand n:42", logger.Event );
        }
    }
}