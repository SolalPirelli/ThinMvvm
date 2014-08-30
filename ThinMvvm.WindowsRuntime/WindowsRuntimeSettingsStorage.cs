// Copyright (c) Solal Pirelli 2014
// See License.txt file for more details

using ThinMvvm.WindowsRuntime.Internals;
using Windows.Storage;

namespace ThinMvvm.WindowsRuntime
{
    /// <summary>
    /// <see cref="ISettingsStorage" /> implementation for the Windows Runtime.
    /// </summary>
    public sealed class WindowsRuntimeSettingsStorage : ISettingsStorage
    {
        private readonly ApplicationDataContainer _settings = ApplicationData.Current.LocalSettings;

        /// <summary>
        /// Gets a value indicating whether the setting with the specified key is defined.
        /// </summary>
        /// <param name="key">The setting key.</param>
        /// <returns>A value indicating whether the setting with the specified key is defined.</returns>
        public bool IsDefined( string key )
        {
            return _settings.Values.ContainsKey( key );
        }

        /// <summary>
        /// Gets the value of the setting with the specified key, as an object of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of the setting value.</typeparam>
        /// <param name="key">The setting key.</param>
        /// <returns>The setting value.</returns>
        public T Get<T>( string key )
        {
            if ( IsDefined( key ) )
            {
                string serializedValue = (string) _settings.Values[key];
                return Serializer.Deserialize<T>( serializedValue );
            }
            return default( T );
        }

        /// <summary>
        /// Sets the value of the setting with the specified key.
        /// </summary>
        /// <param name="key">The setting key.</param>
        /// <param name="value">The value.</param>
        public void Set( string key, object value )
        {
            _settings.Values[key] = Serializer.Serialize( value );
        }
    }
}