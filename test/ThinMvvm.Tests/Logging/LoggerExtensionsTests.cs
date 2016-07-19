using System;
using System.Threading.Tasks;
using ThinMvvm.Infrastructure;
using ThinMvvm.Logging;
using ThinMvvm.Tests.TestInfrastructure;
using Xunit;

namespace ThinMvvm.Tests.Logging
{
    public sealed class LoggerExtensionsTests
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