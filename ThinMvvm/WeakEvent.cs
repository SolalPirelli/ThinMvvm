using System;
using System.Collections.Generic;
using ThinMvvm.Internals;

namespace ThinMvvm
{
    public sealed class WeakEvent
    {
        private readonly List<WeakDelegate> _handlers = new List<WeakDelegate>();

        public void Add( Delegate handler )
        {
            _handlers.Add( new WeakDelegate( handler ) );
        }

        public void Remove( Delegate handler )
        {
            _handlers.RemoveAll( weakHandler =>
            {
                bool result;
                bool isAlive = weakHandler.TryEquals( handler, out result );
                return result || !isAlive;
            } );
        }

        public void Raise( object sender, EventArgs e )
        {
            // Get a frozen copy of the list via ToArray, otherwise it might be changed while we're enumerating
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