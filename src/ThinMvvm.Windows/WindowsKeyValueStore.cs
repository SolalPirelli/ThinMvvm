using System;
using ThinMvvm.Windows.Infrastructure;
using Windows.Storage;

namespace ThinMvvm.Windows
{
    /// <summary>
    /// <see cref="IKeyValueStore" /> implementation for Windows.
    /// Supports settings whose serialized representation uses 8 KB or less.
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

            var value = _container.Values[key];

            if( WindowsSerializer.IsTypeNativelySupported( typeof( T ) ) )
            {
                if( !( value is T ) )
                {
                    throw new ArgumentException( $"The value associated with key '{key}' is of type '{value.GetType()}', not '{typeof( T )}'." );
                }

                return new Optional<T>( (T) value );
            }

            if( !( value is string ) )
            {
                throw new ArgumentException( $"The value associated with key '{key}' is of type '{value.GetType()}', not a serialized object of type '{typeof( T )}'." );
            }

            return new Optional<T>( WindowsSerializer.Deserialize<T>( (string) value ) );
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

            if( WindowsSerializer.IsTypeNativelySupported( typeof( T ) ) )
            {
                _container.Values[key] = value;
            }
            else
            {
                var serializedValue = WindowsSerializer.Serialize( value );
                
                // According to https://msdn.microsoft.com/library/windows/apps/br241622,
                // values can contain up to 8K bytes.
                // There may be more bytes than there are characters, so the UWP call may still fail,
                // but at least let's provide a sensible error message if we know it will fail.
                if( serializedValue.Length > 8 * 1024 )
                {
                    throw new InvalidOperationException( "Cannot store objects whose serialized size is over 8K bytes in Windows storage." );
                }

                _container.Values[key] = serializedValue;
            }
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
        /// Deletes the container, and all of its key-value pairs.
        /// </summary>
        public void Delete()
        {
            EnsureAlive();

            ApplicationData.Current.LocalSettings.DeleteContainer( _container.Name );
            _container = null;
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
    }
}