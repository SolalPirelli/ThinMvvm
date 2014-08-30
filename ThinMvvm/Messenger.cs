// Copyright (c) Solal Pirelli 2014
// See License.txt file for more details

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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
            private const string ClosureMethodToken = "<";

            // If the action is a closure, a strong reference to the target is needed
#pragma warning disable 0414 // Field is assigned but never used
            private readonly object _targetStrongRef;
#pragma warning restore 0414
            private readonly WeakReference<object> _targetRef;
            private readonly MethodInfo _method;


            /// <summary>
            /// Initializes a new instance of the <see cref="Handler{T}" /> class with the specified handler action.
            /// </summary>
            /// <param name="handler">The handler.</param>
            /// <returns>A recipient with the specified message handler.</returns>
            public Handler( Action<T> handler )
            {
                _targetStrongRef = handler.GetMethodInfo().Name.Contains( ClosureMethodToken ) ? handler.Target : null;
                _method = handler.GetMethodInfo();
                _targetRef = handler.Target == null ? null : new WeakReference<object>( handler.Target );
            }


            /// <summary>
            /// Attempts to handle the specified message.
            /// </summary>
            /// <param name="message">The message.</param>
            /// <returns>True if the handler is still alive, false otherwise.</returns>
            public bool TryHandle( T message )
            {
                object target = null;
                if ( _targetRef == null || _targetRef.TryGetTarget( out target ) )
                {
                    _method.Invoke( target, new object[] { message } );
                    return true;
                }

                return false;
            }
        }
    }
}