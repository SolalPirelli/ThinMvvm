// Copyright (c) Solal Pirelli 2014
// See License.txt file for more details

using System;
using ThinMvvm.WindowsRuntime.Internals;
using Windows.Storage;

namespace ThinMvvm.WindowsRuntime
{
    /// <summary>
    /// <see cref="IDataCache" /> implementation for the Windows Runtime.
    /// </summary>
    public sealed class WindowsRuntimeDataCache : IDataCache
    {
        // N.B.: CreateContainer(string, ApplicationDataCreateDisposition) will throw 
        //       if it doesn't exist when using Existing, so we have to use Always instead.

        private const string DateContainerSuffix = "#Date";

        private readonly ApplicationDataContainer _settings = ApplicationData.Current.LocalSettings;


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
            string key = id.ToString();
            var container = _settings.CreateContainer( owner.FullName, ApplicationDataCreateDisposition.Always );
            if ( !container.Values.ContainsKey( key ) )
            {
                value = default( T );
                return false;
            }

            var dateContainer = _settings.CreateContainer( owner.FullName + DateContainerSuffix, ApplicationDataCreateDisposition.Always );
            if ( (DateTimeOffset) dateContainer.Values[key] < DateTimeOffset.UtcNow )
            {
                container.Values.Remove( key );
                dateContainer.Values.Remove( key );

                if ( container.Values.Count == 0 )
                {
                    _settings.DeleteContainer( container.Name );
                    _settings.DeleteContainer( dateContainer.Name );
                }

                value = default( T );
                return false;
            }

            value = Serializer.Deserialize<T>( (string) container.Values[key] );
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
            var typeContainer = _settings.CreateContainer( owner.FullName, ApplicationDataCreateDisposition.Always );
            var dateContainer = _settings.CreateContainer( owner.FullName + DateContainerSuffix, ApplicationDataCreateDisposition.Always );
            string key = id.ToString();

            typeContainer.Values[key] = Serializer.Serialize( value );
            dateContainer.Values[key] = expirationDate;
        }
    }
}