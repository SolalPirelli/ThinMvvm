using System;
using ThinMvvm.Data;
using ThinMvvm.Data.Infrastructure;

namespace ThinMvvm.Logging
{
    /// <summary>
    /// Extensions to <see cref="ILogger" /> to facilitate data error logging.
    /// </summary>
    public static class DataLoggerExtensions
    {
        /// <summary>
        /// Registers the specified <see cref="IDataSource" /> for error logging.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="dataSource">The data source.</param>
        public static void Register( this ILogger logger, IDataSource dataSource )
        {
            if( logger == null )
            {
                throw new ArgumentNullException( nameof( logger ) );
            }
            if( dataSource == null )
            {
                throw new ArgumentNullException( nameof( dataSource ) );
            }

            dataSource.PropertyChanged += ( s, e ) =>
            {
                var source = (IDataSource) s;
                if( e.PropertyName == nameof( IDataSource.Status ) && source.Status == DataSourceStatus.Loaded )
                {
                    var chunk = source.Data[source.Data.Count - 1];

                    Log( logger, "Data source fetch error", chunk.Errors.Fetch );
                    Log( logger, "Data source cache error", chunk.Errors.Cache );
                    Log( logger, "Data source processing error", chunk.Errors.Process );
                }
            };
        }

        /// <summary>
        /// Registers the specified <see cref="DataOperation" /> for error logging.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="operation">The operation.</param>
        public static void Register( this ILogger logger, DataOperation operation )
        {
            if( logger == null )
            {
                throw new ArgumentNullException( nameof( logger ) );
            }
            if( operation == null )
            {
                throw new ArgumentNullException( nameof( operation ) );
            }

            operation.PropertyChanged += ( s, e ) =>
            {
                var f = (DataOperation) s;
                if( e.PropertyName == nameof( DataOperation.IsLoading ) )
                {
                    Log( logger, "Operation error", f.Error );
                }
            };
        }

        /// <summary>
        /// Logs the specified exception, if not null, on the specified logger.
        /// </summary>
        private static void Log( ILogger logger, string name, Exception exception )
        {
            if( exception != null )
            {
                logger.LogError( name, exception );
            }
        }
    }
}