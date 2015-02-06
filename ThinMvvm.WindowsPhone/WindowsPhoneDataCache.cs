// Copyright (c) 2014-15 Solal Pirelli
// See License.txt file for more details

using System;
using System.Globalization;
using System.IO.IsolatedStorage;
using System.Threading.Tasks;

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
        /// Asynchronously gets the value stored by the specified owner type, with the specified ID.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="owner">The owner type.</param>
        /// <param name="id">The ID.</param>
        /// <returns>The cached value.</returns>
        public Task<CachedData<T>> GetAsync<T>( Type owner, long id )
        {
            if ( owner == null )
            {
                throw new ArgumentNullException( "owner" );
            }

            string key = GetKey( owner.FullName, id );

            if ( !_settings.Contains( key ) )
            {
                return Task.FromResult( new CachedData<T>() );
            }

            string dateKey = GetDateKey( owner.FullName, id );
            var expirationDate = (DateTime) _settings[dateKey];

            if ( expirationDate < DateTime.UtcNow )
            {
                _settings.Remove( key );
                _settings.Remove( dateKey );
                _settings.Save();
                return Task.FromResult( new CachedData<T>() );
            }

            return Task.FromResult( new CachedData<T>( (T) _settings[key] ) );
        }

        /// <summary>
        /// Asynchronously sets the specified value for the specified owner type, with the specified ID.
        /// </summary>
        /// <param name="owner">The owner type.</param>
        /// <param name="id">The ID.</param>
        /// <param name="expirationDate">The expiration date.</param>
        /// <param name="value">The value.</param>
        public Task SetAsync( Type owner, long id, DateTimeOffset expirationDate, object value )
        {
            if ( owner == null )
            {
                throw new ArgumentNullException( "owner" );
            }

            _settings[GetKey( owner.FullName, id )] = value;
            _settings[GetDateKey( owner.FullName, id )] = expirationDate.UtcDateTime;
            _settings.Save();

            return Task.FromResult( 0 );
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