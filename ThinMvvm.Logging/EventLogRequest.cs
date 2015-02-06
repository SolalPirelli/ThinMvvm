// Copyright (c) 2014-15 Solal Pirelli
// See License.txt file for more details

namespace ThinMvvm.Logging
{
    /// <summary>
    /// Request message to log an event that occurred.
    /// </summary>
    /// <remarks>
    /// To be sent via the <see cref="Messenger" />.
    /// </remarks>
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
        /// Gets the ID of the <see cref="ViewModel{TParameter}" /> on which the event should be logged, or null for the current one.
        /// </summary>
        public string ViewModelId { get; private set; }


        /// <summary>
        /// Initializes a new instance of the <see cref="EventLogRequest" /> class with the specified parameters.
        /// </summary>
        /// <param name="eventId">The event ID.</param>
        /// <param name="label">The label.</param>
        /// <param name="viewModelId">Optional. The <see cref="ViewModel{TParameter}" /> ID, if it's different from the current one.</param>
        public EventLogRequest( string eventId, string label, string viewModelId = null )
        {
            EventId = eventId;
            Label = label;
            ViewModelId = viewModelId;
        }
    }
}