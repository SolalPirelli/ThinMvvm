using System;
using System.Threading;
using System.Threading.Tasks;

namespace ThinMvvm.Data
{
    public sealed class SimpleDataSource<T> : DataSource<T>
    {
        private readonly Func<CancellationToken, Task<T>> _fetch;


        public SimpleDataSource( Func<CancellationToken, Task<T>> fetch )
        {
            _fetch = fetch;
        }

        public SimpleDataSource( Func<Task<T>> fetch )
        {
            _fetch = _ => fetch();
        }


        public SimpleDataSource<T> WithCache( string id, IDataStore cacheStore, Func<CacheMetadata> metadataCreator = null )
        {
            EnableCache( id, cacheStore, metadataCreator );
            return this;
        }


        protected override Task<T> FetchAsync( CancellationToken cancellationToken )
        {
            return _fetch( cancellationToken );
        }
    }
}