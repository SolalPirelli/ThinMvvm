// Copyright (c) Solal Pirelli 2014
// See License.txt file for more details

using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ThinMvvm.WindowsRuntime.Internals;
using Windows.Storage;

namespace ThinMvvm.WindowsRuntime
{
    /// <summary>
    /// <see cref="IDataCache" /> implementation for the Windows Runtime.
    /// </summary>
    public sealed class WindowsRuntimeDataCache : IDataCache
    {
        // N.B.: The size of settings is too small for practical purposes, we have to serialize to files.

        /// <summary>
        /// Asynchronously gets the value stored by the specified owner type, with the specified ID.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="owner">The owner type.</param>
        /// <param name="id">The ID.</param>
        /// <returns>The cached value.</returns>
        public async Task<CachedData<T>> GetAsync<T>( Type owner, long id )
        {
            var folder = await GetFolderForTypeAsync( owner );
            // There's no way to check whether the file exists outside of this :-(
            var file = ( await folder.GetFilesAsync() ).FirstOrDefault( f => f.Name == owner.FullName );

            if ( file == null )
            {
                return new CachedData<T>();
            }

            using ( var stream = await file.OpenAsync( FileAccessMode.Read ) )
            using ( var reader = new StreamReader( stream.AsStreamForRead() ) )
            {
                string dateString = await reader.ReadLineAsync();
                var date = DateTimeOffset.Parse( dateString, CultureInfo.InvariantCulture );

                if ( date >= DateTimeOffset.Now )
                {
                    string data = await reader.ReadToEndAsync();
                    return new CachedData<T>( Serializer.Deserialize<T>( data ) );
                }
            }

            await file.DeleteAsync();
            return new CachedData<T>();
        }

        /// <summary>
        /// Asynchronously sets the specified value for the specified owner type, with the specified ID.
        /// </summary>
        /// <param name="owner">The owner type.</param>
        /// <param name="id">The ID.</param>
        /// <param name="expirationDate">The expiration date.</param>
        /// <param name="value">The value.</param>
        public async Task SetAsync( Type owner, long id, DateTimeOffset expirationDate, object value )
        {
            var folder = await GetFolderForTypeAsync( owner );
            var file = await folder.CreateFileAsync( id.ToString(), CreationCollisionOption.ReplaceExisting );
            using ( var stream = await file.OpenAsync( FileAccessMode.ReadWrite ) )
            using ( var writer = new StreamWriter( stream.AsStreamForWrite() ) )
            {
                await writer.WriteLineAsync( expirationDate.ToString( CultureInfo.InvariantCulture ) );
                await writer.WriteAsync( Serializer.Serialize( value ) );
            }
        }

        /// <summary>
        /// Asynchronously gets the folder in which cache files for the specified type are stored.
        /// </summary>
        private Task<StorageFolder> GetFolderForTypeAsync( Type owner )
        {
            return ApplicationData.Current.LocalFolder.CreateFolderAsync( owner.FullName, CreationCollisionOption.OpenIfExists ).AsTask();
        }
    }
}