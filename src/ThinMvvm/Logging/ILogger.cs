using System;

namespace ThinMvvm.Logging
{
    /// <summary>
    /// Logs application events.
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// Logs a navigation.
        /// </summary>
        /// <param name="viewModelId">The ID of the ViewModel the navigation is arriving or leaving.</param>
        /// <param name="isArriving">A value indicating whether navigation is arriving or leaving the ViewModel.</param>
        void LogNavigation( string viewModelId, bool isArriving );

        /// <summary>
        /// Logs an event.
        /// </summary>
        /// <param name="viewModelId">The ID of the ViewModel the event belongs to.</param>
        /// <param name="eventId">The event's ID.</param>
        /// <param name="label">The label, if any.</param>
        void LogEvent( string viewModelId, string eventId, string label );

        /// <summary>
        /// Logs an error.
        /// </summary>
        /// <param name="name">The error name.</param>
        /// <param name="exception">The exception associated with the error.</param>
        void LogError( string name, Exception exception );
    }
}