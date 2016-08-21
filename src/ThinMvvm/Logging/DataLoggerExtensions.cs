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
        /// Registers the specified <see cref="IForm" /> for error logging.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="form">The form.</param>
        public static void Register( this ILogger logger, IForm form )
        {
            if( logger == null )
            {
                throw new ArgumentNullException( nameof( logger ) );
            }
            if( form == null )
            {
                throw new ArgumentNullException( nameof( form ) );
            }

            form.PropertyChanged += ( s, e ) =>
            {
                var f = (IForm) s;
                if( e.PropertyName == nameof( IForm.Status ) )
                {
                    if( f.Status == FormStatus.None )
                    {
                        Log( logger, "Form initialization error", f.Error );
                    }
                    else if( f.Status == FormStatus.Submitted )
                    {
                        Log( logger, "Form submission error", f.Error );
                    }
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