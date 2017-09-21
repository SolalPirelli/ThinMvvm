using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace ThinMvvm.Data.Infrastructure
{
    /// <summary>
    /// Contains methods that operate on <see cref="DataChunk{T}" />.
    /// </summary>
    [EditorBrowsable( EditorBrowsableState.Advanced )]
    public static class DataChunkOperations
    {
        /// <summary>
        /// Asynchronously fetches a chunk of data using the specified function.
        /// 
        /// If the fetching function throws, the exception will be set as the <see cref="DataErrors.Fetch" /> 
        /// of the chunk's <see cref="DataChunk{T}.Errors" />.
        /// </summary>
        /// <typeparam name="T">The data type.</typeparam>
        /// <param name="fetcher">The asynchronous function to fetch the data.</param>
        /// <returns>A task that represents the fetch operation.</returns>
        public static async Task<DataChunk<T>> FetchAsync<T>( Func<Task<T>> fetcher )
        {
            try
            {
                var result = await fetcher();
                return new DataChunk<T>( result, DataStatus.Normal, default( DataErrors ) );
            }
            catch( Exception fetchException )
            {
                return new DataChunk<T>( default( T ), DataStatus.Error, new DataErrors( fetchException, null, null ) );
            }
        }

        /// <summary>
        /// Asynchronously caches the specified chunk of data, using the specified cache and metadata creator.
        /// 
        /// If the chunk has a value, it will be stored in the cache; otherwise, the cache will be used to get a value.
        /// 
        /// If the cache or the metadata creator throw, the exception will be set as the <see cref="DataErrors.Cache" /> 
        /// of the chunk's <see cref="DataChunk{T}.Errors" />.
        /// </summary>
        /// <typeparam name="T">The data type.</typeparam>
        /// <param name="chunk">The data chunk.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="metadataCreator">The metadata creator.</param>
        /// <returns>A task that represents the cache operation.</returns>
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

        /// <summary>
        /// Transforms the specified chunk of data using the specified function.
        /// 
        /// If the chunk has no value, it will be returned as is.
        /// 
        /// If the transformation function throws, the exception will be set as the <see cref="DataErrors.Process" />
        /// of the chunk's <see cref="DataChunk{T}.Errors" />.
        /// </summary>
        /// <typeparam name="T">The data type.</typeparam>
        /// <param name="chunk">The data chunk.</param>
        /// <param name="transformer">The transformation function.</param>
        /// <returns>The transformed chunk.</returns>
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