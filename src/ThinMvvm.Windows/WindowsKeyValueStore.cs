using System;
using System.Text;
using ThinMvvm.Windows.Infrastructure;
using Windows.Storage;

namespace ThinMvvm.Windows
{
    /// <summary>
    /// <see cref="IKeyValueStore" /> implementation for Windows.
    /// </summary>
    /// <remarks>
    /// This class assumes that it is the only settings manager in the app;
    /// it therefore makes no attempt at preventing key name conflicts with
    /// other classes that could access settings.
    /// </remarks>
    public sealed class WindowsKeyValueStore : IKeyValueStore
    {
        private readonly string _name;
        private bool _alive;
        private ApplicationDataContainer _container;

        /// <summary>
        /// Initializes a new instance of the <see cref="WindowsKeyValueStore" /> class using the specified settings container.
        /// </summary>
        /// <param name="containerName">The container name, or null to use the default container.</param>
        public WindowsKeyValueStore( string containerName )
        {
            _name = containerName;
            _alive = true;
        }


        /// <summary>
        /// Gets the value corresponding to the specified key, if it exists.
        /// </summary>
        /// <typeparam name="T">The value's type.</typeparam>
        /// <param name="key">The key.</param>
        /// <returns>The value, if it was found.</returns>
        public Optional<T> Get<T>( string key )
        {
            EnsureAlive();
            ValidateKey( key );

            if( !_container.Values.ContainsKey( key ) )
            {
                return default( Optional<T> );
            }

            var value = FromStorageValue<T>( _container.Values[key] );

            if( !( value is T ) )
            {
                throw new ArgumentException( $"The value associated with key '{key}' is of type '{value.GetType()}', not '{typeof( T )}'." );
            }

            return new Optional<T>( value );
        }

        /// <summary>
        /// Sets the specified value for the specified key.
        /// </summary>
        /// <typeparam name="T">The value's type.</typeparam>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public void Set<T>( string key, T value )
        {
            EnsureAlive();
            ValidateKey( key );

            _container.Values[key] = ToStorageValue( value );
        }

        /// <summary>
        /// Deletes the specified key and its associated value, if it exists.
        /// </summary>
        /// <param name="key">The key.</param>
        public void Delete( string key )
        {
            EnsureAlive();
            ValidateKey( key );

            _container.Values.Remove( key );
        }


        /// <summary>
        /// Indicates whether the underlying container has already been created.
        /// </summary>
        /// <returns>A value indicating whether the underlying container has already been created.</returns>
        public bool Exists()
        {
            return ApplicationData.Current.LocalSettings.Containers.ContainsKey( _name );
        }

        /// <summary>
        /// Clears all data in the container.
        /// </summary>
        public void Clear()
        {
            EnsureAlive();

            ApplicationData.Current.LocalSettings.DeleteContainer( _container.Name );
            _container = null;
        }

        /// <summary>
        /// Deletes the container, and all of its data.
        /// </summary>
        public void Delete()
        {
            Clear();
            _alive = false;
        }


        /// <summary>
        /// Ensures that the store is still alive, and creates the underlying container if necessary.
        /// </summary>
        private void EnsureAlive()
        {
            if( _container == null )
            {
                if( _alive )
                {
                    if( _name == null )
                    {
                        _container = ApplicationData.Current.LocalSettings;
                    }
                    else
                    {
                        _container = ApplicationData.Current.LocalSettings.CreateContainer( _name, ApplicationDataCreateDisposition.Always );
                    }
                }
                else
                {
                    throw new InvalidOperationException( "This container has been deleted." );
                }
            }
        }

        /// <summary>
        /// Validates the specified key.
        /// </summary>
        private static void ValidateKey( string key )
        {
            if( key == null )
            {
                throw new ArgumentNullException( nameof( key ) );
            }

            // According to https://msdn.microsoft.com/library/windows/apps/br241622,
            // key names can be up to 255 chars long.
            // It's unclear what happens with special characters that take up more code points than usual;
            // if somebody actually stores settings with keys like that and it fails... it's their own fault. :)
            if( key.Length > 255 )
            {
                throw new ArgumentException( $"Key name '{key}' is too long for Windows setting storage. Max length is 255 chars." );
            }
        }


        /// <summary>
        /// Converts the specified value to an object that can safely be stored.
        /// </summary>
        private static object ToStorageValue<T>( T value )
        {
            // According to https://msdn.microsoft.com/library/windows/apps/br241622
            // serialized values can be 8KB at most, i.e. 2KB max for a string assuming the worst in UTF-16.
            // Storing a string with 4096 ASCII chars fails as well, so there's probably a header,
            // which means we use 2000 characters as the cutoff point.
            const int MaxSize = 2000;

            var isString = typeof( T ) == typeof( string );

            if( !isString && WindowsSerializer.IsTypeNativelySupported( typeof( T ) ) )
            {
                // Non-string native types will never be too large.
                return value;
            }

            var stringValue = isString ? (string) (object) value : WindowsSerializer.Serialize( value );
            if( stringValue.Length <= MaxSize )
            {
                return stringValue;
            }

            var composite = new ApplicationDataCompositeValue();

            var fullCount = stringValue.Length / MaxSize;
            for( var n = 0; n < fullCount; n++ )
            {
                composite[n.ToString()] = stringValue.Substring( n * MaxSize, MaxSize );
            }
            if( stringValue.Length % MaxSize != 0 )
            {
                composite[fullCount.ToString()] = stringValue.Substring( fullCount * MaxSize );
            }

            return composite;
        }

        /// <summary>
        /// Converts the specified stored object to its original representation.
        /// </summary>
        private static T FromStorageValue<T>( object value )
        {
            var composite = value as ApplicationDataCompositeValue;
            if( composite == null )
            {
                if( WindowsSerializer.IsTypeNativelySupported( typeof( T ) ) )
                {
                    return (T) value;
                }

                return WindowsSerializer.Deserialize<T>( (string) value );
            }

            var builder = new StringBuilder();
            var n = 0;
            while( composite.ContainsKey( n.ToString() ) )
            {
                builder.Append( composite[n.ToString()] );
                n++;
            }

            var stringValue = builder.ToString();

            if( typeof( T ) == typeof( string ) )
            {
                return (T) (object) stringValue;
            }

            return WindowsSerializer.Deserialize<T>( stringValue );
        }
    }
}