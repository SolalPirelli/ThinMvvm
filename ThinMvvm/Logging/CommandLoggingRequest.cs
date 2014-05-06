// Copyright (c) Solal Pirelli 2014
// See License.txt file for more details

namespace ThinMvvm.Logging
{
    /// <summary>
    /// A request to enable logging for an object's commands.
    /// </summary>
    public sealed class CommandLoggingRequest
    {
        /// <summary>
        /// Gets the object whose commands should be logged.
        /// </summary>
        public object Object { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandLoggingRequest" /> class for the specified object.
        /// </summary>
        public CommandLoggingRequest( object obj )
        {
            Object = obj;
        }
    }
}