using System.Threading.Tasks;

namespace ThinMvvm
{
    /// <summary>
    /// Stores large chunks of data, each of which has an ID.
    /// </summary>
    public interface IDataStore
    {
        /// <summary>
        /// Asynchronously loads data with the specified ID, if it exists.
        /// </summary>
        /// <typeparam name="T">The data type.</typeparam>
        /// <param name="id">The data ID.</param>
        /// <returns>A task that represents the load operation.</returns>
        Task<Optional<T>> LoadAsync<T>( string id );

        /// <summary>
        /// Asynchronously stores the specified data with the specified ID.
        /// </summary>
        /// <typeparam name="T">The data type.</typeparam>
        /// <param name="id">The data ID.</param>
        /// <param name="data">The data.</param>
        /// <returns>A task that represents the store operation.</returns>
        Task StoreAsync<T>( string id, T data );

        /// <summary>
        /// Asynchronously deletes the data with the specified ID, if it exists.
        /// </summary>
        /// <param name="id">The data ID.</param>
        /// <returns>A task that represents the delete operation.</returns>
        Task DeleteAsync( string id );
    }
}