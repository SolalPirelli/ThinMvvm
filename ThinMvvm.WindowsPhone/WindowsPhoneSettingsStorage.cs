// Copyright (c) Solal Pirelli 2014
// See License.txt file for more details

using System.Collections.Generic;
using System.IO.IsolatedStorage;

namespace ThinMvvm.WindowsPhone
{
    /// <summary>
    /// Windows Phone implementation of <see cref="ISettingsStorage" />.
    /// </summary>
    public sealed class WindowsPhoneSettingsStorage : ISettingsStorage
    {
        // N.B.: To preserve objects, we need to have both in-memory values as well as stored ones.

        private readonly IsolatedStorageSettings _settings = IsolatedStorageSettings.ApplicationSettings;
        private readonly Dictionary<string, object> _values = new Dictionary<string, object>();

        /// <summary>
        /// Gets a value indicating whether the setting with the specified key is defined.
        /// </summary>
        /// <param name="key">The setting key.</param>
        /// <returns>A value indicating whether the setting with the specified key is defined.</returns>
        public bool IsDefined( string key )
        {
            return _settings.Contains( key );
        }

        /// <summary>
        /// Gets the value of the setting with the specified key, as an object of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of the setting value.</typeparam>
        /// <param name="key">The setting key.</param>
        /// <returns>The setting value.</returns>
        public T Get<T>( string key )
        {
            if ( _settings.Contains( key ) )
            {
                // Lazily load requested settings in memory
                if ( !_values.ContainsKey( key ) )
                {
                    _values[key] = (T) _settings[key];
                }

                return (T) _values[key];
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
            _values[key] = value;
            _settings[key] = value;
            _settings.Save();
        }
    }
}