// Copyright (c) 2014-15 Solal Pirelli
// See License.txt file for more details

namespace ThinMvvm
{
    /// <summary>
    /// Key-value settings storage.
    /// </summary>
    public interface ISettingsStorage
    {
        /// <summary>
        /// Gets a value indicating whether the setting with the specified key is defined.
        /// </summary>
        /// <param name="key">The setting key.</param>
        /// <returns>A value indicating whether the setting with the specified key is defined.</returns>
        bool IsDefined( string key );

        /// <summary>
        /// Gets the value of the setting with the specified key, as an object of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of the setting value.</typeparam>
        /// <param name="key">The setting key.</param>
        /// <returns>The setting value.</returns>
        T Get<T>( string key );

        /// <summary>
        /// Sets the value of the setting with the specified key.
        /// </summary>
        /// <param name="key">The setting key.</param>
        /// <param name="value">The value.</param>
        void Set( string key, object value );
    }
}