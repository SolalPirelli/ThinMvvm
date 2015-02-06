// Copyright (c) 2014-15 Solal Pirelli
// See License.txt file for more details

using System.Diagnostics;
using ThinMvvm.Logging;

namespace ThinMvvm.SampleApp.Services
{
    public sealed class DebugLogger : Logger
    {
        public DebugLogger( INavigationService navigationService ) : base( navigationService ) { }


        protected override void LogAction( string viewModelId, LoggedSpecialAction action )
        {
            Debug.WriteLine( "Action on '{0}': {1}", viewModelId, action );
        }

        protected override void LogCommand( string viewModelId, string eventId, string label )
        {
            Debug.WriteLine( "Command on '{0}': {1} (label: {2})", viewModelId, eventId, label );
        }
    }
}