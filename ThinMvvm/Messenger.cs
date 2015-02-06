// Copyright (c) Solal Pirelli 2014
// See License.txt file for more details

using System;
using System.Collections.Generic;
using System.Linq;
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
        private static readonly List<object> _handlers = new List<object>();

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

            _handlers.Add( new Handler<T>( handler ) );
        }

        /// <summary>
        /// Sends the specified message.
        /// </summary>
        /// <typeparam name="T">The message type.</typeparam>
        /// <param name="message">The message.</param>
        public static void Send<T>( T message )
        {
            foreach ( var deadHandler in _handlers.OfType<Handler<T>>().Where( h => !h.TryHandle( message ) ).ToArray() )
            {
                _handlers.Remove( deadHandler );
            }
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
        /// Stores data necessary to handle messages.
        /// </summary>
        /// <typeparam name="T">The message type.</typeparam>
        private sealed class Handler<T>
        {
            private readonly WeakDelegate _action;

            /// <summary>
            /// Initializes a new instance of the <see cref="Handler{T}" /> class with the specified handler action.
            /// </summary>
            /// <param name="handler">The handler.</param>
            public Handler( Action<T> action )
            {
                _action = new WeakDelegate( action );
            }


            /// <summary>
            /// Attempts to handle the specified message.
            /// </summary>
            /// <param name="message">The message.</param>
            /// <returns>True if the handler is still alive, false otherwise.</returns>
            public bool TryHandle( T message )
            {
                object ignored;
                return _action.TryInvoke( new object[] { message }, out ignored );
            }
        }
    }
}