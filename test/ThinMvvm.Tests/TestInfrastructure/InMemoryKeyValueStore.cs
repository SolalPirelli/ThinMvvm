using System;
using System.Collections.Generic;

namespace ThinMvvm.Tests.TestInfrastructure
{
    public sealed class InMemoryKeyValueStore : IKeyValueStore
    {
        private readonly Dictionary<string, object> _values = new Dictionary<string, object>();


        public Optional<T> Get<T>( string key )
        {
            if( _values.ContainsKey( key ) )
            {
                return new Optional<T>( (T) _values[key] );
            }

            return default( Optional<T> );
        }

        public void Set<T>( string key, T value )
        {
            _values[key] = value;
        }

        public void Delete( string key )
        {
            _values.Remove( key );
        }

        public void Clear()
        {
            _values.Clear();
        }
    }
}