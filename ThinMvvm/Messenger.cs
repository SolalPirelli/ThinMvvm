// Copyright (c) 2014-15 Solal Pirelli
// See License.txt file for more details

using System;
using System.Collections.Generic;
using ThinMvvm.Internals;

namespace ThinMvvm
{
    /// <summary>
    /// Sends messages to and from objects.
    /// </summary>
    /// <remarks>
    /// Unregistering functionality is not provided.
    /// </remarks>
    public static class Messenger
    {
        private static readonly List<Handler> _handlers = new List<Handler>();

        /// <summary>
        /// Registers the specified handler for the specified message type.
        /// </summary>
        /// <typeparam name="T">The message type.</typeparam>
        /// <param name="handler">The handler.</param>
        public static void Register<T>( Action<T> handler )
        {
            if ( handler == null )
            {
                throw new ArgumentNullException( "handler" );
            }

            _handlers.Add( Handler.Create( handler ) );
        }

        /// <summary>
        /// Sends the specified message.
        /// </summary>
        /// <typeparam name="T">The message type.</typeparam>
        /// <param name="message">The message.</param>
        public static void Send<T>( T message )
        {
            _handlers.RemoveAll( h => !h.TryHandle( message, typeof( T ) ) );
        }

        /// <summary>
        /// Clears the registered handlers.
        /// </summary>
        [Obsolete( "For unit tests only." )]
        internal static void Clear()
        {
            _handlers.Clear();
        }


        /// <summary>
        /// Handle messages (or refuses to do so).
        /// </summary>
        private sealed class Handler
        {
            private readonly WeakDelegate _action;
            private readonly Type _messageType;

            /// <summary>
            /// Initializes a new instance of the <see cref="Handler" /> class with the specified action and message type.
            /// </summary>
            private Handler( WeakDelegate action, Type messageType )
            {
                _action = action;
                _messageType = messageType;
            }

            /// <summary>
            /// Creates a new <see cref="Handler" /> with the specified action.
            /// </summary>
            /// <param name="action">The action.</param>
            public static Handler Create<T>( Action<T> action )
            {
                return new Handler( new WeakDelegate( action ), typeof( T ) );
            }


            /// <summary>
            /// Handles the specified message if it is of the correct type.
            /// Otherwise, does nothing.
            /// </summary>
            /// <param name="message">The message.</param>
            /// <param name="messageType">The message type. (required because message can be null)</param>
            /// <returns>True if the handler is still alive, false otherwise.</returns>
            public bool TryHandle( object message, Type messageType )
            {
                if ( messageType != _messageType )
                {
                    return true;
                }

                object ignored;
                return _action.TryInvoke( new[] { message }, out ignored );
            }
        }
    }
}