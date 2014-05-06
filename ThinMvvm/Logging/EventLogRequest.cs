// Copyright (c) Solal Pirelli 2014
// See License.txt file for more details

namespace ThinMvvm.Logging
{
    /// <summary>
    /// A request to log an event that occurred.
    /// </summary>
    public sealed class EventLogRequest
    {
        /// <summary>
        /// Gets the ID of the event that should be logged.
        /// </summary>
        public string EventId { get; private set; }

        /// <summary>
        /// Gets the label of the event that should be logged.
        /// </summary>
        public string Label { get; private set; }

        /// <summary>
        /// Gets the ID of the screen on which the event should be logged, or null for the current screen.
        /// </summary>
        public string ScreenId { get; private set; }


        /// <summary>
        /// Initializes a new instance of the <see cref="EventLogRequest" /> class with the specified parameters.
        /// </summary>
        /// <param name="eventId">The event ID.</param>
        /// <param name="label">The label.</param>
        /// <param name="screenId">Optional. The screen ID, if it's different from the current screen.</param>
        public EventLogRequest( string eventId, string label, string screenId = null )
        {
            EventId = eventId;
            Label = label;
            ScreenId = screenId;
        }
    }
}