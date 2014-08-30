// Copyright (c) Solal Pirelli 2014
// See License.txt file for more details

using System;
using ThinMvvm.WindowsRuntime.Internals;
using Windows.Storage;

namespace ThinMvvm.WindowsRuntime
{
    public sealed class WindowsRuntimeDataCache : IDataCache
    {
        private const string DateContainerSuffix = "#Date";

        private readonly ApplicationDataContainer _settings = ApplicationData.Current.LocalSettings;


        public bool TryGet<T>( Type owner, long id, out T value )
        {
            var typeContainer = _settings.CreateContainer( owner.FullName, ApplicationDataCreateDisposition.Existing );
            if ( typeContainer == null )
            {
                value = default( T );
                return false;
            }

            var dateContainer = _settings.CreateContainer( owner.FullName + DateContainerSuffix, ApplicationDataCreateDisposition.Always );

            string key = id.ToString();
            object serializedValue;

            if ( typeContainer.Values.TryGetValue( key, out serializedValue ) )
            {
                var date = (DateTime) dateContainer.Values[key];
                if ( date < DateTime.Now )
                {
                    typeContainer.Values.Remove( key );
                    dateContainer.Values.Remove( key );

                    value = default( T );
                    return false;
                }

                value = Serializer.Deserialize<T>( (string) serializedValue );
                return true;
            }

            value = default( T );
            return false;
        }

        public void Set( Type owner, long id, DateTime expirationDate, object value )
        {
            var typeContainer = _settings.CreateContainer( owner.FullName, ApplicationDataCreateDisposition.Always );
            var dateContainer = _settings.CreateContainer( owner.FullName + DateContainerSuffix, ApplicationDataCreateDisposition.Always );
            string key = id.ToString();

            typeContainer.Values[key] = Serializer.Serialize( value );
            dateContainer.Values[key] = expirationDate;
        }
    }
}