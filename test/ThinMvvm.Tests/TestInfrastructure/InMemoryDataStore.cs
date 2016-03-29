using System.Collections.Generic;
using System.Threading.Tasks;

namespace ThinMvvm.Tests.TestInfrastructure
{
    public sealed class InMemoryDataStore : IDataStore
    {
        private readonly Dictionary<string, object> _values = new Dictionary<string, object>();

        public Task<Optional<T>> LoadAsync<T>( string id )
        {
            object boxedValue;
            if( _values.TryGetValue( id, out boxedValue ) )
            {
                return Task.FromResult( new Optional<T>( (T) boxedValue ) );
            }

            return Task.FromResult( new Optional<T>() );
        }

        public Task StoreAsync<T>( string id, T value )
        {
            _values[id] = value;
            return Task.CompletedTask;
        }
    }
}