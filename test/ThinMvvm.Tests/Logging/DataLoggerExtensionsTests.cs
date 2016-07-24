using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ThinMvvm.Data;
using ThinMvvm.Logging;
using ThinMvvm.Tests.TestInfrastructure;
using Xunit;

namespace ThinMvvm.Tests.Logging
{
    public sealed class DataLoggerExtensionsTests
    {
        private sealed class MyDataSource : DataSource<int>
        {
            public Exception FetchException { get; set; }

            public Exception CacheException { get; set; }

            public Exception TransformException { get; set; }


            public MyDataSource()
            {
                EnableCache( "X", new InMemoryDataStore(), () =>
                {
                    if( CacheException != null )
                    {
                        throw CacheException;
                    }

                    return CacheMetadata.Default;
                } );
            }


            protected override Task<int> FetchAsync( CancellationToken cancellationToken )
            {
                if( FetchException != null )
                {
                    return TaskEx.FromException<int>( FetchException );
                }

                return Task.FromResult( 0 );
            }

            protected override int Transform( int value )
            {
                if( TransformException != null )
                {
                    throw TransformException;
                }

                return base.Transform( value );
            }
        }

        private sealed class Logger : ILogger
        {
            public List<Tuple<string, Exception>> Errors { get; } = new List<Tuple<string, Exception>>();

            public void LogNavigation( string viewModelId, bool isArriving )
            {
                throw new NotSupportedException();
            }

            public void LogEvent( string viewModelId, string eventId, string label )
            {
                throw new NotSupportedException();
            }

            public void LogError( string name, Exception exception )
            {
                Errors.Add( Tuple.Create( name, exception ) );
            }
        }

        [Fact]
        public void CannotRegisterWithNullLogger()
        {
            Assert.Throws<ArgumentNullException>(
                () => DataLoggerExtensions.Register( null, new BasicDataSource<int>( () => Task.FromResult( 0 ) ) )
            );
        }

        [Fact]
        public void CannotRegisterWithNullSource()
        {
            Assert.Throws<ArgumentNullException>(
                () => DataLoggerExtensions.Register( new Logger(), null )
            );
        }

        [Fact]
        public async Task NothingLoggedOnSuccess()
        {
            var source = new MyDataSource();
            var logger = new Logger();

            logger.Register( source );

            await source.RefreshAsync();

            Assert.Equal( 0, logger.Errors.Count );
        }

        [Fact]
        public async Task FetchErrorIsLogged()
        {
            var ex = (Exception) new MyException();
            var source = new MyDataSource { FetchException = ex };
            var logger = new Logger();

            logger.Register( source );

            await source.RefreshAsync();

            Assert.Equal( new[] { Tuple.Create( "Fetch error", ex ) }, logger.Errors );
        }

        [Fact]
        public async Task CacheErrorIsLogged()
        {
            var ex = (Exception) new MyException();
            var source = new MyDataSource { CacheException = ex };
            var logger = new Logger();

            logger.Register( source );

            await source.RefreshAsync();

            Assert.Equal( new[] { Tuple.Create( "Cache error", ex ) }, logger.Errors );
        }

        [Fact]
        public async Task TransformErrorIsLogged()
        {
            var ex = (Exception) new MyException();
            var source = new MyDataSource { TransformException = ex };
            var logger = new Logger();

            logger.Register( source );

            await source.RefreshAsync();

            Assert.Equal( new[] { Tuple.Create( "Processing error", ex ) }, logger.Errors );
        }

        [Fact]
        public async Task MultipleErrorsAreLogged()
        {
            var fetchEx = (Exception) new MyException();
            var cacheEx = (Exception) new MyException();
            var source = new MyDataSource { FetchException = fetchEx, CacheException = cacheEx };
            var logger = new Logger();

            logger.Register( source );

            await source.RefreshAsync();

            Assert.Equal( new[] { Tuple.Create( "Fetch error", fetchEx ), Tuple.Create( "Cache error", cacheEx ) }, logger.Errors );
        }
    }
}