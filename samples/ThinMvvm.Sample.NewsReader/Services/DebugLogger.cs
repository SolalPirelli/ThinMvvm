using System;
using System.Diagnostics;
using ThinMvvm.Logging;

namespace ThinMvvm.Sample.NewsReader.Services
{
    public sealed class DebugLogger : ILogger
    {
        public void LogNavigation( string viewModelId, bool isArriving )
        {
            Debug.WriteLine( $"Navigation {( isArriving ? "to" : "from" )} {viewModelId}" );
        }

        public void LogEvent( string viewModelId, string eventId, string label )
        {
            Debug.WriteLine( $"{viewModelId}: {eventId} with label '{label}'" );
        }

        public void LogError( string name, Exception exception )
        {
            Debug.WriteLine( $"Error {name}: {exception.Message}" );
        }
    }
}