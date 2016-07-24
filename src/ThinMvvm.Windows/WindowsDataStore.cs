using System;
using System.IO;
using System.Threading.Tasks;
using ThinMvvm.Windows.Infrastructure;
using Windows.Storage;

namespace ThinMvvm.Windows
{
    /// <summary>
    /// <see cref="IDataStore" /> implementation for Windows.
    /// </summary>
    /// <remarks>
    /// Since this class uses files internally, it assumes that other classes may also use files,
    /// and therefore attempts to make IDs unique by prefixing them. 
    /// It does, however, assume that once the IDs are prefixed they are in fact unique.
    /// </remarks>
    public sealed class WindowsDataStore : IDataStore
    {
        private static readonly char[] InvalidFileNameChars = Path.GetInvalidFileNameChars();

        private readonly string _folderName;


        /// <summary>
        /// Initializes a new instance of the <see cref="WindowsDataStore" /> class using the specified folder.
        /// </summary>
        /// <param name="folderName">The folder name, or null to use the default folder.</param>
        public WindowsDataStore( string folderName )
        {
            _folderName = folderName;
        }


        /// <summary>
        /// Asynchronously loads data with the specified ID, if it exists.
        /// </summary>
        /// <typeparam name="T">The data type.</typeparam>
        /// <param name="id">The data ID.</param>
        /// <returns>A task that represents the loading operation.</returns>
        public async Task<Optional<T>> LoadAsync<T>( string id )
        {
            id = MakeAndValidateUniqueId( id );

            var folder = await GetFolderAsync();
            var file = await folder.TryGetItemAsync( id );
            if( file == null )
            {
                return default( Optional<T> );
            }
            if( !file.IsOfType( StorageItemTypes.File ) )
            {
                // Don't just return "none", which would imply the value can be set.
                throw new InvalidOperationException( $"Conflicting non-file item found with name '{id}'." );
            }

            var serializedData = await FileIO.ReadTextAsync( (StorageFile) file );

            return new Optional<T>( WindowsSerializer.Deserialize<T>( serializedData ) );
        }

        /// <summary>
        /// Asynchronously stores the specified data with the specified ID.
        /// </summary>
        /// <typeparam name="T">The data type.</typeparam>
        /// <param name="id">The data ID.</param>
        /// <param name="data">The data.</param>
        /// <returns>A task that represents the storing operation.</returns>
        public async Task StoreAsync<T>( string id, T data )
        {
            id = MakeAndValidateUniqueId( id );

            var folder = await GetFolderAsync();
            var file = await folder.CreateFileAsync( id, CreationCollisionOption.ReplaceExisting );

            var serializedData = WindowsSerializer.Serialize( data );

            await FileIO.WriteTextAsync( file, serializedData );
        }

        /// <summary>
        /// Asynchronously deletes the data with the specified ID, if it exists.
        /// </summary>
        /// <param name="id">The data ID.</param>
        /// <returns>A task that represents the deleting operation.</returns>
        public async Task DeleteAsync( string id )
        {
            id = MakeAndValidateUniqueId( id );

            var folder = await GetFolderAsync();
            var file = await folder.TryGetItemAsync( id );
            if( file != null )
            {
                await file.DeleteAsync();
            }
        }

        /// <summary>
        /// Asynchronously gets the folder to use.
        /// </summary>
        private Task<StorageFolder> GetFolderAsync()
        {
            if( _folderName == null )
            {
                return Task.FromResult( ApplicationData.Current.LocalFolder );
            }

            return ApplicationData.Current.LocalFolder.CreateFolderAsync( _folderName, CreationCollisionOption.OpenIfExists ).AsTask();
        }

        /// <summary>
        /// Makes the specified ID unique, and validates it.
        /// </summary>
        private static string MakeAndValidateUniqueId( string id )
        {
            if( id == null )
            {
                throw new ArgumentNullException( nameof( id ) );
            }

            if( id.IndexOfAny( InvalidFileNameChars ) != -1 )
            {
                throw new ArgumentException( $"ID '{id}' is not valid in Windows storage because it uses invalid filename characters." );
            }

            // This should be relatively short, to avoid file name length limits.
            id = "TM_" + id;

            if( id.Length > 255 )
            {
                throw new ArgumentException( $"ID '{id}' is too long for Windows storage. Maximum length is 255." );
            }

            return id;
        }
    }
}