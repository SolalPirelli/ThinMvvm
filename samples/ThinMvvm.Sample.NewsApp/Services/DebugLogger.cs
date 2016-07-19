using System.Diagnostics;
using ThinMvvm.Logging;

namespace ThinMvvm.Sample.NewsApp.Services
{
    public sealed class DebugLogger : ILogger
    {
        public void LogEvent( string viewModelId, string eventId, string label )
        {
            Debug.WriteLine( $"{viewModelId}: {eventId} with label '{label}'" );
        }

        public void LogNavigation( string viewModelId, bool isArriving )
        {
            Debug.WriteLine( $"Navigation {( isArriving ? "to" : "from" )} {viewModelId}" );
        }
    }
}