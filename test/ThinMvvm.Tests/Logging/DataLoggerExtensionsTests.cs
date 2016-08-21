using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ThinMvvm.Data;
using ThinMvvm.Data.Infrastructure;
using ThinMvvm.Logging;
using ThinMvvm.Tests.TestInfrastructure;
using Xunit;

namespace ThinMvvm.Tests.Logging
{
    public sealed class DataLoggerExtensionsTests
    {
        private sealed class TestLogger : ILogger
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

        private sealed class MyDataSource : DataSource<int>
        {
            public Exception FetchError { get; set; }

            public Exception CacheError { get; set; }

            public Exception TransformError { get; set; }


            public MyDataSource()
            {
                EnableCache( "X", new InMemoryDataStore(), () =>
                {
                    if( CacheError != null )
                    {
                        throw CacheError;
                    }

                    return CacheMetadata.Default;
                } );
            }


            protected override Task<int> FetchAsync( CancellationToken cancellationToken )
            {
                if( FetchError != null )
                {
                    return TaskEx.FromException<int>( FetchError );
                }

                return Task.FromResult( 0 );
            }

            protected override Task<int> TransformAsync( int value )
            {
                if( TransformError != null )
                {
                    throw TransformError;
                }

                return Task.FromResult( value );
            }
        }

        [Fact]
        public void CannotRegisterSourceWithNullLogger()
        {
            Assert.Throws<ArgumentNullException>(
                () => DataLoggerExtensions.Register( null, new MyDataSource() )
            );
        }

        [Fact]
        public void CannotRegisterNullSource()
        {
            Assert.Throws<ArgumentNullException>(
                () => DataLoggerExtensions.Register( new TestLogger(), (IDataSource) null )
            );
        }

        [Fact]
        public async Task NothingLoggedOnSourceSuccess()
        {
            var source = new MyDataSource();
            var logger = new TestLogger();

            logger.Register( source );

            await source.RefreshAsync();

            Assert.Equal( 0, logger.Errors.Count );
        }

        [Fact]
        public async Task SourceFetchErrorIsLogged()
        {
            var ex = (Exception) new MyException();
            var source = new MyDataSource { FetchError = ex };
            var logger = new TestLogger();

            logger.Register( source );

            await source.RefreshAsync();

            Assert.Equal( new[] { Tuple.Create( "Data source fetch error", ex ) }, logger.Errors );
        }

        [Fact]
        public async Task SourceCacheErrorIsLogged()
        {
            var ex = (Exception) new MyException();
            var source = new MyDataSource { CacheError = ex };
            var logger = new TestLogger();

            logger.Register( source );

            await source.RefreshAsync();

            Assert.Equal( new[] { Tuple.Create( "Data source cache error", ex ) }, logger.Errors );
        }

        [Fact]
        public async Task SourceTransformErrorIsLogged()
        {
            var ex = (Exception) new MyException();
            var source = new MyDataSource { TransformError = ex };
            var logger = new TestLogger();

            logger.Register( source );

            await source.RefreshAsync();

            Assert.Equal( new[] { Tuple.Create( "Data source processing error", ex ) }, logger.Errors );
        }

        [Fact]
        public async Task MultipleSourceErrorsAreLogged()
        {
            var fetchEx = (Exception) new MyException();
            var cacheEx = (Exception) new MyException();
            var source = new MyDataSource { FetchError = fetchEx, CacheError = cacheEx };
            var logger = new TestLogger();

            logger.Register( source );

            await source.RefreshAsync();

            Assert.Equal( new[]
            {
                Tuple.Create( "Data source fetch error", fetchEx ),
                Tuple.Create( "Data source cache error", cacheEx )
            }, logger.Errors );
        }


        private sealed class MyForm : Form<int>
        {
            public Exception InitialError { get; set; }

            public Exception SubmitError { get; set; }


            protected override Task<int> LoadInitialInputAsync()
            {
                if( InitialError != null )
                {
                    throw InitialError;
                }

                return Task.FromResult( 0 );
            }

            protected override Task SubmitAsync( int input )
            {
                if( SubmitError != null )
                {
                    throw SubmitError;
                }

                return TaskEx.CompletedTask;
            }
        }

        [Fact]
        public void CannotRegisterFormWithNullLogger()
        {
            Assert.Throws<ArgumentNullException>(
                () => DataLoggerExtensions.Register( null, new MyForm() )
            );
        }

        [Fact]
        public void CannotRegisterNullForm()
        {
            Assert.Throws<ArgumentNullException>(
                () => DataLoggerExtensions.Register( new TestLogger(), (IForm) null )
            );
        }

        [Fact]
        public async Task NothingLoggedOnFormSuccess()
        {
            var form = new MyForm();
            var logger = new TestLogger();

            logger.Register( form );

            await form.InitializeAsync();
            await form.SubmitAsync();

            Assert.Equal( 0, logger.Errors.Count );
        }

        [Fact]
        public async Task FormInitialErrorIsLogged()
        {
            var ex = (Exception) new MyException();
            var form = new MyForm() { InitialError = ex };
            var logger = new TestLogger();

            logger.Register( form );

            await form.InitializeAsync();

            Assert.Equal( new[] { Tuple.Create( "Form initialization error", ex ) }, logger.Errors );
        }

        [Fact]
        public async Task FormSubmitErrorIsLogged()
        {
            var ex = (Exception) new MyException();
            var form = new MyForm() { SubmitError = ex };
            var logger = new TestLogger();

            logger.Register( form );

            await form.InitializeAsync();
            await form.SubmitAsync();

            Assert.Equal( new[] { Tuple.Create( "Form submission error", ex ) }, logger.Errors );
        }

    }
}