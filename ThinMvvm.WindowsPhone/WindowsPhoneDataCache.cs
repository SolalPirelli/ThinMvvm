// Copyright (c) Solal Pirelli 2014
// See License.txt file for more details

using System;
using System.Globalization;
using System.IO.IsolatedStorage;

namespace ThinMvvm.WindowsPhone
{
    /// <summary>
    /// <see cref="IDataCache" /> implementation for Windows Phone, using isolated storage settings.
    /// </summary>
    public sealed class WindowsPhoneDataCache : IDataCache
    {
        // N.B.: The default serializer does nonsense with DateTimeOffsets, 
        //       so they have to be converted to and from DateTimes

        private const string DataKeyFormat = "ThinMvvm.WindowsPhone.DataCache.{0}_{1}";
        private const string ExpirationDateKeyFormat = "ThinMvvm.WindowsPhone.ExpirationDateCache.{0}_{1}";

        private readonly IsolatedStorageSettings _settings = IsolatedStorageSettings.ApplicationSettings;


        /// <summary>
        /// Attempts to get the value stored by the specified owner type, with the specified ID.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="owner">The owner type.</param>
        /// <param name="id">The ID.</param>
        /// <param name="value">The value, if any.</param>
        /// <returns>A value indicating whether a value was found.</returns>
        public bool TryGet<T>( Type owner, long id, out T value )
        {
            if ( owner == null )
            {
                throw new ArgumentNullException( "owner" );
            }

            string key = GetKey( owner.FullName, id );

            if ( !_settings.Contains( key ) )
            {
                value = default( T );
                return false;
            }

            string dateKey = GetDateKey( owner.FullName, id );
            var expirationDate = (DateTime) _settings[dateKey];

            if ( expirationDate < DateTime.UtcNow )
            {
                _settings.Remove( key );
                _settings.Remove( dateKey );
                _settings.Save();
                value = default( T );
                return false;
            }

            value = (T) _settings[key];
            return true;
        }

        /// <summary>
        /// Sets the specified value for the specified owner type, with the specified ID.
        /// </summary>
        /// <param name="owner">The owner type.</param>
        /// <param name="id">The ID.</param>
        /// <param name="expirationDate">The expiration date.</param>
        /// <param name="value">The value.</param>
        public void Set( Type owner, long id, DateTimeOffset expirationDate, object value )
        {
            if ( owner == null )
            {
                throw new ArgumentNullException( "owner" );
            }

            _settings[GetKey( owner.FullName, id )] = value;
            _settings[GetDateKey( owner.FullName, id )] = expirationDate.UtcDateTime;
            _settings.Save();
        }


        /// <summary>
        /// Gets the setting key associated with the specified key and ID.
        /// </summary>
        private static string GetKey( string key, long id )
        {
            return string.Format( CultureInfo.InvariantCulture, DataKeyFormat, key, id );
        }

        private static string GetDateKey( string key, long id )
        {
            return string.Format( CultureInfo.InvariantCulture, ExpirationDateKeyFormat, key, id );
        }
    }
}