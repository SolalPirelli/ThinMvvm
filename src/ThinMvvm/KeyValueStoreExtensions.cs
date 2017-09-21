namespace ThinMvvm
{
    /// <summary>
    /// Extensions to <see cref="IKeyValueStore" />.
    /// </summary>
    public static class KeyValueStoreExtensions
    {
        /// <summary>
        /// Gets the value corresponding to the specified key if it exists, or the specified default value.
        /// </summary>
        /// <typeparam name="T">The value's type.</typeparam>
        /// <param name="store">The store.</param>
        /// <param name="key">The key.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>The value if it was found, or the default value.</returns>
        public static T Get<T>( this IKeyValueStore store, string key, T defaultValue )
        {
            return store.Get<T>( key ).OrElse( defaultValue );
        }
    }
}