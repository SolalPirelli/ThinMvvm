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

        // Do not change!
        private const string CacheFolderName = "ThinMvvm.WindowsRuntime.DataCache";

        /// <summary>
        /// Asynchronously gets the value stored by the specified owner type, with the specified ID.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="owner">The owner type.</param>
        /// <param name="id">The ID.</param>
        /// <returns>The cached value.</returns>
        public async Task<CachedData<T>> GetAsync<T>( Type owner, long id )
        {
            var folder = await GetFolderAsync( owner );
            var file = await GetFileAsync( folder, id, ApplicationDataCreateDisposition.Existing );

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
            var folder = await GetFolderAsync( owner );
            var file = await GetFileAsync( folder, id, ApplicationDataCreateDisposition.Always );
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
        private async Task<StorageFolder> GetFolderAsync( Type owner )
        {
            var rootFolder = ApplicationData.Current.LocalFolder;
            var cacheFolder = await rootFolder.CreateFolderAsync( CacheFolderName, CreationCollisionOption.OpenIfExists );
            return await cacheFolder.CreateFolderAsync( owner.FullName, CreationCollisionOption.OpenIfExists );
        }

        /// <summary>
        /// Asynchronously gets the file associated with the specified ID, in the specified folder, 
        /// and optionally creates it if doesn't exist.
        /// </summary>
        private async Task<StorageFile> GetFileAsync( StorageFolder folder, long id, ApplicationDataCreateDisposition disposition )
        {
            // N.B.: There's unfortunately no way to get a file that may not exist without throwing

            string fileName = id.ToString();

            if ( disposition == ApplicationDataCreateDisposition.Always )
            {
                return await folder.CreateFileAsync( fileName, CreationCollisionOption.ReplaceExisting );
            }

            return ( await folder.GetFilesAsync() ).FirstOrDefault( f => f.Name == fileName );
        }
    }
}