// Copyright (c) Solal Pirelli 2015
// See License.txt file for more details

using System;
using System.Collections.Generic;
using ThinMvvm.Internals;

namespace ThinMvvm
{
    /// <summary>
    /// Event that takes weak references to its handlers.
    /// </summary>
    public sealed class WeakEvent
    {
        private readonly List<WeakDelegate> _handlers;

        /// <summary>
        /// Creates a new instance of the <see cref="WeakEvent" /> class.
        /// </summary>
        public WeakEvent()
        {
            _handlers = new List<WeakDelegate>();
        }

        /// <summary>
        /// Adds a handler to the event.
        /// </summary>
        /// <param name="handler">The handler.</param>
        public void Add( Delegate handler )
        {
            if ( handler == null )
            {
                throw new ArgumentNullException( "handler" );
            }

            _handlers.Add( new WeakDelegate( handler ) );
        }

        /// <summary>
        /// Removes a handler from the event.
        /// </summary>
        /// <param name="handler">The handler.</param>
        public void Remove( Delegate handler )
        {
            if ( handler == null )
            {
                throw new ArgumentNullException( "handler" );
            }

            _handlers.RemoveAll( weakHandler =>
            {
                bool result;
                bool isAlive = weakHandler.TryEquals( handler, out result );
                return result || !isAlive;
            } );
        }

        /// <summary>
        /// Raises the event.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        public void Raise( object sender, EventArgs e )
        {
            // ToArray to be able to remove handlers in the loop
            foreach ( var handler in _handlers.ToArray() )
            {
                object ignored;
                if ( !handler.TryInvoke( new[] { sender, e }, out ignored ) )
                {
                    _handlers.Remove( handler );
                }
            }
        }
    }
}