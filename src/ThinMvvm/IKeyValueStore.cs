namespace ThinMvvm
{
    /// <summary>
    /// Stores small key-value pairs that can be quickly and synchronously retrieved.
    /// </summary>
    public interface IKeyValueStore
    {
        /// <summary>
        /// Gets the value corresponding to the specified key, if it exists.
        /// </summary>
        /// <typeparam name="T">The value's type.</typeparam>
        /// <param name="key">The key.</param>
        /// <returns>The value, if it was found.</returns>
        Optional<T> Get<T>( string key );

        /// <summary>
        /// Sets the specified value for the specified key.
        /// </summary>
        /// <typeparam name="T">The value's type.</typeparam>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        void Set<T>( string key, T value );

        /// <summary>
        /// Deletes the specified key and its associated value, if it exists.
        /// </summary>
        /// <param name="key">The key.</param>
        void Delete( string key );
        
        /// <summary>
        /// Deletes all keys and values.
        /// </summary>
        void Clear();
    }
}