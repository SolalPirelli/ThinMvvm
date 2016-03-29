namespace ThinMvvm
{
    /// <summary>
    /// Manages small key-value pairs that can be quickly and synchronously retrieved from persistent storage.
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
    }
}