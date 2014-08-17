// Copyright (c) Solal Pirelli 2014
// See License.txt file for more details

using System;
namespace ThinMvvm.Logging
{
    /// <summary>
    /// Request message to enable logging for an object's commands.
    /// </summary>
    /// <remarks>
    /// To be sent via the <see cref="Messenger" />.
    /// </remarks>
    public sealed class CommandLoggingRequest
    {
        /// <summary>
        /// Gets the object whose commands should be logged.
        /// </summary>
        public object Object { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandLoggingRequest" /> class with the specified object.
        /// </summary>
        /// <param name="obj">The object.</param>
        public CommandLoggingRequest( object obj )
        {
            if ( obj == null )
            {
                throw new ArgumentNullException( "obj" );
            }

            Object = obj;
        }
    }
}