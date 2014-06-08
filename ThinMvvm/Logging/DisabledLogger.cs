// Copyright (c) Solal Pirelli 2014
// See License.txt file for more details

namespace ThinMvvm.Logging
{
    /// <summary>
    /// Logger that does not log anything.
    /// Used to disable the logging features.
    /// </summary>
    public sealed class DisabledLogger : Logger
    {
        /// <summary>
        /// Does not do anything.
        /// </summary>
        /// <param name="id">Ignored.</param>
        protected override void LogNavigation( string id )
        {
        }

        /// <summary>
        /// Does not do anything.
        /// </summary>
        /// <param name="viewModelId">Ignored.</param>
        /// <param name="eventId">Ignored.</param>
        /// <param name="label">Ignored.</param>
        protected override void LogEvent( string viewModelId, string eventId, string label )
        {
        }
    }
}