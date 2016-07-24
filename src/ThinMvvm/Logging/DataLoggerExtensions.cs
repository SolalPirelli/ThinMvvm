using System;
using ThinMvvm.Data;
using ThinMvvm.Data.Infrastructure;

namespace ThinMvvm.Logging
{
    /// <summary>
    /// Extensions to <see cref="ILogger" /> to facilitate data source error logging.
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

            dataSource.PropertyChanged += ( _, e ) =>
            {
                if( e.PropertyName == nameof( IDataSource.Status ) && dataSource.Status == DataSourceStatus.Loaded )
                {
                    var chunk = dataSource.Data[dataSource.Data.Count - 1];

                    Log( logger, "Fetch error", chunk.Errors.Fetch );
                    Log( logger, "Cache error", chunk.Errors.Cache );
                    Log( logger, "Processing error", chunk.Errors.Process );
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