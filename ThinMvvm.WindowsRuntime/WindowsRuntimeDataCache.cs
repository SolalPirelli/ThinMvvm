// Copyright (c) Solal Pirelli 2014
// See License.txt file for more details

using System;
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
        // N.B. 2: CreateContainer(string, ApplicationDataCreateDisposition) will throw 
        //         if it doesn't exist when using Existing, so we have to use Always instead.

        private const string DateContainerSuffix = "#Date";

        private readonly ApplicationDataContainer _settings = ApplicationData.Current.LocalSettings;


        /// <summary>
        /// Asynchronously attempts to get the value stored by the specified owner type, with the specified ID.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="owner">The owner type.</param>
        /// <param name="id">The ID.</param>
        /// <returns>A tuple whose first element indicates whether a value was found, and whose second element is the value.</returns>
        public async Task<Tuple<bool, T>> TryGetAsync<T>( Type owner, long id )
        {
            var folder = await GetFolderForType( owner );
            // There's no way to check whether the file exists outside of this :-(
            var file = ( await folder.GetFilesAsync() ).FirstOrDefault( f => f.Name == owner.FullName );

            if ( file == null )
            {
                return Tuple.Create( false, default( T ) );
            }

            string key = id.ToString();

            var dateContainer = _settings.CreateContainer( owner.FullName + DateContainerSuffix, ApplicationDataCreateDisposition.Always );
            if ( (DateTimeOffset) dateContainer.Values[key] < DateTimeOffset.UtcNow )
            {
                await file.DeleteAsync();
                dateContainer.Values.Remove( key );

                if ( !( await folder.GetFilesAsync() ).Any() )
                {
                    await folder.DeleteAsync();
                }
                if ( dateContainer.Values.Count == 0 )
                {
                    _settings.DeleteContainer( dateContainer.Name );
                }

                return Tuple.Create( false, default( T ) );
            }

            using ( var stream = await file.OpenAsync( FileAccessMode.Read ) )
            using ( var reader = new StreamReader( stream.AsStreamForRead() ) )
            {
                string data = await reader.ReadToEndAsync();
                return Tuple.Create( true, Serializer.Deserialize<T>( data ) );
            }
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
            // HACK
            if ( expirationDate == DateTimeOffset.MaxValue )
            {
                throw new InvalidOperationException(
                    "DateTimeOffset.MaxValue cannot be used as the expiration date on the Windows Runtime because of a Windows Runtime bug."
                  + Environment.NewLine
                  + "Please use new DateTimeOffset( 9999, 12, 31, 00, 00, 00, TimeSpan.Zero ) instead." );
            }

            var folder = await GetFolderForType( owner );
            var file = await folder.CreateFileAsync( id.ToString(), CreationCollisionOption.ReplaceExisting );
            using ( var stream = await file.OpenAsync( FileAccessMode.ReadWrite ) )
            using ( var writer = new StreamWriter( stream.AsStreamForWrite() ) )
            {
                await writer.WriteAsync( Serializer.Serialize( value ) );
            }

            GetExpirationDateContainer( owner ).Values[id.ToString()] = expirationDate;
        }

        private ApplicationDataContainer GetExpirationDateContainer( Type owner )
        {
            return _settings.CreateContainer( owner.FullName + DateContainerSuffix, ApplicationDataCreateDisposition.Always );
        }

        private Task<StorageFolder> GetFolderForType( Type owner )
        {
            return ApplicationData.Current.LocalFolder.CreateFolderAsync( owner.FullName, CreationCollisionOption.OpenIfExists ).AsTask();
        }
    }
}