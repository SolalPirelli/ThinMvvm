using System;
using System.Threading.Tasks;

namespace ThinMvvm.Data.Infrastructure
{
    public static class DataLoader
    {
        public static async Task<DataChunk<T>> LoadAsync<T>( Func<Task<T>> loader )
        {
            try
            {
                var result = await loader();
                return new DataChunk<T>( result, DataStatus.Normal, default( DataErrors ) );
            }
            catch( Exception fetchException )
            {
                return new DataChunk<T>( default( T ), DataStatus.Error, new DataErrors( fetchException, null, null ) );
            }
        }

        public static async Task<DataChunk<T>> CacheAsync<T>( DataChunk<T> chunk, Cache cache, Func<CacheMetadata> metadataCreator )
        {
            CacheMetadata metadata;
            try
            {
                metadata = metadataCreator();
            }
            catch( Exception metadataException )
            {
                return new DataChunk<T>( chunk.Value, chunk.Status, new DataErrors( chunk.Errors.Fetch, metadataException, chunk.Errors.Process ) );
            }

            if( metadata == null )
            {
                return chunk;
            }

            if( chunk.Status == DataStatus.Normal )
            {
                try
                {
                    await cache.StoreAsync( metadata.Id, chunk.Value, metadata.ExpirationDate );
                    return chunk;
                }
                catch( Exception cacheException )
                {
                    return new DataChunk<T>( chunk.Value, DataStatus.Normal, new DataErrors( chunk.Errors.Fetch, cacheException, chunk.Errors.Process ) );
                }
            }

            try
            {
                var cachedResult = await cache.GetAsync<T>( metadata.Id );
                if( cachedResult.HasValue )
                {
                    return new DataChunk<T>( cachedResult.Value, DataStatus.Cached, chunk.Errors );
                }

                return chunk;
            }
            catch( Exception cacheException )
            {
                return new DataChunk<T>( default( T ), DataStatus.Error, new DataErrors( chunk.Errors.Fetch, cacheException, chunk.Errors.Process ) );
            }
        }

        public static DataChunk<T> Transform<T>( DataChunk<T> chunk, Func<T, T> transformer )
        {
            if( chunk.Status == DataStatus.Error )
            {
                return chunk;
            }

            try
            {
                var transformed = transformer( chunk.Value );
                return new DataChunk<T>( transformed, chunk.Status, chunk.Errors );
            }
            catch( Exception transformException )
            {
                return new DataChunk<T>( default( T ), DataStatus.Error, new DataErrors( chunk.Errors.Fetch, chunk.Errors.Cache, transformException ) );
            }
        }
    }
}